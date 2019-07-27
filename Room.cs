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
        public int NumTiles { get; private set; }

        public Room Outer { get; private set; }
        public List<Tile> Doors { get; private set; }
        private List<Tile> walls { get; set; }

        public bool CanRoomFit(Dungeon dungeon)
        {
            InitialiseOuter();

            // is origin corner within the dungeon?
            if (Outer.FirstRow < 0 || Outer.FirstCol < 0)
                return false;
            // is far corner within the dungeon?
            if (Outer.FirstRow + Outer.Height - 1 >= dungeon.Height)
                return false;
            if (Outer.FirstCol + Outer.Width - 1 >= dungeon.Width)
                return false;

            // does the room including outer walls overlap with anything?
            int rowToStop = Outer.FirstRow + Outer.Height;
            int colToStop = Outer.FirstCol + Outer.Width;
            for (int row = Outer.FirstRow; row < rowToStop; row++)
            {
                for (int col = Outer.FirstCol; col < colToStop; col++)
                {
                    if (dungeon.GetTile(row, col).Space != Tile.Type.Rock)
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
            while (Doors.Count < numDoors)
            {
                int index = DungeonGenerator.Rng.Next(0, walls.Count);
                if (!Doors.Contains(walls[index]))
                {
                    Doors.Add(walls[index]);
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
            for (int row = FirstRow; row < FirstRow + Height; row++)
            {
                walls.Add(dungeon.GetTile(row, Outer.FirstCol));
                walls.Add(dungeon.GetTile(row, Outer.FirstCol + Outer.Width - 1));
            }
            for (int col = FirstCol; col < FirstCol + Width; col++)
            {
                walls.Add(dungeon.GetTile(Outer.FirstRow, col));
                walls.Add(dungeon.GetTile(Outer.FirstRow + Outer.Height - 1, col));
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

        private void Initialise(int firstRow, int firstCol, int height, int width)
        {
            FirstRow = firstRow;
            FirstCol = firstCol;
            Height = height;
            Width = width;
            SetNumTiles();

            Outer = null;
            Doors = null;
            walls = null;
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
