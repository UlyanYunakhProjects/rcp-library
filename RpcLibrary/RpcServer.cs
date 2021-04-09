using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using RpcLibrary.Handlers;

namespace RpcLibrary
{
    public class RpcServer
    {
        public int RpcPort { get; set; } = 8006; // port by default
        public string RpcIPAddress { get; set; } = "127.0.0.2"; // localhost by default

        public static object Procedures { get; set; } = null;

        public delegate void Log(string message);
        public event Log LogNotify;

        private Socket listenSocket;
        private int connectionId = 0;

        ~RpcServer()
        {
            listenSocket.Shutdown(SocketShutdown.Both);
            listenSocket.Close();
        }

        public async void ListenAsync()
        {
            LogNotify?.Invoke($"Server: Start.");

            if (Procedures == null)
            {
                LogNotify?.Invoke($"Server error: Server cannot be started. Procedures not set.");
                return;
            }

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint rpcPoint = null;

            if (CreateAddress(ref rpcPoint) == false)
            {
                LogNotify?.Invoke($"Server error: Server cannot be started. Invalid Address.");
                return;
            }

            try
            {
                listenSocket.Bind(rpcPoint);
                listenSocket.Listen(10);
            }
            catch (SocketException)
            {
                LogNotify?.Invoke($"Server error: Server cannot be started. Socket unavailable.");
                return;
            }
            catch (Exception)
            {
                LogNotify?.Invoke($"Server error: Server cannot be started.");
                return;
            }

            await Task.Run(() => Listen());
        }

        private bool CreateAddress(ref IPEndPoint rpcPoint)
        {
            IPAddress iPAddress;

            if (IPAddress.TryParse(RpcIPAddress, out iPAddress) == false)
            {
                LogNotify?.Invoke($"Server error: Server address cannot be created. Invalid IP.");
                return false;
            }

            try
            {
                rpcPoint = new IPEndPoint(iPAddress, RpcPort);
            }
            catch (ArgumentOutOfRangeException)
            {
                LogNotify?.Invoke($"Server error: Server address cannot be created. Invalid Port.");
                return false;
            }

            return true;
        }

        private void Listen()
        {
            LogNotify?.Invoke($"Server: Server running.");

            while (true)
            {
                LogNotify?.Invoke($"Server: Waiting for connection.");

                Socket connectedSocket = listenSocket.Accept();

                LogNotify?.Invoke($"Server: Connection accepted.");

                Task.Run(() => ConnectionHandler(connectedSocket, connectionId++));
            }
        }

        private void ConnectionHandler(Socket connectedSocket, int id)
        {
            LogNotify?.Invoke($"Server: Connection {id}: Waiting request.");

            while (connectedSocket.Available == 0) { }

            LogNotify?.Invoke($"Server: Connection {id}: Receiving request.");

            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[connectedSocket.Available];

            do
            {
                bytes = connectedSocket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (connectedSocket.Available > 0);

            LogNotify?.Invoke($"Server: Connection {id}: Request received: {builder.ToString()}");

            bool isNotification = false;
            string responce = RequestHandler.Handle(builder.ToString(), out isNotification);


            if (!isNotification)
            {
                LogNotify?.Invoke($"Server: Connection {id}: Sending responce.");

                data = new byte[responce.Length];
                data = Encoding.Unicode.GetBytes(responce);
                connectedSocket.Send(data);

                LogNotify?.Invoke($"Server: Connection {id}: Responce sent: {responce}");
            }
            else
            {
                responce = " ";

                data = new byte[responce.Length];
                data = Encoding.Unicode.GetBytes(responce);
                connectedSocket.Send(data);

                LogNotify?.Invoke($"Server: Notification accepted.");
            }

            LogNotify?.Invoke($"Server: Connection {id}: Closing connection.");

            connectedSocket.Shutdown(SocketShutdown.Both);
            connectedSocket.Close();

            LogNotify?.Invoke($"Client: Server: Connection {id}: Connection closed.");
        }
    }
}