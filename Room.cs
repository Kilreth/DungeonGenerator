using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    /// <summary>
    /// A Room represents the vacant tiles in a room.
    /// A Room may create an Outer room which includes surrounding wall tiles.
    /// </summary>
    public class Room
    {

        public int FirstRow { get; private set; }
        public int FirstCol { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public int Id { get; private set; }
        public int NumTiles { get; private set; }

        public Room Outer { get; private set; }
        public List<Tile> Doors { get; private set; }
        private List<Tile> walls { get; set; }

        public bool CanRoomFit(Dungeon dungeon)
        {
            InitialiseOuter();

            // Is origin corner within the dungeon?
            if (!dungeon.IsTileWithinDungeon(Outer.FirstRow, Outer.FirstCol))
                return false;
            // Is far corner within the dungeon?
            if (!dungeon.IsTileWithinDungeon(Outer.FirstRow + Outer.Height - 1,
                                             Outer.FirstCol + Outer.Width - 1))
                return false;

            // Does the room including outer walls overlap with anything?
            int rowToStop = Outer.FirstRow + Outer.Height;
            int colToStop = Outer.FirstCol + Outer.Width;
            for (int row = Outer.FirstRow; row < rowToStop; row++)
            {
                for (int col = Outer.FirstCol; col < colToStop; col++)
                {
                    // Existing room wall tiles are allowed to overlap
                    // This allows walls to be shared by rooms
                    if (dungeon.GetTile(row, col).Space != Space.Rock
                        && dungeon.GetTile(row, col).Space != Space.Wall)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void GenerateDoors(Dungeon dungeon, double doorToWallRatio)
        {
            FindWallLocations(dungeon);

            // How many doors will we make?
            int numDoors = (int) (walls.Count * doorToWallRatio);
            numDoors += DungeonGenerator.Rng.Next(-1, 2);   // add -1, 0, or 1
            if (numDoors == 0)
            {
                ++numDoors;
            }

            Doors = new List<Tile>();
            Tile door;
            while (Doors.Count < numDoors)
            {
                door = walls[DungeonGenerator.Rng.Next(0, walls.Count)];
                if (!Doors.Contains(door) && !dungeon.IsTileSurroundedBy(door, Space.Door))
                {
                    Doors.Add(door);
                    door.Space = Space.Door;
                    door.Room = this;
                }
            }
        }

        /// <summary>
        /// Finds coordinates of all tiles with an edge next to the room.
        /// Corners are excluded.
        /// </summary>
        public void FindWallLocations(Dungeon dungeon)
        {
            InitialiseOuter();
            walls = new List<Tile>();
            Tile tile;
            for (int row = FirstRow; row < FirstRow + Height; row++)
            {
                tile = dungeon.GetTile(row, Outer.FirstCol);
                tile.Direction = Direction.Left;
                walls.Add(tile);
                tile = dungeon.GetTile(row, Outer.FirstCol + Outer.Width - 1);
                tile.Direction = Direction.Right;
                walls.Add(tile);
            }
            for (int col = FirstCol; col < FirstCol + Width; col++)
            {
                tile = dungeon.GetTile(Outer.FirstRow, col);
                tile.Direction = Direction.Up;
                walls.Add(tile);
                tile = dungeon.GetTile(Outer.FirstRow + Outer.Height - 1, col);
                tile.Direction = Direction.Down;
                walls.Add(tile);
            }
        }

        private void SetNumTiles()
        {
            NumTiles = Height * Width;
        }

        private void InitialiseOuter()
        {
            if (Outer == null)
            {
                Outer = new Room(FirstRow - 1, FirstCol - 1, Height + 2, Width + 2);
            }
        }

        private void Initialise(int firstRow, int firstCol, int height, int width, int id)
        {
            FirstRow = firstRow;
            FirstCol = firstCol;
            Height = height;
            Width = width;
            Id = id;
            SetNumTiles();

            Outer = null;
            Doors = null;
            walls = null;
        }

        public void Replace(int firstRow, int firstCol, int height, int width, int id=-1)
        {
            Initialise(firstRow, firstCol, height, width, id);
        }

        public Room(int firstRow, int firstCol, int height, int width, int id=-1)
        {
            Initialise(firstRow, firstCol, height, width, id);
        }
    }
}
