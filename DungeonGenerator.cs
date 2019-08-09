using System;
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

            Tile end = path.Peek().Tile;

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

            // Connect to any adjacent Area objects

            end.Area = area;
            ConnectAreas(dungeon, end);

            // Finally begin carving the corridor

            while (path.Count > 0)
            {
                CorridorTile head = path.Pop();
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
        }

        public void CorridorWalk(Dungeon dungeon, Tile door, double chanceToTurn)
        {
            bool DoorLeadsToOtherRoom(List<Tile> doors)
            {
                foreach (Tile d in doors)
                {
                    if (d.Area != door.Area)
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

                // Have we found a door, or an existing path?
                // If looking for a door, verify not all adjacent doors belong to the room we came from

                if (dungeon.IsTileAdjacentTo(head.Tile, Space.Path, head.From.Tile)
                    || (dungeon.IsTileAdjacentTo(head.Tile, Space.Door, head.From.Tile)
                        && DoorLeadsToOtherRoom(dungeon.GetAdjacentTilesOfType(head.Tile, Space.Door, head.From.Tile))))
                {
                    // Carve the complete path

                    CarveCorridor(dungeon, path);
                    return;
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

            //door.Room.Doors.Remove(door);
            //door.Space = Space.Wall;
        }

        public void GenerateCorridor(Dungeon dungeon, Tile door, double chanceToTurn)
        {
            Tile startOfPath = dungeon.GetTileByDirection(door, door.Direction);

            // If tile outside of door is already an end target, there is nothing to do

            if (Tile.IsWalkable(startOfPath.Space))
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

                CarveCorridor(dungeon, path);
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
        }

        static DungeonGenerator()
        {
            Rng = new Random();
        }
    }
}
