using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbOracleXmlConverter
{
    public  class SequenceValidator
    {
       static public List<int> setOrder(HashSet<Table> tabels,HashSet<ForeignKey> foreignKey)
        {

            List<KeyValuePair<string, string>> dictionary = new List<KeyValuePair<string, string>>();
            foreach (var key in foreignKey)
            {
                if(key.TableName!=key.DestiantionTableName) dictionary.Add(new KeyValuePair<string, string>(key.TableName, key.DestiantionTableName));
            }
            List<string> tabelsList = tabels.Select(e => e.TableName).ToList();
            List<int> order = new List<int>();
            List<Table> tabelList = tabels.ToList();
            while (tabelsList.Count>order.Count)
            for(int i = 0; i < tabelsList.Count; ++i)
            {
                    if (!order.Contains(i))
                    {
                        var result = dictionary.Exists(kvp => kvp.Key == tabelsList[i]);
                        if (!result)
                        {
                            var result1 = dictionary.Where(kvp => kvp.Value == tabelsList[i]).ToList();
                            dictionary = dictionary.Except(result1).ToList();
                            order.Add(i);
                        }
                    }
            }
            return order;
        }
    }
}
