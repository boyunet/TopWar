using System.IO.MemoryMappedFiles;

namespace TopWar.GameGUI.ShareMemoryS
{
    public interface IShareMemory
    {
        MemoryMappedViewAccessor MemoryAccessor { get; }
    }
}
