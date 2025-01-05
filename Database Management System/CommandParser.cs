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
                        HandleCreateTable(command);
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
                        HandleDropTable(command);
                    }
                    else if (tokens[1] == "INDEX")
                    {
                        //TODO: Imlement logic -> DropIndex(tokens[2])
                    }
                    break;

                case "INSERT":
                    InsertInto(command);
                    break;

                case "GET":
                    if (tokens[1] == "ROW")
                        HandleGetRow(command);
                    break;

                case "SELECT":
                    //TODO: Imlement logic -> Select(Command);
                    break;

                case "TABLEINFO":
                    HandleTableInfo(command);
                    break;

                case "USE":
                    UseDatabase(tokens[1]);
                    break;

                case "DELETE":
                    if (tokens[1] == "FROM")
                        HandleDeleteFrom(command);
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
        private void HandleCreateTable(string command)
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
            var tokens = command.Split(new[] { "INSERT INTO", "VALUES" }, StringSplitOptions.RemoveEmptyEntries); //TODO: Make this work with Functions.StringSplit

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

        private List<Column> ParseColumnDefinitions(string columnDefinitions)
        {
            var columns = new List<Column>();
            var columnParts = Utilities.Functions.StringSplit(columnDefinitions, ',');

            // Example: CREATE TABLE Sample(Name:string, BirthDate:date default '01.01.2022')
            foreach (var columnDef in columnParts)
            {
                var parts = Utilities.Functions.StringSplit(columnDef.Trim(), ' ');

                if (parts.Count < 1)
                {
                    throw new Exception($"Invalid column definition: '{columnDef}'.");
                }

                // Extract name and type (e.g., "Name:string")
                var nameType = Utilities.Functions.StringSplit(parts[0], ':');
                if (nameType.Count != 2)
                {
                    throw new Exception($"Invalid column definition: '{columnDef}'. Expected format: 'Name:Type'.");
                }

                var columnName = nameType[0].Trim();
                var columnType = nameType[1].Trim();

                // Extract default value if specified
                object defaultValue = null;
                if (parts.Count > 2)
                {
                    for (int i = 1; i < parts.Count - 1; i++)
                    {
                        if (parts[i].ToLower() == "default")
                        {
                            defaultValue = parts[i + 1].Trim('\''); // Remove surrounding quotes
                            break;
                        }
                    }
                }

                columns.Add(new Column(columnName, columnType, defaultValue));
            }

            return columns;
        }
        private void UseDatabase(string command)
        {
            // Extract the database name
            var tokens = command.Split(' ');
            if (tokens.Length < 1)
            {
                Console.WriteLine("Error: Missing database name for USE command.");
                return;
            }

            string databaseName = tokens[0];

            // Path to the database directory
            string databasePath = Path.Combine(Settings.BaseDirectory, databaseName);

            // Check if the database directory exists
            if (!Directory.Exists(databasePath))
            {
                Console.WriteLine($"Error: Database '{databaseName}' does not exist.");
                return;
            }

            // Set the current database
            currentDatabase = new Database.Database(databaseName); // Load the database
            Console.WriteLine($"Using database '{databaseName}'.");
        }

        private void HandleDropTable(string command)
        {
            // Extract the table name from the command
            var tokens = Utilities.Functions.StringSplit(command, ' ');
            if (tokens.Count < 3)
            {
                Console.WriteLine("Error: Missing table name for DROP TABLE.");
                return;
            }

            string tableName = tokens[2];

            if (currentDatabase == null)
            {
                Console.WriteLine("Error: No database selected.");
                return;
            }

            // Call DropTable on the current database
            currentDatabase.DropTable(tableName);
        }

        private void HandleTableInfo(string command)
        {
            // Extract the table name from the command
            var tokens = command.Split(' ');
            if (tokens.Length < 2)
            {
                Console.WriteLine("Error: Missing table name for TABLEINFO.");
                return;
            }

            string tableName = tokens[1];

            if (currentDatabase == null)
            {
                Console.WriteLine("Error: No database selected.");
                return;
            }

            //TODO: FIX this mess

            //if (!currem)
            //{
            //    Console.WriteLine($"Error: Table '{tableName}' does not exist in the current database.");
            //    return;
            //}

            var table = currentDatabase.GetTable(tableName);
            Console.WriteLine(table.GetTableInfo());
        }
        private void HandleDeleteFrom(string command)  //Delete from {TableName} Where {Condition}
                                                       //  0      1       2        3        4 
        {
            var tokens = Utilities.Functions.StringSplit(command, ' '); 

            if (tokens.Count < 3)
            {
                Console.WriteLine("Error: Missing table name in DELETE FROM command.");
                return;
            }

            string tableName = tokens[2];

            if (currentDatabase == null)
            {
                Console.WriteLine("Error: No database selected.");
                return;
            }

            //if (!currentDatabase.GetTable.ContainsKey(tableName))
            //{
            //    Console.WriteLine($"Error: Table '{tableName}' does not exist in the current database.");
            //    return;
            //}

            var table = currentDatabase.GetTable(tableName);

            // Check for optional WHERE clause
            if (tokens.Count > 3 && tokens[3].Equals("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                // Example: DELETE FROM Sample WHERE Name='John'
                var condition = command.Substring(command.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) + 6);
                table.DeleteRows(whereClause: row =>
                {
                    // Simple condition parser (e.g., Name='John')
                    var parts = Utilities.Functions.StringSplit(condition, '=');
                    var columnName = parts[0].Trim();
                    var value = parts[1].Trim('\'');

                    var columnIndex = table.Columns.FindIndex(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    if (columnIndex == -1) return false;

                    return row[columnIndex]?.ToString().Equals(value, StringComparison.OrdinalIgnoreCase) ?? false;

                });
            }
            else
            {
                // Example: DELETE FROM Sample 2
                if (tokens.Count > 3 && int.TryParse(tokens[3], out int rowNumber))
                {
                    table.DeleteRows(rowNumber);
                }
                else
                {
                    Console.WriteLine("Error: Invalid DELETE FROM syntax.");
                }
            }
        }

        private void HandleGetRow(string command)
        {
            // Split the command using the custom StringSplit function
            var tokens = Utilities.Functions.StringSplit(command, ' ');

            // Ensure the command has at least 4 tokens
            if (tokens.Count < 4)
            {
                Console.WriteLine("Error: Invalid syntax for GET ROW. Use: GET ROW <row_numbers> FROM <table_name>");
                return;
            }

            // Validate "FROM" keyword
            if (!tokens[3].Equals("FROM", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Error: Invalid syntax for GET ROW. Use: GET ROW <row_numbers> FROM <table_name>");
                return;
            }

            // Extract row numbers and table name
            string rowNumbers = tokens[2]; // e.g., "1,3"
            string tableName = tokens[4];  // e.g., "Sample"

            // Ensure row numbers are provided
            if (string.IsNullOrWhiteSpace(rowNumbers))
            {
                Console.WriteLine("Error: No valid row numbers provided.");
                return;
            }

            // Ensure the table name is not empty
            if (string.IsNullOrWhiteSpace(tableName))
            {
                Console.WriteLine("Error: Invalid syntax for GET ROW. Table name cannot be empty.");
                return;
            }

            // Ensure a database is selected
            if (currentDatabase == null)
            {
                Console.WriteLine("Error: No database selected.");
                return;
            }

            // Ensure the table exists in the current database
            if (!currentDatabase.ContainsTable(tableName))
            {
                Console.WriteLine($"Error: Table '{tableName}' does not exist in the current database.");
                return;
            }

            // Retrieve and display the rows
            var table = currentDatabase.GetTable(tableName);
            table.GetRows(rowNumbers); // Call the GetRows method
        }



    }
}
