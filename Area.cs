﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Area
    {
        public HashSet<Area> To { get; private set; }
        public int Id { get; private set; }
        public static int NextId { get; set; }

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
            Id = ++NextId;
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
