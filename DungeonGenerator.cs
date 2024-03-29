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

        /// <summary>
        /// Choose a random start room.
        /// Choose the farthest room from that as the goal room.
        /// Choose a room with medium distance to place the key in.
        /// 
        /// Rooms are ordered by BFS so the heuristic is number of rooms+paths crossed.
        /// </summary>
        public void GenerateStairsAndKey()
        {
            // Although very unlikely, it is possible not all rooms at this point are connected
            // Choose a random room that belongs to the connected dungeon
            Room startRoom;
            List<Room> roomsFromStart;
            do
            {
                startRoom = Dungeon.GetRandomRoom();
                roomsFromStart = Dungeon.FindConnectedRooms(startRoom);
            }
            while (roomsFromStart.Count * 2 < Dungeon.Rooms.Count);
            int totalRooms = roomsFromStart.Count;

            // DEBUG: Draw BFS order on screen
            for (int i = 0; i < totalRooms; ++i)
            {
                roomsFromStart[i].GetRandomTile().Text = i.ToString();
            }

            // The goal room is the farthest room encountered in the breadth-first search
            // The room with the key is somewhere in the middle
            Room goalRoom = roomsFromStart[totalRooms - 1];
            Room keyRoom  = roomsFromStart[Rng.Next((int)(totalRooms * 0.4), (int)(totalRooms * 0.6))];

            startRoom.GetRandomTile().Space = Space.StairsUp;
            goalRoom.GetRandomTile().Space = Space.StairsDown;
            keyRoom.GetRandomTile().Space = Space.Key;
        }

        /// <summary>
        /// Prior corridor generation simply looks for other rooms and paths to connect to.
        /// There is no guarantee that every room can be reached from a given room.
        /// Here we attempt to find rooms unconnected to the largest graph, and connect
        /// them by *spawning a new door and attempting to generate a new corridor*.
        /// 
        /// Because there may be limited space for corridors, we must not waste it, so we discard
        /// any new doors+corridors that make a duplicate connection.
        /// 
        /// There may be unconnected rooms even after this, but they are very rare.
        /// Moreover, because stair and key generation use the connected graph,
        /// any isolated rooms are harmless.
        /// </summary>
        /// <param name="chanceToTurn"></param>
        public void MakeDungeonACompleteGraph(double chanceToTurn)
        {
            List<Room> unconnectedRooms = Dungeon.FindUnconnectedRooms(Dungeon.GetRandomRoom());
            int tries = 0;
            while (unconnectedRooms.Count > 0 && tries < Dungeon.Rooms.Count)
            {
                foreach (Room r in unconnectedRooms)
                {
                    r.FlagDebug();
                }
                Room room = unconnectedRooms[Rng.Next(0, unconnectedRooms.Count)];
                Tile door = room.GenerateDoor();
                if (door != null)
                {
                    door.Debug = true;

                    // Avoid wasting doors on paths that don't connect anywhere new
                    bool allowConnectionToConnectedArea = false;
                    GenerateCorridor(door, chanceToTurn, allowConnectionToConnectedArea);
                }

                unconnectedRooms = Dungeon.FindUnconnectedRooms(Dungeon.GetRandomRoom());
                ++tries;
            }
        }

        /// <summary>
        /// Link all adjacent walkable areas to the area a given tile belongs to.
        /// For example, link the end of a path to the room belonging to a door.
        /// </summary>
        /// <param name="tile"></param>
        public void ConnectAreas(Tile tile)
        {
            foreach (Tile adj in Dungeon.GetAdjacentTiles(tile))
            {
                if (Tile.IsWalkable(adj.Space))
                {
                    tile.Area.ConnectTo(adj.Area);
                }
            }
        }

        /// <summary>
        /// Commits a stack of wrapped corridor tiles to the dungeon.
        /// </summary>
        /// <param name="path"></param>
        public void CarveCorridor(Stack<CorridorTile> path)
        {
            // Unwrap the tiles in the stack to populate a set

            HashSet<Tile> tiles = new HashSet<Tile>();
            foreach (CorridorTile wrappedTile in path)
            {
                tiles.Add(wrappedTile.Tile);
            }

            // Get an existing Path object if there is one; otherwise make a new object

            Area area;
            List<Tile> adjacentPaths = Dungeon.GetAdjacentTilesOfType(path.Peek().Tile, Space.Path);
            if (adjacentPaths.Count > 0)
            {
                area = adjacentPaths[0].Area;
            }
            else
            {
                area = new Path();
                area.InitializeArea();
            }

            // Connect the *end* of the path to any adjacent Area objects

            Tile end = path.Peek().Tile;
            end.Area = area;
            ConnectAreas(end);

            // Finally begin carving the corridor

            CorridorTile head = null;
            while (path.Count > 0)
            {
                head = path.Pop();
                tiles.Remove(head.Tile);
                List<Tile> adjacents = Dungeon.GetAdjacentTiles(head.Tile, head.From.Tile);
                foreach (Tile adjacent in adjacents)
                {
                    if (tiles.Contains(adjacent))
                    {
                        // Loops exists, trim it

                        while (path.Peek().Tile != adjacent)
                        {
                            Tile remove = path.Pop().Tile;
                            tiles.Remove(remove);
                        }
                    }
                }
                head.Tile.Space = Space.Path;
                head.Tile.Area = area;
            }

            // Connect the *start* of the path to the room it came from

            ConnectAreas(head.Tile);
        }

        /// <summary>
        /// Perform a flood-fill search from a door to another room's door or a path.
        /// </summary>
        /// <param name="door"></param>
        /// <param name="chanceToTurn"></param>
        public void CorridorWalk(Tile door, double chanceToTurn, bool allowConnectionToConnectedArea)
        {
            bool DoorLeadsToOtherRoom(List<Tile> doors)
            {
                foreach (Tile d in doors)
                {
                    if (door.Area != d.Area)
                    {
                        return true;
                    }
                }
                return false;
            }

            HashSet<CorridorTile> visited = new HashSet<CorridorTile>();
            Stack<CorridorTile> path = new Stack<CorridorTile>();

            Tile firstTile = Dungeon.GetTileByDirection(door);
            CorridorTile start = new CorridorTile(door, null, door.Direction);
            CorridorTile head = new CorridorTile(firstTile, start, door.Direction);
            path.Push(head);
            visited.Add(head);
            while (path.Count > 0)
            {
                head = path.Peek();

                // Can we carve this tile?

                // If a door is on the head of the stack, it belongs to the room we came from.
                // (If we had found a door to another room, we would already have exited the while loop)
                // Treat these doors as walls.

                if (head.Space == Space.Wall || head.Space == Space.Granite || head.Space == Space.Door)
                {
                    path.Pop();
                    continue;
                }

                // Have we found any doors?
                // If all adjacent doors lead to the room we came from, carry on

                if (Dungeon.IsTileAdjacentTo(head.Tile, Space.Door, head.From.Tile)
                        && DoorLeadsToOtherRoom(Dungeon.GetAdjacentTilesOfType(head.Tile, Space.Door, head.From.Tile)))
                {
                    CarveCorridor(path);
                    return;
                }

                // Have we found an existing path? Connect to it and end
                // If we are not allowed to connect to it, then we must skirt around it

                if (Dungeon.IsTileAdjacentTo(head.Tile, Space.Path, head.From.Tile))
                {
                    if (allowConnectionToConnectedArea || RoomUnconnectedToAdjacentArea(door.Area,
                        Dungeon.GetAdjacentTilesOfType(head.Tile, Space.Path, head.From.Tile)))
                    {
                        CarveCorridor(path);
                        return;
                    }
                    else
                    {
                        // Do not allow our path to touch existing paths. Treat this tile as non-carvable

                        path.Pop();
                        continue;
                    }
                }

                // Decide where to go next, or step back one tile if all paths have been explored

                if (head.DirectionsToTry.Count > 0)
                {
                    CorridorTile next;
                    do
                    {
                        next = head.ChooseNextTile(Dungeon, chanceToTurn);
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

            // There are no doors or paths to connect to, so erase this door
            EraseDoor(door);
        }

        /// <summary>
        /// If a door opens into the wall of another room, carving straight ahead is guaranteed
        /// to open into that room. Make a door at the other end.
        /// It's possible that if the door opens into the corner of another room, our corridor
        /// opens into another corridor first. This is fine, just don't make the last tile a door.
        /// </summary>
        /// <param name="door"></param>
        /// <param name="startOfPath"></param>
        public void CorridorThroughRoomWall(Tile door, Tile startOfPath, bool allowConnectionToConnectedArea)
        {
            // Prepare a stack of path tiles so we can call our general method to carve the corridor

            Stack<CorridorTile> path = new Stack<CorridorTile>();
            CorridorTile wrappedDoor = new CorridorTile(door, null, door.Direction);
            CorridorTile head = new CorridorTile(startOfPath, wrappedDoor, door.Direction);
            path.Push(head);
            while (!Dungeon.IsTileAdjacentTo(head.Tile, Space.WALKABLE, head.From.Tile))
            {
                Tile nextTile = Dungeon.GetTileByDirection(head.Tile, door.Direction);
                head = new CorridorTile(nextTile, head, door.Direction);
                path.Push(head);
            }

            if (!allowConnectionToConnectedArea && !RoomUnconnectedToAdjacentArea(door.Area,
                Dungeon.GetAdjacentTilesOfType(head.Tile, Space.WALKABLE, head.From.Tile)))
            {
                EraseDoor(door);
                return;
            }

            // If the last tile is directly touching another room's interior, make this tile a door
            // As a wall tile originally, the door tile is already facing out from its room

            Tile otherDoor = path.Peek().Tile;
            if (Dungeon.IsTileAdjacentTo(otherDoor, Space.Room))
            {
                path.Pop();
                Room otherRoom = (Room)otherDoor.Area;
                otherRoom.SetTileAsDoor(otherDoor);
            }

            // If we didn't pop our only path tile off of the stack, carve the remaining corridor

            if (path.Count > 0)
            {
                CarveCorridor(path);
            }
            else
            {
                ConnectAreas(door);
            }
        }

        /// <summary>
        /// Account for all the cases a door may open into.
        ///   - Another room. Carve no corridor, just link areas
        ///   - A wall of another room. Carve a corridor straight ahead until a walkable space is found
        ///   - Solid rock. Call the flood-fill search to find a door or path for a corridor to connect to
        /// </summary>
        /// <param name="door"></param>
        /// <param name="chanceToTurn"></param>
        /// <param name="allowConnectionToConnectedArea"></param>
        public void GenerateCorridor(Tile door, double chanceToTurn, bool allowConnectionToConnectedArea)
        {
            Tile startOfPath = Dungeon.GetTileByDirection(door, door.Direction);

            // If door is already connected to a path or another door, there is nothing to do
            // (The initial set of doors touch no other doors, so if a door is adjacent,
            // it was spawned while carving a path straight ahead.)

            if (Dungeon.IsTileAdjacentTo(door, Space.WALKABLE, Dungeon.GetTileByDirection(door, Tile.Invert(door.Direction))))
            {
                if (allowConnectionToConnectedArea || !door.Area.To.Contains(startOfPath.Area))
                {
                    ConnectAreas(door);
                }
                else
                {
                    EraseDoor(door);
                }
                return;
            }

            // If the door has opened into a wall, carve straight ahead until the other room can be entered
            // Then exit this method call

            if (startOfPath.Space == Space.Wall)
            {
                CorridorThroughRoomWall(door, startOfPath, allowConnectionToConnectedArea);
                return;
            }

            // If there is solid stone ahead, start a path

            CorridorWalk(door, chanceToTurn, allowConnectionToConnectedArea);
        }

        private bool RoomUnconnectedToAdjacentArea(Area area, List<Tile> adjacents)
        {
            foreach (Tile adj in adjacents)
            {
                if (!area.IsRecursivelyConnectedTo(adj.Area))
                {
                    return true;
                }
            }
            return false;
        }

        private void EraseDoor(Tile door)
        {
            Room room = (Room)door.Area;
            room.Doors.Remove(door);
            door.Space = Space.Wall;
        }

        public void GenerateCorridors(double chanceToTurn)
        {
            foreach (Room room in Dungeon.Rooms)
            {
                // If no corridor can be formed from a door, the door is removed
                // So a room's list of doors may shrink as we iterate over it

                bool allowConnectionToConnectedArea = true;
                for (int i = room.Doors.Count - 1; i >= 0; --i)
                {
                    GenerateCorridor(room.Doors[i], chanceToTurn, allowConnectionToConnectedArea);
                }
            }
        }

        public void GenerateDoors(double doorToWallRatio)
        {
            foreach (Room room in Dungeon.Rooms)
            {
                room.GenerateDoors(doorToWallRatio);
            }
        }

        /// <summary>
        /// Randomly fill an empty dungeon with rooms until a space ratio is reached,
        /// or until adding another room is unsuccessful after a number of tries.
        /// </summary>
        /// <param name="roomToDungeonRatio"></param>
        /// <param name="minRoomHeight"></param>
        /// <param name="minRoomWidth"></param>
        /// <param name="maxRoomHeight"></param>
        /// <param name="maxRoomWidth"></param>
        public void GenerateRooms(double roomToDungeonRatio,
                                     int minRoomHeight, int minRoomWidth, int maxRoomHeight, int maxRoomWidth)
        {
            // Calculate how many room tiles we have

            int totalTiles = Dungeon.Height * Dungeon.Width;
            int remainingRoomTiles = (int)(totalTiles * roomToDungeonRatio);
            int minRoomSize = minRoomHeight * minRoomWidth;

            while (remainingRoomTiles > minRoomSize * 3)
            {
                // Generate random dimensions for a room

                int roomHeight, roomWidth;
                do
                {
                    roomHeight = Rng.Next(minRoomHeight, maxRoomHeight + 1);
                    roomWidth  = Rng.Next(minRoomWidth, maxRoomWidth + 1);
                } while (roomHeight * roomWidth > remainingRoomTiles);

                // Create the room and put it in a random place

                Room room;
                int row, col;
                int dungeonEdge = 2;
                int maxAttempts = 500;
                int attempts = 0;
                do
                {
                    row = Rng.Next(dungeonEdge, Dungeon.Height - roomHeight - dungeonEdge + 1);
                    col = Rng.Next(dungeonEdge, Dungeon.Width - roomWidth - dungeonEdge + 1);
                    room = new Room(Dungeon, row, col, roomHeight, roomWidth);
                    ++attempts;
                } while (!room.CanRoomFit() && attempts != maxAttempts);
                if (attempts == maxAttempts)
                {
                    break;
                }

                room.InitializeArea();
                Dungeon.CarveRoom(room);

                // If rooms are not allowed to share walls, then roomHeight and roomWidth need +2 size below
                remainingRoomTiles -= roomHeight * roomWidth;
            }
        }

        public DungeonGenerator(int height, int width)
        {
            Dungeon = new Dungeon(height, width);
            GenerateRooms(0.9, 3, 3, 9, 9);
            GenerateDoors(0.1);
            GenerateCorridors(0.2);
            //MakeDungeonACompleteGraph(0.2);
            //GenerateStairsAndKey();
        }

        static DungeonGenerator()
        {
            Rng = new Random();
        }
    }
}
