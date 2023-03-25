using Common;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemontService
{
    public class RemontDictionary
    {
        IReliableStateManager reliableServiceManager { get; set; }
        //DeviceTableHelper deviceTable { get; set; }
        private Thread updateRemontTable;
        DateTime lastChanged { get; set; }

        public RemontDictionary(IReliableStateManager manager)
        {
            this.reliableServiceManager = manager;
        }

        public async void InitDictionary()
        {
            try
            {
                var remontDictionary = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontDictionary");
                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    var elements = RemontTableHelper.GetInstance().GetAllRemonts();
                    foreach (var item in elements)
                    {
                        await remontDictionary.TryAddAsync(tx, item.RowKey, item);
                    }

                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            lastChanged = DateTime.Now;

            this.updateRemontTable = new Thread(UpdateRemontTable)
            {
                IsBackground = true
            };

            this.updateRemontTable.Start();
        }

        public async Task<bool> AddRemontToDictionary(Remont remont)
        {
            try
            {
                var remontDictionary = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontDictionary");

                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    remont.NumberOfRemont = (await remontDictionary.GetCountAsync(tx)).ToString();
                    remont.PartitionKey = "remont";
                    remont.RowKey = remont.NumberOfRemont;

                    if (!await remontDictionary.TryAddAsync(tx, remont.NumberOfRemont, remont))
                    {
                        return false;
                    }
                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            RemontTableHelper.GetInstance().AddOrReplaceRemont(remont);

            return true;
        }

        public async Task<List<Remont>> GetAllRemonts()
        {
            List<Remont> ret = new List<Remont>();

            var remonts = await GetAllElements();

            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/RemontService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IRemontService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IRemontService>>(
                new WcfCommunicationClientFactory<IRemontService>(binding),
                new Uri("fabric:/TestServiceFabric/RemontService"),
                new ServicePartitionKey(index % partitionNumber));

            foreach (var remont in remonts)
            {
                ret.Add(new Remont(remont.TimeInMagacin, remont.TimeOfExploatation, remont.TimeOnRemont, remont.NumberOfRemont));
            }

            return ret;
        }

        public async Task<List<Remont>> GetAllElements()
        {
            List<Remont> ret = new List<Remont>();
            CancellationToken cancellationToken;
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontDictionary");
                using (var tx = reliableServiceManager.CreateTransaction())
                {
                    var enumerable = await dict.CreateEnumerableAsync(tx);
                    var enumerator = enumerable.GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        ret.Add(enumerator.Current.Value);
                    }
                }

                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        private async void UpdateRemontTable()
        {
            DateTime threadTime;
            CancellationToken cancellationToken;
            while (true)
            {
                Thread.Sleep(50000);
                threadTime = DateTime.Now;

                if (lastChanged > threadTime.AddMinutes(-10) && lastChanged < threadTime)
                {
                    var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontDictionary");
                    using (var tx = reliableServiceManager.CreateTransaction())
                    {
                        var enumerable = await dict.CreateEnumerableAsync(tx);
                        var enumerator = enumerable.GetAsyncEnumerator();
                        var tableInstance =RemontTableHelper.GetInstance();
                        while (await enumerator.MoveNextAsync(cancellationToken))
                        {
                            tableInstance.AddOrReplaceRemont(enumerator.Current.Value);
                        }
                    }
                }
            }
        }
    }
}
