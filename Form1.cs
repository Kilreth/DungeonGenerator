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
            Font drawFont = new System.Drawing.Font("Arial", 8);
            StringFormat drawFormat = new System.Drawing.StringFormat();
            Brush textBrush = new SolidBrush(Color.Black);
            Brush debugBrush = new SolidBrush(Color.Red);
            Brush edgeBrush = new SolidBrush(Color.Black);
            Brush graniteBrush = new SolidBrush(Color.DarkViolet);
            Brush rockBrush = new SolidBrush(Color.Gray);
            Brush wallBrush = new SolidBrush(Color.DarkRed);
            Brush pathBrush = new SolidBrush(Color.Yellow);
            Brush roomBrush = new SolidBrush(Color.LightGray);
            Brush doorBrush = new SolidBrush(Color.Orange);
            Brush brush = null;

            Tile[,] tiles = dungeon.Tiles;
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int col = 0; col < tiles.GetLength(1); col++)
                {
                    Tile tile = tiles[row, col];
                    if (tile.Space == Space.Rock)
                    {
                        brush = rockBrush;
                    }
                    else if (tile.Space == Space.Path)
                    {
                        brush = pathBrush;
                    }
                    else if (tile.Space == Space.Room
                          || tile.Space == Space.StairsUp
                          || tile.Space == Space.StairsDown
                          || tile.Space == Space.Key)
                    {
                        brush = roomBrush;
                    }
                    else if (tile.Space == Space.Door)
                    {
                        brush = doorBrush;
                    }
                    else if (tile.Space == Space.Wall)
                    {
                        brush = wallBrush;
                    }
                    else if (tile.Space == Space.Granite)
                    {
                        brush = graniteBrush;
                    }

                    if (tile.Debug)
                    {
                        edgeBrush = debugBrush;
                    }
                    else
                    {
                        edgeBrush = brush;
                    }
                    graphics.FillRectangle(edgeBrush, new Rectangle(
                            startX + col * tileSize, startY + row * tileSize, tileSize, tileSize));
                    graphics.FillRectangle(brush, new Rectangle(
                            startX + 1 + col * tileSize, startY + 1 + row * tileSize, tileSize - 2, tileSize - 2));
                    if (tile.Space == Space.StairsUp)
                    {
                        graphics.DrawString("<", drawFont, textBrush, startX + col * tileSize, startY + row * tileSize - 2, drawFormat);
                    }
                    else if (tile.Space == Space.StairsDown)
                    {
                        graphics.DrawString(">", drawFont, textBrush, startX + col * tileSize, startY + row * tileSize - 2, drawFormat);
                    }
                    else if (tile.Space == Space.Key)
                    {
                        graphics.DrawString("k", drawFont, textBrush, startX + col * tileSize, startY + row * tileSize - 2, drawFormat);
                    }
                    if (tile.Text != null)
                    {
                        graphics.DrawString(tile.Text, drawFont, textBrush, startX + (col-1) * tileSize, startY + row * tileSize - 2, drawFormat);
                    }
                }
            }

        }
    }
}
