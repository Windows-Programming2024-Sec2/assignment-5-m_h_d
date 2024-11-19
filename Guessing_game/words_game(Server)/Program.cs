/*
* FILE : Program.cs
* PROJECT : PROG2121 -  A6
* PROGRAMMER : Mohamad Aldabea
* FIRST VERSION : 2024 / 11 / 19 
* DESCRIPTION :
* The functions in this file are used to to crate a listener class and call 
* the start method that will start the server and wait for the connection
*/

using System;
using word_game_Server_;


namespace WordGuessingServer
{
    /// <summary>
    /// It will initialize a new listerine and call the start method in listerine class
    /// </summary>
    class Program
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
