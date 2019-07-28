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

        /// <summary>
        /// Up:    Decreasing rows
        /// Down:  Increasing rows
        /// Left:  Decreasing columns
        /// Right: Increasing columns
        /// </summary>
        public enum Compass { None, Up, Down, Left, Right }

        /// <summary>
        /// Granite: Impervious edge of the dungeon
        /// Rock:    Solid tile with which to fill the dungeon
        /// Room:    Vacant tiles in a room
        /// Wall:    The rock surrounding Room tiles
        /// Path:    Vacant tiles that make corridors connecting doors
        /// Door:    Vacant tiles bridging rooms and paths
        /// </summary>
        public enum Type { Granite, Rock, Room, Wall, Path, Door }

        public static Compass Invert(Compass direction)
        {
            if (direction == Compass.None)
                throw new ArgumentNullException("direction", "Direction of tile not set");
            if (direction == Compass.Up)
                return Compass.Down;
            else if (direction == Compass.Down)
                return Compass.Up;
            else if (direction == Compass.Left)
                return Compass.Right;
            else //if (direction == Compass.Right)
                return Compass.Left;
        }

        public Tile(int row, int col, Type space)
        {
            Row = row;
            Col = col;
            Space = space;
        }
    }
}
