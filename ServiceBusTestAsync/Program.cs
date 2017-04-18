using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;
using System.Transactions;

namespace ServiceBusTestAsync
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            MessagingFactory fact = MessagingFactory.CreateFromConnectionString(ConfigurationManager.AppSettings["ConnectionString"]);

            var sender = fact.CreateMessageSender("destinationQueue", "queue2");

            var receiver = fact.CreateMessageReceiver("queue2", ReceiveMode.PeekLock);

            BrokeredMessage msg;
            BrokeredMessage recMsg;

            var i = 0;
            while (i < 5000)
            {
                recMsg = await receiver.ReceiveAsync();
                msg = new BrokeredMessage();

                if (recMsg != null)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        msg.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMinutes(6);

                        await recMsg.CompleteAsync();

                        await sender.SendAsync(msg);
                        scope.Complete();
                        i++;
                    }
                }
            }
        }
    }
}
