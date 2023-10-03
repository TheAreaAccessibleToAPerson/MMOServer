using Butterfly;

public sealed class Header : Controller, ReadLine.IInformation
{
    public const string WORK_WITCH_OBJECTS_EVENT = "WorkWitchObjectsEvent.";
    public const string LISTEN_CLIENTS_EVENT = "ListenClientsEvent.";

    /// <summary>
    /// Отвечает за подписку и отписку клиентов от прослушки UDP пакетов
    /// и за прием из потока отвечающего за прослушку UDPSocket.
    /// </summary>
    public const string UDP_WORK_EVENT = "UdpWork.";

    /// <summary>
    /// Отвечает события на котрые подпишутся клиенты для прослушивания сообщений
    /// поступающих в TCPSocket.
    /// </summary>
    public const string RECEIVE_SSL_EVENT = "Receive ssl.";

    /// <summary>
    /// Отвечает за отправку сообщений по TCPSocket.
    /// </summary>
    public const string SEND_SSL_MESSAGE_EVENT = "Send ssl mesage.";

    /// <summary>
    /// Отвечает за отправку сообщений по UDPSocket.
    /// </summary>
    public const string SEND_UDP_MESSAGE_EVENT = "Send udp message.";

    void Construction()
    {
        SystemInformation("Starting program ...");

        listen_events(WORK_WITCH_OBJECTS_EVENT, WORK_WITCH_OBJECTS_EVENT);
        listen_events(LISTEN_CLIENTS_EVENT, LISTEN_CLIENTS_EVENT);
        listen_events(UDP_WORK_EVENT, UDP_WORK_EVENT);
        listen_events(SEND_SSL_MESSAGE_EVENT, SEND_SSL_MESSAGE_EVENT);
        listen_events(SEND_UDP_MESSAGE_EVENT, SEND_UDP_MESSAGE_EVENT);
    }

    void Start()
    {
        ReadLine.Start(this);

        SystemInformation("Start program.", ConsoleColor.Green);
    }

    //Запустить сервер.
    private const string START_SERVER = "start_server";

    private const string CREATING_CLIENT = "creating_client";

    public void Command(string command)
    {
        switch (command)
        {
            case START_SERVER:

                ConsoleLine("Введите имя cервера:");
                string serverName = System.Console.ReadLine();

                if (serverName == "")
                {
                    ConsoleLine($"Назначить имя сервера Server_{server_index}? Enter/n");
                    string isServerIndexName = System.Console.ReadLine();

                    if (isServerIndexName == "")
                    {
                        serverName = $"Server_{server_index++}";
                    }
                    else return;
                }

                if (try_obj(serverName, out Server serverObject))
                {
                    ConsoleLine($"Сервер с именем {serverName} уже сущесвует, желаете отключить его? Enter/n");
                    string serverIsDisable = System.Console.ReadLine();

                    if (serverIsDisable == "")
                    {
                        serverObject.destroy();
                    }
                    else return;
                }

                ConsoleLine("Введите адрес сервера:");
                string serverAddress = System.Console.ReadLine();

                if (serverAddress == "")
                {
                    ConsoleLine($"Выставить аддресс 127.0.0.1? Enter/n");
                    string isLocalhost = System.Console.ReadLine();

                    if (isLocalhost == "")
                    {
                        serverAddress = "127.0.0.1";
                    }
                    else return;
                }

                ConsoleLine("Введите порт сервера:");
                string serverPort = System.Console.ReadLine();

                if (serverPort == "")
                {
                    ConsoleLine($"Установить номер порта:{34134}? Enter/n");
                    string isServerPort = System.Console.ReadLine();

                    if (isServerPort == "")
                    {
                        serverPort = 34134.ToString();
                    }
                    else return;
                }

                obj<Server>(serverName, new string[]
                {
                    serverAddress,
                    serverPort
                });

                break;

            case CREATING_CLIENT:

                obj<Test.TestClient>("TEST_CLIENT", new string[]
                {
                    "127.0.0.1",
                    "34134",
                    "15300"
                });

                break;

            case h.DESTROY:

                destroy();

                break;

            default:
                ConsoleLine($"{START_SERVER} - запустить сервер.");
                ConsoleLine($"{CREATING_CLIENT} - создать клинта.");
                ConsoleLine($"{h.DESTROY} - уничтожить обьект.");
                break;
        }
    }

    void Stop()
    {
        SystemInformation("Stopping program ...");

        if (StateInformation.IsCallStart)
        {
            ReadLine.Stop(this);
        }
    }

    private int server_index = 1;
}