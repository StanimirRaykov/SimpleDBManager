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
            int intoIndex = command.IndexOf("INTO", StringComparison.OrdinalIgnoreCase);
            int valuesIndex = command.IndexOf("VALUES", StringComparison.OrdinalIgnoreCase);

            if (intoIndex == -1 || valuesIndex == -1 || valuesIndex <= intoIndex)
            {
                Console.WriteLine("Invalid command syntax for INSERT INTO.");
                return;
            }

            // Extract the part between "INTO" and "VALUES"
            string intoPart = command.Substring(intoIndex + 4, valuesIndex - (intoIndex + 4)).Trim(); // Remove "INTO" and trim

            // Extract table name and columns
            int openParenIndex = intoPart.IndexOf('(');
            int closeParenIndex = intoPart.IndexOf(')');

            if (openParenIndex == -1 || closeParenIndex == -1 || closeParenIndex <= openParenIndex)
            {
                Console.WriteLine("Invalid command syntax for INSERT INTO. Missing column list.");
                return;
            }

            string tableName = intoPart.Substring(0, openParenIndex).Trim(); // Extract table name
            string columnPart = intoPart.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim(); // Extract columns

            // Extract values
            string valuePart = command.Substring(valuesIndex + "VALUES".Length).Trim();
            if (!valuePart.StartsWith("(") || !valuePart.EndsWith(")"))
            {
                Console.WriteLine("Invalid command syntax for INSERT INTO. Missing or invalid values.");
                return;
            }

            valuePart = valuePart.Substring(1, valuePart.Length - 2).Trim(); // Remove parentheses around values

            // Split columns and values into lists
            var columnNames = Functions.StringSplit(columnPart, ',');
            var values = ParseValues(valuePart);

            // Check if current database is set
            if (currentDatabase == null)
            {
                Console.WriteLine("No database selected. Please create or switch to a database first.");
                return;
            }

            // Locate the table
            string databasePath = Path.Combine(Settings.BaseDirectory, currentDatabase.Name);
            string tableFilePath = Path.Combine(databasePath, $"{tableName}.txt");

            if (!File.Exists(tableFilePath))
            {
                Console.WriteLine($"Table '{tableName}' does not exist in the current database.");
                return;
            }

            // Load the table and validate column mappings
            var table = Table.LoadTable(tableName, databasePath);
            if (columnNames.Count != values.Count)
            {
                Console.WriteLine("Error: Mismatch between column count and value count.");
                return;
            }

            // Map columns to values
            var row = new List<object>();
            for (int i = 0; i < columnNames.Count; i++)
            {
                string columnName = columnNames[i].Trim();
                string value = values[i].Trim();

                // Find the column in the table schema
                var column = table.Columns.Find(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (column == null)
                {
                    Console.WriteLine($"Error: Column '{columnName}' does not exist in table '{tableName}'.");
                    return;
                }

                // Validate the value type and parse it
                object parsedValue = ParseValue(value, column.DataType);
                if (parsedValue == null)
                {
                    Console.WriteLine($"Error: Invalid value '{value}' for column '{columnName}' with type '{column.DataType}'.");
                    return;
                }

                row.Add(parsedValue);
            }

            // Insert the row
            table.InsertRow(row);
        }
        private object ParseValue(string value, string dataType)
        {
            // Trim the value to remove surrounding whitespace and single quotes
            value = value.Trim().Trim('\'');

            switch (dataType.ToLower())
            {
                case "int":
                    if (int.TryParse(value, out int intValue)) return intValue;
                    break;
                case "string":
                    return value; // Strings don't need further parsing
                case "date":
                    if (DateTime.TryParse(value, out DateTime dateValue)) return dateValue;
                    break;
            }
            return null; // Invalid value
        }


        private List<string> ParseValues(string valuePart)
        {
            if (valuePart.StartsWith("(") && valuePart.EndsWith(")"))
            {
                valuePart = valuePart.Substring(1, valuePart.Length - 2);
            }

            // Split values by commas and trim each value
            var values = Functions.StringSplit(valuePart, ',');
            var parsedValues = new List<string>();

            foreach (var value in values)
            {
                parsedValues.Add(value.Trim());
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
