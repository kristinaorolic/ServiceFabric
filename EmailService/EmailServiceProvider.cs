using Common.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using MailKit.Net.Imap;
using System.Configuration;
using MailKit;
using Common;

namespace EmailService
{

    public class EmailServiceProvider : IEmailService
    {
        private Thread emailThread;
        private int readEmails;

        public EmailServiceProvider()
        {
           
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

        private async void GetEmails()
        {
            while (true)
            {
                Thread.Sleep(60000);

                using (var client = new ImapClient())
                {
                    client.Connect("outlook.office365.com", 993, true);
                    client.Authenticate(ConfigurationManager.AppSettings["email"],
                                        ConfigurationManager.AppSettings["password"]);

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    Email email;
                    //for (int i = readEmailMessages; i < inbox.Count; i++)
                    //{
                    //    var message = inbox.GetMessage(i);
                    //    email = new Email()
                    //    {
                    //        Sender = message.From.ToString().Split('<', '>')[1],
                    //        Contents = message.Subject.ToString(),
                    //        Successful = false
                    //    };

                    //    var parameters = email.Contents.Split(',');
                    //    if (parameters.Length == 1)
                    //        email.Successful = !await SendDeviceToRemont(parameters[0]);

                    //}

                    //readEmailMessages = inbox.Count;
                    client.Disconnect(true);
                }
            }
        }
    }
}