using Database_Management_System.Database;
using Database_Management_System.Utilities;
using Microsoft.VisualBasic;
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
                    InsertInto(command);
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
            var table = Table.CreateTable(tableName, columns, currentDatabase.GetPath());
            currentDatabase.AttachTable(table);
        }
        private void InsertInto(string command)
        {
            // Split the command into major parts
            var tokens = Functions.StringSplit(command, ' ');

            if (tokens.Count < 4 || tokens[1].ToUpper() != "INTO")
            {
                Console.WriteLine("Invalid command syntax for INSERT INTO.");
                return;
            }

            // Extract table name
            string tableName = tokens[2].Trim(); //11 chars to '('

            // Check for VALUES keyword
            int valuesIndex = command.IndexOf("VALUES", StringComparison.OrdinalIgnoreCase);

            if (valuesIndex == -1)
            {
                Console.WriteLine("Invalid command syntax for INSERT INTO. Missing 'VALUES' keyword.");
                return;
            }

            // Extract column names and values
            string columnPart = command.Substring(command.IndexOf('(') + 1, command.IndexOf(')') - command.IndexOf('(') - 1);
            string valuePart = command.Substring(valuesIndex + "VALUES".Length).Trim();

            var columnNames = Functions.StringSplit(columnPart, ',');
            var values = ParseValues(valuePart);

            // Locate the table and insert the row
            if (currentDatabase == null)
            {
                Console.WriteLine("No database selected. Please create or switch to a database first.");
                return;
            }

            string databasePath = Path.Combine(Settings.BaseDirectory, currentDatabase.Name);
            string tableFilePath = Path.Combine(databasePath, $"{tableName}.db");

            if (!File.Exists(tableFilePath))
            {
                Console.WriteLine($"Table '{tableName}' does not exist in the current database.");
                return;
            }

            // Create a temporary table object to access methods
            var table = Table.LoadTable(tableName, databasePath);
            table.InsertRow(values);
        }


        private List<object> ParseValues(string valuePart)
        {
            valuePart = valuePart.Trim('(', ')');
            var values = Functions.StringSplit(valuePart, ',');
            var parsedValues = new List<object>();

            foreach (var value in values)
            {
                if (int.TryParse(value.Trim(), out int intValue))
                {
                    parsedValues.Add(intValue);
                }
                else if (DateTime.TryParse(value.Trim('\''), out DateTime dateValue))
                {
                    parsedValues.Add(dateValue);
                }
                else
                {
                    parsedValues.Add(value.Trim('\'').Trim());
                }
            }

            return parsedValues;
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
