#define CSL
#define EX
#define INFO

using Butterfly;

namespace server.component.clientManager.component
{
    public sealed class ClientShell : clientShell.ConnectionController
    {
        protected override void CreatingClientObject(ConnectData connectData)
            => obj<ClientObject>(GetKey(), connectData);

        void Construction()
        {
#if CSL
            send_message(ref I_logger, Logger.BUS.Message.CLIENT_SHELL_COMPONENT);
#endif

            input_to(ref I_process, Header.Event.WORK_OBJECT, Process);
            input_to(ref I_receiveSSL, Header.Event.WORK_SSL, InputReceive);
            input_to(ref I_sendSSL, Header.Event.WORK_SSL, InputSend);

            send_message(ref I_verificationBD, "BD");

            send_echo_2_0(ref I_subscribeReceiveToTCPConnection,
                ListenTCPClientsShell.BUS.Echo.SUBSCRIBE_TO_RECEIVE_CONNECTION)
                    .output_to(Process, Header.Event.WORK_OBJECT);

            send_echo_1_0(ref I_unsubscribeReceiveToTCPConnection,
                ListenTCPClientsShell.BUS.Echo.UNSUBSCRIBE_TO_RECEIVE_CONNECTION)
                    .output_to(Process, Header.Event.WORK_OBJECT);

            send_echo_2_0(ref I_subscribeToReceiveFirstUDPPacket,
                ReceiveUDPShell.BUS.Echo.SUBSCRIBE_TO_RECEIVE_THE_FIRST_PACKET)
                    .output_to(Process, Header.Event.WORK_OBJECT);

            send_echo_1_0(ref I_unsubscribeToReceiveFirstUDPPacket,
                ReceiveUDPShell.BUS.Echo.UNSUBSCRIBE_TO_RECEIVE_THE_FIRST_PACKET)
                    .output_to(Process, Header.Event.WORK_OBJECT);

            ClientInformation = new(4, 16, 4, 16, I_process);
        }

        void Start() => Process();

        void Stop()
        {
            if (StateInformation.IsCallStart)
            {
                ConnectionState.Destroy();

                if (ConnectionState.IsUnsubscribeTCPConnection)
                    I_unsubscribeReceiveToTCPConnection.To
                        (((System.Net.IPEndPoint)Field.Client.RemoteEndPoint)
                            .Address.ToString());

                if (ConnectionState.IsUnsubscribeUDPConnection)
                    I_unsubscribeToReceiveFirstUDPPacket.To
                        (((System.Net.IPEndPoint)Field.Client.RemoteEndPoint)
                            .Address.ToString());
            }
        }

        private sealed class ClientObject : ClientController
        {
            protected IInput<string, ConnectData> I_registerInRoom;
            protected IInput<string, ConnectData> I_subscribeReceiveUDPPackets;
            protected IInput<string> I_unsubscribeReceiveUDPPackets;

            void Construction()
            {
                add_event(Header.Event.PROCESSING_OF_SENDING_UDP_PACKETS,
                    10, Field.Process);

                send_echo_2_0(ref I_subscribeReceiveUDPPackets, 
                    ReceiveUDPShell.BUS.Echo.SUBSCRIBE_TO_RECEIVE_THE_PACKETS)
                        .output_to(() => 
                        {
                        }, 
                        Header.Event.WORK_OBJECT);

                send_echo_1_0(ref I_unsubscribeReceiveUDPPackets, 
                    ReceiveUDPShell.BUS.Echo.UNSUBSCRIBE_TO_RECEIVE_THE_PACKETS)
                        .output_to(() => 
                        {
                        }, 
                        Header.Event.WORK_OBJECT);

                send_echo_2_0(ref I_registerInRoom,
                    WorldManager.BUS.Echo.REGISTER_IN_ROOM)
                        .output_to(() =>
                        {
                        }, 
                        Header.Event.WORK_OBJECT);
            }

            void Start()
            {
            }

            void Stop()
            {

            }

            void Destruction()
            {
            }

            void Configurate()
            {
                try
                {
                    Field.Connect();
                }
                catch { destroy(); }
            }
        }
    }
}