using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Tile
    {
        public int Row { get; }
        public int Col { get; }
        public Type Space { get; set; }
        public Compass Direction { get; set; }

        public enum Compass { Up, Down, Left, Right }
        public enum Type { Granite, Rock, Path, Room, Wall }

        public Tile(int row, int col, Type space)
        {
            Row = row;
            Col = col;
            Space = space;
        }
    }
}
