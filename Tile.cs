﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Tile
    {
        public int Row { get; }
        public int Col { get; }
        public Type Space { get; set; }

        public enum Type { Rock, Path, Room, Wall }

        public Tile(int row, int col, Type type)
        {
            Row = row;
            Col = col;
            Space = type;
        }
    }
}
