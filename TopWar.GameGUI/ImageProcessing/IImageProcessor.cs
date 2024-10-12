using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.GameGUI.ImageProcessing
{
    public interface IImageProcessor
    {
        int WriteToSharedMemory(bool is32Bit = true);
    }
}
