#define INFO

using System.Runtime.CompilerServices;
using Butterfly;
using gameClient.manager.handler;

namespace server.component.clientManager.component
{
    public sealed class WorldManager : Controller
    {
        public const string NAME = "WorldManager:";

        public struct BUS
        {
            public struct Echo
            {
                public const string REGISTER_IN_ROOM = NAME + "RegisterInRoom.";
            }
        }

        private readonly Dictionary<string, Room> _rooms = new();

        void Construction()
        {
            listen_echo_2_0<string, ConnectData>(BUS.Echo.REGISTER_IN_ROOM)
                .output_to((name, connect, @return) =>
                {
#if INFO
                    SystemInformation($"Новый клиент подписался в комнату");
#endif
                    if (_rooms.TryGetValue(name, out Room room))
                    {
                        room.Subscribe(connect);

                        @return.To();
                    }
#if INFO
                    else
                    {
                        SystemInformation($"Комнаты с именем {name} не сущесвует.", ConsoleColor.Red);
                    }
#endif
                });
        }

        void Start()
        {
            _rooms.Add("room", obj<Room>("room"));
        }
    }
}

namespace server.component.clientManager.component
{
    public sealed class Room : RoomController
    {
        void Construction()
        {
            add_event(Header.Event.ROOM_1, Update);
            input_to(ref I_downMove, Header.Event.ROOM_1, DownMove);
        }
        
        void Configurate()
        {
            Define();
        }
    }

    public class RoomController : RoomInformation
    {
        protected IInput<int> I_downMove;

        private const int MAX_COUNT = 128;

        private readonly ConnectData[] _clients
            = new ConnectData[MAX_COUNT];

        private readonly int[] _positionX = new int[MAX_COUNT];
        private readonly int[] _positionY = new int[MAX_COUNT];
        private readonly bool[] _isLeft = new bool[MAX_COUNT];
        private readonly bool[] _isMove = new bool[MAX_COUNT];
        private readonly int[] _speed = new int[MAX_COUNT];
        private int _count = 0;

        private readonly List<byte[]> _packetsBuffer = new();

        public void Subscribe(ConnectData connect)
        {
            connect.Index = _count;
            _clients[_count++] = connect;

            connect.DownMove = I_downMove.To;
        }

        public void DownMove(int index)
        {
            _isMove[index] = true;
        }

        private DateTime d_localDateTime = DateTime.Now;

        protected void Update()
        {
            int delta = (DateTime.Now.Subtract(d_localDateTime).Seconds * 1000)
                + DateTime.Now.Subtract(d_localDateTime).Milliseconds;

            byte[] dateTime = GetStepDateTime();

            for (int i = 0; i < _count; i++)
            {
                if (_isMove[i])
                {
                    if (_isLeft[i])
                    {
                        int newPositionX = _positionX[i] = _positionX[i] +
                            delta * _speed[i];

                        _packetsBuffer.Add(_clients[i].
                            GetPositionX(newPositionX, dateTime));
                    }
                }
            }

            byte[][] capsules = _packetsBuffer.ToArray();
            foreach(ConnectData connectData in _clients)
            {
                connectData.AddCapsules(capsules);
            }
            _packetsBuffer.Clear();

            d_localDateTime = DateTime.Now;
        }
    }

    public abstract class RoomInformation : Controller.Board
    {
        TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

        public void Define()
        {
            try
            {
                DateTime easternTime = new DateTime();
                string easternZoneId = "Eastern Standard Time";

                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);

                moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            }
            catch (Exception ex)
            {
                SystemInformation(ex.ToString());

                destroy();
            }
        }

        public byte[] GetStepDateTime()
        {
            byte[] dateTime = new byte[9];

            DateTime moment = DateTime.UtcNow + moscowTimeZone.BaseUtcOffset;

            // Year gets 1999.
            int year = moment.Year;
            dateTime[PacketHandler.YEAR_INDEX_1b] = (byte)(year >> 8);
            dateTime[PacketHandler.YEAR_INDEX_2b] = (byte)year;

            // Month gets 1 (January).
            dateTime[PacketHandler.MOUNTH_INDEX] = (byte)moment.Month;

            // Day gets 13.
            dateTime[PacketHandler.DAY_INDEX] = (byte)moment.Day;

            // Hour gets 3.
            dateTime[PacketHandler.HOUR_INDEX] = (byte)moment.Hour;

            // Minute gets 57.
            dateTime[PacketHandler.MIN_INDEX] = (byte)moment.Minute;

            // Second gets 32.
            dateTime[PacketHandler.SEC_INDEX] = (byte)moment.Second;

            int millisecond = moment.Millisecond;
            dateTime[PacketHandler.MILL_INDEX_1b] = (byte)(millisecond >> 8);
            dateTime[PacketHandler.MILL_INDEX_2b] = (byte)millisecond;

            return dateTime;
        }
    }
}