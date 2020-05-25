using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows.Controls;

namespace DbOracleXmlConverter
{
    public class PreperQuery
    {

        public static List<string> PreperQueryDropTable(HashSet<Table> tables)
        {
            List<string> Query = new List<string>();
            string example = "DROP TABLE {0}";
            foreach (var table in tables)
            {
                Query.Add(String.Format(example, table.TableName));
            }
            return Query;
        }
        public static List<string> PreperQueryCreateTable(HashSet<Table> tables)
        {
            List<string> Query = new List<string>();
            string example = "CREATE TABLE \"{0}\" ({1})";
            
            foreach (var table in tables)
            {
                string columns = "";
                foreach (var property in table.columns)
                {
                    columns += "\"" + property.COLUMN_NAME + "\" " + property.DATA_TYPE;
                    if (property.DATA_TYPE != "DATE" && !(property.DATA_TYPE== "NUMBER" && property.DATA_PRECISION==null))
                    {
                        if (property.DATA_PRECISION != null) columns += "(" + property.DATA_PRECISION + "," + property.DATA_SCALE + ")";
                        else columns += "(" + property.DATA_LENGTH + " BYTE)";
                        if (!property.NULLABLE) columns += " NOT NULL";
                       
                    }
                    columns += ",";

                }
                columns = columns.Remove(columns.Length - 1);
                Query.Add(String.Format(example, table.TableName,columns));
            }
            return Query;
        }
        public static HashSet<string> PreperQueryCreatePrimaryKey(HashSet<PrimaryKey> primaryKeys)
        {
            HashSet<string> query = new HashSet<string>();
            PreperQueryCreatePrimaryKeyIndex(query, primaryKeys);
            PreperQueryCreatePrimaryKeyKey(query, primaryKeys);
            return query;
        }
        public static HashSet<string> PreperQueryCreateForeignKey(HashSet<ForeignKey> foreignKeys)
        {
            HashSet<string> query = new HashSet<string>();
            string example = " ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" FOREIGN KEY (\"{2}\") REFERENCES \"{3}\"(\"{4}\") {5}";
            foreach (var foreginKey in foreignKeys)
            {
                if (foreginKey.Enable) query.Add(String.Format(example, foreginKey.TableName, foreginKey.Name, foreginKey.ColumnName,foreginKey.DestiantionTableName,foreginKey.DestinationColumnName, "ENABLE"));
                else query.Add(String.Format(example, foreginKey.TableName, foreginKey.Name, foreginKey.ColumnName, foreginKey.DestiantionTableName, foreginKey.DestinationColumnName, "DISABLE"));
            }
            return query;
        }
       public static HashSet<string> PreperInsertQuery(DataFromTable dataFromTables, Table tables)
        {
            string example = "INSERT ALL {0} SELECT* FROM dual";
            string example1 = "INTO {0}({1}) VALUES({2})";
            HashSet<string> query = new HashSet<string>();
                int j = 0;
 
                var result2 = Enumerable.Range(0, tables.columns.ToList().Count).Where(i => tables.columns.ToList()[i].DATA_TYPE == "DATE").ToList();
                StringBuilder columns = new StringBuilder();
                foreach (var column in tables.columns)
                {
                    columns.Append("\"" + column.COLUMN_NAME + "\",");
                }
                columns = columns.Remove(columns.Length - 1,1);
                StringBuilder rows = new StringBuilder();
               foreach (var row in dataFromTables.Table)
                {
                    
                    StringBuilder values = new StringBuilder();
                   
                    int i = 0;
                    foreach (var record in row)
                    {
                        if (record != null)
                        {
                            if(!result2.Contains(i)) values.Append("'" + record + "',");
                            else values.Append("TO_DATE('" + record + "','dd/mm/yyyy hh24:mi:ss'),");
                            
                        }
                        else values.Append("null,");
                        ++i;
                    }
                    values = values.Remove(values.Length - 1, 1);
                    rows.Append(String.Format(example1, tables.TableName, columns, values));
                    ++j;
                    if (j == 200)
                    {
                        query.Add(String.Format(example, rows));
                        rows.Clear();
                        j = 0;
                    }
                }
                query.Add(String.Format(example, rows));
            
            return query;
        }
        private static void PreperQueryCreatePrimaryKeyIndex(HashSet<string> querys,HashSet<PrimaryKey>primaryKeys)
        {
            string example= "CREATE UNIQUE INDEX \"{0}\" ON \"{1}\"(\"{2}\")";
            foreach (var primaryKey in primaryKeys)
            {
                querys.Add(String.Format(example, primaryKey.Name,primaryKey.TableName,primaryKey.ColumnName));
            }
        }
        private static void PreperQueryCreatePrimaryKeyKey(HashSet<string> querys, HashSet<PrimaryKey> primaryKeys)
        {
            string example = " ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" PRIMARY KEY (\"{2}\") {3}";
            foreach (var primaryKey in primaryKeys)
            {
                if (primaryKey.Enable) querys.Add(String.Format(example, primaryKey.TableName, primaryKey.Name, primaryKey.ColumnName, "ENABLE"));
                else querys.Add(String.Format(example, primaryKey.TableName, primaryKey.TableName, primaryKey.ColumnName, "DISABLE"));
            }
        }
      
    }
}
