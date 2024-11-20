using Database_Management_System.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Database_Management_System.Database
{
    public class Table
    {
        public string TableName { get; private set; }
        private string tableFilePath;
        private int rowCount;
        private HashTable<string, (string DataType, object DefaultValue)> schema;

        public Table(string name, List<Column> columns, string databasePath)
        {
            TableName = name;
            schema = new HashTable<string, (string DataType, object DefaultValue)>();

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
            if (!File.Exists(tableFilePath))
            {
                File.Create(tableFilePath).Dispose();
                rowCount = 0;
                Console.WriteLine($"Table '{TableName}' created successfully at '{tableFilePath}'.");
            }
            else
            {
                rowCount = File.ReadAllLines(tableFilePath).Length;
                Console.WriteLine($"Table '{TableName}' already exists at '{tableFilePath}'.");
            }
        }

        public static Table CreateTable(string name, List<Column> columns, string databasePath)
        {

            return new Table(name, columns, databasePath);
        }
        public static Table LoadTable(string name, string databasePath)
        {
            // Load metadata for the table
            var columns = Metadata.LoadTableMetadata(name, databasePath);

            // Create and return the table object
            return new Table(name, columns, databasePath);
        }
        public static void DropTable(string name)
        {
            //TODO: Implement logic for DropTable function!
        }
        //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! FIX INSERT ROW !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! (Columns list doesn't exist anymore.)
        public void InsertRow(List<object> values)
        {
            if (Columns == null || Columns.Count == 0)
            {
                Console.WriteLine($"Error: Table '{TableName}' schema is not loaded.");
                return;
            }

            if (values.Count != Columns.Count)
            {
                Console.WriteLine("Error: Mismatch between number of columns and provided values.");
                return;
            }

            // Validate data types
            for (int i = 0; i < values.Count; i++)
            {
                if (!IsValidType(values[i], Columns[i].DataType))
                {
                    Console.WriteLine($"Error: Value '{values[i]}' does not match the column type '{Columns[i].DataType}'.");
                    return;
                }
            }

            // Serialize the row as a comma-separated string
            string row = SerializeRow(values);

            // Append the row to the table file
            using (StreamWriter writer = new StreamWriter(tableFilePath, append: true))
            {
                writer.WriteLine(row);
            }

            rowCount++;
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
        public void VisualizeTable()
        {
            if (!File.Exists(tableFilePath))
            {
                Console.WriteLine($"Table file '{tableFilePath}' does not exist.");
                return;
            }

            // Print table headers
            Console.WriteLine($"\nTable: {TableName}");
            foreach (var column in Columns)
            {
                Console.Write($"{column.Name}\t");
            }
            Console.WriteLine("\n" + new string('-', 50));

            // Print table rows
            string[] rows = File.ReadAllLines(tableFilePath);
            foreach (string row in rows)
            {
                Console.WriteLine(row.Replace(",", "\t")); // Display rows in tabular format
            }

            Console.WriteLine(new string('-', 50));
        }

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
