using System;
using word_game_Server_;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordGuessingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Listener listener = new Listener();
                listener.StartListener();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Press Enter to End");
            }

        }

    }
}
