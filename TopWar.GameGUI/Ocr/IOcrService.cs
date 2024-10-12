using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopWar.GameGUI.Ocr
{
    public interface IOcrService
    {
        string PerformOcrAsync(byte[] croppedImageBytes);
    }
}
