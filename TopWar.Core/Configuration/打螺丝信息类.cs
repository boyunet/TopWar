using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.Core.Configuration
{
    internal class 打螺丝信息类(bool 是选中, int x, int y)
    {
        public bool 是选中 { get; set; } = 是选中;
        public int x { get; set; } = x;
        public int y { get; set; } = y;
    }
}
