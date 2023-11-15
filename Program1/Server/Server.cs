using Butterfly;

namespace server
{
    public sealed class Server : Controller.LocalField<Settings>
    {
        void Construction()
        {
            obj<component.ClientsManager> 
                (component.ClientsManager.NAME);

            obj<component.ListenSSLClientsShell> 
                (component.ListenSSLClientsShell.NAME, 
                    Field.SSLListenClientsSetting);

            obj<component.ListenTCPClientsShell> 
                (component.ListenTCPClientsShell.NAME, 
                    Field.TCPListenClientsSetting);

            obj<component.ReceiveUDPShell>
                (component.ReceiveUDPShell.NAME, 
                    Field.ReceiveUDPPacketsSetting);

            /**************TEST*****************/
            listen_message<component.clientManager.component.clientShell.information.Client>
                ("BD").output_to((client) => 
                {
                    client.ReturnVerificationResult
                        (component.clientManager.component.clientShell.information.Client.Verification.Success);
                });
            /***********************************/
        }
    }
}
