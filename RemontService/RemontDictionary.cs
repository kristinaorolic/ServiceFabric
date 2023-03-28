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
using System.Diagnostics;
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
        private Thread updateRemontTable;
        private Thread historyThread;

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

        public async void InitHistory()
        {
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontHistoryDictionary");
                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    var elements = RemontTableHistoryHelper.GetInstance().GetAllRemonts();
                    foreach (var item in elements)
                    {
                        await dict.TryAddAsync(tx, item.RowKey, item);
                    }

                    await tx.CommitAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }

            this.historyThread = new Thread(UpdateHistory)
            {
                IsBackground = true
            };
            this.historyThread.Start();
        }

        public async Task<bool> AddRemontToDictionary(Remont remont)
        {
            try
            {
                var remontDictionary = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontDictionary");
                var historyDict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontHistoryDictionary");

                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    remont.NumberOfRemont = (await remontDictionary.GetCountAsync(tx) + await historyDict.GetCountAsync(tx)).ToString();
                    remont.PartitionKey = "remont";
                    remont.RowKey = remont.NumberOfRemont;

                    if (!await remontDictionary.TryAddAsync(tx, remont.NumberOfRemont, remont))
                    {
                        return false;
                    }
                    await tx.CommitAsync();
                    lastChanged = DateTime.Now;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

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
                ret.Add(new Remont(remont.TimeInMagacin, remont.TimeOfExploatation, remont.TimeOnRemont, remont.NumberOfRemont, remont.IdOfDevice));
            }

            return ret;
        }

        public async Task<List<Remont>> GetAllHistoryRemonts()
        {
            List<Remont> ret = new List<Remont>();

            var remonts = await GetAllHistoryElements();

            foreach (var remont in remonts)
            {
                ret.Add(new Remont(remont.TimeInMagacin, remont.TimeOfExploatation, remont.TimeOnRemont, remont.NumberOfRemont, remont.IdOfDevice));
            }

            return ret;
        }

        public async Task<List<Remont>> GetAllHistoryElements()
        {
            List<Remont> ret = new List<Remont>();
            CancellationToken cancellationToken;
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontHistoryDictionary");
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

        private async void UpdateHistory()
        {
            CancellationToken cancellationToken;
            while (true)
            {
                Thread.Sleep(9000);
                List<Remont> finishedRemonts = new List<Remont>();

                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontDictionary");
                using (var tx = reliableServiceManager.CreateTransaction())
                {
                    var enumerable = await dict.CreateEnumerableAsync(tx);
                    var enumerator = enumerable.GetAsyncEnumerator();
                    var tableInstance = RemontTableHelper.GetInstance();
                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        if (enumerator.Current.Value.TimeOfExploatation.AddMinutes(enumerator.Current.Value.TimeInMagacin + enumerator.Current.Value.TimeOnRemont) < DateTime.Now)
                        {
                            finishedRemonts.Add(enumerator.Current.Value);
                        }
                    }
                }

                if (finishedRemonts.Count > 0)
                {
                    var historyDict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Remont>>("remontHistoryDictionary");
                    List<Remont> temp = new List<Remont>();

                    foreach (var item in finishedRemonts)
                    {
                        using (var tx = reliableServiceManager.CreateTransaction())
                        {
                            if(!(await historyDict.ContainsKeyAsync(tx, item.NumberOfRemont)))
                            {
                                await historyDict.TryAddAsync(tx, item.NumberOfRemont, item);
                                await tx.CommitAsync();
                                temp.Add(item);
                            }
                        }
                    }

                    using (var tx = reliableServiceManager.CreateTransaction())
                    {
                        var enumerable = await historyDict.CreateEnumerableAsync(tx);
                        var enumerator = enumerable.GetAsyncEnumerator();
                        var tableInstance = RemontTableHistoryHelper.GetInstance();
                        while (await enumerator.MoveNextAsync(cancellationToken))
                        {
                            tableInstance.AddOrReplaceHistoryRemont(enumerator.Current.Value);
                        }
                    }

                    foreach (var item in temp)
                    {
                        await SendDeviceFromRemont(item.IdOfDevice);
                    }
                }
            }
        }

        public async Task<bool> SendDeviceFromRemont(string id)
        {
            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/DeviceService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IDeviceService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IDeviceService>>(
                new WcfCommunicationClientFactory<IDeviceService>(binding),
                new Uri("fabric:/TestServiceFabric/DeviceService"),
                new ServicePartitionKey(index % partitionNumber));

            return servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.SendBackFromRemont(id)).Result;

        }

        public async Task<bool> SendDeviceToRemont(string id)
        {
            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/DeviceService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IDeviceService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IDeviceService>>(
                new WcfCommunicationClientFactory<IDeviceService>(binding),
                new Uri("fabric:/TestServiceFabric/DeviceService"),
                new ServicePartitionKey(index % partitionNumber));

            return servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.SendToRemont(id)).Result;

        }
    }
}
