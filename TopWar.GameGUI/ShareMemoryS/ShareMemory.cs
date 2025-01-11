using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace TopWar.GameGUI.ShareMemoryS
{
    public class ShareMemory : IShareMemory
    {
        private readonly MemoryMappedFile? _mmf;
        private readonly MemoryMappedViewAccessor? _accessor = null;
        public ShareMemory(string serverID)
        {
            _mmf = MemoryMappedFile.CreateOrOpen($"SharedMemoryScreenshot{serverID}", 5 * 1024 * 1024);
            _accessor = _mmf.CreateViewAccessor();
        }
        public MemoryMappedViewAccessor GetScreenshotAccessor()
        {
            return _accessor!;
        }
        public MemoryMappedViewAccessor MemoryAccessor
        {
            get { return _accessor!; }
        }
    }
}
