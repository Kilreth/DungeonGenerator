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
        public List<Location> Entrances { get; private set; }
        private List<Location> walls { get; set; }

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

        public void GenerateEntrances(double doorToWallRatio)
        {
            FindWallLocations();

            // How many entrances will we make?
            int numEntrances = (int) (walls.Count * doorToWallRatio);
            numEntrances += DungeonGenerator.Rng.Next(-1, 2);   // add -1, 0, or 1
            if (numEntrances == 0)
            {
                ++numEntrances;
            }

            Entrances = new List<Location>();
            while (Entrances.Count < numEntrances)
            {
                int index = DungeonGenerator.Rng.Next(0, walls.Count);
                if (!Entrances.Contains(walls[index]))
                {
                    Entrances.Add(walls[index]);
                }
            }
        }

        /// <summary>
        /// Finds coordinates of all tiles with an edge next to the room.
        /// Corners are excluded.
        /// </summary>
        public void FindWallLocations()
        {
            InitialiseOuter();
            walls = new List<Location>();
            for (int row = FirstRow; row < FirstRow + Height; row++)
            {
                walls.Add(new Location(row, Outer.FirstCol));
                walls.Add(new Location(row, Outer.FirstCol + Outer.Width - 1));
            }
            for (int col = FirstCol; col < FirstCol + Width; col++)
            {
                walls.Add(new Location(Outer.FirstRow, col));
                walls.Add(new Location(Outer.FirstRow + Outer.Height - 1, col));
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
            Entrances = null;
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
