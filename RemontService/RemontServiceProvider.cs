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

        public Task<List<Remont>> GetAllHistoryRemonts()
        {
            return rDictionary.GetAllHistoryRemonts();
        }

        public async Task<bool> SaveRemont(Remont remont)
        {
            var result = await rDictionary.SendDeviceToRemont(remont.IdOfDevice);
            if(result == false)
            {
                return false;
            }
            return await rDictionary.AddRemontToDictionary(remont);
        }
    }
}
