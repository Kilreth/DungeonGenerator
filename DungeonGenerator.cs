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

        public void GenerateCorridor(Dungeon dungeon, Tile door)
        {

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
        }

        static DungeonGenerator()
        {
            Rng = new Random();
        }
    }
}
