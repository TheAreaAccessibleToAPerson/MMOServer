using System.Net;
using System.Net.Sockets;
using Butterfly;

public sealed class ClientsManager : Controller
{
    void Construction()
    {
        obj<World.Main>("World");
        obj<ReceiveUDPPacketForClients>("ReceiveUDPPacketForClients", new string[]
            {"127.0.0.1", "15300"});

        /*
            Получаем нового подключившегося клиeнта и создаем для него обьект.
        */
        listen_message<TcpClient>(BUS.LM_CREATING_CLIENT)
            .output_to((client) =>
            {
                string name = $"{((IPEndPoint)client.Client.RemoteEndPoint).Address}{ClientsListen._}" +
                    $"{((IPEndPoint)client.Client.RemoteEndPoint).Port}";

                if (try_obj(name, out Client createClient))
                    createClient.destroy();

                obj<Client>(name, client);
            },
            Header.WORK_WITCH_OBJECTS_EVENT);
    }

    public struct BUS
    {
        /* 
            Создает нового клинта.
            listem_message<in TcpClient>
        */
        public const string LM_CREATING_CLIENT = "Creating client";
    }
}