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

        public void CorridorWalk(Dungeon dungeon, Tile door)
        {
            List<Tile.Compass> MakeDirections(Tile.Compass lastDirection)
            {
                List<Tile.Compass> directions = new List<Tile.Compass>();
                directions.Add(Tile.Compass.Up);
                directions.Add(Tile.Compass.Down);
                directions.Add(Tile.Compass.Left);
                directions.Add(Tile.Compass.Right);
                directions.Remove(Tile.Invert(lastDirection));
                return directions;
            }

            int initialCapacity = 128;
            Dictionary<Tile, Tile> from = new Dictionary<Tile, Tile>(initialCapacity);
            Dictionary<Tile, List<Tile.Compass>> directionsToTry
                = new Dictionary<Tile, List<Tile.Compass>>(initialCapacity);
            HashSet<Tile> visited = new HashSet<Tile>(initialCapacity);
            Stack<Tile> path = new Stack<Tile>(initialCapacity);

            Tile head = dungeon.GetTileByDirection(door);
            path.Push(head);
            visited.Add(head);
            from.Add(head, door);
            directionsToTry.Add(head, MakeDirections(door.Direction));
            while (path.Count > 0)
            {
                head = path.Peek();
                if (dungeon.IsTileConnectedTo(head, Tile.Type.Path, from[head])
                    || dungeon.IsTileConnectedTo(head, Tile.Type.Door, from[head]))
                {
                    // finalize path and exit
                }

                //if (directionsToTry[from[head].Direction)
            }
        }

        public void GenerateCorridor(Dungeon dungeon, Tile door)
        {
            Tile startOfPath = dungeon.GetTileByDirection(door, door.Direction);

            // If tile outside of door is already an end target, there is nothing to do
            if (startOfPath.Space == Tile.Type.Path || startOfPath.Space == Tile.Type.Door
                || startOfPath.Space == Tile.Type.Room)
            {
                return;
            }

            // If the door has opened into a wall, carve straight ahead until the room can be entered
            if (startOfPath.Space == Tile.Type.Wall)
            {
                Tile current = startOfPath;
                Tile previous = door;
                while (!dungeon.IsTileConnectedTo(current, Tile.Type.Room)
                    && !dungeon.IsTileConnectedTo(current, Tile.Type.Door, previous))   // don't count door we came from
                {
                    current.Space = Tile.Type.Path;
                    previous = current;
                    current = dungeon.GetTileByDirection(current, door.Direction);
                }
                current.Space = Tile.Type.Path;
            }

            // If there is solid stone ahead, start a path

            /*if (dungeon.IsTileConnectedTo(door, Tile.Type.Path)
                || dungeon.IsTileConnectedTo(door, Tile.Type.Door)
                || dungeon.IsTileConnectedTo(door, Tile.Type.Room, dungeon.GetTileByDirection(door, Tile.Invert(door.Direction))))
            {

            }*/
        }

        public void GenerateCorridors(Dungeon dungeon, double turnChance)
        {
            foreach (Room room in dungeon.Rooms)
            {
                foreach (Tile door in room.Doors)
                {
                    GenerateCorridor(dungeon, door);
                }
            }
        }

        public void GenerateDoors(Dungeon dungeon, double doorToWallRatio)
        {
            foreach (Room room in dungeon.Rooms)
            {
                room.GenerateDoors(dungeon, doorToWallRatio);
                dungeon.CarveRoomDoors(room);
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
            GenerateCorridors(Dungeon, 0.7);
        }

        static DungeonGenerator()
        {
            Rng = new Random();
        }
    }
}
