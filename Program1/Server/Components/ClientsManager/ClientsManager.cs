using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace server.component
{
    public sealed class ClientsManager : Controller
    {
        public const string NAME = "ClientsManager:Object:";

        public struct BUS
        {
            public struct Message
            {
                public const string ADD_CLIENT = NAME + "AddClient";
            }
        }

        void Construction()
        {
            listen_message<TcpClient>(BUS.Message.ADD_CLIENT)
                .output_to((newClient) =>
                {
                    string name = $"{((IPEndPoint)newClient.Client.RemoteEndPoint).Address}" + 
                    $"{ClientsListen._}{((IPEndPoint)newClient.Client.RemoteEndPoint).Port}";

                    if (try_obj(name, out clientManager.component.ClientShell client))
                        client.destroy();

                    obj<clientManager.component.ClientShell>(name, newClient);
                },
                Header1.Event.WORK_OBJECT);
        }
    }
}