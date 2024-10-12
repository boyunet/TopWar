using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.Core.Configuration
{
    internal class 编队信息类(string 功能, string 英雄1, string 英雄2)
    {
        public string 功能 { get; set; } = 功能;
        public string 英雄1 { get; set; } = 英雄1;
        public string 英雄2 { get; set; } = 英雄2;
    }
}
