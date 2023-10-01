namespace Butterfly
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            Butterfly.fly<Header>(new Butterfly.Settings()
            {
                Name = "Program",

                SystemEvent = new EventSetting(Header.WORK_WITCH_OBJECTS_EVENT, 50),

                EventsSetting = new EventSetting[]
                {
                    new EventSetting(Header.LISTEN_CLIENTS_EVENT, 200),
                    new EventSetting(Header.UDP_WORK_EVENT, 200),
                    new EventSetting(Header.RECEIVE_TCP_SOCKET_EVENT, 200),
                    new EventSetting(Header.SEND_TCP_SOCKET_EVENT, 200),
                    new EventSetting(Header.MESSAGE_PROCESSING_EVENT, 200)
                }
            });
        }
    }
}