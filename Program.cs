using Database_Management_System;

CommandParser commandParser = new CommandParser();
Console.WriteLine("Welcome!");

while(true)
{
    Console.Write("DBMS> ");
    string command = Console.ReadLine();
    commandParser.ParseCommand(command);
}