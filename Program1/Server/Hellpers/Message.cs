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


namespace ssl
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
                /// Если логин и пороль успешно прошли проверку, то клиент 
                /// создаст новое TCP соединение.
                /// 1)1 байт - результат.
                /// 2)4 байта - если результат удвалитварительный, то эти данные 
                ///     будут хранить ID под которму сервер прослушивает первый UDP пакет.
                ///     иначе эти поля будут пустые.
                /// </summary>
                public struct Step1
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
                        public const int SUCCESS = 1;
                        public const int LOGIN_ERROR = 2;
                        public const int PASSWORD_ERROR = 3;
                    }
                }

                /// <summary>
                /// Заправшивается первый UDP пакет.
                /// </summary>
                public struct Step2 
                {
                    public const int LENGTH = Header.LENGTH;
                    public const int TYPE = (int)Data.Type.ServerToClientConnectionStep2;
                }

                /// <summary>
                /// Сообщаем о том что пакет был доставлен.
                /// </summary>
                public struct Step3
                {
                    public const int LENGTH = Header.LENGTH;
                    public const int TYPE = (int)Data.Type.ServerToClientConnectionStep3;
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
                    public const int LENGTH = Header.LENGTH + LOGIN_LENGTH + PASSWORD_LENGTH;

                    public const int TYPE = (int)Data.Type.ClientToServerConnectionStep1;

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
            ServerToClientConnectionStep1 = 4,
            ServerToClientConnectionStep2 = 8,
            ServerToClientConnectionStep3 = 16,

            /// <summary>
            /// Принимает логин и пароль от клинта.
            /// </summary>
            ClientToServerConnectionStep1 = 32,

            /// <summary>
            /// Проверяем на стороне сервера подключeн ли клиент.
            /// </summary>
            CheckConnectionServerToClient = 64,
        }

    }
}

namespace udp
{
    public struct Header
    {
        /// <summary>
        /// Длина заголовка.
        /// </summary>
        public const int LENGTH = 3;

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
        public const int DATA_TYPE_INDEX = 2;
    }

    public struct Data
    {
        public struct ServerToClient
        {
            public struct Connection
            {
                public struct Step
                {
                    public const int TYPE = (int)Data.Type.ServerToClientConnectionStep1;
                    public const int LENGTH = Header.LENGTH + 4;

                    public const int RECEIVE_ID_1byte = Header.LENGTH;
                    public const int RECEIVE_ID_2byte = RECEIVE_ID_1byte + 1;
                    public const int RECEIVE_ID_3byte = RECEIVE_ID_2byte + 1;
                    public const int RECEIVE_ID_4byte = RECEIVE_ID_3byte + 1;
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
            ClientToServerConnectionStep1 = 4,
            ServerToClientConnectionStep1 = 8,

            Message = 16,

            /// Прибытие пакета с данными.
            Arrival = 32,

            // Прибытие подтвержения что отправленый пакет был доставлен.
            Acknoledgment = 64,
        }
    }

}

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
public struct Capsule
{
    public struct Header
    {
        public const int LENGTH = 6;
        public const int LENGTH_INDEX = 0;

        /// <summary>
        /// Тип капсулы:
        /// ACK - 0 - подтверждение получения сообщения.(получатель -> отправитель)
        /// FIN - 1 - получение подтверждения получения сообщения(отправитель -> получатель)
        /// PUSH - >2 - игровое сообщение: передвежение.
        /// </summary>
        public const int TYPE_INDEX = LENGTH_INDEX + 1;

        public const int ID_INDEX_1byte = TYPE_INDEX + 1;
        public const int ID_INDEX_2byte = ID_INDEX_1byte + 1;
        public const int ID_INDEX_3byte = ID_INDEX_2byte + 1;
        public const int ID_INDEX_4byte = ID_INDEX_3byte + 1;

        /// <summary>
        /// Подтверждение получения PSH сообщения.
        /// </summary>
        public struct ACK
        {
            public const string NAME = "ACK";

            public const int TYPE = 0;

