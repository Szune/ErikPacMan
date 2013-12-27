using System;

namespace PacMan
{
    class Program
    {
        private static int OffsetX = 16;
        private static int OffsetY = 2;

        static void Main( string[] args )
        {
            Console.Title = "ErikPacMan by Erik Iwarson";
            Console.BufferHeight = 25;
            Console.CursorVisible = false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Clear();
            DrawLine("-------------------------------------------", 0);
            DrawLine("-                                         -", 1);
            DrawLine("-               ErikPacMan                -", 2);
            DrawLine("-                                         -", 3);
            DrawLine("-            Erik Iwarson 2013            -", 4);
            DrawLine("-                                         -", 5);
            DrawLine("-           ! - Food, G - Ghost           -", 6);
            DrawLine("-                                         -", 7);
            DrawLine("-          Press Enter to start           -", 8);
            DrawLine("-                                         -", 9);
            DrawLine("-          Press Escape to exit           -", 10);
            DrawLine("-                                         -", 11);
            DrawLine("-       Press Space for highscores        -", 12);
            DrawLine("-                                         -", 13);
            DrawLine("-------------------------------------------", 14);

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
                    DrawLine("Difficulty:", 0);
                    DrawLine("1. Easy", 1);
                    DrawLine("2. Normal", 2);
                    DrawLine("3. Hard", 3);
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
                                DrawLine("Choose either 1, 2 or 3.", 5);
                                Console.SetCursorPosition(OffsetX, OffsetY + 4);
                                for (int i = 0; i < Console.WindowWidth; i++)
                                {
                                    Console.Write(" ");
                                }
                                Console.SetCursorPosition(OffsetX, OffsetY + 4);
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

        private static void DrawLine(string text, int line)
        {
            Console.SetCursorPosition(OffsetX, OffsetY + line);
            Console.WriteLine(text);
        }

    }
}
