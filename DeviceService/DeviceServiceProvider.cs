using Common;
using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    public class DeviceServiceProvider : IDeviceService
    {
        public DeviceDictionary dDictionary;

        public DeviceServiceProvider(IReliableStateManager manager) 
        {
            dDictionary = new DeviceDictionary(manager);
        }

        public Task<List<Device>> GetAllDevices()
        {
            return dDictionary.GetAllDevices();
        }

        public Task<bool> SaveDevice(Device device)
        {
            return dDictionary.AddDeviceToDictionary(device);
        }

        public Task<bool> SendToRemont(string id)
        {
            return dDictionary.ChangeDeviceStatusToRemont(id);
        }

        public Task<bool> SendBackFromRemont(string id)
        {
            return dDictionary.ChangeDeviceStatusFromRemont(id);
        }
    }
}
