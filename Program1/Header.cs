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
        public const string PROCESSING_OF_RECEIVED_UDP_PACKETS = "ProcessingOfReceivedUDPPackets";
        public const string PROCESSING_OF_SENDING_UDP_PACKETS = "ProcessingOfSendingUDPPackets";
        public const string WORK_SSL = "ReceiveSSL";
        public const string WORK_BD = "BD";

        public const string ROOM_1 = "Room_1";
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
        listen_events(Event.PROCESSING_OF_RECEIVED_UDP_PACKETS, Event.PROCESSING_OF_RECEIVED_UDP_PACKETS);
        listen_events(Event.WORK_SSL, Event.WORK_SSL);
        listen_events(Event.ROOM_1, Event.ROOM_1);
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
            obj<Client>("Client111");
        }
    }
}
