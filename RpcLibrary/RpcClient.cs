using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RpcLibrary
{
    public class RpcClient
    {
        public int RpcServerPort { get; set; } = 8006; // server's port by default
        public string RpcServerIPAddress { get; set; } = "127.0.0.2"; // server's localhost by default

        public delegate void Log(string message);
        public event Log LogNotify;

        private int connectionId = 0;

        public async Task<string> SendRequestAsync(string jsonRpcRequest)
        {
            connectionId++;
            return await Task.Run(() => SendRequest(jsonRpcRequest, connectionId));
        }

        private string SendRequest(string jsonRpcRequest, int id)
        {
            LogNotify?.Invoke($"Client: Connection {id}: Start connection.");

            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (ConnectToServer(sendSocket, id) == false)
            {
                LogNotify?.Invoke($"Client error: Connection {id}: No connection  with server.");
                return null;
            }

            LogNotify?.Invoke($"Client: Connection {id}: Server connection established.\nClient: Connection {id}: Sending request: {jsonRpcRequest}");

            byte[] data = Encoding.Unicode.GetBytes(jsonRpcRequest);
            sendSocket.Send(data);

            LogNotify?.Invoke($"Client: Connection {id}: Request sent.\nClient: Connection {id}: Waiting responce.");

            while (sendSocket.Available == 0) { }


            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            data = new byte[sendSocket.Available];

            do
            {
                bytes = sendSocket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (sendSocket.Available > 0);

            LogNotify?.Invoke($"Client: Connection {id}: Responce accepted: {builder.ToString()}\nClient: Connection {id}: Closing connection.");

            sendSocket.Shutdown(SocketShutdown.Both);
            sendSocket.Close();

            LogNotify?.Invoke($"Client: Connection {id}: Connection  closed.");

            return builder.ToString();
        }

        private bool ConnectToServer(Socket sendSocket, int id)
        {
            IPEndPoint rpcServerPoint = null;

            if (CreateAddress(ref rpcServerPoint, id) == false)
            {
                LogNotify?.Invoke($"Client error: Connection {id}: Cannot connect to server. Invalid server address.");
                return false;
            }

            LogNotify?.Invoke($"Client: Connection {id}: Server address created.");

            try
            {
                sendSocket.Connect(rpcServerPoint);
            }
            catch (SocketException)
            {
                LogNotify?.Invoke($"Client error: Connection {id}: Cannot connect to server. Socket unavailable.");
                return false;
            }
            catch (Exception)
            {
                LogNotify?.Invoke($"Client error: Connection {id}: Cannot connect to server.");
                return false;
            }

            return true;
        }

        private bool CreateAddress(ref IPEndPoint rpcServerPoint, int id)
        {
            IPAddress iPAddress;

            if (IPAddress.TryParse(RpcServerIPAddress, out iPAddress) == false)
            {
                LogNotify?.Invoke($"Client error: Connection {id}: Server address cannot be created. Invalid IP.");
                return false;
            }

            try
            {
                rpcServerPoint = new IPEndPoint(iPAddress, RpcServerPort);
            }
            catch (ArgumentOutOfRangeException)
            {
                LogNotify?.Invoke($"Client error: Connection {id}: Server address cannot be created. Invalid Port.");
                return false;
            }

            return true;
        }
    }
}