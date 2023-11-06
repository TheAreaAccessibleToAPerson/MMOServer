﻿using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using Butterfly.system.objects.main;

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
            Butterfly.fly<Header1>(new Butterfly.Settings()
            {
                Name = "Program",

                SystemEvent = new EventSetting(Header1.Event.WORK_OBJECT, 50),

                EventsSetting = new EventSetting[]
                {
                    new EventSetting(Header1.Event.WORK_UDP_PACKETS, 200),
                    new EventSetting(Header1.Event.WORK_SSL, 200),
                    /*
                    new EventSetting(Header.LISTEN_CLIENTS_EVENT, 200),
                    new EventSetting(Header.UDP_WORK_LISTEN_EVENT, 200),
                    new EventSetting(Header.RECEIVE_SSL_EVENT, 200),
                    new EventSetting(Header.SEND_UDP_MESSAGE_EVENT, 200),
                    new EventSetting(Header.SEND_SSL_MESSAGE_EVENT, 200),
                    new EventSetting(RoomsManager.ROOMS_MANAGER_WORK_EVENT, 200),
                    new EventSetting(RoomsManager.ROOMS_WORKS_1, 200),

                    new EventSetting(Header.ROOM_UPDATE_1, 200, 1024, true, Thread.Priority.Normal),
                    */
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
            /*
            StreamReader str = new StreamReader("/home/dmitry/MMOServer/file.txt");

            List<char[]> bufferList = new List<char[]>();
            char[] buffer = new char[1024]; int index = 0;

            foreach (char c in str.ReadToEnd())
            {
                buffer[index++] = c;

                if (c == '&') break;
                if (c == '\n') 
                {
                    bufferList.Add(buffer[0 .. index]);

                    buffer = new char[1024]; index = 0;
                }
            }

            foreach(char[] m in bufferList)
            {
                foreach(char c in m)
                {
                    Console.Write(c);
                }

                //Console.WriteLine();
            }
            */
        }
    }
}