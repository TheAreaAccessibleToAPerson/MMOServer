public class Logger
{
    public struct BUS
    {
        public struct Message
        {
            /// <summary>
            /// Логирование серверных компонентов, жизненые процессы.
            /// </summary>
            public const string SERVER_COMPONENTS = "Logger:ServerComponents.";

            public const string CLIENT_SHELL_COMPONENT = "Logger:ClientShellComponent";
            public const string CLIENT_OBJECT_COMPONENT = "Logger:ClientObjectComponent";
        }
    }

    public void add_server_component_info(string info)
    {
        Console.WriteLine(info);
    }

    public void add_client_shell_component_info(string info)
    {
        Console.WriteLine(info);
    }

    public void add_client_object_component_info(string info)
    {
        Console.WriteLine(info);
    }
}