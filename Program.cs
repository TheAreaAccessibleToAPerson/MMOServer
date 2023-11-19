namespace Butterfly
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            Butterfly.fly<Header>(new Butterfly.Settings()
            {
                Name = "Program",

                SystemEvent = new EventSetting(Header.Event.WORK_OBJECT, 50),

                EventsSetting = new EventSetting[]
                {
                    new EventSetting(Header.Event.PROCESSING_OF_RECEIVED_UDP_PACKETS, 200),
                    new EventSetting(Header.Event.WORK_SSL, 200),
                    new EventSetting(Header.Event.ROOM_1, 10)
                }
            });
        }
    }
}