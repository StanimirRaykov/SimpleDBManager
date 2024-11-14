using Database_Management_System.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System
{
    public class CommandParser
    {
        private Database.Database currentDatabase = null;
        public void ParseCommand(string command)
        {

            var tokens = Utilities.Functions.StringSplit(command, ' ');
            var operation = tokens[0].ToUpper();
            switch (operation)
            {
                case "CREATE":
                    if (tokens[1] == "TABLE")
                    {
                        CreateTable(command);
                        //TODO: Imlement logic -> CreateTable(command);
                    }
                    else if (tokens[1].ToUpper() == "DATABASE")
                    {
                        CreateDatabase(tokens[2]);
                    }
                    else if (tokens[1] == "INDEX")
                    {
                        //TODO: Imlement logic -> CreateIndex(command)
                    }
                    break;

                case "DROP":
                    if (tokens[1] == "TABLE")
                    {
                        //TODO: Imlement logic -> DropTable(tokens[2]);
                    }
                    else if (tokens[1] == "INDEX")
                    {
                        //TODO: Imlement logic -> DropIndex(tokens[2])
                    }
                    break;

                case "INSERT":
                    //TODO: Imlement logic -> InsertInto(command);
                    break;

                case "DELETE":
                    //TODO: Imlement logic -> DeleteFrom(command);
                    break;

                case "SELECT":
                    //TODO: Imlement logic -> Select(Command);
                    break;

                default:
                    Console.WriteLine("Invalid Command!");
                    break;
            }
        }
        private void CreateDatabase(string dbName)
        {
            currentDatabase = Database.Database.CreateDatabase(dbName);
        }
        private void CreateTable(string command)
        {
            if (currentDatabase == null)
            {
                Console.WriteLine("No database selected. Please create or switch to a database first.");
                return;
            }

            var openParenIndex = command.IndexOf('(');
            var closeParenIndex = command.LastIndexOf(')');

            if (openParenIndex == -1 || closeParenIndex == -1 || closeParenIndex <= openParenIndex)
            {
                Console.WriteLine("Invalid command syntax for CREATE TABLE.");
                return;
            }

            var tableName = command.Substring(13, openParenIndex - 13).Trim(); // Extract table name
            var columnDefinitions = command.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);

            // Parse column definitions
            List<Column> columns = ParseColumnDefinitions(columnDefinitions);

            // Create table within the current database
            var table = Table.CreateTable(tableName, columns, currentDatabase.Name);
            currentDatabase.AttachTable(table);
        }
        
        private List<Column> ParseColumnDefinitions(string columnDefinitions)
        {
            var columns = new List<Column>();
            var columnParts =  Utilities.Functions.StringSplit(columnDefinitions, ',');

            foreach (var columnDef in columnParts)
            {
                var parts =  Utilities.Functions.StringSplit(columnDef.Trim(), ' ');
                var nameType =  Utilities.Functions.StringSplit(parts[0], ':');
                var columnName = nameType[0];
                var columnType = nameType[1];

                object defaultValue = null;
                if (parts.Count > 2 && parts[2].ToLower() == "default")
                {
                    defaultValue = parts[3].Trim('\''); // Removing quotes from the default value
                }

                columns.Add(new Column(columnName, columnType, defaultValue));
            }
            return columns;
        }

    }
}
