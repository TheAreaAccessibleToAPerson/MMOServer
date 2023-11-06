using Butterfly;

namespace server.component.BD
{
    public class Authorization : Controller.Board
    {
        public struct BUS 
        {
            private const string NAME = "BD:Authorization:";


            public struct Message 
            {
                public const string SELECT = NAME + "Select";
            }
        }

        void Construction()
        {
            safe_listen_message<server.component.clientManager.component.clientShell.information.Client>
                (BUS.Message.SELECT, "", "");
        }

        void Start()
        {

        }

        void Configurate()
        {

        }
    }
}
