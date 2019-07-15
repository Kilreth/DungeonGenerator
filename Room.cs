using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    /// <summary>
    /// A Room represents the vacant tiles in a room.
    /// </summary>
    public class Room
    {
        private int firstRow;
        private int firstCol;
        private int height;
        private int width;

        public int FirstRow { get { return firstRow; } }
        public int FirstCol { get { return firstCol; } }
        public int Height { get { return height; } }
        public int Width { get { return width; } }

        private void Initialise(int firstRow, int firstCol, int height, int width)
        {
            this.firstRow = firstRow;
            this.firstCol = firstCol;
            this.height = height;
            this.width = width;
        }

        public void Replace(int firstRow, int firstCol, int height, int width)
        {
            Initialise(firstRow, firstCol, height, width);
        }

        public Room(int firstRow, int firstCol, int height, int width)
        {
            Initialise(firstRow, firstCol, height, width);
        }
    }
}
