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

        public int FirstRow { get; private set; }
        public int FirstCol { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public bool CanRoomFit(Dungeon dungeon)
        {
            Room roomIncWalls = new Room(FirstRow - 1, FirstCol - 1,
                                         Height + 2, Width + 2);

            // is origin corner within the dungeon?
            if (roomIncWalls.FirstRow < 0 || roomIncWalls.FirstCol < 0)
                return false;
            // is far corner within the dungeon?
            if (roomIncWalls.FirstRow + roomIncWalls.Height - 1 >= dungeon.Height)
                return false;
            if (roomIncWalls.FirstCol + roomIncWalls.Width - 1 >= dungeon.Width)
                return false;

            int rowToStop = roomIncWalls.FirstRow + roomIncWalls.Height;
            int colToStop = roomIncWalls.FirstCol + roomIncWalls.Width;
            for (int row = roomIncWalls.FirstRow; row < rowToStop; row++)
            {
                for (int col = roomIncWalls.FirstCol; col < colToStop; col++)
                {
                    if (dungeon.GetTile(row, col).Space != Tile.Type.Solid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void Initialise(int firstRow, int firstCol, int height, int width)
        {
            FirstRow = firstRow;
            FirstCol = firstCol;
            Height = height;
            Width = width;
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
