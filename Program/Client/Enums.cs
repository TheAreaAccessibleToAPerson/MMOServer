/// <summary>
/// Состояния в котором находится клиент.
/// </summary>
public enum StateType 
{
    None = 1,

    // Подписываемся на получение первого UDP пакета.
    SubscribeToReceiveFirstUDPPacket,

    // По SSL соединение запросим у клиента первый UDP пакет.
    RequestFirstUDPPacket,

    // Подписываемся на получение UDP пакетов.
    SubscribeToReceiveUDPPackets,
}

/// <summary>
/// Тип системного TCP сообщения.
/// </summary>
public enum RequestTCPType
{
    None = 1,

    // Запросить первый UDP пакет.
    FirstUDPPacket,
}

/// <summary>
/// Данный тип используется для того что бы определить мы подписываемся 
/// или отписываемся от прослушки первого UDP пакета.
/// </summary>
public enum ReceiveFirstUDPPacketType
{
    None = 1,

    Subscribe,

    Unsubscribe,
}
