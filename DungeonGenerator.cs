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

        public void MakeConnectedGraph(Dungeon dungeon, double chanceToTurn)
        {
            List<Room> unconnectedRooms = dungeon.FindUnconnectedRooms();
            while (unconnectedRooms.Count > 0)
            {
                foreach (Room r in unconnectedRooms)
                {
                    r.FlagDebug();
                }
                Room room = unconnectedRooms[Rng.Next(0, unconnectedRooms.Count)];
                Tile door = room.GenerateDoor(dungeon);
                if (door != null)
                {
                    door.Debug = true;

                    // Avoid wasting doors on paths that don't connect anywhere new
                    bool allowConnectionToConnectedPath = false;
                    GenerateCorridor(dungeon, door, chanceToTurn, allowConnectionToConnectedPath);
                }
                else
                {
                    return;
                }

                unconnectedRooms = dungeon.FindUnconnectedRooms();
            }
        }

        /// <summary>
        /// Link all adjacent walkable areas to the area a given tile belongs to.
        /// For example, link the end of a path to the room behind a door.
        /// </summary>
        /// <param name="dungeon"></param>
        /// <param name="tile"></param>
        public void ConnectAreas(Dungeon dungeon, Tile tile)
        {
            foreach (Tile adj in dungeon.GetAdjacentTiles(tile))
            {
                if (Tile.IsWalkable(adj.Space))
                {
                    tile.Area.ConnectTo(adj.Area);
                }
            }
        }

        public void CarveCorridor(Dungeon dungeon, Stack<CorridorTile> path)
        {
            // Unwrap the tiles in the stack to populate a set

            HashSet<Tile> tiles = new HashSet<Tile>();
            foreach (CorridorTile wrappedTile in path)
            {
                tiles.Add(wrappedTile.Tile);
            }

            // Get an existing Path object if there is one; otherwise make a new object

            Area area;
            List<Tile> adjacentPaths = dungeon.GetAdjacentTilesOfType(path.Peek().Tile, Space.Path);
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
            ConnectAreas(dungeon, end);

            // Finally begin carving the corridor

            CorridorTile head = null;
            while (path.Count > 0)
            {
                head = path.Pop();
                tiles.Remove(head.Tile);
                List<Tile> adjacents = dungeon.GetAdjacentTiles(head.Tile, head.From.Tile);
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

            ConnectAreas(dungeon, head.Tile);
        }

        /// <summary>
        /// Perform a flood-fill search from a door to another room's door or a path.
        /// </summary>
        /// <param name="dungeon"></param>
        /// <param name="door"></param>
        /// <param name="chanceToTurn"></param>
        public void CorridorWalk(Dungeon dungeon, Tile door, double chanceToTurn, bool allowConnectionToConnectedPath)
        {
            bool RoomUnconnectedToAdjacentPath(List<Tile> paths)
            {
                foreach (Tile p in paths)
                {
                    if (!door.Area.To.Contains(p.Area))
                    {
                        return true;
                    }
                }
                return false;
            }

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

            Tile firstTile = dungeon.GetTileByDirection(door);
            CorridorTile start = new CorridorTile(door, null, door.Direction);
            CorridorTile head = new CorridorTile(firstTile, start, door.Direction);
            path.Push(head);
            visited.Add(head);
            while (path.Count > 0)
            {
                head = path.Peek();

                // Can we carve this tile?

                // If a door is on the head of the stack, it belongs to the room we came from.
                // Treat those doors as walls.

                if (head.Space == Space.Wall || head.Space == Space.Granite || head.Space == Space.Door)
                {
                    path.Pop();
                    continue;
                }

                // Have we found any doors?
                // If all adjacent doors lead to the room we came from, carry on

                if (dungeon.IsTileAdjacentTo(head.Tile, Space.Door, head.From.Tile)
                        && DoorLeadsToOtherRoom(dungeon.GetAdjacentTilesOfType(head.Tile, Space.Door, head.From.Tile)))
                {
                    CarveCorridor(dungeon, path);
                    return;
                }

                // Have we found an existing path? Connect to it and end
                // If we are not allowed to connect to it, then we must skirt around it

                if (dungeon.IsTileAdjacentTo(head.Tile, Space.Path, head.From.Tile))
                {
                    if (allowConnectionToConnectedPath)
                    {
                        CarveCorridor(dungeon, path);
                        return;
                    }
                    else
                    {
                        if (RoomUnconnectedToAdjacentPath(dungeon.GetAdjacentTilesOfType(head.Tile, Space.Path, head.From.Tile)))
                        {
                            CarveCorridor(dungeon, path);
                            return;
                        }
                        else
                        {
                            // Do not allow our path to touch existing paths. Treat this tile as non-carvable

                            path.Pop();
                            continue;
                        }
                    }
                }

                // Decide where to go next, or step back one tile if all paths have been explored

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

            // There are no doors or paths to connect to, so erase this door

            Room room = (Room)door.Area;
            room.Doors.Remove(door);
            door.Space = Space.Wall;
        }

        public void GenerateCorridor(Dungeon dungeon, Tile door, double chanceToTurn, bool allowConnectionToConnectedPath)
        {
            Tile startOfPath = dungeon.GetTileByDirection(door, door.Direction);

            // If door is already connected to a path or another door, there is nothing to do
            // (The initial set of doors touch no other doors, so if a door is adjacent,
            // it was spawned while carving a path straight ahead.)

            if (dungeon.IsTileAdjacentTo(door, Space.WALKABLE, dungeon.GetTileByDirection(door, Tile.Invert(door.Direction))))
            {
                ConnectAreas(dungeon, door);
                return;
            }

            // If the door has opened into a wall, carve straight ahead until the other room can be entered
            // Then exit this method call

            if (startOfPath.Space == Space.Wall)
            {
                // Prepare a stack of path tiles so we can call our general method to carve the corridor

                Stack<CorridorTile> path = new Stack<CorridorTile>();
                CorridorTile wrappedDoor = new CorridorTile(door, null, door.Direction);
                CorridorTile head = new CorridorTile(startOfPath, wrappedDoor, door.Direction);
                path.Push(head);
                while (!dungeon.IsTileAdjacentTo(head.Tile, Space.WALKABLE, head.From.Tile))
                {
                    Tile nextTile = dungeon.GetTileByDirection(head.Tile, door.Direction);
                    head = new CorridorTile(nextTile, head, door.Direction);
                    path.Push(head);
                }

                // If the last tile is directly touching another room's interior, make this tile a door
                // As a wall tile originally, the door tile is already facing out from its room

                Tile otherDoor = path.Peek().Tile;
                if (dungeon.IsTileAdjacentTo(otherDoor, Space.Room))
                {
                    path.Pop();
                    Room otherRoom = (Room)otherDoor.Area;
                    otherRoom.SetTileAsDoor(otherDoor);
                }

                // If we didn't pop our only path tile off of the stack, carve the remaining corridor

                if (path.Count > 0)
                {
                    CarveCorridor(dungeon, path);
                }
                else
                {
                    ConnectAreas(dungeon, door);
                }
                return;
            }

            // If there is solid stone ahead, start a path

            CorridorWalk(dungeon, door, chanceToTurn, allowConnectionToConnectedPath);
        }

        public void GenerateCorridors(Dungeon dungeon, double chanceToTurn)
        {
            foreach (Room room in dungeon.Rooms)
            {
                // If no corridor can be formed from a door, the door is removed
                // So a room's list of doors may shrink as we iterate over it

                bool allowConnectionToConnectedPath = true;
                for (int i = room.Doors.Count - 1; i >= 0; --i)
                {
                    GenerateCorridor(dungeon, room.Doors[i], chanceToTurn, allowConnectionToConnectedPath);
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
            // Calculate how many room tiles we have

            int totalTiles = dungeon.Height * dungeon.Width;
            int remainingRoomTiles = (int) (totalTiles * roomToDungeonRatio);
            int minRoomSize = minRoomHeight * minRoomWidth;

            while (remainingRoomTiles > minRoomSize * 3)
            {
                // Generate random dimensions for a room

                int roomHeight, roomWidth;
                do
                {
                    roomHeight = Rng.Next(minRoomHeight, maxRoomHeight + 1);
                    roomWidth  = Rng.Next(minRoomWidth,  maxRoomWidth  + 1);
                } while (roomHeight * roomWidth > remainingRoomTiles);

                // Create the room and put it in a random place

                Room room = new Room(0, 0, 0, 0);   // initialized null value
                int row, col;
                int dungeonEdge = 2;
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

                room.InitializeArea();
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
            //MakeConnectedGraph(Dungeon, 0.2);
        }

        static DungeonGenerator()
        {
            Rng = new Random();
        }
    }
}
