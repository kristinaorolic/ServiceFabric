using Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemontService
{
    public class RemontTableHistoryHelper
    {
        private static RemontTableHistoryHelper _instance;
        private static readonly object _lock = new object();

        public static RemontTableHistoryHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new RemontTableHistoryHelper("HistoryRemontTable");
                    }
                }
            }
            return _instance;
        }

        #region Fields
        CloudStorageAccount storageAccount;
        CloudTable table;
        CloudTableClient tableClient;
        #endregion

        #region Kreiranje tabele
        // Kreiranje tabele
        public RemontTableHistoryHelper(string tableName)
        {
            //storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["ConnectionString"]);
            tableClient = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri),
                                                                storageAccount.Credentials);
            table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
        }
        #endregion

        #region Operacije nad tabelom
        //Operacije nad tabelom
        public bool AddOrReplaceHistoryRemont(Remont obj)
        {
            TableOperation add = TableOperation.InsertOrReplace(obj);
            table.Execute(add);

            return true;
        }

        public List<Remont> GetAllRemonts()
        {
            IQueryable<Remont> requests = from g in table.CreateQuery<Remont>()
                                        where g.PartitionKey == "remont"
                                        select g;
            return requests.ToList();
        }


        #endregion
    }
}


