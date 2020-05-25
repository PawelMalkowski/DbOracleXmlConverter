using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace DbOracleXmlConverter
{
   
    public class DataFromDB
    {
        private static OracleConnection connection;
        public DbToXml dbToXml = new DbToXml();
        string Login;
        string Password;
        public DataFromDB(string login,string password)
        {
            Login = login;
            Password = password;
            
            connection = new OracleConnection
            {
                ConnectionString = "User Id="+Login.ToLower()+";Password="+Password.ToLower()+";Data Source = (DESCRIPTION = " +
                                       " (ADDRESS = (PROTOCOL = TCP)(HOST = 217.173.198.135)(PORT = 1522    ))" +
                                       " (CONNECT_DATA =" +
                                       " (SERVER = DEDICATED)" +
                                        " (SERVICE_NAME = orcltp.iaii.local)" +
                                       ")" +
                                       ");"
            };
            try
            {
                connection.Open();

                dbToXml.tables = GetTableList();
                dbToXml.primaryKeys = GetPrimaryKeyList();
                dbToXml.foreignKeys = GetForeignKeyList();
                dbToXml.dataFromTables = GetDataFromTableList();
                connection.Close();
            }
            catch
            {
                throw new System.ArgumentException("Bad loginor password");
            }
        }
        private HashSet<string> GetTableIntList()
        {
            OracleCommand sel = new OracleCommand();
            HashSet<string> indeks = new HashSet<string>();
            String select = "SELECT table_name FROM all_tables WHERE owner Like '"+Login.ToUpper()+"'";
            sel.Connection = connection;
            sel.CommandText = select;
            using (OracleDataReader reader = sel.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int index = reader.GetOrdinal("TABLE_NAME");

                        if (!reader.IsDBNull(index))
                        {
                            indeks.Add(Convert.ToString(reader.GetValue(index)));
                        }
                    }
                }
            }
            return indeks;
        }
        private HashSet<Table> GetTableList()
        {
            HashSet<string> tabelsList = GetTableIntList();
            HashSet<Table> Tables = new HashSet<Table>();
            OracleCommand sel = new OracleCommand();
            string  exampleSelect= "SELECT COLUMN_NAME,DATA_TYPE,DATA_LENGTH,DATA_PRECISION,DATA_SCALE,NULLABLE FROM ALL_TAB_COLUMNS WHERE TABLE_NAME LIKE '{0}' ORDER BY COLUMN_ID";
            foreach (string tableName in tabelsList)
            {
                Table table = new Table
                {
                    TableName = tableName
                };
                string select = String.Format(exampleSelect, tableName);
                sel.Connection = connection;
                sel.CommandText = select;
                using (OracleDataReader reader = sel.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ColumnParameters columnParameters = new ColumnParameters();
                            int index_COLUMN_NAME = reader.GetOrdinal("COLUMN_NAME");
                            int index_DATA_TYPE = reader.GetOrdinal("DATA_TYPE");
                            int index_DATA_LENGTH = reader.GetOrdinal("DATA_LENGTH");
                            int index_DATA_PRECISION = reader.GetOrdinal("DATA_PRECISION");
                            int index_DATA_SCALE = reader.GetOrdinal("DATA_SCALE");
                            int index_NULLABLE = reader.GetOrdinal("NULLABLE");
                            if (!reader.IsDBNull(index_COLUMN_NAME)) columnParameters.COLUMN_NAME = Convert.ToString(reader.GetValue(index_COLUMN_NAME));
                            if (!reader.IsDBNull(index_DATA_TYPE)) columnParameters.DATA_TYPE = Convert.ToString(reader.GetValue(index_DATA_TYPE));
                            if (!reader.IsDBNull(index_DATA_LENGTH)) columnParameters.DATA_LENGTH = Convert.ToInt32(reader.GetValue(index_DATA_LENGTH));
                            if (!reader.IsDBNull(index_DATA_PRECISION)) columnParameters.DATA_PRECISION = Convert.ToInt32(reader.GetValue(index_DATA_PRECISION));
                            if (!reader.IsDBNull(index_DATA_SCALE)) columnParameters.DATA_SCALE = Convert.ToInt32(reader.GetValue(index_DATA_SCALE));
                            if (!reader.IsDBNull(index_NULLABLE)) columnParameters.NULLABLE = (Convert.ToString(reader.GetValue(index_NULLABLE)))=="Y";
                            table.columns.Add(columnParameters);
                        }
                    }
                }
                Tables.Add(table);
            }
            return Tables;

        }
        private HashSet<PrimaryKey> GetPrimaryKeyList()
        {
            OracleCommand sel = new OracleCommand();
            HashSet<PrimaryKey> primaryKeys = new HashSet<PrimaryKey>();
            string select = "SELECT cols.table_name, cols.column_name, cons.status, cons.constraint_name FROM all_constraints cons, all_cons_columns cols " +
                "WHERE cons.constraint_type = 'P'AND cons.constraint_name = cols.constraint_name AND cons.owner = cols.owner " +
                "AND cons.owner LIKE '"+Login.ToUpper()+"' And cons.TABLE_NAME NOT LIKE 'BIN%' ORDER BY cols.table_name, cols.position";
            sel.Connection = connection;
            sel.CommandText = select;
            using (OracleDataReader reader = sel.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PrimaryKey primaryKey = new PrimaryKey();
                        int indexTABLE_NAME = reader.GetOrdinal("TABLE_NAME");
                        int indexCOLUMN_NAME = reader.GetOrdinal("COLUMN_NAME");
                        int indexNAME = reader.GetOrdinal("CONSTRAINT_NAME");
                        int indexSTATUS = reader.GetOrdinal("STATUS");
                        if (!reader.IsDBNull(indexTABLE_NAME))  primaryKey.TableName=Convert.ToString(reader.GetValue(indexTABLE_NAME));
                        if (!reader.IsDBNull(indexCOLUMN_NAME)) primaryKey.ColumnName = Convert.ToString(reader.GetValue(indexCOLUMN_NAME));
                        if((!reader.IsDBNull(indexSTATUS))) primaryKey.Enable = Convert.ToString(reader.GetValue(indexSTATUS))== "ENABLED";
                        if (!reader.IsDBNull(indexNAME)) primaryKey.Name = Convert.ToString(reader.GetValue(indexNAME));
                        primaryKeys.Add(primaryKey);
                    }
                }
            }
            return primaryKeys;
        }
        private HashSet<ForeignKey> GetForeignKeyList()
        {
            OracleCommand sel = new OracleCommand();
            HashSet<ForeignKey > foreignKeys= new HashSet<ForeignKey>();
            string select = "with constraint_colum_list as ( select owner,table_name, constraint_name, listagg(column_name,',') WITHIN GROUP ( order by position ) as column_list FROM DBA_CONS_COLUMNS GROUP BY owner, table_name, constraint_name ) select distinct  c1.table_name, c1.constraint_name, c2.column_list,c1.status, c3.table_name as DestinationTABLE_NAME, c3.column_list as DestinationCOLUMN_NAME from DBA_constraints c1 JOIN constraint_colum_list c2 ON c1.CONSTRAINT_NAME = C2.CONSTRAINT_NAME and c1.owner = c2.owner JOIN constraint_colum_list c3 ON C1.R_CONSTRAINT_NAME = C3.CONSTRAINT_NAME AND C1.R_OWNER = C3.owner where c1.owner LIKE '" + Login.ToUpper() + "' ";
            sel.Connection = connection;
            sel.CommandText = select;
            using (OracleDataReader reader = sel.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ForeignKey foreigKey = new ForeignKey();
                        int indexTABLE_NAME = reader.GetOrdinal("TABLE_NAME");
                        int indexCOLUMN_NAME = reader.GetOrdinal("COLUMN_LIST");
                        int indexNAME = reader.GetOrdinal("CONSTRAINT_NAME");
                        int indexDestinationTABLE_NAME = reader.GetOrdinal("DestinationTABLE_NAME");
                        int indexDestinationCOLUMN_NAME = reader.GetOrdinal("DestinationCOLUMN_NAME");
                        int indexSTATUS = reader.GetOrdinal("STATUS");
                        if (!reader.IsDBNull(indexTABLE_NAME)) foreigKey.TableName = Convert.ToString(reader.GetValue(indexTABLE_NAME));
                        if (!reader.IsDBNull(indexCOLUMN_NAME)) foreigKey.ColumnName = Convert.ToString(reader.GetValue(indexCOLUMN_NAME));
                        if (!reader.IsDBNull(indexDestinationTABLE_NAME)) foreigKey.DestiantionTableName = Convert.ToString(reader.GetValue(indexDestinationTABLE_NAME));
                        if (!reader.IsDBNull(indexDestinationCOLUMN_NAME)) foreigKey.DestinationColumnName = Convert.ToString(reader.GetValue(indexDestinationCOLUMN_NAME));
                        if (!reader.IsDBNull(indexNAME)) foreigKey.Name = Convert.ToString(reader.GetValue(indexNAME));
                        if ((!reader.IsDBNull(indexSTATUS))) foreigKey.Enable = Convert.ToString(reader.GetValue(indexSTATUS)) == "ENABLED";
                        foreignKeys.Add(foreigKey);
                    }
                }
            }
            return foreignKeys;
        }
        private HashSet<DataFromTable> GetDataFromTableList()
        {
            
            HashSet<DataFromTable> dataFromTables = new HashSet<DataFromTable>();
            string example = "Select * From {0} ORDER BY {1}";
            string example1 = "Select * From {0}";
            foreach (var table in dbToXml.tables)
            {
                PrimaryKey result1 = dbToXml.primaryKeys.FirstOrDefault(kvp => kvp.TableName == table.TableName);
                string select = "";
                if (result1!=null) select += String.Format(example, table.TableName, result1.ColumnName);
                else select += String.Format(example1, table.TableName);
                OracleCommand sel = new OracleCommand
                {
                    Connection = connection,
                    CommandText = select
                };
                DataFromTable dataFromTable = new DataFromTable
                {
                    TableName = table.TableName
                };
                using (OracleDataReader reader = sel.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        
                        while (reader.Read())
                        {
                            List<string> vs = new List<string>();
                            for (int i=0; i < reader.FieldCount; ++i)
                            {
                                if (!reader.IsDBNull(i))
                                {
                                    vs.Add(Convert.ToString(reader.GetValue(i)));
                                }
                                else
                                {
                                    vs.Add(null);
                                }
                            }
                            dataFromTable.Table.Add(vs);
                        }
                    }
                }
                dataFromTables.Add(dataFromTable);
            }
              return dataFromTables;
        }
    }
}
