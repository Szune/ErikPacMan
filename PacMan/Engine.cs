﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Timers;
using System.IO;
using System.Xml;

namespace PacMan
{
    class Engine
    {
        Draw draw;
        Map map;

        public int Score = 0;

        private int MapWidth = int.Parse(ConfigurationManager.AppSettings["MapWidth"]);
        private int MapHeight = int.Parse(ConfigurationManager.AppSettings["MapHeight"]);

        private Timer GameTimer;

        private ConsoleKey lastPressedKey = new ConsoleKey();

        private bool Walking = false;

        private bool Ready = false;

        private bool FirstGame = true; // Only used if user views highscore before playing

        private int WalkDirection = 0;

        private int LogicCounter = 0;

        private int GameSpeed;

        private int Difficulty;

        private List<Highscore> Highscores = new List<Highscore>();


        public Random Generator = new Random();


        public const int Direction_North = 1;
        public const int Direction_East = 2;
        public const int Direction_South = 3;
        public const int Direction_West = 4;

        public const int Difficulty_Easy = 1;
        public const int Difficulty_Normal = 2;
        public const int Difficulty_Hard = 3;

        public Engine(int difficulty, bool viewHighscore)
        {
            draw = new Draw();
            map = new Map(draw, int.Parse(ConfigurationManager.AppSettings["SuperPowerStepsPerFood"]), bool.Parse(ConfigurationManager.AppSettings["FriendlyFire"]));

            Difficulty = difficulty;

            GameSpeed = int.Parse(ConfigurationManager.AppSettings["GameSpeed"]);
            GameSpeed -= Difficulty * 160;

            GameTimer = new Timer(GameSpeed); // Skapar timern med ett interval på 'GameSpeed' millisekunder
            GameTimer.Elapsed += new ElapsedEventHandler(GameLoop); // Varje tick kallar vi metoden OnTimedEvent

            LoadHighscores(); // Load highscores

            if (!viewHighscore)
            {
                Start();
            }
            else
            {
                draw.ClearScreen();
                draw.Highscores(Highscores);
                Ready = true;
            }
            ConsoleKey read = new ConsoleKey();

            while (ParseKeys(read)) ;
        }

        private void ChooseDifficulty()
        {
            draw.ChooseDifficulty(false);
            bool done = false;
            while (!done)
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        Difficulty = Difficulty_Easy;
                        done = true;
                        break;
                    case "2":
                        Difficulty = Difficulty_Normal;
                        done = true;
                        break;
                    case "3":
                        Difficulty = Difficulty_Hard;
                        done = true;
                        break;
                    default:
                        draw.ChooseDifficulty(true);
                        break;
                }
            }

