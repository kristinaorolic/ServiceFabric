using Common;
using Common.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Configuration;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace EmaillService
{
    public class EmailServiceProvider : IEmailService
    {
        public EmailDictionary dictHandler;
        private Thread mailThread;
        private int readEmailMessages;
        public EmailServiceProvider(IReliableStateManager manager)
        {
            this.dictHandler = new EmailDictionary(manager, "emailDictionary");
            this.readEmailMessages = EmailTable.GetInstance().EmailsCount();

            this.mailThread = new Thread(GetEmails)
            {
                IsBackground = true
            };
            this.mailThread.Start();
        }

        public async Task<bool> SendDeviceToRemont(string id)
        {
            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/DeviceService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IEmailService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IEmailService>>(
                new WcfCommunicationClientFactory<IEmailService>(binding),
                new Uri("fabric:/TestServiceFabric/DeviceService"),
                new ServicePartitionKey(index % partitionNumber));

            return servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.SendDeviceToRemont(id)).Result;
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
                    for (int i = readEmailMessages; i < inbox.Count; i++)
                    {
                        var message = inbox.GetMessage(i);
                        email = new Email()
                        {
                            Sender = message.From.ToString().Split('<', '>')[1],
                            Contents = message.Subject.ToString(),
                            Successful = false
                        };

                        var parameters = email.Contents.Split(',');
                        if (parameters.Length == 1)
                            email.Successful = !await SendDeviceToRemont(parameters[0]);

                        await dictHandler.AddElement(email);
                    }

                    readEmailMessages = inbox.Count;
                    client.Disconnect(true);
                }
            }
        }
    }
}
