using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace ToyQueue
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }
        static CloudStorageAccount StorageAccount { get; set; }
        
        static string Usage = @"
usage:

dotnet run enq [message...]
    (Enqueues one or more messages)

dotnet run deq [N]
    (Dequeues and prints N messages. Default N = 1)

";
        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] != "enq" && args[0] != "deq")
            {
                Console.Write(Usage);
                return;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("storagekeys.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            StorageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Key1"]);

            if (args[0] == "enq")
            {
                QueueArgs(args.AsSpan(1));
            }
            else if (args.Length > 1)
            {
                int.TryParse(args[1], out int numberOfMessages);
                if (numberOfMessages == 0)
                    numberOfMessages = 1;

                Dequeue(numberOfMessages);
            }
            else
            {
                Dequeue(1);
            }
        }

        static void QueueArgs(Span<string> args)
        {
            CloudQueueClient client = StorageAccount.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference("toyqueue");
            queue.CreateIfNotExists();

            foreach (string arg in args)
            {
                Console.Write($"Queueing {arg}...");
                queue.AddMessage(new CloudQueueMessage(arg));
                Console.WriteLine(" queued.");
            }
        }

        static void Dequeue(int numberOfMessages)
        {
            CloudQueueClient client = StorageAccount.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference("toyqueue");
            queue.CreateIfNotExists();

            for (int i = 0; i < numberOfMessages; i++)
            {
                Console.Write("Dequeue... ");
                CloudQueueMessage message = queue.GetMessage();
                if (message != null)
                {
                    Console.WriteLine(message.AsString);
                    queue.DeleteMessage(message);
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
