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
            string metadataPath = Path.Combine(databasePath, $"{tableName}.meta");

            using (StreamWriter writer = new StreamWriter(metadataPath))
            {
                foreach (var column in columns)
                {
                    writer.WriteLine($"{column.Name}:{column.DataType}:{column.DefaultValue}");
                }
            }
            Console.WriteLine($"Metadata for table '{tableName}' saved successfully in database '{Path.GetFileName(databasePath)}'.");
        }
    }
}
