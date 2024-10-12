using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.Core.Configuration
{
    internal class 跟车信息类(bool 是普通跟车, bool 是强制跟车, bool 是刷满, int 当前次数)
    {
        public bool 是普通跟车 { get; set; } = 是普通跟车;
        public bool 是强制跟车 { get; set; } = 是强制跟车;
        public bool 是刷满 { get; set; } = 是刷满;
        public int 当前次数 { get; set; } = 当前次数;
    }
}
