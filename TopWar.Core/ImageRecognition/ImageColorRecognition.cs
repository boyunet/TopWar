using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TopWar.Core.ImageRecognition
{
    internal class ImageColorRecognition
    {


        #region  👇👇👇 图色OCR相关 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        public async Task<(int, int)> 获取此时坐标()
        {
            try
            {
                string result = await 找字(79, 274, 137, 289);

                // 使用正则表达式提取坐标
                var matchX = Regex.Match(result, @"""Text"":""X:(\d+)""");
                var matchY = Regex.Match(result, @"""Text"":""Y:(\d+)""");

                int x = matchX.Success ? int.Parse(matchX.Groups[1].Value) : -1;
                int y = matchY.Success ? int.Parse(matchY.Groups[1].Value) : -1;

                //Console.WriteLine($"X: {x}, Y: {y}");

                return (x, y);
            }
            catch (Exception e)
            {
                Trint($"{this.Server}'获取此时坐标'出错：" + e.Message);
            }
            return (-1, -1);
        }
        private async Task 通过坐标到达(int x, int y)
        {
            关闭箭头叉叉();

            // 点击收藏夹
            Click(70, 275);
            if (!await 界面符合("o,收藏夹", 608, 66, 675, 91))
                return;

            // 输入坐标
            输入坐标(665, 600, x);
            输入坐标(765, 600, y);

            // 点击前往
            Click(622, 636);
            Click(中间坐标[0], 中间坐标[1]);

            void 输入坐标(int x, int y, int value)
            {
                Click(x, y);
                // 删除 backspace 按键码为8 大漠里代号back
                for (int i = 0; i < 4; i++)
                {
                    dm.KeyPressChar("back");
                    Delay(0.2);
                }
                // 录入
                dm.KeyPressStr(value.ToString(), 300);
            }

        }
        private void 视角拉近()
        {
            MoveTo(160, 400);
            for (int i = 0; i < 20; i++)
            {
                dm.WheelUp(); Delay(0.1);
            }
            Delay(1);
        }
        private void 视角拉远()
        {
            MoveTo(160, 400);
            for (int i = 0; i < 20; i++)
            {
                dm.WheelDown(); Delay(0.1);
            }
            Delay(1);
        }
        private void 鼠标拉动(int 起点x, int 起点y, int 终点x, int 终点y)
        {
            //比如金融中心
            //右边起点x1 = 785; 右边起点y1 = 70; 左边终点x2 = 525; 左边终点y2 = 70;
            Click(起点x, 起点y, 0.2);

            if (起点y == 终点y)
            {
                if (起点x > 终点x)
                {
                    //左拉
                    int 新点x = 起点x;
                    for (int i = 0; i < 1000; i++)
                    {
                        新点x -= 20;
                        if (新点x < 终点x)
                            break;
                        else
                            MoveTo(新点x, 起点y);
                    }
                }
                else
                {
                    //右拉
                    int 新点x = 起点x;
                    for (int i = 0; i < 1000; i++)
                    {
                        新点x += 20;
                        if (新点x > 终点x)
                            break;
                        else
                            MoveTo(新点x, 起点y);
                    }
                }
            }
            else if (起点x == 终点x)
            {
                if (起点y > 终点y)
                {
                    //上拉
                    int 新点y = 起点y;
                    for (int i = 0; i < 1000; i++)
                    {
                        新点y -= 20;
                        if (新点y < 终点y)
                            break;
                        else
                            MoveTo(起点x, 新点y);
                    }
                }
                else
                {
                    //下拉
                    int 新点y = 起点y;
                    for (int i = 0; i < 1000; i++)
                    {
                        新点y += 20;
                        if (新点y > 终点y)
                            break;
                        else
                            MoveTo(起点x, 新点y);
                    }
                }
            }

            MoveTo(终点x, 终点y);
            dm.LeftUp(); Delay(3);
        }
        void 关闭箭头叉叉()
        {
            //叉叉几乎 所有都适用 除了迁城的那个叉叉
            //关闭所有左箭头，叉叉
            int i = 0;
            do
            {
                CloseXX();
                CloseLeftArow();
                i++;

            } while (i < 3);
            Click(取消点[0], 取消点[1], 0.5);
        }
        private void CloseXX() => 关闭叉叉();
        void 关闭叉叉()
        {
            int dm_ret = dm.FindPicSim(0, 0, 1280, 760, "叉叉.bmp", "151515", 70, 0, out object x, out object y);
            if (dm_ret >= 0)
                Click(Convert.ToInt32(x) + 5, Convert.ToInt32(y) + 5, 0.5);
        }
        private void CloseLeftArow() => 关闭左箭头();
        void 关闭左箭头()
        {
            int dm_ret = dm.FindPic(0, 0, 1280, 760, "左箭头.bmp", "000000", 0.7, 0, out object x, out object y);
            if (dm_ret >= 0)
                Click(Convert.ToInt32(x) + 5, Convert.ToInt32(y) + 5, 0.5);
        }
        void 检查战斗界面()
        {
            //卡在队伍集结超时
            if (点击图片("集结超时", 0.2))
                点击图片("红色取消按钮", 0.2);

            //卡在战斗界面
            if (点击图片("战斗界面返回箭头", 0.2))
                点击图片("确定按钮", 0.2);
        }
        private async Task 点击活动标签(string 活动名称)
        {
            var (x, y) = (0, 0);
            for (int i = 0; i < 10; i++)
            {
                int xxx = 120 * 30;  //标签向右拉

                string send = $"s490,75,{xxx},0";

                string recive = await SendRequestAsync($"MyPipe{this.Server}", send);

                var r = await 找字List(439, 53, 855, 105);
                var (TopLeftX, TopLeftY) = r.Where(p => p.Text == 活动名称)
                                     .Select(p => (p.TopLeftX, p.TopLeftY))
                                     .FirstOrDefault();
                if (TopLeftX != 0)
                {
                    x = TopLeftX + 459;
                    y = TopLeftY + 53;
                    Click(x, y);
                    break;
                }
            }
        }
        /// <summary>
        /// 方向是指 鼠标按住 往哪个方向拖动
        /// </summary>
        public async Task Scroll(int 鼠标位置X, int 鼠标位置Y, int 方向, int 距离)
        {
            string send = "";
            距离 = 距离 * 120 * 30;
            //方向是指 鼠标按住 往哪个方向拖动
            if (方向 == 0)
                send = $"s{鼠标位置X},{鼠标位置Y},0,{-距离}";
            else if (方向 == 6)
                send = $"s{鼠标位置X},{鼠标位置Y},0,{距离}";
            else if (方向 == 3)
                send = $"s{鼠标位置X},{鼠标位置Y},{距离},0";
            else if (方向 == 9)
                send = $"s{鼠标位置X},{鼠标位置Y},{-距离},0";
            _ = await SendRequestAsync($"MyPipe{this.Server}", send);
        }
        public async Task<bool> ClickOCR(string 待找文字, int x1, int y1, int x2, int y2, double delay = 1.5, int 偏移X = 0, int 偏移Y = 0)
        {
            var r = await 找字List(x1, y1, x2, y2);
            var (topLeftX, topLeftY) = r.Where(p => 待找文字.Any(c => p.Text.Contains(c)))
                                        .Select(p => (p.TopLeftX, p.TopLeftY))
                                        .FirstOrDefault();

            if (topLeftX != 0)
            {
                int x = topLeftX + x1 + 偏移X;
                int y = topLeftY + y1 + 偏移Y;
                Console.WriteLine($"'点击OCR'X:{x} Y:{y}");
                Click(x, y, delay);
                return true;
            }

            return false;
        }
        public bool 点击图片(string 按钮图片名称, double delayTime = 1.5, int x1 = X1, int y1 = Y1, int x2 = X2, int y2 = Y2, int DeltaX = ClickOffsetX, int DeltaY = ClickOffsetY)
        => ClickPic(按钮图片名称, delayTime, x1, y1, x2, y2, DeltaX, DeltaY);
        public bool ClickPic(string 按钮图片名称, double delayTime = 1.5, int x1 = X1, int y1 = Y1, int x2 = X2, int y2 = Y2, int DeltaX = ClickOffsetX, int DeltaY = ClickOffsetY)
        {
            var (dm_ret, x, y) = FindPic(按钮图片名称, x1, y1, x2, y2);
            if (dm_ret >= 0)
            {
                Click(x + DeltaX, y + DeltaY, delayTime);
                return true;
            }
            return false;
        }
        public (int dm_ret, int x, int y) 找图(string 图片名, string 查找范围 = 窗口大小, double Sim = 0.7)
        {
            图片名 = 图片名.Replace(" ", "");
            string picName = 图片名.Contains('|')
                ? string.Join("|", 图片名.Split('|').Select(s => s + ".bmp"))
                : 图片名 + ".bmp";

            int[] f = 查找范围.Split(',').Select(int.Parse).ToArray();
            int dm_ret = dm.FindPic(f[0], f[1], f[2], f[3], picName, "000000", Sim, 0, out object xx, out object yy);

            return (dm_ret, Convert.ToInt32(xx), Convert.ToInt32(yy));
        }
        public async Task<TResult> ExecuteWithRetryAndTimeout<TResult>(Func<CancellationToken, Task<TResult>> operation, TimeSpan totalTimeout)
        {
            using (var totalCts = new CancellationTokenSource(totalTimeout))
            {
                int attempt = 0;
                string methodName = operation.Method.Name;
                while (!totalCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        return await operation(totalCts.Token);
                    }
                    catch (Exception ex)
                    {
                        // 使用logger记录错误
                        Trint($"ER:{this.Server}第{attempt + 1}次执行'{methodName}'失败: {ex.Message}");
                        attempt++;

                        // 等待指定的重试间隔时间
                        await Task.Delay(图色重试间隔 * 1000);
                    }
                }
            }
            throw new TimeoutException("Operation timed out.");
        }
        //找图的主函数'FindPic'
        public (int dm_ret, int x, int y) FindPic(string 图名, int x1, int y1, int x2, int y2, double Sim = 0.7, int 超时秒数 = 图色超时秒数)
        {

            图名 = 图名.Replace(" ", "");
            string picName = 图名.Contains('|')
                ? string.Join("|", 图名.Split('|').Select(s => s + ".bmp"))
                : 图名 + ".bmp";

            var (dm_ret, x, y) = (-1, 0, 0);

            try
            {
                (dm_ret, x, y) = ExecuteWithRetryAndTimeout<(int, int, int)>(async (cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int dm_ret = dm.FindPic(x1, y1, x2, y2, picName, "000000", Sim, 0, out object xx, out object yy);

                    if (dm_ret < 0)
                    {
                        await Task.Delay(1);
                        throw new Exception($"'{图名}'NotFound");
                    }

                    return (dm_ret, Convert.ToInt32(xx), Convert.ToInt32(yy));
                }, totalTimeout: TimeSpan.FromSeconds(超时秒数)).GetAwaiter().GetResult();
            }
            catch (TimeoutException ex)
            {
                // 处理超时异常
                Trint($"Operation timed out: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 处理其他异常
                Trint($"An error occurred: {ex.Message}");
            }
            return (dm_ret, x, y);
        }
        public (int dm_ret, int x, int y) FindPic原始(string 图名, int x1 = X1, int y1 = Y1, int x2 = X2, int y2 = Y2, double Sim = 0.7)
        {
            图名 = 图名.Replace(" ", "");
            string picName = 图名.Contains('|')
                ? string.Join("|", 图名.Split('|').Select(s => s + ".bmp"))
                : 图名 + ".bmp";

            int dm_ret = dm.FindPic(x1, y1, x2, y2, picName, "000000", Sim, 0, out object xx, out object yy);

            return (dm_ret, Convert.ToInt32(xx), Convert.ToInt32(yy));
        }
        public (int dm_ret, int x, int y) FindPicSim(string 图名, int x1 = X1, int y1 = Y1, int x2 = X2, int y2 = Y2, int Sim = 50)
        {
            图名 = 图名.Replace(" ", "");
            string picName = 图名.Contains('|')
                ? string.Join("|", 图名.Split('|').Select(s => s + ".bmp"))
                : 图名 + ".bmp";

            int dm_ret = dm.FindPicSim(x1, y1, x2, y2, picName, "202020", Sim, 0, out object xx, out object yy);
            return (dm_ret, (int)xx, (int)yy);
        }
        /// <summary>
        /// 比较图片: "p,图名" / 比较文字: "o,文字" / p,o可以忽略大小写
        /// </summary>
        private async Task<bool> 界面符合(string 命令描述, int x1 = X1, int y1 = Y1, int x2 = X2, int y2 = Y2)
        {
            string pattern = @"^[po],.+$"; // 正则表达式模式，匹配以 'p,' 或 'o,' 开头，且逗号后面至少有一个字符的字符串
            if (Regex.IsMatch(命令描述, pattern))
            {
                string[] parts = 命令描述.Split(',');
                string 命令 = parts[0];
                string 参数 = parts[1];
                bool result = false;
                //是一种字符串比较的方法。它的作用是比较两个字符串是否相等，并且忽略大小写
                if (string.Equals(命令, "p", StringComparison.OrdinalIgnoreCase))
                {
                    result = FindPic(参数, x1, y1, x2, y2).dm_ret >= 0;
                    if (!result) Console.WriteLine($"'界面符合'未找到图片{参数}");
                    return result;
                }
                else if (string.Equals(命令, "o", StringComparison.OrdinalIgnoreCase))
                {
                    result = await 找字包含字符(参数, x1, y1, x2, y2);
                    if (!result) Console.WriteLine($"'界面符合'未找到文字{参数}");
                    return result;
                }
            }
            else
            {
                Trint($"{this.Server}'界面符合'参数传递出错,参数{命令描述}");
                return false;
            }
            return false;
        }
        public (int dm_ret, int x, int y) 找图Sim(string 图片名, string 查找范围 = 窗口大小, int Sim = 50)
        {
            图片名 = 图片名.Replace(" ", "");
            string picName = 图片名.Contains('|')
                ? string.Join("|", 图片名.Split('|').Select(s => s + ".bmp"))
                : 图片名 + ".bmp";

            int[] f = 查找范围.Split(',').Select(int.Parse).ToArray();
            int dm_ret = dm.FindPicSim(f[0], f[1], f[2], f[3], picName, "202020", Sim, 0, out object xx, out object yy);
            return (dm_ret, (int)xx, (int)yy);
        }
        public (int dm_ret, int x, int y) 点击按钮(string 按钮图片名称, int 延迟 = 1500, string 查找范围 = 窗口大小)
        {
            var (dm_ret, x, y) = 找图(按钮图片名称, 查找范围);
            if (dm_ret >= 0)
            {
                const int 偏移量 = 15;
                dm.MoveTo(x + 偏移量, y + 偏移量);
                Thread.Sleep(20);
                dm.LeftClick();
                Thread.Sleep(延迟);
                点击坐标(x + 偏移量, y + 偏移量, 延迟);
                return (dm_ret, x, y);
            }
            return (-1, 0, 0);
        }
        public void 点击坐标(int x, int y, int 延迟 = 1500)
        {
            dm.MoveTo(x, y); Thread.Sleep(20); dm.LeftClick(); Thread.Sleep(延迟);
        }
        public void Click(int x, int y, double 延迟 = 1.5)
        {
            MoveTo(x, y);
            dm.LeftClick();
            Delay(延迟);
        }
        private void Delay(double delayInSeconds)
        {
            int delayInMilliseconds = (int)(delayInSeconds * 1000);
            Thread.Sleep(delayInMilliseconds);
        }
        private void MoveTo(int x, int y, double delay = 0.1)
        {
            dm.MoveTo(x, y);
            Delay(delay);
        }
        public async Task<bool> 绑定图色键鼠窗口()
        {
            bool 成功 = false;
            try
            {
                string send = "handle";
                string recive = await SendRequestAsync($"MyPipe{server}", send);

                if (string.IsNullOrEmpty(recive))
                { Print($"{this.Server}获取窗口句柄失败"); return false; }

                int recive图色句柄 = int.Parse(recive.Split(',')[0]);
                int recive键鼠句柄 = int.Parse(recive.Split(',')[1]);

                if (recive图色句柄 != this.图色句柄 || recive键鼠句柄 != this.键鼠句柄)
                {
                    成功 = await 绑定大漠结合(recive图色句柄, recive键鼠句柄);
                    if (成功)
                    { this.图色句柄 = recive图色句柄; this.键鼠句柄 = recive键鼠句柄; }
                }
                else
                { Print($"{this.Server}句柄未改变"); 成功 = true; }
            }
            catch (Exception e)
            {
                Trint($"{this.Server}绑定图色键鼠窗口异常{e.Message}");
            }
            return 成功;
        }
        private async Task<bool> 绑定大漠结合(int 图色句柄, int 键鼠句柄) //设置当前对象用于输入的对象. 结合图色对象和键鼠对象,用一个对象完成操作.
        {
            async Task<bool> 绑定并报告(dmsoft dm, int 句柄, string 绑定类型, string 操作类型)
            {
                int dm_ret = dm.BindWindowEx(句柄, 绑定类型 == "图色" ? "dx.graphic.3d.10plus" : "dx2", "windows3", "windows", "", 0);
                await Task.Delay(2000);
                bool 成功 = dm_ret == 1;
                Print($"{this.Server}{操作类型}绑定窗口{(成功 ? "成功" : "失败")}");
                return 成功;
            }

            bool 图色绑定成功 = await 绑定并报告(this.Dm0, 图色句柄, "图色", "图色");
            bool 键鼠绑定成功 = await 绑定并报告(this.Dm1, 键鼠句柄, "键鼠", "键鼠");

            if (图色绑定成功 && 键鼠绑定成功)
            {
                int dm_ret = this.Dm0.SetInputDm(this.Dm1.GetID(), 0, 0);
                bool 结合成功 = dm_ret == 1;
                Print($"{this.Server}结合图色对象和键鼠对象{(结合成功 ? "成功" : "失败")}");
                if (结合成功)
                    return true;
            }
            return false;
        }
        private static string OCR修正字符(string input)
        {
            //把csharpOcr转换后的 一些常见错误 修正下
            string result = input.Replace("O", "0")
                                         .Replace("o", "0")
                                         .Replace("T", "1")
                                         .Replace("G", "6");
            //另外1有时候会被认为是7
            return result;
        }
        private bool 是字符串包含数字(string input)
        {
            // 检查字符串是否包含数字
            bool containsDigit = input.Any(char.IsDigit);
            // 输出结果
            Console.WriteLine($"Does the string contain a digit? {containsDigit}");
            return containsDigit;
        }
        private bool 是字符串包含字符串(string 源字符串, string 待匹配字符串)
        {
            return 待匹配字符串.Any(源字符串.Contains);
        }
        //Pipe客户端
        public async Task<string> SendRequestAsync(string pipeName, string message)
        {
            //默认超时10秒
            TimeSpan connectTimeout = TimeSpan.FromSeconds(10);
            TimeSpan readTimeout = TimeSpan.FromSeconds(10);

            using (var ctsConnect = new CancellationTokenSource(connectTimeout))
            using (var ctsRead = new CancellationTokenSource())
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                try
                {
                    await pipeClient.ConnectAsync(ctsConnect.Token);

                    using (StreamReader reader = new StreamReader(pipeClient))
                    using (StreamWriter writer = new StreamWriter(pipeClient))
                    {
                        Console.WriteLine($"向服务器 {pipeName} 发送消息: {message}");
                        await writer.WriteLineAsync(message);
                        await writer.FlushAsync();

                        // 使用 Task.WhenAny 来实现读取超时  
                        Task<string> readTask = reader.ReadLineAsync();
                        Task timeoutTask = Task.Delay(readTimeout, ctsRead.Token);

                        if (await Task.WhenAny(readTask, timeoutTask) == readTask)
                        {
                            string response = await readTask;
                            if (string.IsNullOrEmpty(response))
                            {
                                Trint($"{this.Server}出错: 接收到的数据为空");
                                return "";
                            }
                            Console.WriteLine($"{this.Server}从服务器 {pipeName} 接收到响应: {response}");
                            return response;
                        }
                        else
                        {
                            // 读取超时  
                            Trint($"{this.Server}Pipe读取超时");
                            ctsRead.Cancel(); // 虽然此时可能已经超时，但取消可以确保资源被释放  
                            return "";
                        }
                    }
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken == ctsConnect.Token)
                {
                    // 连接超时  
                    Trint($"{pipeName}连接超时");
                    return "";
                }
                catch (Exception ex)
                {
                    // 其他异常  
                    Trint($"{this.Server}Pipe发生错误: {ex.Message}");
                    throw; // 或者根据需要处理异常  
                }
            }
        }
        public class BoxPoint
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        public class DetectionResult
        {
            public List<BoxPoint> BoxPoints { get; set; }
            public float Score { get; set; }
            public string Text { get; set; }
            public int cls_label { get; set; }
            public float cls_score { get; set; }
            public int TopLeftX => BoxPoints != null && BoxPoints.Count > 0 ? BoxPoints[0].X : 0;
            public int TopLeftY => BoxPoints != null && BoxPoints.Count > 0 ? BoxPoints[0].Y : 0;
        }
        //Ocr的主函数'Pipe_Ocr查询'
        private async Task<(string Text, string Json, List<DetectionResult> List)> Pipe_Ocr查询(int x1, int y1, int x2, int y2, int 超时秒数 = 图色超时秒数)
        {
            return await ExecuteWithRetryAndTimeout<(string, string, List<DetectionResult>)>(async (cancellationToken) =>
            {
                string received = null;
                List<DetectionResult> results = new List<DetectionResult>();
                try
                {
                    string send = $"O{x1},{y1},{x2},{y2}";
                    received = await SendRequestAsync($"MyPipe{this.Server}", send);
                    results = JsonConvert.DeserializeObject<List<DetectionResult>>(received);

                    if (string.IsNullOrEmpty(received) || results.Count <= 0)
                        throw new Exception("Pipe接受到的信息为空");

                    // 拼接所有检测结果的文本
                    string combinedText = string.Join(" ", results.ConvertAll(result => result.Text));

                    return (combinedText, received, results);
                }
                catch (JsonException ex)
                {
                    Trint($"{this.Server}'Pipe_Ocr查询'无效JSON格式: " + ex.Message);
                    return ("", "", new List<DetectionResult>());
                }
                catch (Exception ex)
                {
                    Trint($"{this.Server}'Pipe_Ocr查询'发生错误: " + ex.Message);
                    return ("", "", new List<DetectionResult>());
                }
            }, totalTimeout: TimeSpan.FromSeconds(超时秒数));
        }
        private async Task<(string Text, string Json, List<DetectionResult> List)> Pipe_Ocr查询_原始(int x1, int y1, int x2, int y2)
        {
            //可以拼接起来字符串
            //List<DetectionResult> results = JsonConvert.DeserializeObject<List<DetectionResult>>(json);
            //string combinedText = string.Join(" ", results.ConvertAll(result => result.Text));
            //接受到的是  ocrResult.JsonText
            //[{"BoxPoints":[{"X":292,"Y":19},{"X":311,"Y":21},{"X":309,"Y":31},{"X":291,"Y":29}],"Score":0.9917753338813782,"Text":"505","cls_label":-1,"cls_score":0.0},{"BoxPoints":[{"X":68,"Y":28},{"X":84,"Y":27},{"X":85,"Y":35},{"X":69,"Y":36}],"Score":0.9909209609031677,"Text":"30","cls_label":-1,"cls_score":0.0},{"BoxPoints":[{"X":41,"Y":37},{"X":102,"Y":36},{"X":102,"Y":52},{"X":41,"Y":53}],"Score":0.8869246244430542,"Text":"活动自历","cls_label":-1,"cls_score":0.0},{"BoxPoints":[{"X":270,"Y":36},{"X":331,"Y":36},{"X":331,"Y":53},{"X":270,"Y":53}],"Score":0.9898548126220703,"Text":"拯救难民","cls_label":-1,"cls_score":0.0},{"BoxPoints":[{"X":377,"Y":35},{"X":416,"Y":35},{"X":416,"Y":53},{"X":377,"Y":53}],"Score":0.9986720085144043,"Text":"联盟","cls_label":-1,"cls_score":0.0}]

            string received = null;
            List<DetectionResult> results = new List<DetectionResult>();
            try
            {
                string send = $"O{x1},{y1},{x2},{y2}";
                received = await SendRequestAsync($"MyPipe{this.Server}", send);

                if (string.IsNullOrEmpty(received))
                {
                    return ("", "", new List<DetectionResult>());
                }

                results = JsonConvert.DeserializeObject<List<DetectionResult>>(received);

                if (results.Count <= 0)
                    throw new Exception("信息为空");

                return (results[0].Text, received, results);

            }
            catch (JsonException ex)
            {
                Trint("Pipe接受的字符串为 无效的 JSON 格式: " + ex.Message);
                return ("", "", new List<DetectionResult>());
            }

            //DetectionResult firstResult = results[0];
            //Console.WriteLine("First Text: " + firstResult.Text);
            //Console.WriteLine("Top Left X: " + firstResult.TopLeftX);
            //Console.WriteLine("Top Left Y: " + firstResult.TopLeftY);          
        }
        public async Task<string> OCR找字(int x1, int y1, int x2, int y2) => await 找字(x1, y1, x2, y2);
        public async Task<List<DetectionResult>> OCR找字List(int x1, int y1, int x2, int y2) => await 找字List(x1, y1, x2, y2);
        public async Task<string> 找字(int x1, int y1, int x2, int y2)
        {
            return (await Pipe_Ocr查询(x1, y1, x2, y2)).Text;
        }
        public async Task<string> 找字Json(int x1, int y1, int x2, int y2)
        {
            return (await Pipe_Ocr查询(x1, y1, x2, y2)).Json;
        }
        public async Task<List<DetectionResult>> 找字List(int x1, int y1, int x2, int y2)
        {
            return (await Pipe_Ocr查询(x1, y1, x2, y2)).List;
        }
        public async Task<bool> OCR是包含字符(string 待匹配字符, int x1, int y1, int x2, int y2, bool 是完全匹配 = false)
        {
            var (text, json, _) = await Pipe_Ocr查询(x1, y1, x2, y2);

            if (是完全匹配)
            {
                return string.Equals(待匹配字符, text, StringComparison.Ordinal);
            }

            bool containsAny = 待匹配字符.Any(c => json.Contains(c));
            if (!containsAny)
            {
                Console.WriteLine($"{this.Server} 未匹配字符串 {待匹配字符}");
            }

            return containsAny;
        }

        private async Task<bool> 找字包含字符(string 待匹配字符, int x1, int y1, int x2, int y2, bool 是完全匹配 = false)
        {
            string text = await 找字(x1, y1, x2, y2);
            string json = await 找字Json(x1, y1, x2, y2);

            if (是完全匹配)
            {
                return string.Equals(待匹配字符, text, StringComparison.Ordinal);
            }

            bool containsAny = 待匹配字符.Any(c => json.Contains(c));
            if (!containsAny)
            {
                Console.WriteLine($"{this.Server} 未匹配字符串 {待匹配字符}");
            }

            return containsAny;
        }
        private async Task<bool> 找字匹配(int x1, int y1, int x2, int y2, string 待匹配字符)
            => await 找字包含字符(待匹配字符, x1, y1, x2, y2);
        #endregion


    }
}
