using System.Security.Cryptography;
using System.Text.Json;

// 9 байт : 7 - год от 00 - 99, 
// Time : 5 - дата, 4 - месяц, часы - 5, 6 - минуты, 6 - секунды. 10 - милисекунды.

/*****HEADER****/
// totalLength 2
// type 1

// Капсулы.
// length message 1
// id message 
// data time 9                   
// position 8
// direction 1
// DATA
// type 1 Move
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
                    new EventSetting(Header.UDP_WORK_LISTEN_EVENT, 200),
                    new EventSetting(Header.RECEIVE_SSL_EVENT, 200),
                    new EventSetting(Header.SEND_UDP_MESSAGE_EVENT, 200),
                    new EventSetting(Header.SEND_SSL_MESSAGE_EVENT, 200),
                    new EventSetting(RoomsManager.ROOMS_MANAGER_WORK_EVENT, 200),

                    new EventSetting(Header.ROOM_UPDATE_1, 200, 1024, true, Thread.Priority.Normal),
                }
            });

            //RoomSettingJSON room = new RoomSettingJSON();
            //string json = JsonSerializer.Serialize(room);
            //Console.WriteLine(json);

            //StreamReader str = new StreamReader("/home/dmitry/MMOServer/Program/World/Rooms1.json");
            //RoomSettingJSON restoredPerson = JsonSerializer.Deserialize<RoomSettingJSON>(str.ReadToEnd());
            //Console.WriteLine(restoredPerson?.Name); // Tom

            /*
            DateTime easternTime = new DateTime();
            string easternZoneId = "Eastern Standard Time";
            try
            {
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
                Console.WriteLine("The date and time are {0} UTC.",
                                  TimeZoneInfo.ConvertTimeToUtc(easternTime, easternZone));
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("Unable to find the {0} zone in the registry.",
                                  easternZoneId);
            }
            catch (InvalidTimeZoneException)
            {
                Console.WriteLine("Registry data on the {0} zone has been corrupted.",
                                  easternZoneId);
            }
            */
            /*
            System.DateTime moment = new System.DateTime(
                                1999, 1, 13, 3, 57, 32, 11);
            // Year gets 1999.
            int year = moment.Year;

            // Month gets 1 (January).
            int month = moment.Month;

            // Day gets 13.
            int day = moment.Day;

            // Hour gets 3.
            int hour = moment.Hour;

            // Minute gets 57.
            int minute = moment.Minute;

            // Second gets 32.
            int second = moment.Second;

            // Millisecond gets 11.
            int millisecond = moment.Millisecond;

            TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime moscowTime = DateTime.UtcNow + moscowTimeZone.BaseUtcOffset;
            int milli = moscowTime.Millisecond;
            Console.WriteLine(moscowTime + " ." + milli);
            */
        }
    }
}