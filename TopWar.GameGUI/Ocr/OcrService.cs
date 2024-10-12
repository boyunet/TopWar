using PaddleOCRSharp;

namespace TopWar.GameGUI.Ocr
{
    public class OcrService : IOcrService
    {
        //使用默认中英文V4模型
        readonly PaddleOCRSharp.OCRModelConfig? config = null;
        //使用默认参数
        readonly PaddleOCRSharp.OCRParameter oCRParameter = new();

        //oCRParameter.det_db_score_mode=true;
        //识别结果对象
        PaddleOCRSharp.OCRResult _ocrResult = new();
        readonly PaddleOCRSharp.PaddleOCREngine _engine;

        public OcrService()
        {
            //中英文模型V4
            config = new OCRModelConfig();
            string modelPathroot = @"C:\Users\Kash\Project\TopWar\Models";
            config.det_infer = modelPathroot + @"\ch_PP-OCRv4_det_server_infer";
            config.cls_infer = modelPathroot + @"\ch_ppocr_mobile_v2.0_cls_infer";
            config.rec_infer = modelPathroot + @"\ch_PP-OCRv4_rec_server_infer";
            config.keys = modelPathroot + @"\ppocr_keys.txt";
            //建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。 
            _engine = new PaddleOCRSharp.PaddleOCREngine(config, oCRParameter);
        }

        public string PerformOcrAsync(byte[] croppedImageBytes)
        {
            // 实现OCR逻辑
            _ocrResult = _engine.DetectText(croppedImageBytes);
            return _ocrResult.JsonText;
        }
    }
}
