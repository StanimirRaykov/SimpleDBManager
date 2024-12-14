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
            // Parse command: INSERT INTO TableName(Column1, Column2, ...) VALUES (Value1, Value2, ...)
            var tokens = command.Split(new[] { "INSERT INTO", "VALUES" }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                Console.WriteLine("Error: Invalid syntax for INSERT INTO.");
                return;
            }

            string tableName = tokens[0].Split('(')[0].Trim();
            string columnPart = tokens[0].Split('(')[1].Trim(' ', ')');
            string valuePart = tokens[1].Trim(' ', '(', ')');

            var columnNames = columnPart.Split(',').Select(c => c.Trim()).ToList();
            var values = valuePart.Split(',').Select(v => v.Trim(' ', '\'')).Cast<object>().ToList();

            // Check if database is selected
            if (currentDatabase == null)
            {
                Console.WriteLine("Error: No database selected.");
                return;
            }

            // Get table from database
            Table table;
            try
            {
                table = currentDatabase.GetTable(tableName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                table.InsertRow(columnNames, values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
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