            public const int LENGTH = Header.LENGTH + 0;

            /// <summary>
            /// ID отправленого push сообщения.
            /// </summary>
            public const int ACKNOLEDGMENT_MESSAGE_ID_1byte = TYPE_INDEX + 1;
            /// <summary>
            /// ID отправленого push сообщения.
            /// </summary>
            public const int ACKNOLEDGMENT_MESSAGE_ID_2byte = ACKNOLEDGMENT_MESSAGE_ID_1byte + 1;
            /// <summary>
            /// ID отправленого push сообщения.
            /// </summary>
            public const int ACKNOLEDGMENT_MESSAGE_ID_3byte = ACKNOLEDGMENT_MESSAGE_ID_2byte + 1;
            /// <summary>
            /// ID отправленого push сообщения.
            /// </summary>
            public const int ACKNOLEDGMENT_MESSAGE_ID_4byte = ACKNOLEDGMENT_MESSAGE_ID_3byte + 1;
        }

        public struct FIN
        {
            public const string NAME = "FIN";

            public const int TYPE = 1;

            public const int LENGTH = Header.LENGTH + 0;

            /// <summary>
            /// ID отправленого ack сообщения.
            /// </summary>
            public const int FIN_ACKNOLEDGMENT_ID_1byte = TYPE_INDEX + 1;
            /// <summary>
            /// ID отправленого ack сообщения.
            /// </summary>
            public const int FIN_ACKNOLEDGMENT_ID_2byte = FIN_ACKNOLEDGMENT_ID_1byte;
            /// <summary>
            /// ID отправленого ack сообщения.
            /// </summary>
            public const int FIN_ACKNOLEDGMENT_ID_3byte = FIN_ACKNOLEDGMENT_ID_2byte;
            /// <summary>
            /// ID отправленого ack сообщения.
            /// </summary>
            public const int FIN_ACKNOLEDGMENT_ID_4byte = FIN_ACKNOLEDGMENT_ID_3byte;
        }

        public struct PSH
        {
            public const string NAME = "PSH";

            /// <summary>
            /// Даный тип указывает что данная капсула сообщает об передвижении.
            /// </summary>
            public const int MOVE_TYPE = 1;

            public const int LENGTH = Header.LENGTH + 19;

            public const int DATA_AGE_INDEX = ID_INDEX_4byte + 1;
            public const int DATA_MOUNTH_INDEX = DATA_AGE_INDEX + 1;
            public const int DATA_DAY_INDEX = DATA_MOUNTH_INDEX + 1;
            public const int TIME_HOUR_INDEX = DATA_DAY_INDEX + 1;
            public const int TIME_MIN_INDEX = TIME_HOUR_INDEX + 1;
            public const int TIME_SEC_INDEX = TIME_MIN_INDEX + 1;
            public const int TIME_MILL_INDEX_1byte = TIME_SEC_INDEX + 1;
            public const int TIME_MILL_INDEX_2byte = TIME_MILL_INDEX_1byte + 1;

            public const int POSITION_X_INDEX_1byte = TIME_MILL_INDEX_2byte + 1;
            public const int POSITION_X_INDEX_2byte = POSITION_X_INDEX_1byte + 1;
            public const int POSITION_X_INDEX_3byte = POSITION_X_INDEX_2byte + 1;
            public const int POSITION_X_INDEX_4byte = POSITION_X_INDEX_3byte + 1;

            public const int POSITION_Y_INDEX_1byte = POSITION_X_INDEX_4byte + 1;
            public const int POSITION_Y_INDEX_2byte = POSITION_Y_INDEX_1byte + 1;
            public const int POSITION_Y_INDEX_3byte = POSITION_Y_INDEX_2byte + 1;
            public const int POSITION_Y_INDEX_4byte = POSITION_Y_INDEX_3byte + 1;

            /// <summary>
            /// 0 - LEFT, 1 - RIGHT
            /// </summary>
            public const int DIRECTIONG_INDEX = POSITION_Y_INDEX_4byte + 1;
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


