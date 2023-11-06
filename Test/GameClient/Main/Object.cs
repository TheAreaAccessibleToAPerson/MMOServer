using System.ComponentModel;

namespace gameClient.main
{
    public abstract class Object 
    {
        private readonly string Explorer;

        public Object(string explorer)
        {
            Explorer = explorer;
        }

        protected void SystemInformation(string info, ConsoleColor color = ConsoleColor.Yellow)
        {
            System.Console.ForegroundColor = color;

            System.Console.WriteLine($"{Explorer}:{info}");
        }
    }
}
