/*
   HEADER MESSAGE 6 byte
----------------------
   message_length
1)[256, 256, 
     id client
2) 256, 256, 256, 256]
----------------------
MESSAGE DATA 5 byte
----------------------
   capsule count
3)[256
   id message
4)256, 256, 256, 256]
----------------------
   CAPSUL HEADER
----------------------
   capsul length
5)[256
   capsul type
6) 
[
   100 - position
]
7) DATA ...

*/

using System.Net.Http.Headers;

public struct UDPMessage
{
    /// <summary>
    /// Максимально возможная длина сообщения.
    /// </summary>
    public const int MAX_LENGTH = 512;

    /// <summary>
    /// Минимально возможная длина сообщения.
    /// В этих 11 байтах содержаться данные:
    /// 1)2байта - длина сообщения.
    /// 2)4байта - id клиента.
    /// 3)1байт - минимально возможно количесво капсул.(при создании сообщения к нему
    /// прикрепляются капсулы ради которых это сообщение и создавалось, далее пока сообщение
    /// ждет подтверждение, и совершает повторные отправки, к нему прикрепляются новые капсулы.
    /// У новых капсул отсутсвует HeaderMessage(длина сообщения и id сообщения))
    /// 4)4 байта - id сообщения.
    /// </summary>
    public const int MIN_LENGTH = 11;

    /// <summary>
    /// Приходящее сообщение обезательно содержит заголовок сообщения.
    /// В первый двух байтах указывается длина данного сообщения.
    /// Данная констранда содержит индекс первого байта.
    /// </summary>
    public const int LENGTH_INDEX_1byte = 0;

    /// <summary>
    /// Приходящее сообщение обезательно содержит заголовок сообщения.
    /// В первый двух байтах указывается длина данного сообщения.
    /// Данная констранда содержит индекс второго байта.
    /// </summary>
    public const int LENGTH_INDEX_2byte = 1;

    /// <summary>
    /// Сообщения приходят определенному клинту.
    /// Какому иммено определяется по id.
    /// ID клинта может быть от 0 до 4 294 967 295, тоесть 4 байта.
    /// Данный индекс указывает на первый байт.
    /// </summary>
    public const int ID_CLIENT_1byte = 2;

    /// <summary>
    /// Сообщения приходят определенному клинту.
    /// Какому иммено определяется по id.
    /// ID клинта может быть от 0 до 4 294 967 295, тоесть 4 байта.
    /// Данный индекс указывает на второй байт.
    /// </summary>
    public const int ID_CLIENT_2byte = 3;

    /// <summary>
    /// Сообщения приходят определенному клинту.
    /// Какому иммено определяется по id.
    /// ID клинта может быть от 0 до 4 294 967 295, тоесть 4 байта.
    /// Данный индекс указывает на третий байт.
    /// </summary>
    public const int ID_CLIENT_3byte = 4;

    /// <summary>
    /// Сообщения приходят определенному клинту.
    /// Какому иммено определяется по id.
    /// ID клинта может быть от 0 до 4 294 967 295, тоесть 4 байта.
    /// Данный индекс указывает на четвертый байт.
    /// </summary>
    public const int ID_CLIENT_4byte = 5;

    public struct Capsule
    {
        /// <summary>
        /// Капсула может придти одна, но всегда подразумеваеться что это состав капсул.
        /// Данный индекс указывает на количесво пришедших капсул.
        /// Максимально возможный размер капсул 255.
        /// Данный индекс указывает на это значение.
        /// </summary>
        public const int COUNT_INDEX = 6;

        /// <summary>
        /// Для состава капсул определяется уникальный ID.
        /// В подтверждение нужно высылать данный ID.
        /// ID сообщение может быть от 0 до 4 294 967 295, тоесть 4 байта.
        /// Данный индекс указывает на первый байт.
        /// </summary>
        public const int ID_MESSAGE_INDEX_1byte = 7;

