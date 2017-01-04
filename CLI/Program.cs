using CLI;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            ExampleServer server = new ExampleServer("0.0.0.0",1122);
            server.SetRoot("./");
            server.Start();
        }
    }
}
