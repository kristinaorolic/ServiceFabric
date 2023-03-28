using Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailStatefulService
{
    public class EmailTable
    {
        private static EmailTable _instance;
        private static readonly object _lock = new object();

        public static EmailTable GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EmailTable("EmailTable");
                    }
                }
            }
            return _instance;
        }

        CloudStorageAccount storageAccount;
        CloudTable table;
        CloudTableClient tableClient;

        public EmailTable(string tableName)
        {
            //storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["ConnectionString"]);
            tableClient = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri),
                                                                storageAccount.Credentials);
            table = tableClient.GetTableReference(tableName);
            if (table.CreateIfNotExists())
            {
                InitTable();
            }
        }

        private void InitTable()
        {
            TableBatchOperation tableOperations = new TableBatchOperation();

            Email initialEmail = new Email("0")
            {
                Contents = "test email",
                Sender = "kristinaorolic@gmail.com",
                Successful = true
            };

            tableOperations.InsertOrReplace(initialEmail);
            table.ExecuteBatch(tableOperations);
        }

        public bool AddOrReplaceEmail(Email obj)
        {
            TableOperation add = TableOperation.InsertOrReplace(obj);
            table.Execute(add);

            return true;
        }

        public List<Email> GetAllEmails()
        {
            IQueryable<Email> requests = from g in table.CreateQuery<Email>()
                                         where g.PartitionKey == "email"
                                         select g;
            return requests.ToList();
        }

        public int EmailsCount()
        {
            IQueryable<Email> requests = from g in table.CreateQuery<Email>()
                                         where g.PartitionKey == "email"
                                         select g;
            var test = requests.ToList();
            return requests.ToList().Count;
        }
    }
}
