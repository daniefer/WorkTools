What is this?:
This is a Console app that drops a log entry to the console every time the query 
(specified in the class) returns a different value from the last 
time it polled the database.

How to use:
To Use the tool, you will need to update every line in the Program.cs 
file with a comment of "// UPDATE THIS" on it. Once configured, just click 
start in Visual Studio and the tool will poll the database every 2 second 
for changes to the query that you specified.

Limitation:
This only works for select queries that return one table if more than one 
table is returned only the first table will be watched. This could be fixed 
easily enough, but I didn't need it so I didn't add it.
