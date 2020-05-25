using System;
using System.Collections.Generic;
using System.Text;

namespace DbOracleXmlConverter
{
    public class Table
    {
        public string TableName { get; set; }
        public HashSet<ColumnParameters> columns { get; set; }
        public Table()
        {
           columns  = new HashSet<ColumnParameters>();
        }
           
    }
}
