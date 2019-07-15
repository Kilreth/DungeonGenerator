using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Tile
    {
        public enum Type { Solid, Path, Room }

        public Type Space { get; set; }

        public Tile(Type type)
        {
            this.Space = type;
        }
    }
}
