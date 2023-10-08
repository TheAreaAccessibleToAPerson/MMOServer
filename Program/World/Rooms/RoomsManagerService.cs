#define EXCEPTION
#define INFORMATION

using Butterfly;

/// <summary>
/// Как приблежась к центу прогружаются следующие комнаты.
/// </summary>
public abstract class RoomsManagerServices : Controller
{
    private readonly Dictionary<ulong, IInput<string, uint, byte[], Client.IReceiveRoomMessage>> _rooms
        = new Dictionary<ulong, IInput<string, uint, byte[], Client.IReceiveRoomMessage>>();

    /// <summary>
    /// Подписывает способ пердачи данных в комнату.
    /// </summary>
    /// <param name="roomKey">Данный ключ является позицией в пространсве x, y</param>
    /// <param name="roomInput"></param>
    protected void Subscribe(ulong roomKey, IInput<string, uint, byte[], Client.IReceiveRoomMessage> roomInput)
    {
        if (_rooms.ContainsKey(roomKey))
        {
#if EXCEPTION
            throw new Exception();
#endif
        }
        else
        {
#if INFORMATION
            SystemInformation($"Комната {roomKey} подписалась.", ConsoleColor.Green);
#endif
            _rooms.Add(roomKey, roomInput);
        }
    }

    protected void CreatingRoom(string roomName)
    {
    }

    protected void DestroyingRoom(string roomName)
    {
    }

    protected void SendMessageToRoom(ulong position, string clientName, uint clientID, byte[] message,
        Client.IReceiveRoomMessage receiveRoomMessage)
    {
        if (_rooms.TryGetValue(position, out IInput<string, uint, byte[], Client.IReceiveRoomMessage> room))
        {
            room.To(clientName, clientID, message, receiveRoomMessage);
        }
#if EXCEPTION
        else throw Exception(Ex.x01);
#endif
    }

    private struct Ex
    {
        public const string x01 = @"Клиент {0} пытается отправить сообщение в несущесвующюю комнату {1}.";
        public const string x02 = @"";
        public const string x03 = @"";
        public const string x04 = @"";
        public const string x05 = @"";
        public const string x06 = @"";
        public const string x08 = @"";
        public const string x09 = @"";
    }
}