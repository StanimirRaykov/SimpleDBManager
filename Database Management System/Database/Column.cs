using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Database
{
    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public object DefaultValue { get; set; }

        public Column(string name, string dataType, object defaultValue = null)
        {
            Name = name;
            DataType = dataType;
            DefaultValue = defaultValue;
        }
    }
}
