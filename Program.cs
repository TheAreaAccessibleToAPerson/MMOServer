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
                    new EventSetting(Header.RECEIVE_SSL_EVENT, 200),
                    new EventSetting(Header.SEND_UDP_MESSAGE_EVENT, 200),
                    new EventSetting(Header.SEND_SSL_MESSAGE_EVENT, 200),
                }
            });
        }
    }
}