﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace PacMan
{
    class Map
    {

        public List<Tile> Tiles = new List<Tile>();
        public List<Creature> Creatures = new List<Creature>();
        public List<Player> Players = new List<Player>();
        public List<Entity> Food = new List<Entity>();

        private Draw draw;

        public int CreatureCount;

        public int AddSuperPowerSteps;

        public bool FriendlyFire;

        private Random Generator = new Random();

        public Map() { }

        public Map(Draw drawEngine, int SuperPowerStepsPerFood, bool friendlyFire)
        {
            draw = drawEngine;
            //Creatures.Add(new Creature("P", new Coordinates(0, 0), 1, ConsoleColor.Green, 1)); <- old way of doing it
            Players.Add(new Player("P", new Coordinates(0, 0), ConsoleColor.Green, 1, 1));

            /* TODO: Change Players[0] to a variable ID to allow for multiplayer */

            if (bool.Parse(ConfigurationManager.AppSettings["PlayerDebug"])) { Players[0].SuperPowerSteps = 15000; }

            AddSuperPowerSteps = SuperPowerStepsPerFood;
            FriendlyFire = friendlyFire;
        }

        public void GeneratePathFromCreature(Creature FromCreature, Coordinates Target)
        {
            FromCreature.Destination = Target;
            FromCreature.ResetPath();
            AI ai = new AI(this);
            FromCreature.Path = ai.PathTo(Target, FromCreature.Position);
        }

        public void GeneratePaths()
        {
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (GetPlayerByID(Creatures[i].TargetID).Health > 0)
                {
                    if (Creatures[i].Health > 0) // && !IsAdjacent(Creatures[i], GetCreatureByID(Creatures[i].TargetID)))
                    {
                        if (!Creatures[i].hasPath() || (Creatures[i].hasPath() && !SamePosition(Players[0].Position, Creatures[i].Destination)))
                        {
                            /* TODO: Change Players[0] to a variable ID to allow for multiplayer */
                            GeneratePathFromCreature(Creatures[i], Players[0].Position);
                            draw.MoveObject(Creatures[i], Creatures[i].NextStep());
                        }
                    }
                }
            }
        }

        public void MoveCreatures()
        {
            for ( int i = 0; i < Creatures.Count; i++ )
            {
                if (Creatures[i].Health > 0 && Creatures[i].hasPath())
                {
                    MoveCreature(Creatures[i], Creatures[i].NextStep());
                }
            }
        }

        public void MoveCreature(Entity creature, Coordinates step)
        {
            if (IsTileWalkable(step) && DistanceTo(creature.Position, step) == 1)
            {
                draw.MoveObject(creature, step);
            }
            else
            {
                int EntityType = GetEntityTypeFromTile(step);
                if(EntityType == Entity.CreatureEntity)
                {
                    /* TODO: Change Players[0] to a variable ID and add method to attack other players to allow for multiplayer */
                    if (creature.EntityType == Entity.PlayerEntity)
                    {
                        Creature monster = GetCreatureByID(GetCreatureIDFromTile(step));
                        PlayerAttack(Players[0], monster);
                    }
                }
                else if(EntityType == Entity.PlayerEntity)
                {
                    if (creature.EntityType == Entity.CreatureEntity)
                    {
                        CreatureAttack(GetCreatureByID(creature.ID), Players[0]);
                    }
                }
                else if (EntityType == Entity.ItemEntity)
                {
                    /* TODO: Change Players[0] to a variable ID to allow for multiplayer */
                    Eat(Players[0], step);
                }
            }
        }

        public bool AllDead()
        {
            bool dead = true;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (Creatures[i].Health > 0)
                {
                    dead = false;
                    break;
                }
            }
            return dead;
        }

        public void ResetExperience()
        {
            for (int i = 0; i < Creatures.Count; i++)
            {
                Creatures[i].Experience = 0;
            }
        }

        public bool PlayerAttack(Player player, Creature target)
        {
            if (player.SuperPowerSteps > 0 && CanAttack(player, target))
            {
                target.Die();
                draw.MoveObject(player, target.Position);
                player.Experience += 1 + target.Experience;
                return true;
            }
            return false;
        }

        public bool CreatureAttack(Creature creature, Player target)
        {
            if (target.SuperPowerSteps < 1 && CanAttack(creature, target))
            {
                target.Die();
                draw.MoveObject(creature, target.Position);
                creature.Experience += 1 + target.Experience;
                return true;
            }
            return false;
        }


        public bool CanAttack(Entity creature, Entity target)
        {
            if (creature.Name == target.Name && !FriendlyFire)
            {
                return false;
            }
            return true;
        }

        public bool Eat(Player creature, Coordinates step)
        {
            /* TODO: Change Players[0] to a variable ID to allow for multiplayer */
            if (IsTileFood(step))
            {
                SetFoodEaten(step);
                draw.MoveObject(creature, step);
                creature.SuperPowerSteps += AddSuperPowerSteps;
            }
            return true;
        }

        public void SetFoodEaten(Coordinates step)
        {
            for (int i = 0; i < Food.Count; i++)
            {
                if (SamePosition(step, Food[i].Position) && Food[i].Visible)
                {
                    Food[i].Visible = false;
                }
            }
        }

        public int GetEntityTypeFromTile(Coordinates Tile)
        {
            if (IsTileCreature(Tile)) return Entity.CreatureEntity;
            if (IsTilePlayer(Tile)) return Entity.PlayerEntity;
            if (IsTileFood(Tile)) return Entity.ItemEntity;
            return Entity.UnknownEntity;
        }

        public bool IsAdjacent(Entity Object1, Entity Object2)
        {
            return (DistanceTo(Object2.Position, Object1.Position) == 1);
        }

        public int DistanceTo(Coordinates Source, Coordinates Destination)
        {
            return (Math.Abs((Source.X - Destination.X) + Math.Abs(Source.Y - Destination.Y)));
        }

        public int DistanceToDiagonal(Coordinates Source, Coordinates Destination)
        {
            return (int)Math.Sqrt(Math.Pow((Source.X - Destination.X), 2) + Math.Pow(Source.Y - Destination.Y, 2));
        }

        public bool IsTileWalkable(Coordinates Tile)
        {
            bool tileWalkable = true;
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (SamePosition(Tile, Tiles[i].Position))
                {
                    tileWalkable = false;
                    break;
                }
            }

            if (tileWalkable) { tileWalkable = !IsTilePlayer(Tile); }
            if (tileWalkable) { tileWalkable = !IsTileCreature(Tile); }
            if (tileWalkable) { tileWalkable = !IsTileFood(Tile); }
            if (tileWalkable) { tileWalkable = !OutOfBoundaries(Tile); }

            return tileWalkable;
        }

        public bool IsTilePlayer(Coordinates Tile)
        {
            /* TODO: Change Players[0] to a variable ID to allow for multiplayer */
            if (SamePosition(Tile, Players[0].Position) && Players[0].Health > 0)
            {
                return true;
            }
            return false;
        }

        public bool IsTileCreature(Coordinates Tile)
        {
            bool tileCreature = false;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (SamePosition(Tile, Creatures[i].Position) && Creatures[i].Health > 0)
                {
                    tileCreature = true;
                    break;
                }
            }
            return tileCreature;
        }

        public bool IsTileAnimate(Coordinates Tile)
        {
            return IsTileCreature(Tile) || IsTilePlayer(Tile);
        }

        public bool IsTileFood(Coordinates Tile)
        {
            bool tileFood = false;
            for (int i = 0; i < Food.Count; i++)
            {
                if (SamePosition(Tile, Food[i].Position) && Food[i].Visible)
                {
                    tileFood = true;
                    break;
                }
            }
            return tileFood;
        }

        public int GetCreatureIDFromTile(Coordinates Tile)
        {
            int CreatureID = -1;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (SamePosition(Tile, Creatures[i].Position) && Creatures[i].Health > 0)
                {
                    CreatureID = Creatures[i].ID;
                    break;
                }
            }
            return CreatureID;
        }

        public Creature GetCreatureByID(int ID)
        {
            int CreatureID = -1;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (Creatures[i].ID == ID)
                {
                    CreatureID = i;
                    break;
                }
            }
            
            if (CreatureID == -1)
            {
                return new Creature("null");
            }
            return Creatures[CreatureID];
        }

        public Player GetPlayerByID(int ID)
        {
            /* TODO: Change Players[0] to a variable ID to allow for multiplayer */
            return Players[0];
        }

        public Entity GetCreatureByName(string Name)
        {
            int CreatureID = -1;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (Creatures[i].Name == Name)
                {
                    CreatureID = i;
                    break;
                }
            }
            if (CreatureID == -1)
            {
                return new Entity("null");
            }
            return Creatures[CreatureID];
        }

        public bool SamePosition(Coordinates Source, Coordinates Destination)
        {
            return (Source.X == Destination.X && Source.Y == Destination.Y);
        }

        public bool OutOfBoundaries(Coordinates Coordinates)
        {
            return !(Coordinates.X >= 0 && Coordinates.Y >= 0 && Coordinates.X < Console.WindowWidth && Coordinates.Y < Console.WindowHeight - 1);
        }


        public void DrawTiles()
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].Visible)
                {
                    draw.DrawObject(Tiles[i]);
                }
            }
        }

        public void DrawCreatures()
        {
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (Creatures[i].Health > 0)
                {
                    draw.DrawObject(Creatures[i]);
                }
            }
        }

        public void DrawPlayers()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Health > 0)
                {
                    draw.DrawObject(Players[i]);
                }
            }
        }

        public void DrawFood()
        {
            for (int i = 0; i < Food.Count; i++)
            {
                if (Food[i].Visible)
                {
                    draw.DrawObject(Food[i]);
                }
            }
        }

        public void DrawMap()
        {
            DrawTiles();
            DrawCreatures();
            DrawFood();
            DrawPlayers();
        }
    }
}
