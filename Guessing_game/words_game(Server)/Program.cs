using System;
using word_game_Server_;

namespace WordGuessingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();
            listener.StartListener();

            Console.WriteLine("Press Enter to End");
            Console.ReadLine();
        }

    }
}
