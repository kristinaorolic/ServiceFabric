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
    class RemontTableHelper
    {
        private static RemontTableHelper _instance;
        private static readonly object _lock = new object();

        public static RemontTableHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new RemontTableHelper("RemontTable");
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
        public RemontTableHelper(string tableName)
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["ConnectionString"]);
            tableClient = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri),
                                                                storageAccount.Credentials);
            table = tableClient.GetTableReference(tableName);
            if (table.CreateIfNotExists())
            {
                //InitTable();
            }

        }


        #endregion

        private void InitTable()
        {
            TableBatchOperation tableOperations = new TableBatchOperation();

            table.ExecuteBatch(tableOperations);
        }

        #region Operacije nad tabelom
        //Operacije nad tabelom
        public bool AddOrReplaceRemont(Remont remont)
        {
            TableOperation add = TableOperation.InsertOrReplace(remont);
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


