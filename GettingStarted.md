# Getting started

This is a quick guide to getting the project to an operational state.. beyond that it's up to you! Please follow the entire guide before running the solution for the first time, as it covers some important points.

PT is a Visual Studio 2017 solution & targets .NET 4.6.2 with each project. For some reason, the VS2017 installer doesn't have the option for this, so you'll have to install it manually.

### Get the .NET 4.6.2 SDK / targeting pack
Download and install the developer pack from [here](https://www.microsoft.com/en-us/download/details.aspx?id=53321).

### GitHub extension for Visual Studio
While it's not absolutely necessary, it's pretty handy. Consider installing it via the Visual Studio Installer (where you manually select the additional packages to install).

### Clone the repo
If you're already familiar with how to clone or have your own way of doing it, go ahead and do that here. Otherwise, read on.

In Visual Studio, open Team Explorer and click 'Clone' underneath the GitHub section. If you're not already logged in, you'll be prompted for authentication, and then you'll be able to select a repository to clone. Find the PrancingTurtle repository, select which path you'd like to clone it to, and hit the Clone button below.

### Copy example files (mail account info and DB connection strings)
In the PrancingTurtle (web) project, the two files you'll need to create are **PrancingTurtle/ConnectionStrings.config** and **PrancingTurthe/Helpers/Mail/AccountInfo.cs**. Example files are provided next to where the live files should live, so you can copy/paste their contents and create the new files. This is in place so that you're able to point your debug copy of PT to a local MySQL server for ease of use, as well as the mail engine if you'd like to debug that too.

You can change the name of the connection strings away from **PTGalera** and **PTHangfire** if you wish, but be sure to change them in the dependency injection registry as well (**PrancingTurtle/DependencyResolution/Registries/RepositoryRegistry.cs**)

### Set the web project as the startup project
Right-click the PrancingTurtle web project and click "Set as Startup Project" if it isn't set already... unless, of course, you're debugging the AutoExtracter or AutoParser projects.

### Import the DB schema to a local MySQL server
You can find the default schema [here](https://github.com/hewisaurus/PrancingTurtle/blob/master/PTSchema-20171016.sql). It includes create statements for the tables, so you can copy/paste it directly into something like MySQL Workbench and execute it to get a working (but empty) database.

The following files include the bare minimum data required in order to have a useful site. There is one user, **testuser@domain.com** and its password is **qwerty12345**
 * Minimum data with no sessions [here](https://github.com/hewisaurus/PrancingTurtle/blob/master/PTBasicData-IncGuild-NoSessions.sql)
 * Minimum data with 2 sessions (here soon)
 * Minimum data with 5 sessions (here soon)

**Schemas that include test guilds and existing sessions will be linked here also**

**TODO: Provide options for schema (smaller / larger in terms of encounter and session #s)

### Hangfire - a background job execution framework
PT uses Hangfire to coordinate a series of background tasks and the existing Quartz.NET tasks will be moved to Hangfire in the future. As long as you configure a working MySQL/MariaDB server within the web project's **ConnectionStrings.config** file, the Hangfire database will be created automatically.

## Information specific to individual projects

### AutoExtracter

To run this project (rather than debug it), first you need to configure the settings for the application itself. These are located within the **AutoExtracterAsync.exe.config** file that sits alongside the executable. The key values to modify are:
 * watchFolder (the folder to watch for incoming .zip files)
 * extractionFolder (the folder that the logs should be extracted into - this is the watch folder for the parser)
 * archiveFolder (the folder to move the archive to once it has been extracted)
 * PTGalera connection string (similar to the web project)
 
 The **archiveFolder** value should have a trailing backslash (\) but the **watchFolder** and **extractionFolder** values should not.

These folders must all exist before the application runs, or it will exit. Also, if any of the folders used by this application are in protected locations, you'll want to run the app as an administrator (UAC)

### AutoParser

Similar to AutoExtracter, this application has a config file that it reads from when it's initialized, **AutoParser.exe.config**. On top of the connection string, the only setting you need to configure is the **watchFolder**, which should be the same path as the **extractionFolder** for the AutoExtracter application.

**TO ADD: AutoParser needs the folder databaseimport**
