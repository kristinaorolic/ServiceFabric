using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IDeviceService
    {
        [OperationContract]
        Task<bool> SaveDevice(Device device);
        [OperationContract]
        Task<List<Device>> GetAllDevices();
        [OperationContract]
        Task<bool> SendToRemont(string id);
        [OperationContract]
        Task<bool> SendBackFromRemont(string id);
    }
}
