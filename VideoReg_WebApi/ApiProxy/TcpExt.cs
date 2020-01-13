using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ApiProxy
{
    static class TcpExt
    {
        public static TcpState GetState(this Socket socket)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .SingleOrDefault(x => x.LocalEndPoint.Equals(socket.LocalEndPoint));
            return foo?.State ?? TcpState.Unknown;
        }

        public static bool IsClosed(this Socket socket)
        {
            var state = socket.GetState();
            return state != TcpState.Established;
        }
        public static void FullClose(this Socket socket)
        {
            if (socket == null) return;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                socket.Close();
                socket.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            GC.SuppressFinalize(socket);
        }
    }
}
