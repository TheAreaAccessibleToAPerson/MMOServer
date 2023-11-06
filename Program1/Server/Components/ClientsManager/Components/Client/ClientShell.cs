#define CSL
#define EX

using Butterfly;

namespace server.component.clientManager.component
{
    public sealed class ClientShell : clientShell.ConnectionController
    {
        void Construction()
        {
#if CSL
            send_message(ref i_logger, Logger.BUS.Message.CLIENT_SHELL_COMPONENT);
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

        void Start()
        {
            Process();
        }

        void Destruction() 
        {
            SystemInformation("Destruction");
        }

        void Stop()
        {
            if (StateInformation.IsCallStart)
            {
                ConnectionState.Destroy();

                if (ConnectionState.IsUnsubscribeTCPConnection)
                    I_unsubscribeReceiveToTCPConnection.To
                        (((System.Net.IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString());

                if (ConnectionState.IsUnsubscribeUDPConnection)
                    I_unsubscribeToReceiveFirstUDPPacket.To
                        (((System.Net.IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString());
            }
        }

        private sealed class Object : ConnectController
        {
            void Construction()
            {
            }

            void Start()
            {
            }

            void Stop()
            {

            }

        }
    }
}