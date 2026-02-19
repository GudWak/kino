using System;

namespace Cinema
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== КИНОТЕАТР 'ЭКРАН' ===\n");

            CinemaMenu menu = new CinemaMenu();
            menu.ShowMainMenu();

            Console.WriteLine("\nПриятного просмотра! До новых встреч!");
            Console.ReadKey();
        }
    }
}