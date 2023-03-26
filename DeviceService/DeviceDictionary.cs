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

namespace DeviceService
{
    public class DeviceDictionary
    {
        IReliableStateManager reliableServiceManager { get; set; }
        //DeviceTableHelper deviceTable { get; set; }
        private Thread updateDeviceTable;
        DateTime lastChanged { get; set; }

        public DeviceDictionary (IReliableStateManager manager) 
        {
            this.reliableServiceManager = manager;
            //this.deviceTable = new DeviceTableHelper("DeviceTable");
        }

        public async void InitDictionary()
        {
            try
            {
                var deviceDictionary = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Device>>("deviceDictionary");
                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    var elements = DeviceTableHelper.GetInstance().GetAllDevices();
                    foreach (var item in elements)
                    {
                        await deviceDictionary.TryAddAsync(tx, item.RowKey, item);
                    }

                    await tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            lastChanged = DateTime.Now;

            this.updateDeviceTable = new Thread(UpdateDeviceTable)
            {
                IsBackground = true
            };

            this.updateDeviceTable.Start();
        }

        public async Task<bool> AddDeviceToDictionary(Device device)
        {
            try
            {
                var deviceDictionary = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Device>>("deviceDictionary");

                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    device.Id = (await deviceDictionary.GetCountAsync(tx)).ToString();
                    device.PartitionKey = "device";
                    device.RowKey = device.Id;

                    if(!await deviceDictionary.TryAddAsync(tx, device.Id, device)) 
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

            DeviceTableHelper.GetInstance().AddOrReplaceDevice(device);

            return true;
        }

        public async Task<List<Device>> GetAllDevices()
        {
            List<Device> ret = new List<Device>();

            var devices = await GetAllElements();

            foreach (var device in devices)
            {
                ret.Add(new Device(device.Id, device.Name, device.IsOnRemont));
            }

            return ret;
        }

        public async Task<List<Device>> GetAllElements()
        {
            List<Device> ret = new List<Device>();
            CancellationToken cancellationToken;
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Device>>("deviceDictionary");
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

        private async void UpdateDeviceTable()
        {
            DateTime threadTime;
            CancellationToken cancellationToken;
            while (true)
            {
                Thread.Sleep(50000);
                threadTime = DateTime.Now;

                if (lastChanged > threadTime.AddMinutes(-10) && lastChanged < threadTime)
                {
                    var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Device>>("deviceDictionary");
                    using (var tx = reliableServiceManager.CreateTransaction())
                    {
                        var enumerable = await dict.CreateEnumerableAsync(tx);
                        var enumerator = enumerable.GetAsyncEnumerator();
                        var tableInstance = DeviceTableHelper.GetInstance();
                        while (await enumerator.MoveNextAsync(cancellationToken))
                        {
                            tableInstance.AddOrReplaceDevice(enumerator.Current.Value);
                        }
                    }
                }
            }
        }

        public async Task<bool> ChangeDeviceStatusToRemont(string id)
        {
            var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Device>>("deviceDictionary");

            try
            {

                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    var device = await dict.TryGetValueAsync(tx, id);
                    if (device.HasValue)
                    {
                        device.Value.IsOnRemont = true;
                        await tx.CommitAsync();
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ChangeDeviceStatusFromRemont(string id)
        {
            var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Device>>("deviceDictionary");

            try
            {

                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    var device = await dict.TryGetValueAsync(tx, id);
                    if (device.HasValue)
                    {
                        device.Value.IsOnRemont = false;
                        await tx.CommitAsync();
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
