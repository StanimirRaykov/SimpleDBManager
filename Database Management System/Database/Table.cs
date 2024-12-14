using Database_Management_System.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Database_Management_System.Database
{
    public class Table
    {
        public string TableName { get; private set; }
        public List<Column> Columns { get; private set; }
        private string tableFilePath;
        private HashTable<string, (string DataType, object DefaultValue)> schema;
        private int nextId;


        public Table(string name, List<Column> columns, string databasePath)
        {
            TableName = name;
            schema = new HashTable<string, (string DataType, object DefaultValue)>();
            Columns = columns ?? new List<Column>();

            if (!Columns.Any(c => c.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                Columns.Insert(0, new Column("Id", "int", null)); // Add Id column as the first column
            }

            foreach (var column in columns)
            {
                schema.Add(column.Name, (column.DataType, column.DefaultValue)); 
            }

            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
                Console.WriteLine($"Directory '{databasePath}' created for database.");
            }

            // Set the table file path
            tableFilePath = Path.Combine(databasePath, $"{TableName}.txt");

            // Create table file within the database directory
            if (File.Exists(tableFilePath))
            {
                // Validate existing file schema and initialize nextId
                using (var reader = new StreamReader(tableFilePath))
                {
                    string fileHeader = reader.ReadLine();
                    var expectedHeader = string.Join(",", Columns.Select(column =>
                        $"{column.Name}:{column.DataType}"));

                    if (!fileHeader.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception($"Schema mismatch in table file '{tableFilePath}'. Expected: {expectedHeader}, Found: {fileHeader}");
                    }

                    // Read rows to determine the next ID
                    nextId = 1; // Default to 1 if no rows exist
                    while (!reader.EndOfStream)
                    {
                        string row = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(row))
                        {
                            var rowParts = Functions.StringSplit(row, ',');
                            if (int.TryParse(rowParts[0], out int lastId))
                            {
                                nextId = Math.Max(nextId, lastId + 1);
                            }
                        }
                    }
                }

                Console.WriteLine($"Table '{TableName}' already exists at '{tableFilePath}'.");
            }
            else
            {
                // Create new table file
                using (var writer = File.CreateText(tableFilePath))
                {
                    var headerRow = string.Join(",", Columns.Select(column =>
                        $"{column.Name}:{column.DataType}"));
                    writer.WriteLine(headerRow);
                }

                nextId = 1; // Initialize nextId
                Console.WriteLine($"Table '{TableName}' created successfully at '{tableFilePath}'.");
            }
        }

        public static Table CreateTable(string name, List<Column> columns, string databasePath)
        {

            return new Table(name, columns, databasePath);
        }
        public static Table LoadTable(string name, string databasePath)
        {
            // Build the path to the table file
            string tableFilePath = Path.Combine(databasePath, $"{name}.txt");

            if (!File.Exists(tableFilePath))
            {
                throw new FileNotFoundException($"Table file '{name}.txt' not found in database path '{databasePath}'.");
            }

            // Load schema from the table file's header row (first line)
            List<Column> columns = LoadSchemaFromHeader(tableFilePath);

            // Create and return the Table object
            return new Table(name, columns, databasePath);
        }

        private static List<Column> LoadSchemaFromHeader(string tableFilePath)
        {
            using (StreamReader reader = new StreamReader(tableFilePath))
            {
                string headerLine = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    throw new Exception($"Table file '{tableFilePath}' is missing a valid header row.");
                }

                var columns = new List<Column>();
                var columnDefinitions = headerLine.Split(',');

                foreach (var columnDef in columnDefinitions)
                {
                    // Split each column definition into parts
                    var parts = columnDef.Split(':');

                    if (parts.Length < 2) // Ensure we have at least "Name:Type"
                    {
                        throw new Exception($"Malformed column definition: '{columnDef}'. Expected format: 'Name:Type:DefaultValue'.");
                    }

                    // Extract column details safely
                    string columnName = parts[0];
                    string dataType = parts[1];
                    string defaultValue = parts.Length > 2 ? parts[2] : null;

                    columns.Add(new Column(columnName, dataType, defaultValue));
                }

                return columns;
            }
        }
        public static void DropTable(string name)
        {
            //TODO: Implement logic for DropTable function!
        }

        //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! FIX INSERT ROW !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! (Columns list doesn't exist anymore.)
        public void InsertRow(List<string> columnNames, List<object> values)
        {
            // Add automatic ID to the row
            var row = new List<object> { nextId }; // Start with the ID
            nextId++; // Increment for the next row

            // Populate remaining columns in schema order
            foreach (var key in schema.GetKeys())
            {
                if (key.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip ID column (already added)
                }

                int index = columnNames.FindIndex(name => name.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    // Use the value provided for this column
                    row.Add(values[index]);
                }
                else
                {
                    // Use the default value for this column
                    var columnDetails = schema.Get(key);
                    row.Add(columnDetails.DefaultValue ?? "null");
                }
            }

            // Serialize the row and append it to the table file
            using (StreamWriter writer = new StreamWriter(tableFilePath, append: true))
            {
                writer.WriteLine(string.Join(",", row));
            }

            Console.WriteLine($"Row inserted successfully into table '{TableName}'.");
        }

        private bool IsValidType(object value, string dataType)
        {
            switch (dataType.ToLower())
            {
                case "int":
                    return int.TryParse(value.ToString(), out _);
                case "string":
                    return value is string;
                case "date":
                    return DateTime.TryParse(value.ToString(), out _);
                default:
                    return false;
            }
        }
        private string SerializeRow(List<object> values)
        {
            return string.Join(",", values);
        }

        //TODO: FIX VISUALIZE TABLE METHOD BECAUSE COLUMNS LIST DOESN'T EXIST ANYMORE
        //public void VisualizeTable()
        //{
        //    if (!File.Exists(tableFilePath))
        //    {
        //        Console.WriteLine($"Table file '{tableFilePath}' does not exist.");
        //        return;
        //    }

        //    // Print table headers
        //    Console.WriteLine($"\nTable: {TableName}");
        //    foreach (var column in Columns)
        //    {
        //        Console.Write($"{column.Name}\t");
        //    }
        //    Console.WriteLine("\n" + new string('-', 50));

        //    // Print table rows
        //    string[] rows = File.ReadAllLines(tableFilePath);
        //    foreach (string row in rows)
        //    {
        //        Console.WriteLine(row.Replace(",", "\t")); // Display rows in tabular format
        //    }

        //    Console.WriteLine(new string('-', 50));
        //}

        public void DeleteRow(int rowIndex)
        {
            //TODO: Implement logic for DeleteRow function!
        }

        public void SelectRows(string condition)
        {
            //TODO: Implement logic for SelectRows function!
        }
    }
}
