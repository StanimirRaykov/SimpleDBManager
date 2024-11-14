using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Database_Management_System.Database
{
    public class Table
    {
        private string TableName;
        private List<Column> Columns;
        private string tableFilePath;

        public Table(string name, List<Column> columns, string databasePath)
        {
            TableName = name;
            Columns = columns;
            tableFilePath = Path.Combine(databasePath, $"{TableName}.db");
            if (!File.Exists(tableFilePath))
            {
                File.Create(tableFilePath).Dispose();
                Console.WriteLine($"Table '' created successfully within database.");
            }
        }

        public static Table CreateTable(string name, List<Column> columns, string databasePath)
        {
            //TODO: Implement logic for CreateTable function!
            Table newTable = new Table(name, columns, databasePath);
            Metadata.SaveTableMetadata(name, columns, databasePath);
            return newTable;
        }

        public static void DropTable(string name)
        {
            //TODO: Implement logic for DropTable function!
        }

        public void InsertRow(List<object> values)
        {
            //TODO: Implement logic for InsertRow function!
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
