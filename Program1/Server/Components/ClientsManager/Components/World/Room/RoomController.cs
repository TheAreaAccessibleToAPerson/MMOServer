using Butterfly;

namespace server.component.clientManager.component
{
    public struct II 
    {
        public readonly ConnectData ConnectData;
        public readonly IReturn @EndSubscribeReturn;

        public II(ConnectData connectData, IReturn @endSubscribe)
        {
            ConnectData = connectData;
            @EndSubscribeReturn = @endSubscribe;
        }
    }

    public class RoomController : RoomInformation
    {
        private readonly List<byte[]> _packetsBuffer = new();

        protected IInput<ConnectData> I_subscribe;

        // Подписывает нового клиента.
        // Если подписка производится во время создания
        // обьекта, просто запишим данные клиeнта.
        // Если во время работы обьекта, то запустим
        // подписку во внутремен потокe.
        public void Subscribe(ConnectData connect)
        {
            if (StateInformation.IsCallConstruction)
            {
                I_subscribe.To(connect);
            }
            else AddClient(connect);
        }

        private void AddClient(ConnectData connect)
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
            foreach (ConnectData connectData in _clients)
            {
                connectData.AddCapsules(capsules);
            }
            _packetsBuffer.Clear();

            d_localDateTime = DateTime.Now;
        }
    }
}