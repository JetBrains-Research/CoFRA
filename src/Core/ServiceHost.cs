using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cofra.Core.Util;
using CommandLine;

namespace Cofra.Core
{
    public class ServiceHost
    {
        private readonly bool myRemoteAccess;
        private readonly string myDatabasePath;
        private readonly int myPort;

        public ServiceHost(bool remoteAccess, int port, string databasePath)
        {
            myRemoteAccess = remoteAccess;
            myDatabasePath = databasePath;
            myPort = port;
        }

        public void Start()
        {
            if (myRemoteAccess)
            {
                Logging.Log($"Starting remote service at port {myPort} with database at path {myDatabasePath}");

                var listener = new TcpListener(IPAddress.Loopback, myPort);
                listener.Start();

                TcpClient client = listener.AcceptTcpClient();
                var service = new Service(client, myDatabasePath);
                
                try
                {
                    service.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                client.Close();
            }
            else
            {
                //TODO: Offline analyzes runner
                var service = new Service(null, myDatabasePath);
                service.Start();
            }
        }

        public static void Main(string[] args)
        {
            Logging.Log($"Options: {string.Join(" ", args)}");

            var parsedOptions = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);

            CommandLineOptions options = null;
            parsedOptions.WithParsed(result => options = result);

            if (options == null)
            {
                return;
            }

            var host = new ServiceHost(options.AsService, options.Port, options.Database);
            host.Start();
        }

        private class CommandLineOptions
        {
            [Option('s', "as-service")] 
            public bool AsService { get; set; }

            [Option('p', "port", Default = 8888)]
            public int Port { get; set; }

            [Option('a', "analysis")]
            public string Analysis { get; set; }

            [Option('b', "database")] 
            public string Database {get; set; }
        }
    }
}
