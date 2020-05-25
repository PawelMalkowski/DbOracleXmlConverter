using System;
using System.Collections.Generic;
using System.Text;

namespace DbOracleXmlConverter
{
    public class ColumnParameters
    {
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public int? DATA_LENGTH{ get; set; }
        public int? DATA_PRECISION{ get; set; }
        public int? DATA_SCALE{ get; set; }
        public bool NULLABLE{ get; set; }
    }
}
