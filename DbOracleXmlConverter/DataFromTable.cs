using System;
using System.Collections.Generic;
using System.Text;

namespace DbOracleXmlConverter
{
    public class DataFromTable
    {
        public string TableName { get; set; }
        public HashSet<List<string>> Table { get; set; }
        public DataFromTable()
        {
            Table = new HashSet<List<string>>();
        }
    }
}
