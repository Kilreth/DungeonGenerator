using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class CorridorTile
    {
        public Tile Tile { get; }
        public CorridorTile From { get; }
        public Tile.Compass PreviousDirection { get; }
        public List<Tile.Compass> DirectionsToTry { get; }

        public CorridorTile TryNextTile(Dungeon dungeon, float chanceToTurn)
        {
            Tile.Compass nextDirection = ChooseNextDirection(chanceToTurn);
            Tile nextTile = dungeon.GetTileByDirection(Tile, nextDirection);
            return new CorridorTile(nextTile, this, nextDirection);
        }

        public Tile.Compass ChooseNextDirection(float chanceToTurn)
        {
            if (DirectionsToTry.Count == 0)
            {
                throw new InvalidOperationException("All directions already explored");
            }
            if (DirectionsToTry.Contains(PreviousDirection)
                && DungeonGenerator.Rng.NextDouble() > chanceToTurn)
            {
                DirectionsToTry.Remove(PreviousDirection);
                return PreviousDirection;
            }
            int index;
            do
            {
                index = DungeonGenerator.Rng.Next(0, DirectionsToTry.Count);
            } while (DirectionsToTry[index] == PreviousDirection && DirectionsToTry.Count > 1);
            Tile.Compass direction = DirectionsToTry[index];
            DirectionsToTry.RemoveAt(index);
            return direction;
        }

        public CorridorTile(Tile tile, CorridorTile from, Tile.Compass dir)
        {
            Tile = tile;
            From = from;
            PreviousDirection = dir;

            DirectionsToTry = new List<Tile.Compass>();
            DirectionsToTry.Add(Tile.Compass.Up);
            DirectionsToTry.Add(Tile.Compass.Down);
            DirectionsToTry.Add(Tile.Compass.Left);
            DirectionsToTry.Add(Tile.Compass.Right);
            DirectionsToTry.Remove(Tile.Invert(dir));
        }
    }
}
