using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ApiProxy
{
    class Program
    {
        private static Config config;
        static Config ReadConfig()
        {
            var lines = File.ReadAllLines("config.txt");
            var res = new Config(lines);
            return res;
        }

        static Socket SocketRun(IPEndPoint endPoint)
        {
            var res = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine($"Proxy is connecting to {endPoint.Address}:{endPoint.Port}");
            res.ReceiveBufferSize = config.receiveBufferSizeBytes;
            res.SendBufferSize = config.sendBufferSizeBytes;
            res.NoDelay = true;
            res.ReceiveTimeout = config.receiveTimeoutMs;
            res.SendTimeout = config.sendTimeoutMs;
            //res.Blocking = false;

            while (!res.Connected)
            {
                try
                {
                    res.Connect(endPoint);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine($"Proxy is connected to {endPoint.Address}:{endPoint.Port}");
            return res;
        }

        static void Main(string[] args)
        {
            config = ReadConfig();
            byte[] buf = new byte[config.bufferSizeBytes];
            Socket client = null;
            Socket api = null;

            while (true)
            {
                try
                {
                    client = SocketRun(config.client);
                    api = SocketRun(config.api);

                    Thread.Sleep(config.firstDelayMs);

                    while (true)
                    {
                        while (client.Available > 0)
                        {
                            var read = client.Receive(buf);
                            api.Send(buf, 0, read, SocketFlags.None);
                        }

                        while (api.Available > 0)
                        {
                            var read = api.Receive(buf);
                            client.Send(buf, 0, read, SocketFlags.None);
                        }

                        if (client.IsClosed())
                            throw new Exception($"client is failed status={client.GetState()}");
                        if (api.IsClosed())
                            throw new Exception($"api is failed status={api.GetState()}");

                        Thread.Sleep(config.iterationDelayMs);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    client?.FullClose();
                    api?.FullClose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(config.exceptionDelayMs);
                }
            }
        }
    }
}
