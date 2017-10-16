# Getting started

This is a quick guide to getting the project to an operational state.. beyond that it's up to you! 

PT is a Visual Studio 2017 solution & targets .NET 4.6.2 with each project. For some reason, the VS2017 installer doesn't have the option for this, so you'll have to install it manually.

## Get the .NET 4.6.2 SDK / targeting pack
Download and install the developer pack from [here](https://www.microsoft.com/en-us/download/details.aspx?id=53321).



## Copy example files (mail account info and DB connection strings)


#### AutoExtracter (Console Application)
The purpose of the AutoExtracter is to watch a specific folder for incoming files, and when new files arrive, the logs within them are extracted to another folder, and the file is archived in case it needs to be used again later. Files that don't match a specific filter (which currently is simply .zip) are deleted.
