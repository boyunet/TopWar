using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.GameGUI.ShareMemoryS
{
    public interface IShareMemory
    {
        MemoryMappedViewAccessor MemoryAccessor { get; }
    }
}
