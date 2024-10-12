using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.Core.Configuration
{
    internal class PipeJson
    {
        public string Text { set; get; } = string.Empty;
        public double Score { set; get; }
        public int x { set; get; }
        public int y { set; get; }
    }
}
