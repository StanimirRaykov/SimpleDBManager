using Database_Management_System.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Database
{
    public class Database
    {
        public string Name { get; private set; }
        public string dbPath;
        private HashTable<string, Table> tables;

        public Database(string name)
        {
            Name = name;
            dbPath = Path.Combine(Settings.BaseDirectory, Name);
            tables = new HashTable<string, Table>();

            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
                Console.WriteLine($"Database '{Name}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Database '{Name}' already exists.");
            }
        }

        public static Database CreateDatabase(string name)
        {
            return new Database(name);
        }

        public void AttachTable(Table table)
        {
            if (tables.ContainsKey(table.TableName))
            {
                Console.WriteLine($"Table '{table.TableName}' already exists in database '{Name}'.");
                return;
            }

            tables.Add(table.TableName, table); // Add the table using HashTable
            Console.WriteLine($"Table '{table.TableName}' attached to database '{Name}'.");
        }
        public bool ContainsTable(string tableName)
        {
            return tables.ContainsKey(tableName);
        }
        public Table GetTable(string tableName)
        {
            if (!tables.ContainsKey(tableName))
            {
                throw new Exception($"Table '{tableName}' does not exist in database '{Name}'.");
            }

            return tables.Get(tableName); // Retrieve the table from HashTable
        }
        public void DeleteDatabase()
        {
            if (Directory.Exists(dbPath))
            {
                Directory.Delete(dbPath, true);
                Console.WriteLine($"Database '{Name}' deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Database '{Name}' does not exist.");
            }
        }
        public void DropTable(string tableName)
        {
            if (!tables.ContainsKey(tableName))
            {
                Console.WriteLine($"Error: Table '{tableName}' does not exist in database '{Name}'.");
                return;
            }

            // Get the table's file path
            string tableFilePath = Path.Combine(dbPath, $"{tableName}.txt");

            // Delete the table file if it exists
            if (File.Exists(tableFilePath))
            {
                File.Delete(tableFilePath);
                Console.WriteLine($"Table file '{tableFilePath}' deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Error: Table file '{tableFilePath}' does not exist.");
            }

            // Remove the table from the in-memory schema
            tables.Remove(tableName);
            Console.WriteLine($"Table '{tableName}' dropped successfully from database '{Name}'.");
        }
        public string GetPath()
        {
            return dbPath;
        }
    }
}
