﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Dungeon_Generator
{
    public class DungeonGenerator
    {
        public Dungeon Dungeon { get; }
        public static Random Rng { get; }

        public void CorridorWalk(Dungeon dungeon, Tile door, double chanceToTurn)
        {
            HashSet<CorridorTile> visited = new HashSet<CorridorTile>();
            Stack<CorridorTile> path = new Stack<CorridorTile>();

            Tile headTile = dungeon.GetTileByDirection(door);
            CorridorTile start = new CorridorTile(door, null, door.Direction);
            CorridorTile head = new CorridorTile(headTile, start, door.Direction);
            path.Push(head);
            visited.Add(head);
            while (path.Count > 0)
            {
                head = path.Peek();

                // Can we carve this tile?
                if (head.Space == Space.Wall || head.Space == Space.Granite)
                {
                    path.Pop();
                    continue;
                }

                // Have we found a door, or an existing path?
                if (dungeon.IsTileConnectedTo(head.Tile, Space.Path, head.From.Tile)
                    || dungeon.IsTileConnectedTo(head.Tile, Space.Door, head.From.Tile))
                {
                    while (path.Count > 0)
                    {
                        path.Pop().Tile.Space = Space.Path;
                    }
                    break;
                }

                if (head.DirectionsToTry.Count > 0)
                {
                    CorridorTile next;
                    do
                    {
                        next = head.ChooseNextTile(dungeon, chanceToTurn);
                    } while (visited.Contains(next) && head.DirectionsToTry.Count > 0);
                    if (visited.Contains(next))
                    {
                        path.Pop();
                    }
                    else
                    {
                        path.Push(next);
                        visited.Add(next);
                    }
                }
                else
                {
                    path.Pop();
                }
            }
        }

        public void GenerateCorridor(Dungeon dungeon, Tile door, double chanceToTurn)
        {
            Tile startOfPath = dungeon.GetTileByDirection(door, door.Direction);

            // If tile outside of door is already an end target, there is nothing to do

            if (startOfPath.Space == Space.Path || startOfPath.Space == Space.Door
                || startOfPath.Space == Space.Room)
            {
                return;
            }

            // If the door has opened into a wall, carve straight ahead until the room can be entered

            if (startOfPath.Space == Space.Wall)
            {
                Tile current = startOfPath;
                Tile previous = door;
                while (!dungeon.IsTileConnectedTo(current, Space.Room)
                    && !dungeon.IsTileConnectedTo(current, Space.Door, previous))   // don't count door we came from
                {
                    current.Space = Space.Path;
                    previous = current;
                    current = dungeon.GetTileByDirection(current, door.Direction);
                }
                current.Space = Space.Path;
                return;
            }

            // If there is solid stone ahead, start a path

            CorridorWalk(dungeon, door, chanceToTurn);
        }

        public void GenerateCorridors(Dungeon dungeon, double chanceToTurn)
        {
            foreach (Room room in dungeon.Rooms)
            {
                foreach (Tile door in room.Doors)
                {
                    GenerateCorridor(dungeon, door, chanceToTurn);
                }
            }
        }

        public void GenerateDoors(Dungeon dungeon, double doorToWallRatio)
        {
            foreach (Room room in dungeon.Rooms)
            {
                room.GenerateDoors(dungeon, doorToWallRatio);
            }
        }

        public void GenerateRooms(Dungeon dungeon, double roomToDungeonRatio,
                                     int minRoomHeight, int minRoomWidth, int maxRoomHeight, int maxRoomWidth)
        {
            List<Room> rooms = new List<Room>();

            // calculate how many room tiles we have
            int totalTiles = dungeon.Height * dungeon.Width;
            int remainingRoomTiles = (int) (totalTiles * roomToDungeonRatio);
            int minRoomSize = minRoomHeight * minRoomWidth;
            while (remainingRoomTiles > minRoomSize * 3)
            {
                Debug.WriteLine("go");
                // generate random dimensions for a room
                int roomHeight, roomWidth;
                do
                {
                    roomHeight = Rng.Next(minRoomHeight, maxRoomHeight + 1);
                    roomWidth  = Rng.Next(minRoomWidth,  maxRoomWidth  + 1);
                } while (roomHeight * roomWidth > remainingRoomTiles);

                // create the room and put it in a random place

                Room room = new Room(0, 0, 0, 0);
                int row, col;
                int dungeonEdge = 3;
                int attempts = 0;
                do
                {
                    row = Rng.Next(dungeonEdge, dungeon.Height - roomHeight - dungeonEdge + 1);
                    col = Rng.Next(dungeonEdge, dungeon.Width - roomWidth - dungeonEdge + 1);
                    room.Replace(row, col, roomHeight, roomWidth);
                    ++attempts;
                } while (!room.CanRoomFit(dungeon) && attempts != 100);
                if (attempts == 100)
                {
                    break;
                }
                dungeon.CarveRoom(room);
                remainingRoomTiles -= roomHeight * roomWidth;
            }
        }

        public DungeonGenerator(int height, int width)
        {
            Dungeon = new Dungeon(height, width);
            GenerateRooms(Dungeon, 0.9, 3, 3, 9, 9);
            GenerateDoors(Dungeon, 0.1);
            GenerateCorridors(Dungeon, 0.2);
        }

        static DungeonGenerator()
        {
            Rng = new Random();
        }
    }
}
