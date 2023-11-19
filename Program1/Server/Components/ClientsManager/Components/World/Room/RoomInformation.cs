using Butterfly;

namespace server.component.clientManager.component
{
    public abstract class RoomInformation : Controller.Board.LocalField<RoomSettings>
    {
        protected IInput<int> I_downMove;

        private const int MAX_COUNT = 128;

        protected readonly ConnectData[] _clients
            = new ConnectData[MAX_COUNT];

        protected readonly int[] _positionX = new int[MAX_COUNT];
        protected readonly int[] _positionY = new int[MAX_COUNT];
        protected readonly bool[] _isLeft = new bool[MAX_COUNT];
        protected readonly bool[] _isMove = new bool[MAX_COUNT];
        protected readonly int[] _speed = new int[MAX_COUNT];
        protected int _count = 0;

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