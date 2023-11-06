#define INFO

namespace gameClient.manager.handler
{
    public abstract class Message : main.Object
    {
        public Message(string Explorer)
            : base (Explorer)
        { }

        #region Input

        /// <summary>
        /// Сообщения могут придти склеиные, нужно их разделить.
        /// </summary>
        protected byte[][] SplitTCPMessage(byte[] message, int size)
        {
            if (message.Length == 0)
                return new byte[0][];

            int messageLength = message.Length;

            int length = 0;
            int index = 0;

            int messagesIndex = 0;
            byte[][] messages = new byte[1][];
            do
            {
                SystemInformation("MessageLength:" + messageLength + ", index:" + index);
                length = GetTCPMessageLength(message, index);

#if INFO
                SystemInformation($"После проверки на слипание и коректность, длина сообщения равна:" +
                    $"{length}.", ConsoleColor.Green);
#endif

                // Если сообщение равно 0, то проигнорируем его.
                if (length > 0)
                {
                    if (message.Length == messagesIndex++)
                        Array.Resize(ref messages, messages.Length + 1);

                    messages[^1] = message[index..length];

                    index = length;
                }
                else
                {
#if INFO
                    SystemInformation($"Сообщение равно 0.", ConsoleColor.Red);
#endif
                    return new byte[0][];
                }
            }
            while ((size -= index) > 0);

#if EXCEPTION
        if (messageLength < 0)
            throw Exception(Ex.x002, message.Length, index, messageLength, ConsoleColor.Red);
#endif

            return messages;
        }

        /// <summary>
        /// Определяет размер пришедшего сообщение по протоколу TCP.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        private int GetTCPMessageLength(byte[] message, int startIndex)
        {
            if ((message.Length - startIndex) >= ssl.Header.LENGTH)
            {
                int i = message[startIndex + ssl.Header.DATA_LENGTH_INDEX_1byte] << 8 ^
                    (message[startIndex + ssl.Header.DATA_LENGTH_INDEX_2byte]);

                return i;
            }
#if INFO
            SystemInformation($"Индекс начала сообщения больше чем само сообщение " +
                $" index = {startIndex}, message length = {message.Length}", ConsoleColor.Red);
#endif
            return 0;
        }

        #endregion

        #region Output
        #endregion
    }
}