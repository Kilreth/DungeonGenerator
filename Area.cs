﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    /// <summary>
    /// An Area is a traversable place included in a dungeon's connected graph.
    /// Currently these areas are Rooms and Paths.
    /// 
    /// Equality only consists of a unique ID.
    /// </summary>
    public class Area
    {
        public HashSet<Area> To { get; private set; }
        public int Id { get; private set; }
        public static int NextId { get; set; }

        public bool IsRecursivelyConnectedTo(Area other)
        {
            HashSet<Area> visited = new HashSet<Area>();
            Queue<Area> queue = new Queue<Area>();
            queue.Enqueue(this);
            visited.Add(this);
            while (queue.Count > 0)
            {
                Area area = queue.Dequeue();
                foreach (Area neighbor in area.To)
                {
                    if (neighbor == other)
                        return true;

                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            return false;
        }

        public void ConnectTo(Area other)
        {
            if (other != this)
            {
                To.Add(other);
                other.To.Add(this);
            }
        }

        public void InitializeArea()
        {
            To = new HashSet<Area>();
            Id = NextId++;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType().IsSubclassOf(typeof(Area)))
            {
                Area other = (Area)obj;
                return Id == other.Id;
            }
            else
            {
                return false;
            }
        }

        static Area()
        {
            NextId = 0;
        }
    }
}
