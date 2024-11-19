using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Database
{
    public static class Metadata
    {
        public static void SaveTableMetadata(string tableName, List<Column> columns, string databasePath)
        {
            string metadataPath = Path.Combine(databasePath, $"{tableName}_metadata.txt");

            using (StreamWriter writer = new StreamWriter(metadataPath))
            {
                foreach (var column in columns)
                {
                    writer.WriteLine($"{column.Name}:{column.DataType}:{column.DefaultValue}");
                }
            }
            Console.WriteLine($"Metadata for table '{tableName}' saved successfully in database '{Path.GetFileName(databasePath)}'.");
        }

        public static List<Column> LoadTableMetadata(string tableName, string databasePath)
        {
            string metadataPath = Path.Combine(databasePath, $"{tableName}.meta");

            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException($"Metadata file for table '{tableName}' not found in database path '{databasePath}'.");
            }

            var columns = new List<Column>();

            using (StreamReader reader = new StreamReader(metadataPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(':');
                    string name = parts[0];
                    string type = parts[1];
                    string defaultValue = parts.Length > 2 ? parts[2] : null;

                    columns.Add(new Column(name, type, defaultValue));
                }
            }

            return columns;
        }
    }
}
