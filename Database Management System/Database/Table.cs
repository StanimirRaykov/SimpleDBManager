using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Database
{
    public class Table
    {
        private string TableName;
        private List<Column> Columns;
        private string DataFilePath;

        public Table(string name, List<Column> columns)
        {
            TableName = name;
            Columns = columns;
            DataFilePath = $"{name}.db";
        }

        public static void CreateTable(string name, List<Column> columns)
        {
            //TODO: Implement logic for CreateTable function!
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
