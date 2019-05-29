using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace ToyQueue
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }
        static CloudStorageAccount StorageAccount { get; set; }
        static string QueueName { get; set; }
        
        static string Usage = @"
usage:

dotnet run enq [message...]
    (Enqueues one or more messages)

dotnet run deq [N]
    (Dequeues and prints N messages. Default N = 1)

";
        static async Task Main(string[] args)
        {
            if (args.Length == 0 || args[0] != "enq" && args[0] != "deq")
            {
                Console.Write(Usage);
                return;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("storagekeys.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            StorageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Key1"]);
            QueueName = Configuration["QueueName"];

            if (args[0] == "enq")
            {
                await QueueArgs(args.AsSpan().Slice(1).ToArray());
            }
            else if (args.Length > 1)
            {
                int.TryParse(args[1], out int numberOfMessages);
                if (numberOfMessages == 0)
                    numberOfMessages = 1;

                await Dequeue(numberOfMessages);
            }
            else
            {
                await Dequeue(1);
            }
        }

        static async Task QueueArgs(string[] args)
        {
            CloudQueueClient client = StorageAccount.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference(QueueName);
            await queue.CreateIfNotExistsAsync();

            foreach (string arg in args)
            {
                Console.Write($"Queueing {arg}...");
                await queue.AddMessageAsync(new CloudQueueMessage(arg));
                Console.WriteLine(" queued.");
            }
        }

        static async Task Dequeue(int numberOfMessages)
        {
            CloudQueueClient client = StorageAccount.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference(QueueName);
            await queue.CreateIfNotExistsAsync();

            for (int i = 0; i < numberOfMessages; i++)
            {
                Console.Write("Dequeue... ");
                CloudQueueMessage message = await queue.GetMessageAsync();
                if (message != null)
                {
                    Console.WriteLine(message.AsString);
                    await queue.DeleteMessageAsync(message);
                }
                else
                {
                    var frown = char.ConvertFromUtf32(0x1F61F);
                    Console.WriteLine($"empty {frown}");
                    break;
                }
            }
        }
    }
}
