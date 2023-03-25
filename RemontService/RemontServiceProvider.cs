using Common;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
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
            return rDictionary.AddRemontToDictionary(remont);
        }
    }
}
