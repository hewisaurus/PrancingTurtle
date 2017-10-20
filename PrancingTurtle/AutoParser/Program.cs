using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using Database;
using Database.Repositories;
using Database.Repositories.Interfaces;
using Logging;

namespace AutoParser
{
    class Program
    {
        private static string _watchFolder;
        private static IAutoParserRepository _repository;
        
        private static IConnectionFactory _connectionFactory;
        private static ILogger _logger;

        private static string smallSeparator = "-----------------------";
        private static string largeSeparator = "=======================";

        static void Main(string[] args)
        {
            // Regardless of what mode we're starting in, ensure that the databaseImport folder exists so we can create the CSV files for bulk import!
            if (!Directory.Exists("databaseImport"))
            {
                // Folder doesn't exist. Attempt to create it here
                try
                {
                    Directory.CreateDirectory("databaseImport");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("This application cannot continue without the databaseImport folder existing alongside the application, and will now exit.");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }
            }

            if (!args.Any())
            {
                #region Watcher mode
                Console.WriteLine("Started in 'watcher' mode");
                #region Setup
                try
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    _watchFolder = appSettings["watchFolder"];
                    _connectionFactory = new MySqlConnectionFactory(ConfigurationManager.ConnectionStrings["PTGalera"].ConnectionString);
                    _logger = new NLogHandler();

                    _repository = new AutoParserRepository(_connectionFactory, _logger);

                    _logger.Debug("Debug log initialized");
                    _logger.Info("Info log initialized");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an setting up, and this application cannot continue.");
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                #endregion

                #region Check that folders exist
                if (Directory.Exists(_watchFolder))
                {
                    Console.WriteLine("Watching directory: {0}", _watchFolder);
                }
                else
                {
                    Console.WriteLine("The folder \"{0}\" specified by the watchFolder setting doesn't exist!", _watchFolder);
                    Environment.Exit(1);
                }
                #endregion


                try
                {
                    var quitEvent = new ManualResetEvent(false);

                    var fsw = new FileSystemWatcher(_watchFolder)
                    {
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.FileName
                    };
                    fsw.Created += FswOnCreated;

                    fsw.EnableRaisingEvents = true;

                    Console.WriteLine();
                    Console.WriteLine("===== Watcher attached! =====");
                    Console.WriteLine("Waiting for new files...");
                    Console.WriteLine();

                    Thread.Sleep(2000);

                    quitEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error attaching the FileSystemWatcher! {0}", ex.Message);
                    Console.ReadLine();
                }
                #endregion
            }

            if (args.Count() != 2)
            {
                Console.WriteLine("The wrong number of arguments have been specified, and this application will now exit.");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            string folderFile = args[0].Trim();
            string fullPath = args[1].Trim();

            // Check that the fullpath actually refers to a file and if it does, parse it

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("\"{0}\" does not exist, and this application will now exit.", fullPath);
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            #region Manual Mode - setup

            try
            {
                _connectionFactory = new MySqlConnectionFactory(ConfigurationManager.ConnectionStrings["PTGalera"].ConnectionString);
                _logger = new NLogHandler();

                _repository = new AutoParserRepository(_connectionFactory, _logger);

                _logger.Debug("Debug log initialized");
                _logger.Info("Info log initialized");

                Console.WriteLine("Starting in manual mode");
                Console.WriteLine("-----------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error setting up in manual mode! {0}", ex.Message);
                Thread.Sleep(5000);
                Environment.Exit(1);
            }
            
            #endregion

            try
            {
                //Console.WriteLine(folderFile);
                //Console.WriteLine(fullPath);
                ImportLog(folderFile, fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred in the main parsing method! {0}", ex.Message);
                Thread.Sleep(5000);
            }
            
#if DEBUG
            Console.WriteLine("Finished, but not exiting as we're in debug mode");
            Console.ReadLine();
#endif

            Console.WriteLine("Finished! Exiting in 3 seconds.");
            Thread.Sleep(3000);
            Environment.Exit(0);
        }

        private static void LogLine(string message)
        {
            Console.WriteLine(message);
            _logger.Debug(message);
        }

        private static void ImportLog(string name, string fullPath)
        {
            // name should be folder\\filename.txt
            // fullPath should be c:\path\to\file.txt

            string logFilename = null;
            #region Check to see if we can continue
            // We're expecting to see folder\file so if there's no backslash, ignore the file
            if (name.IndexOf(@"\", StringComparison.Ordinal) != -1)
            {
                // Secondly, if there is more than one backslash, also ignore it
                string[] changePath = name.Split('\\');
                if (changePath.Length == 2)
                {
                    // All good so far
                    // Regardless of what the actual log names are (there may be more than one), check that our folder name
                    // exists as an UploadToken on a session that's waiting for its log
                    var token = changePath[0].ToLower();
                    LogLine(string.Format("Looking for a session with the token {0}", token));
                    var logInfo = _repository.GetInfoByToken(token);

                    if (logInfo == null)
                    {
                        Console.WriteLine("No SessionLog exists with this token. Deleting...");
                        var file = new FileInfo(fullPath);
                        file.Delete();
                        return;
                    }

                    logFilename = fullPath.Replace("\\\\", "\\");

                    LogLine(string.Format("Log matches session {0} ({1})", logInfo.SessionId, logInfo.SessionName));
                    LogLine(string.Format("Owner: {0}", logInfo.OwnerInfo));
                    LogLine(string.Format("Uploader: {0}", logInfo.UploaderInfo));

                    Methods.ParseAndSave(_logger, logInfo, logFilename, smallSeparator, largeSeparator, _repository);
                    //testing
                    //Methods.ParseAndSave_v2(_logger, logInfo, logFilename, smallSeparator, largeSeparator, _repository);

                    _logger.Debug(string.Format("Finished with {0}", logFilename));
                }
                else
                {
                    Console.WriteLine("The new file detected is nested inside too many folders and will be deleted.");
                }
            }
            else
            {
                Console.WriteLine("The new file detected is not inside a folder and will be deleted.");
            }
            #endregion
            Thread.Sleep(2000);
            #region Remove log
            // Delete this log file, and if there are no other files in the directory, delete it as well.
            if (string.IsNullOrEmpty(logFilename)) logFilename = fullPath.Replace("\\\\", "\\");
            var thisLog = new FileInfo(logFilename);
            try
            {
                thisLog.Delete();
                Console.WriteLine("{0} deleted.", logFilename);
                _logger.Debug(string.Format("{0} deleted.", logFilename));
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("The following error occurred while trying to delete {0}: {1}", logFilename, ex.Message));
            }
            #endregion
            #region Remove parent if it's empty
            // Now check the folder to see if there are any more files in it - if there are not, delete it
            string parentPath = logFilename.Substring(0, logFilename.LastIndexOf("\\", StringComparison.Ordinal));
            DirectoryInfo parentDi = new DirectoryInfo(parentPath);
            if (!parentDi.GetDirectories().Any() && !parentDi.GetFiles().Any())
            {
                try
                {
                    Console.WriteLine("{0} is empty, removing folder", parentPath);
                    parentDi.Delete();
                    Console.WriteLine("{0} removed successfully.", parentPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error removing {0}: {1}", parentPath, ex.Message);
                }
            }
            #endregion
        }

        private static void FswOnCreated(object sender, FileSystemEventArgs args)
        {
            try
            {
                Console.WriteLine(smallSeparator);
                Console.WriteLine("New file detected: {0}", args.FullPath);
                Console.WriteLine("Opening new window!");
                // Sleep before we do anything in case the OS / antivirus still has a handle on the file for whatever reason, or it's taking a while to fully extract
                Thread.Sleep(15000);

                // WORKS! Use this to import files one at a time
                //ImportLog(args.Name, args.FullPath);

                var pArgs = string.Format("\"{0}\" \"{1}\"", args.Name, args.FullPath);

                var p = new Process
                {
                    StartInfo = new ProcessStartInfo("AutoParser.exe", pArgs)
                    {
                        //UseShellExecute = false
                    }
                };
                p.Start();
                
                // Wait a few second before we potentially pop open another window
                Thread.Sleep(5000);
                Console.WriteLine(smallSeparator);
            }
            catch (Exception ex)
            {
                Console.WriteLine(largeSeparator);
                Console.WriteLine("An error has occurred within the watcher method!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(largeSeparator);
            }
        }


    }
}
