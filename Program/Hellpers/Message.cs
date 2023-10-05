/*
   HEADER MESSAGE 2 byte
----------------------
message_type, message_length
[11 - type 111111, 11111111 - length]
1)[256, 256, 
----------------------
MESSAGE DATA 9 byte
----------------------
     id client
2) 256, 256, 256, 256]
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

using System.ComponentModel.DataAnnotations;

/*
public struct UDPHeader
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
    /// Тип сообщения.
    /// </summary>
    public const int TYPE_INDEX = 0;

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
*/


namespace SSL
{
    /// <summary>
    /// Заголовок 4 байта
    /// 1) 2 байта - длина сообщения.
    /// 2) 2 байта - тип сообщения.
    /// </summary>
    public struct Header
    {
        /// <summary>
        /// Длина заголовка.
        /// </summary>
        public const int LENGTH = 4;

        /// <summary>
        /// Индекс который указывает на первый байт хранящий длину всего сообщения.
        /// </summary>
        public const int DATA_LENGTH_INDEX_1byte = 0;
        /// <summary>
        /// Индекс который указывает на второй байт хранящий длину всего сообщения.
        /// </summary>
        public const int DATA_LENGTH_INDEX_2byte = 1;
        /// <summary>
        /// Индекс который указывает на первый байт хранящий тип сообщения.
        /// </summary>
        public const int DATA_TYPE_INDEX_1byte = 2;
        /// <summary>
        /// Индекс который указывает на второй байт хранящий тип сообщения.
        /// </summary>
        public const int DATA_TYPE_INDEX_2byte = 3;
    }

    public struct Data
    {
        public struct ServerToClient
        {
            public struct Connection
            {
                /// <summary>
                /// 1)1 байт - результат.
                /// 2)4 байта - если результат удвалитварительный, то эти данные 
                ///     будут хранить ID под которму сервер прослушивает первый UDP пакет.
                ///     иначе эти поля будут пустые.
                /// </summary>
                public struct Step
                {
                    public const int TYPE = (int)Data.Type.ServerToClientConnectionStep1;
                    public const int LENGTH = Header.LENGTH + 5;

                    public const int RESULT_INDEX = Header.LENGTH;

                    public const int RECEIVE_ID_INDEX_1byte = RESULT_INDEX + 1;
                    public const int RECEIVE_ID_INDEX_2byte = RECEIVE_ID_INDEX_1byte + 1;
                    public const int RECEIVE_ID_INDEX_3byte = RECEIVE_ID_INDEX_2byte + 1;
                    public const int RECEIVE_ID_INDEX_4byte = RECEIVE_ID_INDEX_3byte + 1;


                    /// <summary>
                    /// Результат авторизации.
                    /// </summary>
                    public struct Result
                    {
                        public const int ACCESS = 1;
                        public const int LOGIN_ERROR = 2;
                        public const int PASSWORD_ERROR = 3;

                    }
                }
            }

            public struct Message
            {
            }

            public struct Disconnection
            {
            }
        }

        public struct ClientToServer
        {
            public struct Connection
            {
                /// <summary>
                /// Первый шаг:Клиент устанавливает соединение.
                /// 1) 16 - логин.
                /// 2) 16 - пароль.
                /// </summary>
                public struct Step
                {
                    public const int TYPE = (int)Data.Type.ClientToServerConnectionStep1;
                    public const int LENGTH = Header.LENGTH + LOGIN_LENGTH + PASSWORD_LENGTH;

                    public const int LOGIN_START_INDEX = Header.LENGTH;
                    public const int LOGIN_LENGTH = 16;

                    public const int PASSWORD_START_INDEX = LOGIN_START_INDEX + LOGIN_LENGTH;
                    public const int PASSWORD_LENGTH = 16;
                }
            }

            public struct Message
            {
            }

            public struct Disconnection
            {
            }
        }
        public enum Type
        {
            ServerToClientConnectionStep1,

            ClientToServerConnectionStep1,
        }

    }
}

namespace UDP
{
    public struct Header
    {
        /// <summary>
        /// Длина заголовка.
        /// </summary>
        public const int LENGTH = 4;

        /// <summary>
        /// Максимальная длина.
        /// </summary>
        public const int MAX_LENGTH = 512;

        /// <summary>
        /// Индекс который указывает на первый байт хранящий длину всего сообщения.
        /// </summary>
        public const int DATA_LENGTH_INDEX_1byte = 0;
        /// <summary>
        /// Индекс который указывает на второй байт хранящий длину всего сообщения.
        /// </summary>
        public const int DATA_LENGTH_INDEX_2byte = 1;
        /// <summary>
        /// Индекс который указывает на первый байт хранящий тип сообщения.
        /// </summary>
        public const int DATA_TYPE_INDEX_1byte = 2;
        /// <summary>
        /// Индекс который указывает на второй байт хранящий тип сообщения.
        /// </summary>
        public const int DATA_TYPE_INDEX_2byte = 3;
    }

    public struct Data
    {
        public struct ServerToClient
        {
            public struct Connection
            {
                public struct Step
                {
                }
            }

            public struct Message
            {
            }

            public struct Disconnection
            {
            }
        }
        public struct ClientToServer
        {
            public struct Connection
            {
                /// <summary>
                /// Начинает высылать UDP пакеты пока по SSL.Connection.Step2 не будет получено
                /// сообщение что пакет был доставлен.
                /// </summary>
                public struct Step
                {
                    public const int TYPE = (int)Data.Type.ClientToServerConnectionStep1;
                    public const int LENGTH = Header.LENGTH + 4;

                    public const int RECEIVE_ID_1byte = Header.LENGTH;
                    public const int RECEIVE_ID_2byte = RECEIVE_ID_1byte + 1;
                    public const int RECEIVE_ID_3byte = RECEIVE_ID_2byte + 1;
                    public const int RECEIVE_ID_4byte = RECEIVE_ID_3byte + 1;
                }
            }

            public struct Message
            {
               public const int TYPE = (int)Type.Message;
            }

            public struct Disconnection
            {
            }
        }

        public enum Type
        {
            ClientToServerConnectionStep1,

            Message,
        }
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


