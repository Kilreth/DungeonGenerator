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
        public Tile[,] Tiles { get; private set; }
        public List<Room> Rooms { get; private set; }

        public Tile GetTile(int row, int col)
        {
            //if (row < 0 || col < 0 || row >= Height || col >= Width)
            //    return null;
            return Tiles[row, col];
        }

        public void CarveRoomDoors(Room room)
        {
            foreach (Location loc in room.Entrances)
            {
                Tiles[loc.Row, loc.Col].Space = Tile.Type.Path;
            }
        }

        private void CarveRoomHelper(Room room, Tile.Type material)
        {
            int rowToStop = room.FirstRow + room.Height;
            int colToStop = room.FirstCol + room.Width;
            for (int row = room.FirstRow; row < rowToStop; row++)
            {
                for (int col = room.FirstCol; col < colToStop; col++)
                {
                    Tiles[row, col].Space = material;
                }
            }
        }

        public void CarveRoom(Room room)
        {
            CarveRoomHelper(room.Outer, Tile.Type.Wall);
            CarveRoomHelper(room, Tile.Type.Room);
            Rooms.Add(room);
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
                    dungeon[row, col] = new Tile(Tile.Type.Rock);
                }
            }
            return dungeon;
        }

        public Dungeon(int height, int width)
        {
            Height = height;
            Width = width;
            Tiles = Initialize(height, width);
            Rooms = new List<Room>();
        }
    }
}
