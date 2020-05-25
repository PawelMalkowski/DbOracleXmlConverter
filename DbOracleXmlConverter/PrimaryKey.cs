using System;
using System.Collections.Generic;
using System.Text;

namespace DbOracleXmlConverter
{
    public class PrimaryKey
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool Enable { get; set; }
    }
}
