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

Once you follow the next few steps to create necessary configuration files, make sure that Git ignores them so they won't be checked in. This is to ensure that your sensitive info (usernames / passwords) isn't shared with everyone.

### Copy example files (mail account info, DB connection strings)

#### Common project

Either rename **UserGroups.example.cs** to **UserGroups.cs** or create a new UserGroups.cs file and copy the contents from the example into it. The string for Admin can be set to anything, the only requirement is that a group with a matching name exists in the **UserGroup** table once you've imported the schema. For example, if you change the line to read

```C#
public const string Admin = "PTAdminUsers";
```

then you'd need to add a record to the **UserGroup** table, with the name **PTAdminUsers** and then give your test account access by adding a record into **UserGroupMembership** that links the two together. By default it's set to **AdminUsers** and there is a UserGroup in the database that matches this - so you don't have to change it if you don't want to.

#### Web project

In the PrancingTurtle (web) project, the two files you'll need to create are **PrancingTurtle/ConnectionStrings.config** and **PrancingTurthe/Helpers/Mail/AccountInfo.cs**. Example files are provided next to where the live files should live, so you can copy/paste their contents and create the new files. This is in place so that you're able to point your debug copy of PT to a local MySQL server for ease of use, as well as the mail engine if you'd like to debug that too.

You can change the name of the connection strings away from **PTGalera** and **PTHangfire** if you wish, but be sure to change them in the dependency injection registry as well (**PrancingTurtle/DependencyResolution/Registries/RepositoryRegistry.cs**)

The last file to copy/paste / rename is **PrancingTurtle/Helpers/Authorization/ApplicationSid.cs**. This is really just a unique string for this application and can be anything you like.

#### AutoExtracter

Rename **App.example.config** to **App.config** or create a new config file with the same contents. Make sure to change the connection string to match your local database connection.

#### AutoParser

Rename **App.example.config** to **App.config** or create a new config file with the same contents. Make sure to change the connection string to match your local database connection.

### Set the web project as the startup project
Right-click the PrancingTurtle web project and click "Set as Startup Project" if it isn't set already... unless, of course, you're debugging the AutoExtracter or AutoParser projects.

### Import the DB schema to a local MySQL server
You can find the default schema [here](https://github.com/hewisaurus/PrancingTurtle/blob/master/PTSchema-20171016.sql). It includes create statements for the tables, so you can copy/paste it directly into something like MySQL Workbench and execute it to get a working (but empty) database.

If you'd also like some seed data for the database, choose from the following three files. There is one user, **testuser@domain.com** and its password is **qwerty12345**
 * Minimum data with no sessions [here](https://github.com/hewisaurus/PrancingTurtle/blob/master/PTBasicData-IncGuild-NoSessions.sql) - Instances, boss fights only
 * Minimum data with 2 sessions [here](https://github.com/hewisaurus/PrancingTurtle/blob/master/PTBasicData-IncGuild-2Sessions.zip) - 7.6mb zipped, 74mb uncompressed
 * Minimum data with 5 sessions [here](https://github.com/hewisaurus/PrancingTurtle/blob/master/PTBasicData-IncGuild-5Sessions.zip) - 16.9mb zipped, 162mb uncompressed

### Hangfire - a background job execution framework
PT uses Hangfire to coordinate a series of background tasks and the existing Quartz.NET tasks will be moved to Hangfire in the future. As long as you configure a working MySQL/MariaDB server within the web project's **ConnectionStrings.config** file, the Hangfire database will be created automatically.

## Information specific to individual projects

### AutoExtracter

To run this project (rather than debug it), first you need to configure the settings for the application itself. Before the project is built, you'll find these settings in **App.config**, and when VS builds the project this config file is renamed to **AutoExtracterAsync.exe.config**. The key values to modify are:
 * watchFolder (the folder to watch for incoming .zip files)
 * extractionFolder (the folder that the logs should be extracted into - this is the watch folder for the parser)
 * archiveFolder (the folder to move the archive to once it has been extracted)
 * PTGalera connection string (similar to the web project)
 
 The **archiveFolder** value should have a trailing backslash (\) but the **watchFolder** and **extractionFolder** values should not.

These folders must all exist before the application runs, or it will exit. Also, if any of the folders used by this application are in protected locations, you'll want to run the app as an administrator (UAC).

If you run the application after it has been built in **debug** mode in VS, the child windows that spawn won't close on their own (this is deliberate). If you don't need to watch the console as it works, run the app in **Release** and it'll close automatically.

### AutoParser

Similar to AutoExtracter, this application has a config file that it reads from when it's initialized, **AutoParser.exe.config**. On top of the connection string, the only setting you need to configure is the **watchFolder**, which should be the same path as the **extractionFolder** for the AutoExtracter application. Again, consider running it "as Administrator" if you run into permissions issues.

This project uses MySQL's Bulk Loader in order to insert damage, healing and shielding records - particularly when inserting upwards of 30000 records at a time, this is an efficient method. Smaller tables don't require this and have a much simpler method of inserting data.

If you run the application after it has been built in **debug** mode in VS, the child windows that spawn won't close on their own (this is deliberate). If you don't need to watch the console as it works, run the app in **Release** and it'll close automatically.

## Last, but not least...

Make sure that all of the configuration files that you created or renamed are included in Git's ignore list, so they're not checked in. There should be 6:
* /PrancingTurtle/PrancingTurtle/ConnectionStrings.config
* /PrancingTurtle/PrancingTurtle/Helpers/Mail/AccountInfo.cs
* /PrancingTurtle/PrancingTurtle/Helpers/Authorization/ApplicationSid.cs
* /PrancingTurtle/AutoExtracterAsync/App.config
* /PrancingTurtle/AutoParser/App.config
* /PrancingTurtle/Common/UserGroups.cs
