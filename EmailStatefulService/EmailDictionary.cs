using Common;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailStatefulService
{
    public class EmailDictionary
    {
        IReliableStateManager reliableServiceManager { get; set; }
        DateTime LastTimeChanged { get; set; }
        private Thread updateThread;
        public EmailDictionary(IReliableStateManager manager, string dictName)
        {
            this.reliableServiceManager = manager;
        }

        public async void Init()
        {
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Email>>("emailDictionary");
                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    var elements = EmailTable.GetInstance().GetAllEmails();
                    foreach (var item in elements)
                    {
                        await dict.TryAddAsync(tx, item.RowKey, item);
                    }

                    await tx.CommitAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }

            LastTimeChanged = DateTime.Now;
            this.updateThread = new Thread(TableUpdate)
            {
                IsBackground = true
            };
            this.updateThread.Start();
        }

        public async Task<bool> AddElement(Email element)
        {
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Email>>("emailDictionary");

                using (var tx = this.reliableServiceManager.CreateTransaction())
                {
                    Email newEmail = new Email((await dict.GetCountAsync(tx)).ToString())
                    {
                        Contents = element.Contents,
                        Sender = element.Sender,
                        Successful = element.Successful
                    };

                    if (!await dict.TryAddAsync(tx, newEmail.RowKey, newEmail))
                        return false;

                    await tx.CommitAsync();
                    LastTimeChanged = DateTime.Now;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<List<Email>> GetAllElements()
        {
            List<Email> ret = new List<Email>();
            CancellationToken cancellationToken;
            try
            {
                var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Email>>("emailDictionary");
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

        private async void TableUpdate()
        {
            DateTime threadTime;
            CancellationToken cancellationToken;
            while (true)
            {
                Thread.Sleep(60000);
                threadTime = DateTime.Now;

                if (LastTimeChanged > threadTime.AddMinutes(-10) && LastTimeChanged < threadTime)
                {
                    var dict = await this.reliableServiceManager.GetOrAddAsync<IReliableDictionary<string, Email>>("emailDictionary");
                    using (var tx = reliableServiceManager.CreateTransaction())
                    {
                        var enumerable = await dict.CreateEnumerableAsync(tx);
                        var enumerator = enumerable.GetAsyncEnumerator();
                        var tableInstance = EmailTable.GetInstance();
                        while (await enumerator.MoveNextAsync(cancellationToken))
                        {
                            tableInstance.AddOrReplaceEmail(enumerator.Current.Value);
                        }
                    }
                }
            }
        }
    }
}