        /// <summary>
        /// Для состава капсул определяется уникальный ID.
        /// В подтверждение нужно высылать данный ID.
        /// ID сообщение может быть от 0 до 4 294 967 295, тоесть 4 байта.
        /// Данный индекс указывает на второй байт.
        /// </summary>
        public const int ID_MESSAGE_INDEX_2byte = 8;

        /// <summary>
        /// Для состава капсул определяется уникальный ID.
        /// В подтверждение нужно высылать данный ID.
        /// ID сообщение может быть от 0 до 4 294 967 295, тоесть 4 байта.
        /// Данный индекс указывает на третий байт.
        /// </summary>
        public const int ID_MESSAGE_INDEX_3byte = 9;

        /// <summary>
        /// Для состава капсул определяется уникальный ID.
        /// В подтверждение нужно высылать данный ID.
        /// ID сообщение может быть от 0 до 4 294 967 295, тоесть 4 байта.
        /// Данный индекс указывает на четвертый байт.
        /// </summary>
        public const int ID_MESSAGE_INDEX_4byte = 10;
    }
}

public struct TCPMessage
{
    /// <summary>
    /// Количесво байтов в которых содержиться размер  TCP сообщения.
    /// </summary>
    public const int LENGTH_BYTE_COUNT = 2;

    /// <summary>
    /// Приходящее сообщение по протоколу TCP обязательно имеет заголовок.
    /// Первые два байта указывают размер сообщения.
    /// Указывает индекс на первый байт.
    /// </summary>
    public const int LENGTH_INDEX_1byte = 0;

    /// <summary>
    /// Приходящее сообщение по протоколу TCP обязательно имеет заголовок.
    /// Первые два байта указывают размер сообщения.
    /// Указывает индекс на второй байт.
    /// </summary>
    public const int LENGTH_INDEX_2byte = 1;

    /// <summary>
    /// Указывает на тип сообщения.
    /// </summary>
    public const int TYPE_INDEX = 2;
}

public struct ServiceTCPMessage
{
    /// <summary>
    /// Минимально возможный размер сообщения который может придти по протоколу TCP.
    /// </summary>
    public const int MIN_LENGTH = 3;

    /// <summary>
    /// Размер пакета от клинта в котором приходит порт.
    /// </summary>
    public const int TRANSFER_PORT_LENGTH = 3;

    /// <summary>
    /// Индекс на первый байт в сообщение которое содержит порт клинта 
    /// с которого будут приходить UDP пакеты.
    /// </summary>
    public const int TRANSFER_PORT_INDEX_1byte = 1;
    /// <summary>
    /// Индекс на второй байт в сообщение которое содержит порт клинта 
    /// с которого будут приходить UDP пакеты.
    /// </summary>
    public const int TRANSFER_PORT_INDEX_2byte = 2;

    public struct ServerToClient
    {
        /// <summary>
        /// Превышено время ожидания.
        /// </summary>
        public const int TIME_OUT = 1;

        /// <summary>
        /// Клиет отключeн.
        /// </summary>
        public const int CLIENT_DISCONNECTING = 2;

        /// <summary>
        /// Запрашивает у клинта номер порта по которому 
        /// тот будет передавать UDP пакеты.
        /// </summary>
        public const int REQUEST_UDP_PORT = 3;
    }

    public struct ClientToServer
    {
        /// <summary>
        /// Передаем сообщения в котором укажим номер порта
        /// с которого будут передоваться UDP пакеты.
        /// </summary>
        public const int TRANSFER_PORT = 1;
    }
}

public struct ServiceUDPMessage
{
    public struct ClientToServer
    {
        /// <summary>
        /// Передаем сообщения
        /// </summary>
        public const int TRANSFER_PORT = 1;
    }
}

public static class Message
{
    public static string Show(string name, byte[] message, int lengthLine)
    {
        string m = $"\n************{name}*************\n";

        int t = 0;
        for (int i = 0; i < message.Length; i++)
        {
            if (t++ == lengthLine)
            { t = 0; m += "\n"; }

            m += $"{message[i]} ";
        }

        m += "\n************************************";

        return m;
    }
}


