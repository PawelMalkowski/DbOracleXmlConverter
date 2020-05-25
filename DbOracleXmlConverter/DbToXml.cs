using System;
using System.Collections.Generic;
using System.Text;

namespace DbOracleXmlConverter
{
    public class DbToXml
    {
        public HashSet<Table> tables { get; set; }
        public HashSet<PrimaryKey> primaryKeys { get; set; }
        public HashSet<ForeignKey> foreignKeys { get; set; }
        public HashSet<DataFromTable> dataFromTables { get; set; }

    }
}
