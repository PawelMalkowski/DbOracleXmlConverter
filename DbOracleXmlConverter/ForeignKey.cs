using System;
using System.Collections.Generic;
using System.Text;

namespace DbOracleXmlConverter
{
    public class ForeignKey
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Name { get; set; }
        public string DestiantionTableName { get; set; }
        public string DestinationColumnName { get; set; }
        public bool Enable {get;set;}
    }
}
