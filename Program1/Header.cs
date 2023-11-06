using Butterfly;

public class Header : Controller, ReadLine.IInformation
{
    public struct Event
    {
        /// <summary>
        /// Данное событие обрабатывает:
        /// 1) Все системные вызовы
        /// 2) Работу с связаную с системными обьектами.
        /// </summary>
        public const string WORK_OBJECT = "WorkObject";

        /// <summary>
        /// Событие отвечающее за работу и обработку получаемых UDP пакетов.
        /// </summary>
        public const string WORK_UDP_PACKETS = "WorkUDPPackets";

        public const string WORK_SSL = "ReceiveSSL";

        public const string WORK_BD = "BD";
    }

    private readonly Logger _logger = new();

    void Construction()
    {
        listen_message<string>(Logger.BUS.Message.SERVER_COMPONENTS)
            .output_to(_logger.add_server_component_info);

        listen_message<string>(Logger.BUS.Message.CLIENT_SHELL_COMPONENT)
            .output_to(_logger.add_client_shell_component_info);

        listen_message<string>(Logger.BUS.Message.CLIENT_OBJECT_COMPONENT)
            .output_to(_logger.add_client_object_component_info);

        listen_events(Event.WORK_OBJECT, Event.WORK_OBJECT);
        listen_events(Event.WORK_UDP_PACKETS, Event.WORK_UDP_PACKETS);
        listen_events(Event.WORK_SSL, Event.WORK_SSL);
    }

    public const string ADDRESS = "127.0.0.1";
    public const int SSL_PORT = 34134;
    public const int TCP_PORT = 40000;
    public const int UDP_PORT = 15300;

    void Start()
    {
        ReadLine.Start(this);

        obj<server.Server>("Server",
            new server.Settings("127.0.0.1", SSL_PORT, 100, "127.0.0.1", TCP_PORT, 100,
                "127.0.0.1", UDP_PORT, 100));
    }

    void ReadLine.IInformation.Command(string command)
    {
        if (command == "c")
        {
            obj<Client1>("Client111");
        }
    }
}

public class Logger
{
    public struct BUS
    {
        public struct Message
        {
            /// <summary>
            /// Логирование серверных компонентов, жизненые процессы.
            /// </summary>
            public const string SERVER_COMPONENTS = "Logger:ServerComponents.";

            public const string CLIENT_SHELL_COMPONENT = "Logger:ClientShellComponent";
            public const string CLIENT_OBJECT_COMPONENT = "Logger:ClientObjectComponent";
        }
    }

    public void add_server_component_info(string info)
    {
        Console.WriteLine(info);
    }

    public void add_client_shell_component_info(string info)
    {
        Console.WriteLine(info);
    }

    public void add_client_object_component_info(string info)
    {
        Console.WriteLine(info);
    }
}