            draw.ClearScreen();
            if (FirstGame)
            {
                FirstGame = false;
                Start();
            }
            else
            {
                Start(true);
            }
        }

        private void Start(bool restart = false)
        {
            map.CreatureCount = 1;

            if (!restart) // First time, gotta initialize stuff
            {
                LoadMap("map1.map"); // Load the map
            }
            else
            {
                map.Player.Health = map.Player.MaxHealth;
                map.Player.SuperPowerSteps = 0;

                map.Creatures.RemoveRange(1, map.Creatures.Count - 1);
                map.Food.Clear();

                map.ResetExperience();
            }

            GenerateCreatures(Generator.Next(int.Parse(ConfigurationManager.AppSettings["minCreatures"]) + Difficulty, int.Parse(ConfigurationManager.AppSettings["maxCreatures"]) + (Difficulty * 2))); // Higher difficulty - more ghosts

            GenerateFood(12 - (Difficulty * 3)); // Higher difficulty - less food

            map.DrawMap();

            Console.Title = "ErikPacMan - Difficulty: " + DifficultyString(Difficulty);

            GameTimer.Enabled = true; // Starta timern
        }

        private void Lose()
        {
            GameTimer.Enabled = false;
            draw.GameOver();
            System.Threading.Thread.Sleep(50);
            Ready = true;
        }

        private void Win()
        {
            GameTimer.Enabled = false;
            if (MadeItToTheHighscores(map.Player.Experience, Difficulty, map.Player.SuperPowerSteps))
            {
                draw.Win(true);
                System.Threading.Thread.Sleep(50);
                Highscores.Add(new Highscore(Console.ReadLine(), map.Player.Experience, Difficulty, map.Player.SuperPowerSteps, DifficultyString(Difficulty)));
                Highscores.Sort();
                SaveHighscores();
                draw.ClearScreen();
                draw.Highscores(Highscores);
            }
            else
            {
                draw.Win(false);
                System.Threading.Thread.Sleep(50);
            }
            Ready = true;
        }

        private bool MadeItToTheHighscores(int experience, int difficulty, int superPower)
        {
            int Score = CalculateScore(experience, difficulty, superPower);

            if (Highscores.Count >= 10)
            {
                if (Score > Highscores[9].Score)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        private int CalculateScore(int experience, int difficulty, int superPower)
        {
            return (difficulty * experience * 50) + (difficulty * superPower);
        }

        private void SaveHighscores()
        {
            Highscores.Sort();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            using (XmlWriter writer = XmlWriter.Create("highscores.xml", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Highscores");
                if (Highscores.Count >= 10)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        writer.WriteStartElement("Highscore");
                        writer.WriteElementString("Name", Highscores[i].Name);
                        writer.WriteElementString("Score", Highscores[i].Score.ToString());
                        writer.WriteElementString("Difficulty", Highscores[i].DifficultyString);
                        writer.WriteEndElement();
                    }
                }
                else
                {
                    for (int i = 0; i < Highscores.Count; i++)
                    {
                        writer.WriteStartElement("Highscore");
                        writer.WriteElementString("Name", Highscores[i].Name);
                        writer.WriteElementString("Score", Highscores[i].Score.ToString());
                        writer.WriteElementString("Difficulty", Highscores[i].DifficultyString);
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void LoadHighscores()
        {
            string tempName = "Niet";
            int tempScore = 0;
            string tempDifficulty = "Ingen";
            try
            {
                if (File.Exists("highscores.xml"))
                {
                    using (XmlReader reader = XmlReader.Create("highscores.xml"))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name)
                                {
                                    case "Name":
                                        reader.Read();
                                        tempName = reader.Value;
                                        break;
                                    case "Score":
                                        reader.Read();
                                        tempScore = int.Parse(reader.Value);
                                        break;
                                    case "Difficulty":
                                        reader.Read();
                                        tempDifficulty = reader.Value;
                                        Highscores.Add(new Highscore(tempName, tempScore, tempDifficulty));
                                        break;
                                }
                            }
                        }
                    }

                    Highscores.Sort();
                }
            }
            catch (Exception ex)
            {
                draw.ClearScreen();
                draw.TypeWriterWrite("XML Error, could not load highscores.", 4);
                draw.TypeWriterWrite("Not to worry though, you can still play.", 5);
                draw.TypeWriterWrite("Press any key to continue.", 6);
                draw.StatusMessage(ex.Message);
                Console.ReadKey();
            }
        }

        private string DifficultyString(int difficulty)
        {
            switch (difficulty)
            {
                case Difficulty_Easy:
                    return "Easy";
                case Difficulty_Normal:
                    return "Normal";
                case Difficulty_Hard:
                    return "Hard";
                default:
                    return "None";
            }
        }

        private void GameLoop(object source, ElapsedEventArgs e) // När timern tickar
        {
            // Game Loop
            // Förflyttar karaktären
            if (Walking && map.Player.Health > 0)
            {
                switch (WalkDirection)
                {
                    case Direction_North:
                        map.MoveCreature(map.Player, new Coordinates(map.Player.Coordinates.X, map.Player.Coordinates.Y - 1));
                        break;
                    case Direction_East:
                        map.MoveCreature(map.Player, new Coordinates(map.Player.Coordinates.X + 1, map.Player.Coordinates.Y));
                        break;
                    case Direction_South:
                        map.MoveCreature(map.Player, new Coordinates(map.Player.Coordinates.X, map.Player.Coordinates.Y + 1));
                        break;
                    case Direction_West:
                        map.MoveCreature(map.Player, new Coordinates(map.Player.Coordinates.X - 1, map.Player.Coordinates.Y));
                        break;
                    default:
                        break;
                }
                Walking = false;
            }

            if (map.Player.SuperPowerSteps < 1)
            {
                draw.StatusMessage("Score: " + CalculateScore(map.Player.Experience, Difficulty, map.Player.SuperPowerSteps).ToString());
            }
            else
            {
                draw.StatusMessage("Score: " + CalculateScore(map.Player.Experience, Difficulty, map.Player.SuperPowerSteps).ToString() + " - Super power: " + map.Player.SuperPowerSteps.ToString());
                map.Player.SuperPowerSteps -= 1;
            }

            /* For testing purposes
            //draw.StatusMessage(map.Creatures[1].hasPath().ToString());
            //draw.StatusMessage("isAdjacent: " + map.isAdjacent(Player, map.Creatures[1]).ToString());
            //draw.StatusMessage("hasPath: " + map.Creatures[1].hasPath());
            //draw.StatusMessage(Player.Coordinates.X.ToString() + " " + Player.Coordinates.Y.ToString());
             */

            LogicCounter += 1;

            PerformLogic();

            lastPressedKey = new ConsoleKey(); // Ser till så att spelaren inte kan gå snabbare än menat            
        }

        private void PerformLogic()
        {
            if (LogicCounter % 2 == 0)
            {
                map.MoveCreatures(); // Kallar metoden för att förflytta spökena
            }

            if ((LogicCounter / 3) % 2 == 1)
            {
                map.GeneratePaths();
            }

            /*if (LogicCounter == 6) { SpawnCreatures(); LogicCounter = 0; }
                Not currently in use, possibly for the future
             */

            if (map.Player.Health < 1)
            {
                Lose();
            }

            if (map.AllDead())
            {
                Win();
            }
        }

        private void LoadMap(string mapn) // Redan kommenterad i PacMan - MapEditor
        {
            int SpawnX = 0;
            int SpawnY = 0;
            FileInfo file = new FileInfo(mapn);

            if (file.Exists)
            {
                StreamReader read = new StreamReader(mapn);

                string[] coordinateList = read.ReadLine().Split(",".ToCharArray());

                for (int i = 0; i < coordinateList.Length; i++)
                {
                    string[] Coordinates = coordinateList[i].Split("|".ToCharArray());
                    int x = int.Parse(Coordinates[0]);
                    int y = int.Parse(Coordinates[1]);

                    map.Tiles.Add(new Entity("+", new Coordinates(x, y), ConsoleColor.DarkCyan));
                }

                coordinateList = read.ReadLine().Split("|".ToCharArray());
                SpawnX = int.Parse(coordinateList[0]);
                SpawnY = int.Parse(coordinateList[1]);
                map.Player.Coordinates = new Coordinates(SpawnX, SpawnY);
                map.Player.SpawnCoordinates = new Coordinates(SpawnX, SpawnY);

                read.Close();
            }
            else
            {
                Console.WriteLine("Map file could not be found.");
            }
        }

        private void GenerateCreatures(int amount)
        {
            Coordinates SpawnCoordinates = new Coordinates();

            for (int i = 0; i < amount; i++)
            {
                map.CreatureCount += 1;
                SpawnCoordinates = FindEmptyTile(MapWidth, MapHeight);
                map.Creatures.Add(new Entity("G", SpawnCoordinates, map.Player.ID, ConsoleColor.Red, 1, map.CreatureCount));
            }
        }

        private void GenerateFood(int amount)
        {
            Coordinates SpawnCoordinates = new Coordinates();

            for (int i = 0; i < amount; i++)
            {
                SpawnCoordinates = FindEmptyTile(MapWidth, MapHeight);
                map.Food.Add(new Entity("!", SpawnCoordinates, ConsoleColor.Yellow, 1));
            }
        }


        /* 
         * SpawnCreatures() is currently not in use, saved for the future
         */
        private void SpawnCreatures()
        {
            for (int i = 0; i < map.Creatures.Count; i++)
            {
                if (map.Creatures[i].Health < 1)
                {
                    if (map.IsTileWalkable(map.Creatures[i].Coordinates))
                    {
                        map.Creatures[i].Spawn();
                        draw.DrawObject(map.Creatures[i]);
                    }
                    else
                    {
                        map.Creatures[i].SpawnCoordinates = FindEmptyTile(MapWidth, MapHeight);
                        map.Creatures[i].Spawn();
                        draw.DrawObject(map.Creatures[i]);
                    }
                }

            }
        }

        private Coordinates FindEmptyTile(int width, int height)
        {
            Coordinates EmptyTileCoordinates = new Coordinates();
            EmptyTileCoordinates.X = Generator.Next(0, width); //Console.WindowWidth - 1);
            EmptyTileCoordinates.Y = Generator.Next(0, height); //Console.WindowHeight - 2);
            while (!map.IsTileWalkable(EmptyTileCoordinates))
            {
                if (bool.Parse(ConfigurationManager.AppSettings["DebugMode"])) { draw.DrawObject(new Entity("e", EmptyTileCoordinates)); }
                EmptyTileCoordinates = new Coordinates(Generator.Next(0, MapWidth), Generator.Next(0, MapHeight - 2));
            }
            return EmptyTileCoordinates;
        }

        private bool ParseKeys(ConsoleKey read)
        {
            bool parse = true;
            read = Console.ReadKey(true).Key;

            if (!Walking && Ready == false)
            {
                if (lastPressedKey != read)
                {
                    switch (read)
                    {
                        case ConsoleKey.Escape:
                            parse = false;
                            break;
                        case ConsoleKey.LeftArrow: // Förklaring finns på MoveObject i Draw-klassen
                            WalkDirection = Direction_West;
                            Walking = true;
                            break;
                        case ConsoleKey.RightArrow:
                            WalkDirection = Direction_East;
                            Walking = true;
                            break;
                        case ConsoleKey.DownArrow:
                            WalkDirection = Direction_South;
                            Walking = true;
                            break;
                        case ConsoleKey.UpArrow: // Flytta upp spelaren
                            WalkDirection = Direction_North;
                            Walking = true;
                            break;
                    }
                    lastPressedKey = read;
                }
            }
            else if (Ready)
            {
                if (read == ConsoleKey.Enter)
                {
                    draw.ClearScreen();
                    ChooseDifficulty();
                    Ready = false;
                }
                else if (read == ConsoleKey.Spacebar)
                {
                    draw.ClearScreen();
                    Highscores.Sort();
                    draw.Highscores(Highscores);
                }
            }
            return parse;
        }




    }
}
