using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Dungeon
    {
        public int Height { get; }
        public int Width { get; }
        private Tile[,] tiles;
        private List<Room> rooms;
        public Tile[,] Tiles { get { return tiles; } }
        public List<Room> Rooms { get { return rooms; } }

        public Tile GetTile(int row, int col)
        {
            //if (row < 0 || col < 0 || row >= Height || col >= Width)
            //    return null;
            return tiles[row, col];
        }

        public void CarveRoom(Room room)
        {
            int rowToStop = room.FirstRow + room.Height;
            int colToStop = room.FirstCol + room.Width;
            for (int row = room.FirstRow; row < rowToStop; row++)
            {
                for (int col = room.FirstCol; col < colToStop; col++)
                {
                    tiles[row, col].Space = Tile.Type.Room;
                }
            }
            rooms.Add(room);
        }

        /// <summary>
        /// Creates and returns a blank slate for a dungeon.
        /// </summary>
        /// <param name="height">total dungeon rows</param>
        /// <param name="width">total dungeon columns</param>
        /// <returns>2D array of solid dungeon tiles</returns>
        public Tile[,] Initialize(int height, int width)
        {
            Tile[,] dungeon = new Tile[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    dungeon[row, col] = new Tile(Tile.Type.Solid);
                }
            }
            return dungeon;
        }

        public Dungeon(int height, int width)
        {
            Height = height;
            Width = width;
            tiles = Initialize(height, width);
            rooms = new List<Room>();
        }
    }
}
