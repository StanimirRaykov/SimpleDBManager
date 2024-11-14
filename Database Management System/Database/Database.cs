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

        private string dbPath;
        private List<Table> tables;

        public Database(string name)
        {
            Name = name;
            dbPath = Path.Combine(Settings.BaseDirectory, name);
            tables = new List<Table>();

            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(Settings.BaseDirectory);
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
            tables.Add(table);
            Console.WriteLine($"Table '{table}' attached to database '{Name}'.");
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
    }
}
