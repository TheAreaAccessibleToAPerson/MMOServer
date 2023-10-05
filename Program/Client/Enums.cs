/// <summary>
/// Состояния в котором находится клиент.
/// </summary>
public enum StateType
{
    None = 4,

    // Подписываемся на получение первого UDP пакета.
    SubscribeToReceiveFirstUDPPacket = 8,

    // По SSL соединение запросим у клиента первый UDP пакет.
    RequestFirstUDPPacket = 16,

    // Подписываемся на получение UDP пакетов.
    SubscribeToReceiveUDPPackets = 32
}
