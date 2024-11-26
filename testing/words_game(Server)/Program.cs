using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace words_game_Server_
{
    public class Program
    {
        //
        // Method :Main
        // DESCRIPTION : this is the start point of the server 
        // PARAMETERS :(string[] args)
        // RETURNS : void
        //
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
