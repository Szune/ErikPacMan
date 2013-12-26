using System;

namespace PacMan
{
    class Program
    {

        static void Main( string[] args )
        {
            int OffsetX = 16;
            int OffsetY = 2;
            Console.Title = "ErikPacMan by Erik Iwarson";
            Console.BufferHeight = 25;
            Console.CursorVisible = false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Clear();
            Console.SetCursorPosition(OffsetX, OffsetY);
            Console.WriteLine("-------------------------------------------");
            Console.SetCursorPosition(OffsetX, OffsetY + 1);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 2);
            Console.WriteLine("-               ErikPacMan                -");
            Console.SetCursorPosition(OffsetX, OffsetY + 3);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 4);
            Console.WriteLine("-            Erik Iwarson 2013            -");
            Console.SetCursorPosition(OffsetX, OffsetY + 5);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 6);
            Console.WriteLine("-           ! - Food, G - Ghost           -");
            Console.SetCursorPosition(OffsetX, OffsetY + 7);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 8);
            Console.WriteLine("-          Press Enter to start           -");
            Console.SetCursorPosition(OffsetX, OffsetY + 9);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 10);
            Console.WriteLine("-          Press Escape to exit           -");
            Console.SetCursorPosition(OffsetX, OffsetY + 11);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 12);
            Console.WriteLine("-       Press Space for highscores        -");
            Console.SetCursorPosition(OffsetX, OffsetY + 13);
            Console.WriteLine("-                                         -");
            Console.SetCursorPosition(OffsetX, OffsetY + 14);
            Console.WriteLine("-------------------------------------------");

            bool MenuChosen = false;
            ConsoleKey read;


            while (!MenuChosen)
            {
                read = Console.ReadKey(true).Key;
                if (read == ConsoleKey.Enter)
                {
                    MenuChosen = true;
                    int Difficulty = 0;
                    Console.Clear();
                    Console.SetCursorPosition(OffsetX, OffsetY);
                    Console.WriteLine("Difficulty:");
                    Console.SetCursorPosition(OffsetX, OffsetY + 1);
                    Console.WriteLine("1. Easy");
                    Console.SetCursorPosition(OffsetX, OffsetY + 2);
                    Console.WriteLine("2. Normal");
                    Console.SetCursorPosition(OffsetX, OffsetY + 3);
                    Console.WriteLine("3. Hard");
                    Console.SetCursorPosition(OffsetX, OffsetY + 4);
                    bool DifficultyChosen = false;
                    while (!DifficultyChosen)
                    {
                        switch (Console.ReadLine())
                        {
                            case "1":
                                Difficulty = 1;
                                DifficultyChosen = true;
                                break;
                            case "2":
                                Difficulty = 2;
                                DifficultyChosen = true;
                                break;
                            case "3":
                                Difficulty = 3;
                                DifficultyChosen = true;
                                break;
                            default:
                                Console.SetCursorPosition(OffsetX, 7);
                                Console.WriteLine("Choose either 1, 2 or 3.");
                                Console.SetCursorPosition(OffsetX, 6);
                                for (int i = 0; i < Console.WindowWidth; i++)
                                {
                                    Console.Write(" ");
                                }
                                Console.SetCursorPosition(OffsetX, 6);
                                break;
                        }
                    }

                    Console.Clear();

                    Engine engine = new Engine(Difficulty, false);
                }
                else if (read == ConsoleKey.Spacebar)
                {
                    MenuChosen = true;
                    Engine engine = new Engine(0, true);
                }
                else if (read == ConsoleKey.Escape)
                {
                    MenuChosen = true;
                }
            }
        }

        void DrawLine(string text, int line)
        {

        }

    }
}
