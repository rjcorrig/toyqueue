using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ToyQueue
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }
        
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
            foreach (string arg in args)
            {
                Console.WriteLine($"Queue {arg}");
            }
        }

        static void Dequeue(int numberOfMessages)
        {
            for (int i = 0; i < numberOfMessages; i++)
            {
                Console.WriteLine("Dequeue");
            }
        }
    }
}
