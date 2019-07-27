using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dungeon_Generator
{
    public partial class Form1 : Form
    {
        Dungeon dungeon;
        readonly int tileSize = 10;
        readonly int startX = 30;
        readonly int startY = 30;

        public Form1(Dungeon d)
        {
            dungeon = d;
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = CreateGraphics();
            Brush edgeBrush = new SolidBrush(Color.Black);
            Brush graniteBrush = new SolidBrush(Color.Brown);
            Brush rockBrush = new SolidBrush(Color.Gray);
            Brush wallBrush = new SolidBrush(Color.DarkRed);
            Brush pathBrush = new SolidBrush(Color.DarkBlue);
            Brush roomBrush = new SolidBrush(Color.LightGray);
            Brush brush = null;

            Tile[,] tiles = dungeon.Tiles;
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int col = 0; col < tiles.GetLength(1); col++)
                {
                    if (tiles[row, col].Space == Tile.Type.Rock)
                    {
                        brush = rockBrush;
                    }
                    else if (tiles[row, col].Space == Tile.Type.Path)
                    {
                        brush = pathBrush;
                    }
                    else if (tiles[row, col].Space == Tile.Type.Room)
                    {
                        brush = roomBrush;
                    }
                    else if (tiles[row, col].Space == Tile.Type.Wall)
                    {
                        brush = wallBrush;
                    }
                    else if (tiles[row, col].Space == Tile.Type.Granite)
                    {
                        brush = graniteBrush;
                    }
                    graphics.FillRectangle(edgeBrush, new Rectangle(
                            startX + col * tileSize, startY + row * tileSize, tileSize, tileSize));
                    graphics.FillRectangle(brush, new Rectangle(
                            startX + 1 + col * tileSize, startY + 1 + row * tileSize, tileSize - 2, tileSize - 2));
                }
            }
        }
    }
}
