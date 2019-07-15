using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Dungeon_Generator
{
    class DungeonGenerator
    {
        private Dungeon dungeon;
        private Random rng;
        public Dungeon Dungeon { get { return dungeon; } }

        private bool CanRoomFit(Dungeon dungeon, Room room)
        {
            Room roomIncWalls = new Room(room.FirstRow - 1, room.FirstCol - 1,
                                         room.Height + 2, room.Width + 2);

            // is origin corner within the dungeon?
            if (roomIncWalls.FirstRow < 0 || roomIncWalls.FirstCol < 0)
                return false;
            // is far corner within the dungeon?
            if (roomIncWalls.FirstRow + roomIncWalls.Height - 1 >= dungeon.Height)
                return false;
            if (roomIncWalls.FirstCol + roomIncWalls.Width - 1 >= dungeon.Width)
                return false;

            int rowToStop = roomIncWalls.FirstRow + roomIncWalls.Height;
            int colToStop = roomIncWalls.FirstCol + roomIncWalls.Width;
            for (int row = roomIncWalls.FirstRow; row < rowToStop; row++)
            {
                for (int col = roomIncWalls.FirstCol; col < colToStop; col++)
                {
                    if (dungeon.GetTile(row, col).Space != Tile.Type.Solid)
                    {
                        return false;
                    }
                }
            }
            return true;
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
                    roomHeight = rng.Next(minRoomHeight, maxRoomHeight + 1);
                    roomWidth  = rng.Next(minRoomWidth,  maxRoomWidth  + 1);
                } while (roomHeight * roomWidth > remainingRoomTiles);

                // create the room and put it in a random place

                Room room = new Room(0, 0, 0, 0);
                int row, col;
                int attempts = 0;
                do
                {
                    row = rng.Next(1, dungeon.Height - roomHeight);
                    col = rng.Next(1, dungeon.Width - roomWidth);
                    room.Replace(row, col, roomHeight, roomWidth);
                    ++attempts;
                } while (!CanRoomFit(dungeon, room) && attempts != 100);
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
            dungeon = new Dungeon(height, width);
            rng = new Random();
            GenerateRooms(dungeon, 0.9, 3, 3, 9, 9);
        }
    }
}
