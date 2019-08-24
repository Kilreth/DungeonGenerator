using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dungeon_Generator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Dungeon dungeon = new Dungeon(20, 30);
            DungeonGenerator dungeonGenerator = new DungeonGenerator(60, 80);
            Form1 obj = new Form1(dungeonGenerator.Dungeon);
            Application.Run(obj);

            dungeonGenerator.MakeConnectedGraph(dungeonGenerator.Dungeon, 0.2);
            dungeonGenerator.GenerateStairsAndKey(dungeonGenerator.Dungeon);
            obj = new Form1(dungeonGenerator.Dungeon);
            Application.Run(obj);

            //Application.Run(new Form1());
        }
    }
}
