using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Location
    {
        public int Row { get; }
        public int Col { get; }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Location other = (Location)obj;
                return (Row == other.Row) && (Col == other.Col);
            }
        }

        public override int GetHashCode()
        {
            return (Row << 2) ^ Col;
        }

        public Location(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}
