using Common;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemontService
{
    public class RemontServiceProvider : IRemontService
    {
        public RemontDictionary rDictionary;

        public RemontServiceProvider(IReliableStateManager manager)
        {
            rDictionary = new RemontDictionary(manager);
        }

        public Task<List<Remont>> GetAllRemonts()
        {
            return rDictionary.GetAllRemonts();
        }

        public Task<bool> SaveRemont(Remont remont)
        {
            var result = SendDeviceToRemont(remont.IdOfDevice);
            return rDictionary.AddRemontToDictionary(remont);
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

            return servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.CheckIfDeviceIsOnRemont(id)).Result;

        }
    }
}
