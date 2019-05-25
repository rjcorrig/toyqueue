using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ToyQueue
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("storagekeys.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            Configuration = builder.Build();

            Console.WriteLine("Hello World!");
        }
    }
}
