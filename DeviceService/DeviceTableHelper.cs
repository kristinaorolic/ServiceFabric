using Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    public class DeviceTableHelper
    {
        private static DeviceTableHelper _instance;
        private static readonly object _lock = new object();

        public static DeviceTableHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DeviceTableHelper("DeviceTable");
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
        public DeviceTableHelper(string tableName)
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["ConnectionString"]);
            tableClient = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri),
                                                                storageAccount.Credentials);
            table = tableClient.GetTableReference(tableName);
            if (table.CreateIfNotExists())
            {
                InitTable();
            }

        }


        #endregion

        private void InitTable()
        {
            TableBatchOperation tableOperations = new TableBatchOperation();

            Device a1 = new Device("1", "nameee", false);
            //Device a1 = new Film("123", 10);
            //a2 = new Film("456", 10);
            //Film a3 = new Film("789", 10);
            //Film a4 = new Film("000", 10);

            tableOperations.InsertOrReplace(a1);
            //tableOperations.InsertOrReplace(a2);
            //tableOperations.InsertOrReplace(a3);
            //tableOperations.InsertOrReplace(a4);

            table.ExecuteBatch(tableOperations);
        }

        #region Operacije nad tabelom
        //Operacije nad tabelom
        //Find: Film -> Replace: Naziv klase koja se koristi
        public bool AddOrReplaceDevice(Device device)
        {
            TableOperation add = TableOperation.InsertOrReplace(device);
            table.Execute(add);

            return true;
        }

        public List<Device> GetAllDevices()
        {
            IQueryable<Device> requests = from g in table.CreateQuery<Device>()
                                        where g.PartitionKey == "device"
                                        select g;
            return requests.ToList();
        }

        //public Device GetOneDevice(string id)
        //{
        //    IQueryable<Device> requests = from g in table.CreateQuery<Device>()
        //                                where g.PartitionKey == "device" && g.RowKey == id
        //                                select g;

        //    return requests.ToList()[0];
        //}

        //public List<Film> GetAllFilmByName(string name)
        //{
        //    IQueryable<Film> requests = from g in table.CreateQuery<Film>()
        //                                where g.PartitionKey == "Film" && g.Naziv == name
        //                                select g;

        //    return requests.ToList();
        //}
        #endregion
    }
}

