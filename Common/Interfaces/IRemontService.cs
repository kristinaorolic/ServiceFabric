using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IRemontService
    {
        [OperationContract]
        Task<bool> SaveRemont(Remont remont);
        [OperationContract]
        Task<List<Remont>> GetAllRemonts();
        [OperationContract]
        Task<List<Remont>> GetAllHistoryRemonts();

    }
}
