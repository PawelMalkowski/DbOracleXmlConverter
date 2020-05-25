using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace DbOracleXmlConverter
{
    public class FillDB
    {
        private static OracleConnection connection;
        public DbToXml dbToXml = new DbToXml();
        private readonly List<int> sequence;
        string Login;
        string Password;
        public FillDB(DbToXml dbToXml,string login,string password)
        {
            Login = login;
            Password = password;
            connection = new OracleConnection
            {
                ConnectionString = "User Id="+login.ToLower()+";Password="+password.ToLower()+"; Data Source = (DESCRIPTION = " +
                                       " (ADDRESS = (PROTOCOL = TCP)(HOST = 217.173.198.135)(PORT = 1522    ))" +
                                       " (CONNECT_DATA =" +
                                       " (SERVER = DEDICATED)" +
                                        " (SERVICE_NAME = orcltp.iaii.local)" +
                                       ")" +
                                       ");"
            };
            sequence = SequenceValidator.setOrder(dbToXml.tables,dbToXml.foreignKeys);
            try
            {
                connection.Open();
                DeleteDb(dbToXml.tables);
                CreateTables(dbToXml.tables);
                CreatePrimaryKeys(dbToXml.primaryKeys);
                CreateForeignKeys(dbToXml.foreignKeys);
                InsertData(dbToXml.tables, dbToXml.dataFromTables);
                connection.Close();
            }
            catch
            {
                throw new System.ArgumentException("Bad loginor password");
            }

            

        }
        private void DeleteDb(HashSet<Table> tables)
        {

            List<string> ListQuery = PreperQuery.PreperQueryDropTable(tables);
            for(int i=sequence.Count-1;i>=0;--i)
            {
                Insertquery(ListQuery[sequence[i]]);
            }
        }
        private void CreateTables(HashSet<Table> tables)
        {
            List<string> ListQuery = PreperQuery.PreperQueryCreateTable(tables);
            for (int i = 0; i <sequence.Count; ++i)
            {
                Insertquery(ListQuery[sequence[i]]);
            }
        }
        private void CreatePrimaryKeys(HashSet<PrimaryKey> primaryKeys)
        {
            HashSet<string> ListQuery = PreperQuery.PreperQueryCreatePrimaryKey(primaryKeys);
            foreach(string query in ListQuery)
            {
                Insertquery(query);
            }
        }
        private void CreateForeignKeys(HashSet<ForeignKey> foreignKeys)
        {
            HashSet<string> ListQuery = PreperQuery.PreperQueryCreateForeignKey(foreignKeys);
            foreach (string query in ListQuery)
            {
                Insertquery(query);
            }
        }
        private void InsertData(HashSet<Table> tables, HashSet<DataFromTable> dataFromTables)
        {
            List<Table> TabelList = tables.ToList();
            List<DataFromTable> DataTableList = dataFromTables.ToList();
            for (int i = 0;i<sequence.Count; ++i)
            {
                HashSet<string>ListQuery= PreperQuery.PreperInsertQuery(DataTableList[sequence[i]], TabelList[sequence[i]]);
                foreach (string query in ListQuery)
                {
                    Insertquery(query);
                }
            }
        }
        private void Insertquery(string query)
        {
            OracleCommand ins = new OracleCommand
            {
                CommandText = query,
                Connection = connection
            };
            try
            {
                ins.ExecuteNonQuery();
            }
            catch(OracleException e)
            {
                
            }

        }
    }
}
