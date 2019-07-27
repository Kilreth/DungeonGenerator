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
            return Tiles[row, col];
        }

        public void CarveRoomDoors(Room room)
        {
            foreach (Tile tile in room.Entrances)
            {
                Tiles[tile.Row, tile.Col].Space = Tile.Type.Path;
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
        public void Initialize()
        {
            Tiles = new Tile[Height, Width];

            // impervious granite edge of the dungeon
            for (int row = 0; row < Height; row++)
            {
                Tiles[row, 0] = new Tile(row, 0, Tile.Type.Granite);
                Tiles[row, Width-1] = new Tile(row, Width-1, Tile.Type.Granite);
            }
            for (int col = 0; col < Width; col++)
            {
                Tiles[0, col] = new Tile(0, col, Tile.Type.Granite);
                Tiles[Height-1, col] = new Tile(Height-1, col, Tile.Type.Granite);
            }

            // rock interior
            for (int row = 1; row < Height-1; row++)
            {
                for (int col = 1; col < Width-1; col++)
                {
                    Tiles[row, col] = new Tile(row, col, Tile.Type.Rock);
                }
            }
        }

        public Dungeon(int height, int width)
        {
            Height = height;
            Width = width;
            Initialize();
            Rooms = new List<Room>();
        }
    }
}
