using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace PacMan
{
    class Map
    {

        public List<Entity> Tiles = new List<Entity>();
        public List<Entity> Creatures = new List<Entity>();
        public List<Entity> Food = new List<Entity>();

        private Draw draw;

        public Entity Player;

        public int CreatureCount;

        public int AddSuperPowerSteps;

        public bool FriendlyFire;

        private Random Generator = new Random();

        public Map() { }

        public Map(Draw drawEngine, int SuperPowerStepsPerFood, bool friendlyFire)
        {
            draw = drawEngine;
            Creatures.Add(new Entity("P", new Coordinates(0, 0), ConsoleColor.Green, 1, 1));
            Player = GetCreatureByID(1);

            if (bool.Parse(ConfigurationManager.AppSettings["PlayerDebug"])) { Player.SuperPowerSteps = 15000; }

            AddSuperPowerSteps = SuperPowerStepsPerFood;
            FriendlyFire = friendlyFire;
        }

        public void GeneratePath(Entity Creature, Entity Target)
        {
            Creature.Destination = Target.Coordinates;
            Creature.ResetPath();
            AI ai = new AI(this);
            Creature.Path = ai.PathTo(Target.Coordinates, Creature.Coordinates);
        }

        public void GeneratePaths()
        {
            for (int i = 1; i < Creatures.Count; i++)
            {
                if (GetCreatureByID(Creatures[i].TargetID).Health > 0)
                {
                    if (Creatures[i].Health > 0) // && !IsAdjacent(Creatures[i], GetCreatureByID(Creatures[i].TargetID)))
                    {
                        if (!Creatures[i].hasPath() || (Creatures[i].hasPath() && !SamePosition(GetCreatureByID(Creatures[i].TargetID).Coordinates, Creatures[i].Destination)))
                        {
                            GeneratePath(Creatures[i], GetCreatureByID(Creatures[i].TargetID));
                            draw.MoveObject(Creatures[i], Creatures[i].NextStep());
                        }
                    }
                }
            }
        }

        public void MoveCreatures()
        {
            for ( int i = 1; i < Creatures.Count; i++ )
            {
                if (Creatures[i].Health > 0 && Creatures[i].hasPath())
                {
                    MoveCreature(Creatures[i], Creatures[i].NextStep());
                }
            }
        }

        public void MoveCreature(Entity creature, Coordinates step)
        {
            if (IsTileWalkable(step) && DistanceTo(creature.Coordinates, step) == 1)
            {
                draw.MoveObject(creature, step);
            }
            else
            {
                if (!SlayCreature(creature, step))
                {
                    Eat(creature, step);
                }
            }
        }

        public bool AllDead()
        {
            bool dead = true;
            for (int i = 1; i < Creatures.Count; i++)
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

        public bool SlayCreature(Entity creature, Coordinates step)
        {
            if ((creature.SuperPowerSteps > 0 || creature.SuperPowerSteps == -1) && IsTileCreature(step))
            {
                int CreatureID = TileCreature(step);
                if (Creatures[CreatureID].SuperPowerSteps < 1 && CanAttack(creature, Creatures[CreatureID]))
                {
                    Creatures[CreatureID].Die();
                    draw.MoveObject(creature, step);
                    creature.Experience += 1 + Creatures[CreatureID].Experience;
                    return true;
                }
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

        public bool Eat(Entity creature, Coordinates step)
        {
            if (creature.ID == Player.ID && IsTileFood(step))
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
                if (SamePosition(step, Food[i].Coordinates) && Food[i].Health > 0)
                {
                    Food[i].Health = 0;
                }
            }
        }

        public bool IsAdjacent(Entity Object1, Entity Object2)
        {
            return (DistanceTo(Object2.Coordinates, Object1.Coordinates) == 1);
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
                if (SamePosition(Tile, Tiles[i].Coordinates))
                {
                    tileWalkable = false;
                    break;
                }
            }

            if (tileWalkable) { tileWalkable = !IsTileCreature(Tile); }
            if (tileWalkable) { tileWalkable = !IsTileFood(Tile); }
            if (tileWalkable) { tileWalkable = !OutOfBoundaries(Tile); }

            return tileWalkable;
        }

        public bool IsTileCreature(Coordinates Tile)
        {
            bool tileCreature = false;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (SamePosition(Tile, Creatures[i].Coordinates) && Creatures[i].Health > 0)
                {
                    tileCreature = true;
                    break;
                }
            }
            return tileCreature;
        }

        public bool IsTileFood(Coordinates Tile)
        {
            bool tileFood = false;
            for (int i = 0; i < Food.Count; i++)
            {
                if (SamePosition(Tile, Food[i].Coordinates) && Food[i].Health > 0)
                {
                    tileFood = true;
                    break;
                }
            }
            return tileFood;
        }

        public int TileCreature(Coordinates Tile)
        {
            int CreatureID = -1;
            for (int i = 0; i < Creatures.Count; i++)
            {
                if (SamePosition(Tile, Creatures[i].Coordinates) && Creatures[i].Health > 0)
                {
                    CreatureID = i;
                    break;
                }
            }
            return CreatureID;
        }

        public Entity GetCreatureByID(int ID)
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
                return new Entity("null");
            }
            return Creatures[CreatureID];
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
                if (Tiles[i].Health > 0)
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

        public void DrawFood()
        {
            for (int i = 0; i < Food.Count; i++)
            {
                if (Food[i].Health > 0)
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
        }
    }
}
