using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacMan
{
    class Entity
    {
        public string Name { get; set; }
        public ConsoleColor Color { get; set; }

        public Coordinates Coordinates { get; set; }
        public Coordinates SpawnCoordinates { get; set; }
        
        public List<Coordinates> Path = new List<Coordinates>();
        public Coordinates Destination { get; set; }

        public int TargetID { get; set; }
        public int ID { get; set; }

        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int SuperPowerSteps { get; set; }
        public int Experience { get; set; }

        public Entity() { MaxHealth = 1; Health = 1; ID = 0; }

        public Entity(string name, int id = 0) { Name = name; MaxHealth = 1; Health = 1; ID = id; }

        public Entity(string name, Coordinates coordinates, ConsoleColor color = ConsoleColor.White, int health = 1, int id = 0)
        {
            /* Spelare */
            Name = name;
            Coordinates = coordinates;
            SpawnCoordinates = coordinates;
            Color = color;
            MaxHealth = health;
            Health = health;
            ID = id;
            SuperPowerSteps = 0;
        }

        public Entity(string name, Coordinates coordinates, int targetID, ConsoleColor color = ConsoleColor.White, int health = 1, int id = 0)
        {
            /* Spöken */
            Name = name;
            Coordinates = coordinates;
            SpawnCoordinates = coordinates;
            Color = color;
            MaxHealth = health;
            Health = health;
            ID = id;
            TargetID = targetID;
            SuperPowerSteps = -1;
        }

        public bool hasPath()
        {
            if (Path.Count > 0)
            {
                return SamePosition(new Coordinates(Path[0].X, Path[0].Y), Destination);
            }
            else
            {
                return false;
            }
            
        }

        public void ResetPath()
        {
            Path.Clear();
        }

        public Coordinates NextStep()
        {
            Coordinates Step = null;
            if (Path.Count > 0)
            {
                Step = Path[Path.Count - 1];
                Path.RemoveAt(Path.Count - 1);
            }
            return Step;
        }

        public void Die()
        {
            Health = 0;
        }

        public void Spawn()
        {
            Health = MaxHealth;
            Coordinates = SpawnCoordinates;
            ResetPath();
        }

        public bool SamePosition(Coordinates Source, Coordinates Destination)
        {
            return (Source.X == Destination.X && Source.Y == Destination.Y);
        }
    }
}
