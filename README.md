# PrancingTurtle
A web-based CombatLog parser for [RIFT](http://www.riftgame.com). Link to the live PT site is [here](https://prancingturtle.com/).

The whole project is written in C#, and consists of two console applications & one ASP.NET MVC project.

## Components

#### AutoExtracter (Console Application)
The purpose of the AutoExtracter is to watch a specific folder for incoming files, and when new files arrive, the logs within them are extracted to another folder, and the file is archived in case it needs to be used again later. Files that don't match a specific filter (which currently is simply .zip) are deleted.

#### AutoParser (Console Application)
This is the application responsible for parsing the uncompressed logs & updating the database. Similar to the AutoExtracter, it watches a specific folder for incoming logs and parses them when they do. The only requirement here is that the incoming log is attributable to a previously created session from the web interface.

#### Common (Class Library)
This project is pretty small, and contains objects that are shared between the parser and the web project.

#### Database (MySQL/MariaDB)
This project handles all of the database interaction and is used by both the parser and the web project.

#### Logging (NLog)
A simple implementation of NLog

#### Web (ASP.NET 5 MVC)
This project is responsible for everything other than the database interaction and the log parsing.

Notable packages used by this project:
 * [Glimpse](http://getglimpse.com/) for debugging & insight
 * [Quartz.NET](https://www.quartz-scheduler.net/) for the early scheduling implementation
 * [Hangfire](https://www.hangfire.io/) - a better way of executing background tasks
 * [StructureMap](http://structuremap.github.io/) for Dependency Injection / IoC

## History
PrancingTurtle originally existed as a very simple web project for internal use within a RIFT guild. The idea came from a site that I had used before but had been decommissioned. I managed to get in contact with the author and was kindly given a copy of the source that was used there. The structure of the logs themselves had changed significantly but looking through the existing code gave me a few ideas, and a very early iteration of PrancingTurtle was born. Looking back at my first series of commits, it turns out that development started around August 2014.

Being a side project, it didn't get worked on full-time, but as there were plenty of feature requests from guildies, it continued to evolve until it was (amusingly) used to point out how much better some players were than others. This spawned a series of conversations which ultimately led to PrancingTurtle turning into a site for anyone and everyone within the RIFT community to use. PrancingTurtle was published publicly in April 2015.
