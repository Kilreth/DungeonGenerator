using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon_Generator
{
    public class Area
    {
        public List<Area> To { get; private set; }
        public int Id { get; private set; }
        public static int NextId { get; set; }

        public void ConnectTo(Area other)
        {
            To.Add(other);
        }

        public void InitializeNode()
        {
            To = new List<Area>();
            Id = ++NextId;
        }

        static Area()
        {
            NextId = 0;
        }
    }
}
