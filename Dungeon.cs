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

        public bool IsTileAdjacentTo(Tile tile, Space otherType, Tile from = null)
        {
            return GetAdjacentTilesOfType(tile, otherType, from).Count > 0;
        }

        public bool IsTileSurroundedBy(Tile tile, Space otherType, Tile from = null)
        {
            return GetSurroundingTilesOfType(tile, otherType, from).Count > 0;
        }

        public List<Tile> GetAdjacentTilesOfType(Tile tile, Space otherType, Tile from = null)
        {
            return TilesOfType(GetSurroundingTilesImpl(tile, from, false), otherType);
        }

        public List<Tile> GetSurroundingTilesOfType(Tile tile, Space otherType, Tile from = null)
        {
            return TilesOfType(GetSurroundingTilesImpl(tile, from, true), otherType);
        }

        public List<Tile> GetAdjacentTiles(Tile tile, Tile from = null)
        {
            return GetSurroundingTilesImpl(tile, from, false);
        }

        public List<Tile> GetSurroundingTiles(Tile tile, Tile from = null)
        {
            return GetSurroundingTilesImpl(tile, from, true);
        }

        public List<Tile> TilesOfType(List<Tile> surrounding, Space space)
        {
            for (int i = surrounding.Count - 1; i >= 0; --i)
            {
                if (space == Space.WALKABLE)
                {
                    if (!Tile.IsWalkable(surrounding[i]))
                    {
                        surrounding.RemoveAt(i);
                    }
                }
                else if (surrounding[i].Space != space)
                {
                    surrounding.RemoveAt(i);
                }
            }
            return surrounding;
        }

        private List<Tile> GetSurroundingTilesImpl(Tile tile, Tile from, bool includeCorners)
        {
            List<Tile> surrounding = new List<Tile>
            {
                GetTile(tile.Row - 1, tile.Col),
                GetTile(tile.Row + 1, tile.Col),
                GetTile(tile.Row, tile.Col - 1),
                GetTile(tile.Row, tile.Col + 1)
            };
            if (includeCorners)
            {
                surrounding.Add(GetTile(tile.Row - 1, tile.Col - 1));
                surrounding.Add(GetTile(tile.Row - 1, tile.Col + 1));
                surrounding.Add(GetTile(tile.Row + 1, tile.Col - 1));
                surrounding.Add(GetTile(tile.Row + 1, tile.Col + 1));
            }
            if (from != null)
            {
                surrounding.Remove(from);
            }
            return surrounding;
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
                return GetTile(tile.Row - 1, tile.Col);
            }
            else if (direction == Direction.Down)
            {
                return GetTile(tile.Row + 1, tile.Col);
            }
            else if (direction == Direction.Left)
            {
                return GetTile(tile.Row, tile.Col - 1);
            }
            else if (direction == Direction.Right)
            {
                return GetTile(tile.Row, tile.Col + 1);
            }
            throw new ArgumentNullException("direction", "Unknown how to use this direction");
        }

        public bool IsTileWithinDungeon(int row, int col)
        {
            if (row < 0 || col < 0)
            {
                return false;
            }
            if (row >= Height || col >= Width)
            {
                return false;
            }
            return true;
        }

        public Tile GetTile(int row, int col)
        {
            return Tiles[row, col];
        }

        private void CarveRoomHelper(Room room, Space space)
        {
            int rowToStop = room.FirstRow + room.Height;
            int colToStop = room.FirstCol + room.Width;
            for (int row = room.FirstRow; row < rowToStop; row++)
            {
                for (int col = room.FirstCol; col < colToStop; col++)
                {
                    Tile tile = GetTile(row, col);
                    tile.Space = space;
                    tile.Area = room;
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

            // Impervious granite edge of the dungeon
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

            // Rock interior
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
