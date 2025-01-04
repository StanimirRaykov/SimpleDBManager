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
        public void DeleteRows(int? rowNumber = null, Func<List<string>, bool> whereClause = null)
        {
            if (!File.Exists(tableFilePath))
            {
                Console.WriteLine($"Error: Table file '{tableFilePath}' does not exist.");
                return;
            }

            // Read all rows from the file
            var rows = File.ReadAllLines(tableFilePath).ToList();
            if (rows.Count <= 1)
            {
                Console.WriteLine($"Error: No rows to delete in table '{TableName}'.");
                return;
            }

            var schema = rows[0]; // Header row
            var dataRows = rows.Skip(1).ToList();

            // Determine rows to keep
            var updatedRows = new List<string> { schema }; // Keep the header row

            for (int i = 0; i < dataRows.Count; i++)
            {
                var row = dataRows[i];
                var columns = Utilities.Functions.StringSplit(row, ',').ToList();

                // Determine if the row should be deleted
                bool deleteRow = false;

                if (rowNumber.HasValue && i == rowNumber.Value - 1) // Row number is 1-based
                {
                    deleteRow = true;
                }
                else if (whereClause != null && whereClause(columns))
                {
                    deleteRow = true;
                }

                if (!deleteRow)
                {
                    updatedRows.Add(row); // Keep the row
                }
            }

            // Rewrite the file with updated rows
            File.WriteAllLines(tableFilePath, updatedRows);

            Console.WriteLine($"Row(s) deleted successfully from table '{TableName}'.");
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

        public string GetTableInfo()
        {
            // Get the schema
            string schemaInfo = string.Join(", ", Columns.Select(c => $"{c.Name}:{c.DataType}"));

            // Get the number of records
            int recordCount = 0;
            if (File.Exists(tableFilePath))
            {
                // Count lines excluding the header
                recordCount = File.ReadAllLines(tableFilePath).Skip(1).Count();
            }

            // Get the file size
            long fileSize = new FileInfo(tableFilePath).Length;

            // Build the table info string
            var tableInfo = new StringBuilder();
            tableInfo.AppendLine($"Table Name: {TableName}");
            tableInfo.AppendLine($"Schema: {schemaInfo}");
            tableInfo.AppendLine($"Number of Records: {recordCount}");
            tableInfo.AppendLine($"File Size: {fileSize} bytes");

            return tableInfo.ToString();
        }

    }
}
