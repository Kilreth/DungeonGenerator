﻿using System;
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
    public class Room : Area
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
            if (numDoors < 1)
            {
                numDoors = 1;
            }

            while (Doors.Count < numDoors)
            {
                GenerateDoor(dungeon);
            }
        }

        public Tile GenerateDoor(Dungeon dungeon)
        {
            int tries = 0;
            while (tries < 100)
            {
                Tile door = walls[DungeonGenerator.Rng.Next(0, walls.Count)];
                if (!Doors.Contains(door) && !dungeon.IsTileSurroundedBy(door, Space.Door)
                    && dungeon.GetTileByDirection(door).Space != Space.Granite)
                {
                    SetTileAsDoor(door);
                    return door;
                }
                ++tries;
            }
            foreach (Tile tile in walls)
            {
                tile.Debug = true;
            }
            return null;
            //throw new InvalidOperationException("Room for another door could not be found");
        }

        public void SetTileAsDoor(Tile tile)
        {
            Doors.Add(tile);
            tile.Space = Space.Door;
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

        public void FlagDebug()
        {
            foreach (Tile tile in walls)
            {
                tile.Debug = true;
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
            Doors = new List<Tile>();
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
