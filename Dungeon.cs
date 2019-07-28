﻿using System;
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

        public bool IsTileConnectedTo(Tile tile, Space otherType, Tile from=null)
        {
            List<Tile> adjacent = new List<Tile>();
            adjacent.Add(GetTile(tile.Row - 1, tile.Col));
            adjacent.Add(GetTile(tile.Row + 1, tile.Col));
            adjacent.Add(GetTile(tile.Row, tile.Col - 1));
            adjacent.Add(GetTile(tile.Row, tile.Col + 1));
            if (from != null)
            {
                adjacent.Remove(from);
            }
            foreach (Tile adjacentTile in adjacent)
            {
                if (adjacentTile.Space == otherType)
                {
                    return true;
                }
            }
            return false;
        }

        public Tile GetTileByDirection(Tile tile)
        {
            return GetTileByDirection(tile, tile.Direction);
        }

        public Tile GetTileByDirection(Tile tile, Direction direction)
        {
            if (direction == Direction.None)
            {
                throw new ArgumentNullException("direction", "Direction of tile not set");
            }
            if (direction == Direction.Up)
            {
                return Tiles[tile.Row - 1, tile.Col];
            }
            else if (direction == Direction.Down)
            {
                return Tiles[tile.Row + 1, tile.Col];
            }
            else if (direction == Direction.Left)
            {
                return Tiles[tile.Row, tile.Col - 1];
            }
            else //if (direction == Direction.Right)
            {
                return Tiles[tile.Row, tile.Col + 1];
            }
        }

        public Tile GetTile(int row, int col)
        {
            return Tiles[row, col];
        }

        public void CarveRoomDoors(Room room)
        {
            foreach (Tile tile in room.Doors)
            {
                tile.Space = Space.Door;
                // BELOW IS TEMPORARY
                //GetTileByDirection(tile).Space = Space.Path;
            }
        }

        private void CarveRoomHelper(Room room, Space material)
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
            CarveRoomHelper(room.Outer, Space.Wall);
            CarveRoomHelper(room, Space.Room);
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
                Tiles[row, 0] = new Tile(row, 0, Space.Granite);
                Tiles[row, Width-1] = new Tile(row, Width-1, Space.Granite);
            }
            for (int col = 0; col < Width; col++)
            {
                Tiles[0, col] = new Tile(0, col, Space.Granite);
                Tiles[Height-1, col] = new Tile(Height-1, col, Space.Granite);
            }

            // rock interior
            for (int row = 1; row < Height-1; row++)
            {
                for (int col = 1; col < Width-1; col++)
                {
                    Tiles[row, col] = new Tile(row, col, Space.Rock);
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
