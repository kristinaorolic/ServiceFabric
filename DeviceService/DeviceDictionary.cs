using Common;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    public class DeviceDictionary
    {
        IReliableStateManager reliableServiceManager { get; set; }
        //DeviceTableHelper deviceTable { get; set; }

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
            }
            catch (Exception e)
            {
                throw e;
            }
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
    }
}
