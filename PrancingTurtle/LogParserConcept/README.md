# Log parser concept

The idea behind this project is to rewrite the parsing of log files, right from upload to recording into the database, into a series of microservices that can be hosted in Azure.

While this may not actually be completed, it's an interesting exercise into making the parsing itself serverless and not require the resources that it currently does (i.e a Virtual Machine with two applications installed)

This concept will specifically be built for running on a machine rather than serverless. If the concept proceeds, a serverless version will be written.
