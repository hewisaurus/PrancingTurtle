using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Logging;
using MySql.Data.MySqlClient;
using Dapper;

namespace AutoExtracterAsync
{
    class Program
    {
        private static string _watchFolder;
        private static string _extractParent;
        private static string _afterExtractionParent;
        private static string _databaseConnectionString;

        private const string SmallSeparator = "-----------------------";
        private const string LargeSeparator = "=======================";

        private static IConnectionFactory _connectionFactory;
        private static ILogger _logger;

        static void Main(string[] args)
        {
            if (!args.Any())
            {
                #region Normal watcher mode (Original startup)
                #region Setup
                try
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    _watchFolder = appSettings["watchFolder"];
                    _extractParent = appSettings["extractionFolder"];
                    _afterExtractionParent = appSettings["archiveFolder"];
                    var connectionStrings = ConfigurationManager.ConnectionStrings;
                    _databaseConnectionString = connectionStrings["PTGalera"].ConnectionString;
                    _logger = new NLogHandler();


                    _logger.Debug("Debug log initialized");
                    _logger.Info("Info log initialized");

                    Console.WriteLine("Starting in watcher mode");
                    Console.WriteLine("-----------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error setting up in watcher mode! {0}", ex.Message);
                    Thread.Sleep(5000);
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

                if (Directory.Exists(_extractParent))
                {
                    Console.WriteLine("Extraction directory: {0}", _extractParent);
                }
                else
                {
                    Console.WriteLine("The folder \"{0}\" specified by the extractionFolder setting doesn't exist!", _extractParent);
                    Environment.Exit(1);
                }

                if (Directory.Exists(_afterExtractionParent))
                {
                    Console.WriteLine("Archive directory: {0}", _afterExtractionParent);
                }
                else
                {
                    Console.WriteLine("The folder \"{0}\" specified by the archiveFolder setting doesn't exist!", _afterExtractionParent);
                    Environment.Exit(1);
                }
                #endregion
                #region Attach FileSystemWatcher
                var quitEvent = new ManualResetEvent(false);

                var uploadWatcher = new FileSystemWatcher(_watchFolder);
                uploadWatcher.NotifyFilter = NotifyFilters.FileName;
                uploadWatcher.Created += UploadWatcherOnCreated;
                uploadWatcher.EnableRaisingEvents = true;

                Console.WriteLine();
                Console.WriteLine("=============================");
                Console.WriteLine("===== Watcher attached! =====");
                Console.WriteLine("==--Waiting for new files--==");
                Console.WriteLine("=============================");
                Console.WriteLine();

                quitEvent.WaitOne();
                #endregion
                #endregion

            }

            if (args.Count() != 1)
            {
                Console.WriteLine("The wrong number of arguments have been specified, and this application will now exit.");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            string fullPath = args[0].Trim();

            // Check that the fullpath actually refers to a file and if it does, extract it

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("\"{0}\" does not exist, and this application will now exit.", fullPath);
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            #region Manual Mode - setup

            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                _watchFolder = appSettings["watchFolder"];
                _extractParent = appSettings["extractionFolder"];
                _afterExtractionParent = appSettings["archiveFolder"];
                var connectionStrings = ConfigurationManager.ConnectionStrings;
                _databaseConnectionString = connectionStrings["PTGalera"].ConnectionString;
                _logger = new NLogHandler();


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
                ExtractLog(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred in the main extraction method! {0}", ex.Message);
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

        private static void UploadWatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            try
            {
                Console.WriteLine(SmallSeparator);
                Console.WriteLine("New file detected: {0}", fileSystemEventArgs.FullPath);
                Console.WriteLine("Opening new window!");
                // Sleep before we do anything in case the OS / antivirus still has a handle on the file for whatever reason, or it's taking a while to fully extract
                Thread.Sleep(5000);

                var pArgs = string.Format("\"{0}\"", fileSystemEventArgs.FullPath);

                var p = new Process
                {
                    StartInfo = new ProcessStartInfo("AutoExtracterAsync.exe", pArgs)
                    {
                        //UseShellExecute = false
                    }
                };
                p.Start();

                // Wait a few second before we potentially pop open another window
                Thread.Sleep(5000);
                Console.WriteLine(SmallSeparator);
            }
            catch (Exception ex)
            {
                Console.WriteLine(LargeSeparator);
                Console.WriteLine("An error has occurred within the watcher method!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(LargeSeparator);
            }
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private static void ExtractLog(string fullPath)
        {
            Console.WriteLine("New file detected: {0}", fullPath);
            Console.WriteLine("---------------------------");

            var info = new FileInfo(fullPath);
            #region Check that it's a supported file
            if (info.Extension != ".zip")
            {
                Console.WriteLine("{0} does not match the required file types and will be deleted.", info.Name);
                Thread.Sleep(3000);
                try
                {
                    info.Delete();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to delete {0}: {1}", info.FullName, ex.Message);
                    return;
                }
            }
            #endregion
            #region Check that a this archive matches a session
            var dbConnection = new MySqlConnection(_databaseConnectionString);
            dbConnection.Open();
            string uploadToken = info.Name.Replace(".zip", "");
            Console.WriteLine("Looking for a SessionLog with the token {0}", uploadToken);
            var sessionLog = dbConnection.Query<SessionLog, Guild, SessionLog>
                (Database.MySQL.AutoExtracter.GetSessionLogByToken,
                    (sl, g) =>
                    {
                        sl.Guild = g;
                        return sl;
                    },
                new { @token = uploadToken }).SingleOrDefault();
            dbConnection.Close();

            if (sessionLog == null)
            {
                Console.WriteLine("{0} does not match a valid session token and will be moved to the orphaned folder.", info.Name);
                if (File.Exists(_afterExtractionParent + "OrphanedArchives\\" + info.Name))
                {
                    try
                    {
                        Console.Write("Deleting {0} (already exists in the orphaned folder)", info.Name);
                        info.Delete();
                        Console.WriteLine("   (OK)");
                        Console.WriteLine("------------------------");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("   (FAILED)");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("==============================");
                        Console.WriteLine();
                    }
                }
                else
                {
                    try
                    {
                        var archiveParent = new DirectoryInfo(_afterExtractionParent);
                        DirectoryInfo orphanFolder = new DirectoryInfo(archiveParent + "\\OrphanedArchives");
                        info.MoveTo(orphanFolder.FullName + "\\" + info.Name);
                        Console.WriteLine(" (OK)");
                        Console.WriteLine("------------------------");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to move {0}: {1}", info.FullName, ex.Message);
                        return;
                    }
                }
            }

            Console.WriteLine("Session Log belongs to {0}", sessionLog.Guild.Name);
            #endregion
            #region Remove spaces if there are any and replace
            if (info.Name.Contains(" "))
            {
                string newFilename = info.Name.Replace(" ", "");
                Console.Write("Renaming {0} to {1}", info.Name, newFilename);
                try
                {
                    info.MoveTo(newFilename);
                    Console.WriteLine("   (OK)");
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   (FAILED)");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("==============================");
                    Console.WriteLine();
                    return;
                }
            }

            var cleanFilename = CleanFileName(info.Name);
            if (cleanFilename != info.Name)
            {
                Console.Write("Removing invalid characters: Renaming {0} to {1}", info.Name, cleanFilename);
                try
                {
                    info.MoveTo(cleanFilename);
                    Console.WriteLine("   (OK)");
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   (FAILED)");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("==============================");
                    Console.WriteLine();
                    return;
                }
            }
            #endregion
            try
            {
                #region Create the parent folder to hold the CombatLog

                var parent = new DirectoryInfo(_extractParent);
                string folderName = info.Name.Replace(info.Extension, "");
                DirectoryInfo extractParentFolder = null;
                if (!Directory.Exists(parent + "\\" + folderName))
                {
                    Console.Write("Creating folder {0}", folderName);
                    try
                    {
                        extractParentFolder = parent.CreateSubdirectory(folderName);
                        Console.WriteLine("   (OK)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("   (FAILED)");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("==============================");
                        Console.WriteLine();
                    }
                }
                else
                {
                    extractParentFolder = new DirectoryInfo(parent + "\\" + folderName);
                }
                #endregion
                Thread.Sleep(1000);
                #region Extract the archive

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "7za.exe",
                    Arguments = "e \"" + info.FullName + "\"" + " -o" + extractParentFolder.FullName + " -r -y",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                // Extract all files and overwrite if the file already exists
                //Console.WriteLine("About to run 7-zip with these arguments: {0}", psi.Arguments);
                int extractionfailures = 0;
                while (true)
                {
                    try
                    {
                        Console.Write("Extracting {0} (Attempt #{1})", info.Name, extractionfailures + 1);
                        Process proc = Process.Start(psi);
                        proc.WaitForExit();
                        if (proc.ExitCode == 0)
                        {
                            Console.WriteLine("   (OK)");
                            Thread.Sleep(2000);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("   (ERROR {0})", proc.ExitCode);
                            extractionfailures++;
                            if (extractionfailures > 360) // 60 minutes
                            {
                                Console.WriteLine("Too many failures!");
                                break;
                            }
                            Console.WriteLine("Waiting 10 seconds, then trying again...");
                            Thread.Sleep(10000);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("   (FAILED)");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("==============================");
                        extractionfailures++;
                        if (extractionfailures > 360)
                        {
                            Console.WriteLine("Too many failures!");
                            break;
                        }
                        Console.WriteLine("Waiting 10 seconds, then trying again...");
                        Thread.Sleep(10000);
                    }
                }

                #endregion

                DirectoryInfo guildParent = null;
                #region Make sure Guild folder exists
                string guildFolderName = "Guild" + sessionLog.GuildId;
                if (!Directory.Exists(_afterExtractionParent + guildFolderName))
                {
                    try
                    {
                        DirectoryInfo afterExtractionParentDi = new DirectoryInfo(_afterExtractionParent);
                        guildParent = afterExtractionParentDi.CreateSubdirectory(guildFolderName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error creating {0}", guildFolderName);
                    }
                }
                guildParent = new DirectoryInfo(_afterExtractionParent + guildFolderName);
                #endregion
                #region Make sure Session folder exists inside the guild folder
                DirectoryInfo sessionFolder = null;
                string sessionFolderName = "Session" + sessionLog.SessionId;
                if (!Directory.Exists(guildParent.FullName + "\\" + sessionFolderName))
                {
                    try
                    {
                        sessionFolder = guildParent.CreateSubdirectory(sessionFolderName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error creating {0}", sessionFolderName);
                    }
                }
                sessionFolder = new DirectoryInfo(guildParent.FullName + "\\" + sessionFolderName);
                #endregion

                // Move the archive out of the folder into our archive location. No pun intended.
                if (File.Exists(sessionFolder.FullName + "\\" + info.Name))
                {
                    // File already exists in the target location, so just delete it
                    try
                    {
                        Console.Write("Deleting {0} (already exists in the archive location)", info.Name);
                        info.Delete();
                        Console.WriteLine("   (OK)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("   (FAILED)");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("==============================");
                        Console.WriteLine();
                    }
                }
                else
                {
                    int moveFailures = 0;
                    while (true)
                    {
                        try
                        {
                            Console.Write("Moving {0} to {1}", info.Name, sessionFolder.FullName);
                            info.MoveTo(sessionFolder.FullName + "\\" + info.Name);
                            Console.WriteLine("   (OK)");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("   (FAILED)");
                            Console.WriteLine(ex.Message);
                            moveFailures++;
                            if (moveFailures > 60)
                            {
                                Console.WriteLine("Too many failures!");
                                break;
                            }
                            Console.WriteLine("Waiting 10 seconds, then trying again...");
                            Thread.Sleep(10000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred:");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("==============================");
            Console.WriteLine();
        }
    }
}
