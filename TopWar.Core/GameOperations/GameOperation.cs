using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static TopWar.Core.ImageRecognition.ImageColorRecognition;
using System.Windows;
using TopWar.Core.Configuration;
using TopWar.Core.ImageRecognition;

namespace TopWar.Core.GameOperations
{
    internal class GameOperation
    {


        #region  👇👇👇 任务相关 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        // 👇👇👇 任务的一些方法 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        private async Task TaskMain()
        {
            while (!this.ctsMainTask.IsCancellationRequested)
            {
                if (是低频任务列表可执行())
                {
                    foreach (var mission in this.特殊任务列表)
                    {
                        await 执行任务(mission);
                    }
                    foreach (var mission in this.联盟外任务列表)
                    {
                        await 执行任务(mission);
                    }

                    foreach (var mission in this.联盟内任务列表)
                    {
                        await 执行任务(mission);
                    }

                    if (this.每日军情任务列表.Any(任务 => 检查任务是否可执行(任务)))
                    {
                        await 每日军情任务();
                    }
                }
                else
                {
                    //高频任务
                    await 正式打野();
                    await 集结任务();
                }
                await Task.Delay(100);
            }
            this.主进程运行中 = false;
        }
        public List<string> 特殊任务列表 = new List<string>
            {
              "特殊任务"
            };
        public List<string> 联盟内任务列表 = new List<string>
            {
              "联盟遗迹道具","联盟科技捐献","联盟机甲捐献","联盟帮助与请求","联盟礼物"
            };
        public List<string> 联盟外任务列表 = new List<string>
            {
              "联盟科技捐献之遗迹","英雄招募","英雄高级招募","礼包商城","军级奖励","收取金币","模块研究","打螺丝","战争之源"
            };
        public List<string> 每日军情任务列表 = new List<string>
            {
              "每日军情荒野行动","每日军情沙盘演习","每日军情远征行动","每日军情跨战区演习","每日军情次元矿洞","每日军情岛屿作战"
            };
        public List<string> 金融中心任务列表 = new List<string>
            {
              "金融中心军团商店","金融中心远征商店","金融中心岛屿商店","金融中心套装商店"
            };
        public void 方法入口(string 任务名称)
        {
            Print($"{this.Server}执行任务 {任务名称}");
            if (this.任务信息[任务名称].入口标记 && !this.任务信息[任务名称].出口标记)
            {
                this.任务信息[任务名称].未成功运行次数++;
                Print($"任务未正常退出，错误累计: {this.任务信息[任务名称].未成功运行次数}");
                Console.WriteLine($"任务未正常退出，错误累计: {this.任务信息[任务名称].未成功运行次数}");
            }
            this.任务信息[任务名称].入口标记 = true;
            this.任务信息[任务名称].出口标记 = false;
        }
        public void 方法出口记录当前时间(string 任务名称)
        {
            Print($"{this.Server}结束任务 {任务名称}");
            this.任务信息[任务名称].未成功运行次数 = 0;
            this.任务信息[任务名称].出口标记 = true;
            this.任务信息[任务名称].入口标记 = false;
            this.任务信息[任务名称].修改时间 = DateTime.Now;
            this.写入配置();
        }
        public void 方法出口记录当日时间(string 任务名称)
        {
            Print($"{this.Server}结束任务 {任务名称}");
            this.任务信息[任务名称].未成功运行次数 = 0;
            this.任务信息[任务名称].出口标记 = true;
            this.任务信息[任务名称].入口标记 = false;
            this.任务信息[任务名称].修改时间 = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            写入配置();
        }
        private async Task 集结任务()
        {
            await Task.Delay(1000);
            Console.WriteLine("集结一次");
        }
        private async Task 打野任务()
        {
            await Task.Delay(1000);
            Console.WriteLine("打野一次");
        }
        private async Task 机械田任务()
        {
            await Task.Delay(1000);
        }
        private bool 是低频任务列表可执行()
        {
            return this.特殊任务列表.Any(任务 => 检查任务是否可执行(任务)) ||
                   this.联盟内任务列表.Any(任务 => 检查任务是否可执行(任务)) ||
                   this.联盟外任务列表.Any(任务 => 检查任务是否可执行(任务)) ||
                   this.每日军情任务列表.Any(任务 => 检查任务是否可执行(任务));
        }
        private bool 检查任务是否可执行(string 任务名称)
        {
            int 执行间隔 = this.任务信息[任务名称].执行间隔; //触发一下属性才能更新冻结时间
            DateTime 现在时间 = DateTime.Now;
            DateTime 冻结时间 = this.任务信息[任务名称]._冻结结束时间;
            DateTime 修改时间 = this.任务信息[任务名称].修改时间;

            if (this.ctsMainTask.IsCancellationRequested ||
                !this.任务信息[任务名称].是开启 ||
                现在时间 < 冻结时间
                )
                return false;

            //隔天立刻重置CD
            if (this.任务信息[任务名称].修改时间.Date != 现在时间.Date)
                return true;

            //最后看间隔
            if ((现在时间 - 修改时间).TotalMinutes > 执行间隔)
                return true;

            return false;

        }
        private async Task 执行任务(string 任务名称, params object[] 参数)
        {
            if (检查任务是否可执行(任务名称))
            {
                MethodInfo method = this.GetType().GetMethod(任务名称, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    var returnType = method.ReturnType;
                    var isAsync = returnType == typeof(Task) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));

                    ParameterInfo[] parameterInfos = method.GetParameters();
                    object[] parameters = new object[parameterInfos.Length];

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        if (i < 参数.Length)
                        {
                            parameters[i] = 参数[i];
                        }
                        else
                        {
                            parameters[i] = Type.Missing;
                        }
                    }

                    if (method.IsStatic)  // 静态方法
                    {
                        if (isAsync)
                        {
                            await (Task)method.Invoke(null, parameters);
                        }
                        else
                        {
                            method.Invoke(null, parameters);
                        }
                    }
                    else  // 实例方法
                    {
                        if (isAsync)
                        {
                            await (Task)method.Invoke(this, parameters);
                        }
                        else
                        {
                            method.Invoke(this, parameters);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Method '{任务名称}' not found.");
                }
            }
        }
        private bool 处于界面(string 界面名称, double 相似度阈值 = 0.9)
        {
            var 界面图标 = 界面名称 switch
            {
                "联盟界面" => ("体力加号", 808, 232, 827, 252),
                "世界界面" => ("雷达按钮", 1211, 358, 1263, 409),
                "主城界面" => ("主城左侧边按钮", 5, 368, 23, 397),
                "集结界面" => ("集结字样", 607, 5, 674, 41),
                "每日军情界面" => ("每日军情", 582, 3, 697, 41),
                "金融中心界面" => ("金融中心", 578, 3, 692, 41),
                "军火库界面" => ("军火库", 583, 4, 698, 44),
                "集结无队伍界面" => ("集结无队伍", 599, 69, 688, 103),
                _ => (null, 0, 0, 0, 0)
            };

            if (界面图标.Item1 == null)
            {
                Trint($"{server}'ERROR:'处于界面'");
                return false;
            }

            return FindPic(界面图标.Item1, 界面图标.Item2, 界面图标.Item3, 界面图标.Item4, 界面图标.Item5, 相似度阈值, 超时秒数: 1).dm_ret >= 0;
        }
        /// <summary>
        /// 世界界面 | 联盟界面 | 集结界面 | 每日军情界面 | 金融中心界面 | 军火库界面
        /// </summary>
        public bool 打开标签页(string 标签名称)
        {
            if (!尝试进入标签页(标签名称))
            {
                Print($"{this.Server} ---> 未能打开{标签名称}");
                return false;
            }

            return true;

            bool 尝试进入标签页(string 标签名称)
            {
                return 返回(标签名称) || 进入(标签名称);
            }

            bool 返回(string 标签名称)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (处于界面(标签名称))
                        return true;

                    关闭叉叉();
                    关闭左箭头();
                }
                return false;
            }
            bool 进入(string 标签名称)
            {
                switch (标签名称)
                {
                    case "世界界面":
                        Print($"{server} 世界界面");
                        break;
                    case "联盟界面":
                        ClickPic("联盟按钮");
                        break;
                    case "集结界面":
                        点击按钮("联盟按钮");
                        点击按钮("联盟战争按钮");
                        break;
                    case "每日军情界面":
                        点击坐标(450, 630);
                        break;
                    case "金融中心界面":
                        点击坐标(50, 75);  //点击军级坐标                      
                        点击坐标(480, 190);  //点击军级商店
                        break;
                    case "军火库界面":
                        点击坐标(125, 70);   // 点击战斗力标签
                        点击坐标(780, 600);  //军火库战斗力标签
                        break;
                    default:
                        Trint($"{server} 界面选择 switch 没有符合的界面 此为默认值");
                        return false;
                }
                return 处于界面(标签名称);
            }
        }
        private async Task<int> 返回分钟数(int x1, int y1, int x2, int y2)
        {

            //比如 得到遗迹倒计时 27:15:50

            string input = await OCR找字(x1, y1, x2, y2);

            int totalMinutes = GetTotalMinutesFromCountdown(input);

            if (totalMinutes >= 0)
            {
                Print($"{server} 得到分钟数 {totalMinutes}");
            }
            return totalMinutes;
        }
        int GetTotalMinutesFromCountdown(string input)
        {
            string pattern = @"^\d{1,2}:\d{2}:\d{2}$";
            if (Regex.IsMatch(input, pattern))
            {
                string[] parts = input.Split(':');
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                return hours * 60 + minutes;
            }
            return -1;
        }
        // 👇👇👇 特殊任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        private async Task 特殊任务()
        {
            this.方法入口("特殊任务");
            await Task.Delay(1000);
            this.方法出口记录当前时间("特殊任务");
        }
        //👇👇👇 联盟面板内任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        private void 联盟遗迹道具()
        {
            this.方法入口("联盟遗迹道具");

            if (!打开标签页("联盟界面"))
                return;

            var 道具优先级 = new[] { "遗迹大体", "遗迹三级强化", "遗迹紫碎", "遗迹绿色材料随机箱", "遗迹技能抽卡卷", "遗迹职业道具自选宝箱" };
            var 遗迹道具 = 道具优先级.Select((name, index) => new 遗迹的道具类 { 优先级 = index, 道具名字 = name, 是否存在 = false }).ToList();

            int[] 下拉的下边 = { 710, 620 };
            int[] 下拉到上边 = { 710, 320 };
            string 道具范围 = "458,287,721,653";

            bool 第一页购买按钮灰色 = false;
            bool 第二页购买按钮灰色 = false;

            if (点击按钮("联盟领地按钮").dm_ret >= 0 &&
                点击按钮("联盟领地之遗迹按钮").dm_ret >= 0 &&
                点击按钮("2级遗迹按钮").dm_ret >= 0)
            {
                第一页购买按钮灰色 = 处理遗迹页面("上方");
                往下拉();
                遗迹道具.ForEach(s => s.是否存在 = false);
                第二页购买按钮灰色 = 处理遗迹页面("下方");
            }

            if (第一页购买按钮灰色 && 第二页购买按钮灰色)
                this.方法出口记录当前时间("联盟遗迹道具");

            关闭左箭头();


            bool 处理遗迹页面(string 位置)
            {
                if (点击按钮("遗迹道具按钮").dm_ret >= 0)
                {
                    var dm_ret = 找图("联盟遗迹购买按钮灰色").dm_ret;
                    if (dm_ret < 0)
                    {
                        string s = 得到遗迹物品清单();
                        if (string.IsNullOrEmpty(s))
                        {
                            Print($"{server} {位置}遗迹清单未获得");
                            return false;
                        }
                        购买本页(s);
                    }

                    dm_ret = 找图("联盟遗迹购买按钮灰色").dm_ret;
                    关闭叉叉();
                    return dm_ret >= 0;
                }
                return false;
            }
            string 得到遗迹物品清单()
            {
                var 前半页 = 得到半页清单();
                Print("前半页清单--->" + 前半页.道具名);
                往下拉();
                var 后半页 = 得到半页清单();
                Print("后半页清单--->" + 后半页.道具名);
                往上拉();

                return 前半页.优先级 >= 后半页.优先级 ? 后半页.道具名 : 前半页.道具名;
            }
            (string 道具名, int 优先级) 得到半页清单()
            {
                string 道具名 = "";
                int 优先级 = 999;

                foreach (var s in 道具优先级)
                {
                    if (找图Sim(s, 道具范围).dm_ret >= 0)
                    {
                        var 道具 = 遗迹道具.FirstOrDefault(ss => ss.道具名字 == s);
                        if (道具 != null)
                        {
                            道具.是否存在 = true;
                            道具名 = 道具.道具名字;
                            优先级 = 道具.优先级;
                            break;
                        }
                    }
                }

                return (道具名, 优先级);
            }
            void 购买本页(string 购买道具名)
            {
                if (!尝试购买(购买道具名))
                {
                    往下拉();
                    尝试购买(购买道具名);
                }
            }
            bool 尝试购买(string 购买道具名)
            {
                var r = 找图Sim(购买道具名, 道具范围);
                if (r.dm_ret >= 0)
                {
                    购买偏移(购买道具名, r.x, r.y);
                    关闭叉叉();
                    return true;
                }
                return false;
            }
            void 购买偏移(string s, int x, int y)
            {
                x += 270;
                y += 8;
                // 领取按钮
                点击坐标(x, y);

                if (点击按钮("遗迹领取奖励按钮", 5000).dm_ret >= 0)
                {
                    // 取消领奖
                    点击坐标(x, y);
                    Print("购买了  <<< " + s + " >>>");
                }
                else
                {
                    关闭叉叉();
                }
            }
            void 往下拉()
            {
                滚动(下拉的下边, 下拉到上边, -20);
            }

            void 往上拉()
            {
                滚动(下拉到上边, 下拉的下边, 20);
            }

            void 滚动(int[] 起点, int[] 终点, int 步长)
            {
                dm.MoveTo(起点[0], 起点[1]);
                dm.LeftDown();
                for (int i = 起点[1]; i != 终点[1]; i += 步长)
                {
                    dm.MoveTo(终点[0], i);
                    Delay(0.1);
                }
                dm.MoveTo(终点[0], 终点[1]);
                dm.LeftUp();
                Thread.Sleep(3000);
            }
        }
        private async Task 联盟科技捐献()
        {
            this.方法入口("联盟科技捐献");

            //10个科技点,30分钟恢复1点，10点需要5小时
            //先查询贡献值，够1K就 任务.联盟科技捐献间隔=570
            if (!打开标签页("联盟界面"))
                return;

            //联盟排行按钮
            点击坐标(475, 655);
            if (!await 找字匹配(605, 10, 673, 37, "联盟榜"))
                return;

            //贡献排行
            点击坐标(560, 62);
            //周排行
            点击坐标(600, 120);
            //if (!await 找字匹配(server, 482, 119, 534, 138, "日排行"))
            //    return;

            string 贡献值 = await 获取贡献值(server);
            if (double.TryParse(贡献值, out double 贡献值double) && 贡献值double >= 6)
            {
                Print($"{server} 发现周贡献满6K，结束今日科技捐献");
                this.方法出口记录当日时间("联盟科技捐献");
                return;
            }
            else
            {
                Print($"{server} 周贡献未满6K,准备开始捐献");
            }

            if (!打开标签页("联盟界面"))
                return;

            if (点击按钮("联盟科技").dm_ret >= 0 && await 找字匹配(594, 13, 686, 37, "联盟科技"))
            {
                消除手指动画();

                var 科技推荐 = 找图Sim("联盟科技推荐", "435,97,844,744");

                if (科技推荐.dm_ret >= 0)
                {
                    点击坐标(科技推荐.x + 15, 科技推荐.y + 15);
                    if (点击按钮("联盟科技捐献按钮").dm_ret >= 0)
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            dm.LeftClick();
                            await Task.Delay(1000);
                        }
                        if (找图("联盟科技捐献满").dm_ret >= 0)
                            this.方法出口记录当前时间("联盟科技捐献");
                        关闭叉叉();
                    }
                }

            }
            async Task<string> 获取贡献值(int server)
            {
                string 贡献值 = await 找字(611, 718, 675, 737);
                贡献值 = 贡献值.Replace("K", "");
                return OCR修正字符(贡献值);
            }
            void 消除手指动画()
            {
                //点一下再点回来，动画就没了
                if (找图("联盟科技之发展亮").dm_ret >= 0)
                {
                    点击坐标(720, 120); 点击坐标(520, 120);
                }
                else
                {
                    点击坐标(520, 120); 点击坐标(720, 120);
                }
            }
        }
        async Task 联盟科技捐献之遗迹()
        {
            this.方法入口("联盟科技捐献之遗迹");

            关闭箭头叉叉();
            //通过输入坐标到达遗迹 并且点击
            await 通过坐标到达(255, 189);

            if (!await 找字匹配(508, 382, 570, 404, "和平状态"))
                return;

            //点击捐献按钮
            点击坐标(685, 460);
            if (!await 找字匹配(669, 576, 719, 603, "捐献"))
                return;

            for (int i = 0; i < 11; i++)
            {
                //捐献11次
                点击坐标(710, 575);
            }

            关闭叉叉();
            点击坐标(取消点[0], 取消点[1]);

            this.方法出口记录当前时间("联盟科技捐献之遗迹");

        }
        private void 联盟机甲捐献()
        {
            this.方法入口("联盟机甲捐献");

            //联盟界面->联盟活动有红点->进去捐献
            //每天固定时间段每隔多少时间进去看是否能挑战机甲
            if (!打开标签页("联盟界面"))
                return;

            if (ClickPic("联盟活动按钮"))
            {
                捐献机甲("联盟机甲按钮0");
                捐献机甲("联盟机甲按钮1");

                关闭左箭头();
                关闭左箭头();
            }

            this.方法出口记录当前时间("联盟机甲捐献");

            void 捐献机甲(string 按钮名称)
            {
                if (ClickPic(按钮名称))
                {
                    if (ClickPic("联盟机甲免费捐献按钮"))
                    {
                        this.方法出口记录当日时间("联盟机甲捐献");
                    }
                }
            }
        }
        private async Task 联盟帮助与请求()
        {
            this.方法入口("联盟帮助与请求");
            int[] 请求帮助四个宝石坐标 = { 500, 600, 700, 800, 256 };//最后一位是Y坐标

            if (!打开标签页("联盟界面"))
                return;

            if (点击按钮("联盟帮助按钮").dm_ret >= 0)
            {
                点击按钮("帮助全部按钮");

                await 领取奖励();
                await 请求帮助();

                string 今日获得联盟积分 = await 找字(616, 121, 713, 144);
                string result = await 找字(724, 640, 809, 679);
                if (!是字符串包含字符串(result, "领取奖励") && !是字符串包含字符串(result, "请求帮助") && 今日获得联盟积分 == "1000/1000")
                {
                    Print($"{server}今日请求次数满");
                    this.方法出口记录当日时间("联盟帮助与请求");
                    关闭叉叉();
                    return;
                }

                关闭叉叉();
                this.方法出口记录当前时间("联盟帮助与请求");
            }
            async Task 领取奖励()
            {
                if (点击按钮("联盟帮助领取奖励按钮").dm_ret >= 0)
                {
                    Print($"{server}发现请求帮助满，领奖");
                    dm.LeftClick();
                    await Task.Delay(3000);
                }
            }
            async Task 请求帮助()
            {
                if (点击按钮("帮助界面请求帮助按钮").dm_ret >= 0)
                {
                    //动作($"{server}发现可以请求帮助宝石，开始请求帮助");
                    Console.WriteLine($"{server}发现可以请求帮助宝石，开始请求帮助");

                    var 宝石数量 = await 获取宝石数量();
                    //动作($"宝石数量 月亮石{宝石数量.月亮石} 翡翠石{宝石数量.翡翠石} 玛瑙石{宝石数量.玛瑙石} 黑曜石{宝石数量.黑曜石}");
                    Console.WriteLine($"宝石数量 月亮石{宝石数量.月亮石} 翡翠石{宝石数量.翡翠石} 玛瑙石{宝石数量.玛瑙石} 黑曜石{宝石数量.黑曜石}");

                    int 最小值 = Math.Min(Math.Min(宝石数量.月亮石, 宝石数量.翡翠石), Math.Min(宝石数量.玛瑙石, 宝石数量.黑曜石));
                    //动作($"宝石数量最少为 -- {最小值}");
                    Console.WriteLine($"宝石数量最少为 -- {最小值}");

                    点击最少宝石(最小值, 宝石数量);
                }
            }
            async Task<(int 月亮石, int 翡翠石, int 玛瑙石, int 黑曜石)> 获取宝石数量()
            {
                string 月亮石 = OCR修正字符(await 找字(505, 270, 538, 290));
                string 翡翠石 = OCR修正字符(await 找字(595, 270, 627, 290));
                string 玛瑙石 = OCR修正字符(await 找字(689, 270, 718, 290));
                string 黑曜石 = OCR修正字符(await 找字(776, 270, 809, 290));

                return (
                    int.TryParse(月亮石, out int 月亮石数量) ? 月亮石数量 : 999,
                    int.TryParse(翡翠石, out int 翡翠石数量) ? 翡翠石数量 : 999,
                    int.TryParse(玛瑙石, out int 玛瑙石数量) ? 玛瑙石数量 : 999,
                    int.TryParse(黑曜石, out int 黑曜石数量) ? 黑曜石数量 : 999
                );
            }
            void 点击最少宝石(int 最小值, (int 月亮石, int 翡翠石, int 玛瑙石, int 黑曜石) 宝石数量)
            {
                if (最小值 == 宝石数量.月亮石)
                {
                    点击坐标(请求帮助四个宝石坐标[0], 请求帮助四个宝石坐标[4], 3000);
                }
                else if (最小值 == 宝石数量.翡翠石)
                {
                    点击坐标(请求帮助四个宝石坐标[1], 请求帮助四个宝石坐标[4], 3000);
                }
                else if (最小值 == 宝石数量.玛瑙石)
                {
                    点击坐标(请求帮助四个宝石坐标[2], 请求帮助四个宝石坐标[4], 3000);
                }
                else if (最小值 == 宝石数量.黑曜石)
                {
                    点击坐标(请求帮助四个宝石坐标[3], 请求帮助四个宝石坐标[4], 3000);
                }
                else
                {
                    Print($"{server} 宝石最小值和当前宝石数据不匹配");
                }

                点击按钮("联盟帮助请求帮助确定按钮");
            }
        }
        private void 联盟礼物()
        {
            this.方法入口("联盟礼物");

            if (!打开标签页("联盟界面"))
                return;

            if (点击按钮("联盟礼物按钮").dm_ret >= 0)
            {
                领取礼物(500, 325, "一键领取联盟礼物按钮"); // 普通礼物
                领取稀有礼物(700, 325, "一键领取10个联盟礼物按钮"); // 稀有礼物
                领取联盟大箱子(660, 160, "联盟礼物大箱子蓝色确定按钮"); // 联盟大箱子
                关闭左箭头();
            }

            this.方法出口记录当前时间("联盟礼物");

            void 领取礼物(int x, int y, string 按钮名称)
            {
                点击坐标(x, y);
                点击按钮(按钮名称);
            }
            void 领取稀有礼物(int x, int y, string 按钮名称)
            {
                点击坐标(x, y);
                for (int i = 0; i < 50; i++)
                {
                    if (点击按钮(按钮名称).dm_ret < 0)
                        break;
                }
            }
            void 领取联盟大箱子(int x, int y, string 按钮名称)
            {
                点击坐标(x, y);
                var s = 点击按钮(按钮名称);
                if (s.dm_ret >= 0)
                    点击坐标(s.x, s.y);
            }
        }
        //👇👇👇 联盟面板外任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        private async Task 英雄招募()
        {
            this.方法入口("英雄招募");

            bool 普通招募次数无 = false;
            bool 技能研究次数无 = false;

            if (!打开标签页("世界界面"))
                return;

            //招募按钮
            Click(1220, 430);
            if (!await OCR是包含字符("英雄列表", 580, 11, 670, 35))
                return;

            //英雄招募字样
            Click(670, 700);
            if (!await OCR是包含字符("英雄招募", 580, 11, 670, 35))
                return;

            //点击普招按钮
            Click(450, 690);
            if (ClickPic("招募1次免费", 5))
            {
                dm.LeftClick();
                Delay(2);
            }

            if (await OCR是包含字符("0/5", 634, 509, 662, 524))
            { Print($"{server}普通招募次数无"); 普通招募次数无 = true; }

            //点击技能研究按钮
            Click(800, 690);
            if (ClickPic("招募1次免费", 5))
            {
                dm.LeftClick();
                Delay(2);
            }

            if (await OCR是包含字符("0/5", 634, 509, 662, 524))
            { Print($"{server}技能研究次数无"); 技能研究次数无 = true; }


            if (普通招募次数无 && 技能研究次数无)
            { this.方法出口记录当日时间("英雄招募"); 关闭箭头叉叉(); return; }

            this.方法出口记录当前时间("英雄招募");

            打开标签页("世界界面");

        }
        private async Task 英雄高级招募()
        {
            this.方法入口("英雄高级招募");

            if (!打开标签页("世界界面"))
                return;

            //招募按钮
            Click(1220, 430);
            if (!await OCR是包含字符("英雄列表", 580, 11, 670, 35))
                return;

            //英雄招募字样
            Click(670, 700);
            if (!await OCR是包含字符("英雄招募", 580, 11, 670, 35))
                return;

            //默认进入高级招募栏目
            if (ClickPic("招募1次免费", 5))
            {
                dm.LeftClick();
                Delay(2);
            }

            int 高招剩余分钟数 = await 返回分钟数(540, 617, 594, 632);
            if (高招剩余分钟数 > 0)
            {
                this.任务信息["英雄高级招募"].执行间隔 = 高招剩余分钟数;
                this.方法出口记录当前时间("英雄高级招募");
                写入配置();
            }
            else
                Trint($"{this.Server}'英雄高级招募'识别剩余时间出错");

            打开标签页("世界界面");

        }
        private void 礼包商城()
        {
            this.方法入口("礼包商城");

            关闭箭头叉叉();
            //礼包商城按钮
            点击坐标(1200, 20);
            每日补给();
            //特惠标签
            点击坐标(580, 70);
            特惠礼包();
            关闭左箭头();
            this.方法出口记录当前时间("礼包商城");
            void 每日补给()
            {
                //礼包坐标
                点击坐标(460, 225); Thread.Sleep(6000);
                //取消领奖页面
                dm.LeftClick(); Thread.Sleep(3000);
            }
            void 特惠礼包()
            {
                //礼包坐标
                点击坐标(805, 155); Thread.Sleep(6000);
                //取消领奖页面
                dm.LeftClick(); Thread.Sleep(3000);

                //第一格积累奖励礼包
                点击坐标(520, 275); Thread.Sleep(6000);
                //取消领奖页面
                dm.LeftClick(); Thread.Sleep(3000);
            }
        }
        private async Task 军级奖励()
        {
            this.方法入口("军级奖励");

            关闭箭头叉叉();
            //点击军级奖励坐标
            点击坐标(65, 90);
            string 军级界面 = await 找字(616, 11, 664, 34);
            if (!是字符串包含字符串(军级界面, "军级"))
                return;

            // 点击特惠礼包领奖
            点击坐标(780, 105); Thread.Sleep(3000);
            //取消领奖界面
            dm.LeftClick(); Thread.Sleep(2000);

            //确定点击过
            string 剩余时间 = await 找字(766, 150, 831, 163);
            if (是字符串包含数字(剩余时间))
            {
                Print($"{server}今日军级奖励已领取");
                this.方法出口记录当日时间("军级奖励");
                关闭箭头叉叉();
            }
        }
        private async Task 收取金币()
        {
            this.方法入口("收取金币");

            关闭箭头叉叉();
            点击坐标(800, 720); Thread.Sleep(8000);  //世界 主城 按钮坐标
            if (找图("主城标识", "7,370,20,394").dm_ret < 0)
                return;
            //主城视角拉最远
            视角拉远();
            //左上金币
            dm.MoveTo(177, 15); Delay(0.1); dm.LeftClick(); Thread.Sleep(3000);
            //锁定金币收割机
            dm.MoveTo(490, 555); dm.LeftDown(); Delay(0.1); dm.MoveTo(490, 460); dm.LeftUp(); Thread.Sleep(3000);

            int dm_ret = dm.FindPic(屏幕分辨率[0], 屏幕分辨率[1], 屏幕分辨率[2], 屏幕分辨率[3], "金币收割机按钮.bmp", "000000", 0.6, 0, out object x, out object y);
            if (dm_ret >= 0)
            {
                点击坐标((int)x, (int)y, 10000);

                await 金融中心购物车();

                dm_ret = dm.FindPicSim(13, 165, 944, 672, "金币收割机.bmp", "202020", 50, 0, out x, out y);
                if (dm_ret >= 0)
                {
                    dm.MoveTo((int)x, (int)y); Delay(0.1); dm.LeftClick(); Thread.Sleep(3000);
                    //再点一下收取每日免费次数
                    dm.MoveTo((int)x, (int)y); Delay(0.1); dm.LeftClick(); Thread.Sleep(3000);

                    //如果弹出友情提示
                    if (await 找字匹配(592, 188, 696, 225, "友情提示"))
                    {
                        //点击确定
                        点击坐标(700, 480);
                    }


                    if (await 找字匹配(648, 512, 690, 533, "免费"))
                    {
                        点击坐标(648, 512);
                        Print($"{server}免费收割了一次金币");
                    }

                    点击坐标(取消点[0], 取消点[1]);
                }
            }
            int 返回世界 = 0;
            while (true)
            {
                点击坐标(800, 720); Thread.Sleep(5000);//世界 主城 按钮坐标
                Print($"{server}金币收割后尝试返回主城: {返回世界}");
                if (找图("主城标识", "7,370,20,394").dm_ret < 0)
                    break;
            }
            this.方法出口记录当前时间("收取金币");
        }
        private async Task 金融中心购物车()
        {
            int dm_ret = dm.FindPicSim(13, 165, 944, 672, "金融中心闹钟.bmp", "202020", 50, 0, out object x, out object y);
            if (dm_ret >= 0)
            {
                点击坐标((int)x, (int)y, 3000);

                点击坐标(590, 75); //购物车

                if (await 找字匹配(743, 691, 838, 739, "一键兑换"))
                {
                    点击坐标(777, 711); //一键兑换
                    if (await 找字匹配(612, 232, 672, 269, "提示"))
                    {
                        点击坐标(730, 500, 5000); //确定
                        点击坐标(取消点[0], 取消点[1]);
                    }
                }
                关闭左箭头();
            }
        }
        private async Task 模块研究()
        {
            this.方法入口("模块研究");

            打开标签页("世界界面");

            //常规活动
            点击坐标(1240, 85, 3000);

            if (!await 尝试进入模块研究())
            { Print($"未找到模块研究栏目"); return; }

            if (await 找字匹配(735, 596, 819, 631, "领取"))
            {
                点击坐标(760, 610);
                点击坐标(600, 500);
            }
            if (await 找字匹配(735, 596, 819, 631, "已领取"))
            {
                this.方法出口记录当日时间("模块研究");
            }

            if (await 找字匹配(548, 624, 569, 638, "0/1"))
            {

                if (!打开标签页("每日军情界面"))
                    return;

                if (军情任务标签存在("每日军情荒野行动", out int x, out int y))
                {
                    点击坐标(x, y);

                    string[] 房间列表 = { "荒废研究所", "无尘贮藏库", "破旧信号塔", "损毁能源站" };
                    foreach (var 房间 in 房间列表)
                    {
                        if (await 荒野行动扫荡(房间))
                            return;
                    }

                }

            }
            async Task<bool> 尝试进入模块研究()
            {
                int[] xCoordinates = { 500, 600, 700, 800 };

                foreach (int x in xCoordinates)
                {
                    if (await 找字匹配(618, 699, 661, 722, "研究"))
                        return true;

                    点击坐标(x, 75);
                }

                return await 找字匹配(618, 699, 661, 722, "研究");
            }
            async Task<bool> 荒野行动扫荡(string 房间名称)
            {
                var 房间 = 房间名称 switch
                {
                    "荒废研究所" => (450, 200),
                    "无尘贮藏库" => (455, 570),
                    "破旧信号塔" => (800, 300),
                    "损毁能源站" => (700, 630),
                    _ => (0, 0)
                };

                点击坐标(房间.Item1, 房间.Item2);
                int 卡片数量 = await 得到卡片数量();
                if (卡片数量 > 1)
                {
                    点击坐标(680, 680);
                    点击坐标(取消点[0], 取消点[1]);
                    打开标签页("世界界面");
                    return true;
                }
                else
                {
                    Print($"{房间名称} 卡片数量不足");
                    关闭左箭头();
                }
                return false;
            }
            async Task<int> 得到卡片数量()
            {
                string s = await 找字(469, 63, 539, 86);
                s = OCR修正字符(s);
                int.TryParse(s, out int 卡片数量);
                return 卡片数量;
            }

        }
        private static bool 是战争之源时间段()
        {
            //4-7 11-14 18-21
            TimeSpan[] startTimes = { new TimeSpan(4, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(18, 0, 0) };
            TimeSpan[] endTimes = { new TimeSpan(6, 45, 0), new TimeSpan(13, 45, 0), new TimeSpan(20, 45, 0) };

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            for (int i = 0; i < startTimes.Length; i++)
            {
                if (currentTime >= startTimes[i] && currentTime <= endTimes[i])
                {
                    return true;
                }
            }

            return false;
        }
        private async Task 战争之源()
        {
            this.方法入口("战争之源");
            关闭箭头叉叉();

            if (!是战争之源时间段())
                goto 条件不足退出;

            if (!查询队列有空闲())
            { Print($"{server} 战争之源任务 无队列"); goto 条件不足退出; }

            if (!打开标签页("世界界面"))
                goto 条件不足退出;


            //查询体力
            点击坐标(130, 15);
            bool 恢复体力 = await 找字匹配(597, 219, 684, 247, "恢复体力");
            if (!恢复体力)
            { Print($"{server}体力药剂栏点击失败"); goto 条件不足退出; }

            //查询体力
            int 体力值 = await 获取当前体力值();

            if (体力值 < 0 || 体力值 > 75)
            { Print($"{server}当前体力值获取失败"); goto 条件不足退出; }

            if (体力值 > 0 && 体力值 < 5)
            { Print($"{server}当前体力值小于5"); goto 条件不足退出; }

            if (体力值 > 5)
                Print($"{server}当前体力{体力值}");
            点击坐标(取消点[0], 取消点[1]);


            点击坐标(1245, 80);  //常规活动
            //战争之源标签
            var (x, y) = (0, 0);
            for (int i = 0; i < 10; i++)
            {
                int xxx = 120 * 30;
                string send = $"s490,75,{xxx},0";

                string recive = await SendRequestAsync($"MyPipe{this.Server}", send);

                var r = await 找字List(439, 53, 855, 105);
                var cood = r.Where(p => p.Text == "战争之源")
                                     .Select(p => (p.TopLeftX, p.TopLeftY))
                                     .FirstOrDefault();
                if (cood.TopLeftX != 0)
                {
                    x = cood.TopLeftX + 459;
                    y = cood.TopLeftY + 53;
                    break;
                }
            }
            点击坐标(x, y);


            if (!await 找字匹配(590, 690, 688, 723, "快速搜索"))
                return;

            点击坐标(590, 690, 5000);

            //屏幕中间
            点击坐标(640, 380);

            string Str攻击次数 = await 找字(659, 311, 691, 329);
            if (string.IsNullOrEmpty(Str攻击次数))
            { Print($"{server}攻击次数获取失败"); return; }
            Str攻击次数 = Str攻击次数.Replace("/5", "");
            int 攻击次数 = Convert.ToInt32(Str攻击次数);
            if (攻击次数 < 0 || 攻击次数 > 5)
            { Print($"{server}攻击次数转换整数失败"); return; }
            if (攻击次数 == 5)
                Print($"{server}今日战争之源攻击次数{攻击次数}");

            bool 进入战斗 = false;
            bool 脱离战斗 = false;
            if (攻击次数 != 5)
            {
                //攻击
                点击坐标(700, 475);
                进入战斗 = 找图("战斗界面返回箭头", "0,14,59,61").dm_ret >= 0;
                //2#队伍
                点击坐标(1010, 640);
                //出征
                点击坐标(640, 300);
                脱离战斗 = 找图("战斗界面返回箭头", "0,14,59,61").dm_ret < 0;
            }

            //发生过一次成功的攻击
            if (进入战斗 && 脱离战斗)
            {
                点击坐标(取消点[0], 取消点[1]);
                更新战争之源今日攻击时间();
            }

            //没有发生攻击
            if (攻击次数 == 5)
            {
                点击坐标(取消点[0], 取消点[1]);
                this.方法出口记录当日时间("战争之源");
            }
            return;

        条件不足退出:
            {
                this.方法出口记录当前时间("战争之源");
                return;
            }

            void 更新战争之源今日攻击时间()
            {
                //成功攻击1次才会进入这里
                DateTime now = DateTime.Now;
                int todayDay = now.Day;

                foreach (var entry in this.战争之源信息)
                {
                    if (entry.Value.攻击时间.Day != todayDay)
                    {

                        entry.Value.攻击时间 = now;

                        this.方法出口记录当前时间("战争之源");

                        this.其他信息["战争之源攻击次数"].Info = this.战争之源今日攻击次数;

                        this.写入配置();

                        break;
                    }
                }

            }

        }
        private void 打螺丝()
        {
            this.方法入口("打螺丝");

            if (!打开标签页("军火库界面"))
                return;

            //兵种装备
            点击坐标(640, 70);
            //生产材料
            点击坐标(600, 400);
            //取消可能出现的领奖界面
            点击坐标(取消点[0], 取消点[1]);
            //开始打螺丝
            var r = this.螺丝选中项.Value;
            for (int i = 0; i < 6; i++)
            {
                点击坐标(r.Value.x, r.Value.y);
            }
            Print($"{server}打了螺丝 ---> {r.Key}");
            this.方法出口记录当前时间("打螺丝");

            打开标签页("世界界面");

        }
        //👇👇👇 每日军情任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        private bool 军情任务标签存在(string 任务名, out int x, out int y)
        {

            //先找字体,比如远征行动得到坐标 750 771 确定下方前进按钮查找范围  740,860,791,889
            //确定偏移量,确定按钮查找偏移 按钮x-10,按钮y+89,按钮x+41,按钮y+118
            var r = 找图Sim(任务名, "686,67,826,733");
            //找到图并且按钮查找范围不能越界
            if (r.dm_ret >= 0 && (r.y + 118) < 733)
            {
                //动作("进入 ---> " + 军情类别);
                string 前进按钮查找范围 = (r.x - 50).ToString() + "," + (r.y + 89).ToString() + "," + (r.x + 50).ToString() + "," + (r.y + 118).ToString();
                r = 找图Sim("每日军情前往按钮", 前进按钮查找范围);
                if (r.dm_ret >= 0)
                { x = r.x; y = r.y; return true; }
            }
            x = -1;
            y = -1;
            return false;

        }
        private async Task 每日军情任务()
        {
            int[] 标签范围 = { 428, 52, 850, 91 };

            if (!打开标签页("每日军情界面"))
                return;

            //动作($"{server} 开始执行每日军情任务");

            //动作条拉动默认三次
            for (int i = 0; i < 3 && !this.ctsMainTask.IsCancellationRequested; i++)
            {

                foreach (var 任务 in this.每日军情任务列表)
                {
                    if (!this.ctsMainTask.IsCancellationRequested && 检查任务是否可执行(任务) && 军情任务标签存在(任务, out int x, out int y))
                    {
                        点击坐标(x, y);
                        await 执行任务(任务);
                    }
                }

                鼠标拉动(650, 680, 650, 220);

            }

            打开标签页("世界界面");

        }
        private void 每日军情荒野行动()
        {
            this.方法入口("每日军情荒野行动");

            //领取奖励
            点击坐标(460, 150);
            //领取按钮
            点击坐标(640, 610, 5000);
            点击坐标(取消点[0], 取消点[1]);

            关闭左箭头();
            this.方法出口记录当前时间("每日军情荒野行动");

        }
        private void 每日军情沙盘演习()
        {
            this.方法入口("每日军情沙盘演习");

            //沙盘演习 挑战海军
            点击坐标(600, 70);

            //挑战按钮灰
            for (int i = 0; i < 5; i++)
            {
                if (!挑战按钮是灰色())
                {
                    执行挑战流程();
                }
                else
                {
                    break;
                }
            }

            if (挑战按钮是灰色())
            {
                this.方法出口记录当日时间("每日军情沙盘演习");
            }

            关闭左箭头();

            bool 挑战按钮是灰色()
            {
                return 找图("沙盘演习挑战按钮灰").dm_ret >= 0;
            }

            void 执行挑战流程()
            {
                // 点击挑战按钮
                点击坐标(630, 705, 3000);
                // 选择2#队伍
                点击坐标(1010, 635);
                // 点击战斗
                点击坐标(640, 300, 3000);
                // 点击跳过按钮
                点击坐标(15, 120, 3000);
                // 点击返回
                点击坐标(680, 666, 3000);
            }

        }
        private void 每日军情远征行动()
        {
            this.方法入口("每日军情远征行动");

            //点船领奖
            点击坐标(535, 625);
            点击坐标(640, 655);
            //点击取消
            dm.LeftClick();
            Thread.Sleep(3000);

            //免费快速战斗
            if (找图("远征行动快速战斗").dm_ret >= 0)
            {
                //点击快速战斗按钮
                点击坐标(789, 706);
                //远征快速战斗免费按钮
                点击坐标(635, 533);
                //远征快速战斗跳过动画
                点击坐标(805, 670);
                //远征行动领奖领取按钮
                点击坐标(640, 655);
            }

            关闭左箭头();
            this.方法出口记录当前时间("每日军情远征行动");
        }
        private async Task 每日军情跨战区演习()
        {
            this.方法入口("每日军情跨战区演习");

            //未开启-下轮开启倒计时   开启中-本轮结束倒计时
            if (!await 找字匹配(441, 90, 538, 108, "结束"))
            {
                Print("跨区演习未开启");
                return;
            }

            //点击挑战
            点击坐标(600, 700);
            //
            for (int i = 0; i < 12; i++)
            {

                //查看 今日免费挑战次数              
                string s = await 找字(690, 161, 705, 178);
                s = OCR修正字符(s);
                if (s == "0")
                {
                    this.方法出口记录当日时间("每日军情跨战区演习");
                    return;
                }

                // Perform challenge actions
                PerformChallengeActions();

            }

            关闭叉叉();
            关闭左箭头();

            void PerformChallengeActions()
            {
                点击坐标(740, 585); // Click free challenge
                点击坐标(1010, 640); // Select team 2
                点击坐标(630, 290); // Click battle
                点击坐标(18, 118); // Click skip
                点击坐标(600, 645); // Click return
            }

        }
        private async Task 每日军情次元矿洞()
        {
            this.方法入口("每日军情次元矿洞");
            var 采矿星级 = Convert.ToInt32(this.其他信息["采矿星级"].Info);
            var 采矿等级 = Convert.ToString(this.其他信息["采矿等级"].Info);
            var 采矿类型 = Convert.ToString(this.其他信息["采矿类型"].Info);
            if (采矿星级 == 0 || string.IsNullOrEmpty(采矿等级) || string.IsNullOrEmpty(采矿类型))
            { Print($"{server}本地采矿星级/等级/类别读取失败"); return; }

            int 获取到未采集矿脉的剩余分钟数 = 0;
            bool 存在未收取的矿脉 = 找图("矿洞没有可收集的矿脉", "490,637,621,702", 0.9).dm_ret < 0;
            if (存在未收取的矿脉)
            {
                Print($"{server}存在未收取的矿脉");
                await 收集盟友矿洞采集的矿脉();
            }

            存在未收取的矿脉 = 找图("矿洞没有可收集的矿脉", "490,637,621,702", 0.9).dm_ret < 0;
            if (存在未收取的矿脉)
            {
                Print($"{server}尝试收取后依然 存在未收取的矿脉");
            }

            int 采矿计数 = 0;
            var (当前剩余行军队列, 当前剩余采矿次数) = await 返回可采集的次数();

            if (采矿计数 != 0 && !存在未收取的矿脉)
            {
                if (进入盟友矿洞成功())
                {
                    for (int i = 0; i < 200; i++)
                    {
                        if (采矿计数 == 0)
                        {
                            Console.WriteLine($"采矿完成,次数用完");
                            break;
                        }
                        await 采集此页矿脉();
                        盟友矿洞右箭头("点击");
                    }
                }
                else
                    Print($"{server}进入盟友矿洞失败");
            }

            if (当前剩余采矿次数 == 0)
            {
                Print($"{server}剩余行军队列{当前剩余行军队列} 剩余采矿次数{当前剩余采矿次数} 当日采矿完成");
                Console.WriteLine($"{server}剩余行军队列{当前剩余行军队列} 剩余采矿次数{当前剩余采矿次数} 当日采矿完成");
                this.方法出口记录当日时间("每日军情次元矿洞");
                返回军情页面();
                return;
            }
            else
            {
                if (获取到未采集矿脉的剩余分钟数 > 0)
                {
                    this.任务信息["每日军情次元矿洞"].执行间隔 = 获取到未采集矿脉的剩余分钟数;
                }
                else
                {
                    this.任务信息["每日军情次元矿洞"].执行间隔 = 5;
                }
            }

            this.方法出口记录当前时间("每日军情次元矿洞");

            //左箭头退出
            返回军情页面();

            //打开标签页(服务器, "每日军情界面", $"{server}每日军情次元矿洞任务完毕");


            void 返回军情页面()
            {
                for (int i = 0; i < 5; i++)
                {
                    if (!处于界面("每日军情界面"))
                    {
                        点左箭头();
                        关闭叉叉();
                    }
                    else
                        return;
                }
            }
            void 点左箭头()
            {
                点击坐标(440, 20);
            }
            async Task<bool> 收集盟友矿洞采集的矿脉()
            {
                //采集奖励 点第一个位置就可以了 如果采集时间未到 则关闭采集页面
                for (int i = 0; i < 3; i++)
                {

                    点击坐标(535, 670, 5000);  //领取奖励位置

                    if (await 处理开采预览()) //发现有采集未到时间的矿脉就退出(到时间就领取奖励)
                    {
                        return true;
                    }

                }
                //矿车位置奖励
                点击坐标(490, 455, 5000);
                点击坐标(取消点[0], 取消点[1]);
                return false;
            }
            async Task<bool> 处理开采预览()
            {

                //领取奖励
                if (await 找字匹配(600, 502, 683, 540, "领取"))
                {
                    点击坐标(570, 510, 5000);
                    点击坐标(取消点[0], 取消点[1]);
                    Print($"{server} 领取了一次盟友矿脉奖励");
                    return false;
                }

                //奖励未到时间领取
                else if (await 找字匹配(586, 572, 702, 607, "前往查看"))
                {
                    int 剩余分钟数 = await 返回分钟数(500, 253, 559, 270);
                    if (剩余分钟数 > 0)
                    {
                        获取到未采集矿脉的剩余分钟数 = 剩余分钟数;
                        Print($"{server} 采矿剩余时间 {剩余分钟数} 分钟");
                    }
                    else
                    {
                        Print($"{server} 采矿剩余时间 获取失败");
                    }

                    关闭叉叉();
                    Print($"{server} 发现采矿时间未到");
                    return true;
                }

                else
                {
                    点击坐标(取消点[0], 取消点[1]);
                    Print($"{server} 默认收取了一次矿脉 或者 未发现可收集矿脉");
                    return false;
                }

            }
            async Task<(int, int)> 返回可采集的次数()
            {
                string 剩余 = await 找字(464, 652, 491, 667);
                剩余 = 剩余.Replace(" ", "");
                string 当前剩余行军队列 = 剩余.Split('/')[0];
                Console.WriteLine($"当前剩余行军队列{当前剩余行军队列}"); await Task.Delay(2000);

                剩余 = await 找字(463, 677, 490, 692);
                剩余 = 剩余.Replace(" ", "");
                string 当前剩余采矿次数 = 剩余.Split('/')[0];
                Console.WriteLine($"当前剩余采矿次数{当前剩余采矿次数}"); await Task.Delay(2000);


                int int当前剩余采矿次数 = Convert.ToInt32(当前剩余采矿次数);
                int int当前剩余行军队列 = Convert.ToInt32(当前剩余行军队列);
                int int当前队列数 = Convert.ToInt32(this.其他信息["队列数"].Info);
                采矿计数 = new int[] { int当前剩余采矿次数, int当前剩余行军队列, int当前队列数 }.Min();

                Print($"{server} 当前剩余行军队列'{当前剩余行军队列}'");
                Print($"{server} 当前剩余采矿次数'{当前剩余采矿次数}'");
                Print($"{server} 采矿计数为'{采矿计数}'");
                return (Convert.ToInt32(当前剩余行军队列), Convert.ToInt32(当前剩余采矿次数));
            }
            bool 进入盟友矿洞成功()
            {
                //点击盟友矿洞
                点击坐标(500, 725);
                //右箭头
                点击坐标(805, 175);
                return 盟友矿洞右箭头("发现");
            }
            bool 盟友矿洞右箭头(string 指令)
            {
                var result = 找图("盟友矿洞右箭头", "818,342,860,413");
                if (指令 == "发现")
                    return result.dm_ret >= 0;
                else if (指令 == "点击")
                {
                    if (result.dm_ret >= 0)
                    {
                        点击坐标(result.x, result.y, 2500);
                        return true;
                    }
                }
                return false;
            }
            async Task 采集此页矿脉()
            {

                //上面搜索范围 下面搜索范围
                int[][] 搜索范围 = { new int[] { 633, 216, 742, 304 }, new int[] { 622, 452, 746, 547 } };

                int index = 0;
                foreach (var 范围 in 搜索范围)
                {
                    if (await 是符合要求的矿脉(范围[0], 范围[1], 范围[2], 范围[3]))
                    {
                        //上边 下边 矿
                        点击坐标(695, index == 0 ? 210 : 440);

                        // 自动布阵
                        点击坐标(510, 640);
                        // 派遣
                        点击坐标(710, 640);
                        // 点一下加速
                        点击坐标(index == 0 ? 688 : 677, index == 0 ? 98 : 345);
                        Print($"采集了一次 {采矿星级}星 {采矿等级}级 {采矿类型}矿");
                        采矿计数--;

                        关闭叉叉();
                    }
                    index++;
                }
            }
            async Task<bool> 是符合要求的矿脉(int x1, int y1, int x2, int y2)
            {

                int 星级 = 返回星星数量(x1, y1, x2, y2);
                if (星级 != 采矿星级)
                    return false;

                string 矿脉信息 = await 返回矿脉信息(x1, y1, x2, y2);
                if (!矿脉信息.Contains("盟友专属"))
                    return false;
                if (!矿脉信息.Contains(采矿类型))
                    return false;
                if (!矿脉信息.Contains(采矿等级))
                    return false;

                return true;

            }
            int 返回星星数量(int x1, int y1, int x2, int y2)
            {
                string dm_ret = dm.FindPicEx(x1, y1, x2, y2, "矿洞星星.bmp", "050505", 0.7, 0);
                int count = dm_ret.Count(c => c == '|');
                if (count > 0)
                    return count + 1;
                else
                    return 0;
            }
            async Task<string> 返回矿脉信息(int x1, int y1, int x2, int y2)
            {
                string json = await 找字Json(x1, y1, x2, y2);
                List<DetectionResult> results = JsonConvert.DeserializeObject<List<DetectionResult>>(json);
                string combinedText = string.Join(" ", results.ConvertAll(result => result.Text));
                return combinedText;
            }

        }
        private async Task 每日军情岛屿作战()
        {
            this.方法入口("每日军情岛屿作战");

            int[] 岛屿作战范围 = { 428, 46, 852, 668 };

            for (int i = 0; i < 2; i++)
            {

                //优先点击蓝色按钮
                if (出现蓝色重置按钮然后点击("岛屿之蓝色前往按钮") >= 0)
                    Print($"{server}重置了岛屿作战");


                //尝试扫荡
                if (await 找字匹配(427, 698, 486, 725, "扫荡"))
                {
                    //点击扫荡
                    点击坐标(450, 690);
                    if (点击按钮("扫荡收取奖励", 7000).dm_ret >= 0)
                    {
                        //胜利确定按钮
                        dm.LeftClick(); Thread.Sleep(3000);
                        Print($"{server}扫荡成功一次!");

                        //发现小飞机,打通
                        bool 发现小飞机 = false;
                        for (int j = 0; j < 2; j++)
                        {
                            var r = 自己飞机();
                            if (r.自己飞机已找到)
                            { 发现小飞机 = true; break; }
                        }
                        //作战
                        if (发现小飞机)
                        {
                            int 飞机未找到的未知情况 = 0;
                            for (int j = 0; i < 100; j++)
                            {
                                int result0 = 岛屿作战();

                                if (result0 == -1)
                                {
                                    Print($"{server}未找到飞机次数:" + 飞机未找到的未知情况.ToString());
                                    飞机未找到的未知情况++;
                                }
                                else if (result0 == 0)
                                {
                                    Print($"{server}发现重置按钮并重置"); break;
                                }
                                //else if (result0 == 1)
                                //{
                                //    //动作("岛屿作战完成");
                                //    写入任务执行时间("岛屿作战");
                                //    return;
                                //}
                                if (飞机未找到的未知情况 > 20)
                                { Print($"{server}20次未找到飞机"); return; }
                            }
                        }
                    }
                }


                if (await 找字匹配(427, 698, 486, 725, "岛屿日志"))
                {
                    Print($"{server}发现岛屿日志,今日扫荡完毕!");
                    this.方法出口记录当日时间("每日军情岛屿作战");
                }

            }

            int 出现蓝色重置按钮然后点击(string s) //相似度0.9
            {
                //特定未知出现的按钮
                s += ".bmp";
                int dm_ret = dm.FindPic(658, 414, 701, 441, s, "000000", 0.9, 0, out object xx, out object yy);
                if (dm_ret >= 0)
                { 点击坐标((int)xx, (int)yy); Thread.Sleep(3000); }
                return dm_ret;
            }
            (bool 自己飞机已找到, int[] 飞机坐标, bool 血量足八成, int[] 自己上方查找范围) 自己飞机()
            {
                bool 血量足八成 = false;
                bool 自己飞机是否找到 = false;
                int[] 自己上方查找范围 = { -1, -1, -1, -1 };
                int[] 血条范围 = { -1, -1, -1, -1 };
                //实例(满血): 飞机坐标488,479  血条左480,455  血条右537,455   血条左右距离57
                //            上方查找范围(未采取这个实例) 429,323,859,448
                //又一例      飞机坐标707,512 此时上方范围应该为 426,355,854,481 上方x是固定的x1:426 x2:854
                //             上方y发生偏移，上移:y1-157 y2-31                       
                int dm_ret = dm.FindPicSim(岛屿作战范围[0], 岛屿作战范围[1], 岛屿作战范围[2], 岛屿作战范围[3], "岛屿之自己飞机.bmp", "202020", 50, 0, out object x, out object y);
                if (dm_ret >= 0)
                {
                    自己飞机是否找到 = true;
                    int thisX = (int)x;
                    int thisY = (int)y;
                    血条范围[0] = thisX - 19;
                    血条范围[1] = thisY - 35;
                    血条范围[2] = thisX + 61;
                    血条范围[3] = thisY - 15;
                    自己上方查找范围[0] = 436;
                    自己上方查找范围[1] = thisY - 157;
                    自己上方查找范围[2] = 854;
                    自己上方查找范围[3] = thisY - 31;
                    int 血条最左 = -1;
                    int 血条最右 = -1;
                    dm_ret = dm.FindColor(血条范围[0], 血条范围[1], 血条范围[2], 血条范围[3], "ffc209-000000", 1.0, 0, out x, out y);
                    if (dm_ret >= 0)
                    { 血条最左 = (int)x; }
                    dm_ret = dm.FindColor(血条范围[0], 血条范围[1], 血条范围[2], 血条范围[3], "ffc209-000000", 1.0, 2, out x, out y);
                    if (dm_ret >= 0)
                    { 血条最右 = (int)x; }
                    if (血条最左 != -1 && 血条最右 != -1)
                    {
                        //满血差57
                        int 血条差 = 血条最右 - 血条最左;
                        if (血条差 > 45)
                            血量足八成 = true;
                        else if (血条差 <= 45)
                            血量足八成 = false;
                    }
                    int[] 飞机坐标 = { thisX, thisY };
                    return (自己飞机是否找到, 飞机坐标, 血量足八成, 自己上方查找范围);
                }
                return (自己飞机是否找到, null, 血量足八成, 自己上方查找范围);
            }
            int 岛屿作战()
            {
                //按照图片优先级点击 
                //血量低则补血最优先
                List<前方的敌人> 敌人 = new List<前方的敌人>();

                bool 找图岛屿(string pic, int[] 作战范围, out int xxx, out int yyy)
                {
                    pic += ".bmp";
                    int dm_ret = dm.FindPicSim(作战范围[0], 作战范围[1], 作战范围[2], 作战范围[3], pic, "202020", 50, 0, out object x, out object y);
                    if (dm_ret >= 0)
                    { xxx = (int)x; yyy = (int)y; return true; }
                    xxx = -1;
                    yyy = -1;
                    return false;
                }
                void Boss()
                {
                    点击坐标(640, 108); Thread.Sleep(3000);
                    if (点击按钮("岛屿之红色攻击按钮").dm_ret >= 0)
                    //下一关
                    { Thread.Sleep(6000); 点击坐标(640, 190); Thread.Sleep(3000); }
                }
                int 点击岛屿按钮(string s) //相似度0.9
                {
                    //点击敌人后出现的前往或者攻击按钮 (岛屿之红色攻击按钮/岛屿之蓝色前往按钮)
                    s += ".bmp";
                    int dm_ret = dm.FindPic(岛屿作战范围[0], 岛屿作战范围[1], 岛屿作战范围[2], 岛屿作战范围[3], s, "000000", 0.9, 0, out object xx, out object yy);
                    if (dm_ret >= 0)
                    { 点击坐标((int)xx, (int)yy); Thread.Sleep(3000); }
                    return dm_ret;
                }
                //int 特定位置出现红色退出按钮然后点击(string s) //相似度0.9
                //{
                //    //特定未知出现的按钮
                //    s += ".bmp";
                //    int dm_ret = dm.FindPic(664, 419, 709, 432, s, "000000", 0.9, 0, out object xx, out object yy);
                //    if (dm_ret >= 0)
                //    { 点击坐标(dm, (int)xx, (int)yy); Thread.Sleep(3000); }
                //    return dm_ret;
                //}
                void 检查前方敌人(int[] 范围, bool 血量足八成)
                {
                    int xxx, yyy;
                    if (!血量足八成)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (找图岛屿("岛屿之补血帐篷", 范围, out xxx, out yyy))
                                foreach (var s in 敌人)
                                { if (s.敌人名字 == "补血") { s.x = xxx; s.y = yyy; s.是否存在 = true; return; } }
                        }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (找图岛屿("岛屿之兵营", 范围, out xxx, out yyy))
                            foreach (var s in 敌人)
                            { if (s.敌人名字 == "兵营") { s.x = xxx; s.y = yyy; s.是否存在 = true; return; } }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (找图岛屿("岛屿之士兵", 范围, out xxx, out yyy))
                            foreach (var s in 敌人)
                            { if (s.敌人名字 == "士兵") { s.x = xxx; s.y = yyy; s.是否存在 = true; return; } }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (找图岛屿("岛屿之金箱子", 范围, out xxx, out yyy))
                            foreach (var s in 敌人)
                            { if (s.敌人名字 == "金箱子") { s.x = xxx; s.y = yyy; s.是否存在 = true; return; } }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (找图岛屿("岛屿之紫碎", 范围, out xxx, out yyy))
                            foreach (var s in 敌人)
                            { if (s.敌人名字 == "紫碎") { s.x = xxx; s.y = yyy; s.是否存在 = true; return; } }
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (找图岛屿("岛屿之难民信", 范围, out xxx, out yyy))
                            foreach (var s in 敌人)
                            { if (s.敌人名字 == "难民信") { s.x = xxx; s.y = yyy; s.是否存在 = true; return; } }
                    }
                }
                (string 敌人名, int x, int y) 返回敌人信息(string 排序方式)
                {
                    //进入到这里已经默认为有敌人了
                    if (排序方式 == "补血优先")
                    {
                        foreach (var s in 敌人)
                        {
                            if (s.是否存在 && s.敌人名字 == "补血")
                                return ("补血", s.x, s.y);
                        }
                    }
                    //默认方式
                    foreach (var s in 敌人)
                    {
                        if (s.是否存在 && s.敌人名字 == "兵营")
                            return ("兵营", s.x, s.y);
                    }
                    foreach (var s in 敌人)
                    {
                        if (s.是否存在 && s.敌人名字 == "士兵")
                            return ("士兵", s.x, s.y);
                    }
                    foreach (var s in 敌人)
                    {
                        if (s.是否存在 && s.敌人名字 == "金箱子")
                            return ("金箱子", s.x, s.y);
                    }
                    foreach (var s in 敌人)
                    {
                        if (s.是否存在 && s.敌人名字 == "难民信")
                            return ("难民信", s.x, s.y);
                    }
                    foreach (var s in 敌人)
                    {
                        if (s.是否存在 && s.敌人名字 == "紫碎")
                            return ("紫碎", s.x, s.y);
                    }
                    return ("", 0, 0);
                }

                //主程
                #region 敌人数据初始化
                敌人.Add(new 前方的敌人
                {
                    敌人名字 = "补血",
                    是否存在 = false,
                    x = -1,
                    y = -1
                });
                敌人.Add(new 前方的敌人
                {
                    敌人名字 = "兵营",
                    是否存在 = false,
                    x = -1,
                    y = -1
                });
                敌人.Add(new 前方的敌人
                {
                    敌人名字 = "士兵",
                    是否存在 = false,
                    x = -1,
                    y = -1
                });
                敌人.Add(new 前方的敌人
                {
                    敌人名字 = "金箱子",
                    是否存在 = false,
                    x = -1,
                    y = -1
                });
                敌人.Add(new 前方的敌人
                {
                    敌人名字 = "紫碎",
                    是否存在 = false,
                    x = -1,
                    y = -1
                });
                敌人.Add(new 前方的敌人
                {
                    敌人名字 = "难民信",
                    是否存在 = false,
                    x = -1,
                    y = -1
                });
                #endregion
                //保险起见,扫描三次
                var r = 自己飞机();
                if (!r.自己飞机已找到)
                    r = 自己飞机();
                if (!r.自己飞机已找到)
                    r = 自己飞机();
                if (!r.自己飞机已找到)
                {
                    Print("出大事了,自己飞机找不到");
                    //阵亡后/通关后的重置按钮 都是这个
                    if (出现蓝色重置按钮然后点击("岛屿之蓝色前往按钮") >= 0)
                    {
                        return 0;
                    }
                    //其实如果没有蓝色重置按钮都可以看作出问题,超过次数退出完事
                    //这里通过判断右半边一定范围有无红色按钮判断
                    //if (特定位置出现红色退出按钮然后点击("岛屿之红色退出按钮") >= 0)
                    //{
                    //    动作("岛屿作战完成,再见!");
                    //    return 1;
                    //}
                    //顺利通关 664,419,709,432
                    //string 作战完成 = dm.Ocr(580, 245, 704, 284, "f9c657-000000", 1.0);
                    //if (作战完成 == "作战完成")
                    //    if (出现红色退出按钮然后点击("岛屿之红色攻击按钮") >= 0)
                    //    {
                    //        动作("作战完成");
                    //        return 1;
                    //    }
                    ////次数用完的阵亡画面
                    //string IsDead = dm.Ocr(575, 247, 712, 285, "aeaeb0-000000", 1.0);
                    //if (IsDead == "您已阵亡")
                    //{
                    //    if (出现蓝色重置按钮然后点击("岛屿之红色攻击按钮") >= 0)
                    //    {
                    //        动作("您已阵亡,可以重置");
                    //        return 1;
                    //    }
                    //}
                    return -1;
                }
                if (r.自己飞机已找到)
                {
                    //到达最上一格
                    if (r.飞机坐标[1] < 260)
                    { Print("抵达最上面, 进攻--->BOSS"); Boss(); return -1; }

                    int[] 上方范围 = r.自己上方查找范围;
                    检查前方敌人(上方范围, r.血量足八成);
                    bool 未发现敌人 = true;
                    foreach (var s in 敌人)
                        if (s.是否存在)
                            未发现敌人 = false;
                    if (!未发现敌人 && !r.血量足八成)
                    {
                        var s = 返回敌人信息("补血优先");
                        if (s.敌人名 == "")
                            Print("出大麻烦了,敌人信息未取到");
                        Print("血量不足八成, 进攻--->" + s.敌人名);
                        点击坐标(s.x, s.y);
                    }
                    else if (!未发现敌人 && r.血量足八成)
                    {
                        var s = 返回敌人信息("默认排序");
                        if (s.敌人名 == "")
                            Print("出大麻烦了,敌人信息未取到");
                        Print("血量足八成, 进攻--->" + s.敌人名);
                        点击坐标(s.x, s.y);
                    }
                    else if (未发现敌人)
                    {
                        //未发现敌人 默认点左边第一个
                        //点击坐标函数默认+15，这里-15
                        Print("未找到任何前方敌人,默认点最左边的");
                        int 飞机y坐标 = r.飞机坐标[1];
                        if (飞机y坐标 > 565)  //点击坐标(570, 555);
                            点击坐标(555, 540);
                        if (飞机y坐标 >= 475 && 飞机y坐标 < 565)  //点击坐标(500, 445);
                            点击坐标(485, 430);
                        if (飞机y坐标 >= 375 && 飞机y坐标 < 475)  //点击坐标(570, 350);
                            点击坐标(555, 335);
                        if (飞机y坐标 > 275 && 飞机y坐标 < 375)  //点击坐标(500, 240);
                            点击坐标(485, 225);
                    }
                    if (点击岛屿按钮("岛屿之红色攻击按钮") < 0)
                        if (点击岛屿按钮("岛屿之蓝色前往按钮") < 0)
                        { }
                }
                else
                    Print("未发现本方飞机");
                //写入任务执行时间("岛屿作战");
                return 0;
            }


        }
        //👇👇👇 金融中心任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        private async Task 金融中心套装商店()
        {
            this.方法入口("金融中心套装商店");
            int[] 标签范围 = { 428, 52, 850, 91 };

            try
            {
                if (!打开标签页("世界界面"))
                    Throw($"{this.Server}未返回世界界面");

                Click(45, 75);
                if (!await OCR是包含字符("军级", 616, 11, 664, 34))
                    Throw($"{this.Server}未找到'军级'字样");

                Click(515, 200);
                if (!await OCR是包含字符("金融中心", 583, 4, 695, 42))
                    Throw($"{this.Server}未找到'金融中心'字样");

                for (int i = 0; i < 10 && !this.ctsMainTask.IsCancellationRequested; i++)
                {
                    if (!await ClickOCR("套装", 428, 52, 850, 91))
                        await this.Scroll(480, 80, 3, 1);
                    else
                        break;
                }

                if (!await ClickOCR("获取套装金币", 679, 702, 803, 737))
                    Throw($"{this.Server}未找到'获取套装金币'字样");

                if (!await ClickOCR("免费领取", 501, 196, 575, 214, 5))
                    Throw($"{this.Server}未找到'免费领取'字样");
                else
                    Click(取消点[0], 取消点[1]);

                if (!await OCR是包含字符("免费领取", 501, 196, 575, 214))
                {
                    this.方法出口记录当日时间("金融中心套装商店");
                    if (!打开标签页("世界界面"))
                        Throw($"{this.Server}未返回世界界面");
                }

            }
            catch (Exception e)
            {
                Print($"{this.Server}'金融中心套装商店'ERROR: {e.Message}");
            }
        }
        //👇👇👇 集结相关的任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        public class 集结
        {
            //集结显示的三排
            public static int[][] 已加入范围 = new int[][] {
            new int[] { 478,192,522,209 },
            new int[] { 481,374,522,391 },
            new int[] { 479,555,519,572 }
            };
            public static int[][] 加号范围 = new int[][] {
            new int[] { 615,145,654,182 },
            new int[] { 615,327,653,364 },
            new int[] { 615,508,653,545 }
            };
            public static int[][] 加号坐标 = new int[][] {
            new int[] { 630,162 },
            new int[] { 630,340 },
            new int[] { 630,530 }
            };
            //坐标下面的一条红线,用了判断是否此栏有车
            public static int[][] 红线范围 = new int[][] {
            new int[] { 744,227,805,228 },
            new int[] { 744,408,805,409 },
            new int[] { 744,589,805,590 }
            };
            //集结类型的字体说明显示的范围
            public static int[][] 类型范围 = new int[][] {
            new int[] { 705,177,828,209 },
            new int[] { 709,358,834,389 },
            new int[] { 709,540,840,573 }
            };
            public static string[] 图片名 = { "嘿车", "战锤", "难民", "惧星", "精卫", "砰砰" };
            public static string[] 优先顺序 = { "嘿车", "战锤", "难民", "惧星", "精卫", "砰砰" };
            public static int[] 战斗按钮坐标 = { 636, 303 };
            public static int[] 单兵位置坐标 = { 616, 718 };
            public string 集结类型 { get; set; }
            public class 集结信息
            {
                public int 第几排 { set; get; }  //从0开始 0 ，1 ，2
                public string 集结类型 { set; get; }
            }
        }
        private void 测试集结界面识图()
        {

            while (!this.ctsMainTask.IsCancellationRequested)
            {
                for (int i = 0; i < 3; i++)
                {
                    // 清空文本框内容
                    //ClearTextBox();

                    for (int j = 0; j < 6; j++)
                    {
                        if (IsTypePresent(dm, i, 集结.图片名[j]))
                        {
                            Print($"{i}排 {集结.图片名[j]} {(IsSlotAvailable(dm, i) ? "空" : "")} {(IsAlreadyJoined(dm, i) ? "加入" : "")} |");
                        }
                    }
                }
            }

            bool IsTypePresent(dmsoft dm, int i, string imageName)
            {
                return dm.FindPic(集结.类型范围[i][0], 集结.类型范围[i][1], 集结.类型范围[i][2], 集结.类型范围[i][3], $"{imageName}{i}.bmp", "333333", 0.7, 0, out _, out _) >= 0;
            }

            bool IsSlotAvailable(dmsoft dm, int i)
            {
                return dm.FindPic(集结.加号范围[i][0], 集结.加号范围[i][1], 集结.加号范围[i][2], 集结.加号范围[i][3], "加号大.bmp", "000000", 0.8, 0, out _, out _) >= 0;
            }

            bool IsAlreadyJoined(dmsoft dm, int i)
            {
                return dm.FindPic(集结.已加入范围[i][0], 集结.已加入范围[i][1], 集结.已加入范围[i][2], 集结.已加入范围[i][3], $"已加入{i}.bmp", "000000", 0.8, 0, out _, out _) >= 0;
            }
        }
        private bool 未进入战斗界面(int 第几排)
        {
            //红线存在说明 还在集结界面
            return dm.FindPic(集结.红线范围[第几排][0], 集结.红线范围[第几排][1], 集结.红线范围[第几排][2], 集结.红线范围[第几排][3], "集结红线.bmp", "151515", 0.9, 0, out _, out _) >= 0;
        }
        private List<集结.集结信息> 返回集结信息(List<集结.集结信息> 集结信息, bool 是否嘿车, bool 是否难民, bool 是否战锤, bool 是否惧星, bool 是否精卫, bool 是否砰砰)
        {
            foreach (var result in 集结信息)
            {
                var i = result.第几排;

                if (!IsRedLinePresent(dm, i)) //此排没有队伍 直接返回
                    return 集结信息;

                if (!IsSlotAvailable(dm, i))
                    continue;

                if (IsAlreadyJoined(dm, i))
                    continue;

                result.集结类型 = Get集结类型(dm, i, 是否嘿车, 是否难民, 是否战锤, 是否惧星, 是否精卫, 是否砰砰);
            }

            return 集结信息;

            bool IsRedLinePresent(dmsoft dm, int i)
            {
                return dm.FindPic(集结.红线范围[i][0], 集结.红线范围[i][1], 集结.红线范围[i][2], 集结.红线范围[i][3], "集结红线.bmp", "151515", 0.9, 0, out _, out _) >= 0;
            }

            bool IsSlotAvailable(dmsoft dm, int i)
            {
                return dm.FindPic(集结.加号范围[i][0], 集结.加号范围[i][1], 集结.加号范围[i][2], 集结.加号范围[i][3], "加号大.bmp", "000000", 0.8, 0, out _, out _) >= 0;
            }

            bool IsAlreadyJoined(dmsoft dm, int i)
            {
                return dm.FindPic(集结.已加入范围[i][0], 集结.已加入范围[i][1], 集结.已加入范围[i][2], 集结.已加入范围[i][3], $"已加入{i}.bmp", "000000", 0.8, 0, out _, out _) >= 0;
            }

            string Get集结类型(dmsoft dm, int i, bool 是否嘿车, bool 是否难民, bool 是否战锤, bool 是否惧星, bool 是否精卫, bool 是否砰砰)
            {
                if (是否嘿车 && IsTypePresent(dm, i, 集结.图片名[0])) return "嘿车";
                if (是否战锤 && IsTypePresent(dm, i, 集结.图片名[1])) return "战锤";
                if (是否难民 && IsTypePresent(dm, i, 集结.图片名[2])) return "难民";
                if (是否惧星 && IsTypePresent(dm, i, 集结.图片名[3])) return "惧星";
                if (是否精卫 && IsTypePresent(dm, i, 集结.图片名[4])) return "精卫";
                if (是否砰砰 && IsTypePresent(dm, i, 集结.图片名[5])) return "砰砰";
                return null;
            }

            bool IsTypePresent(dmsoft dm, int i, string imageName)
            {
                return dm.FindPic(集结.类型范围[i][0], 集结.类型范围[i][1], 集结.类型范围[i][2], 集结.类型范围[i][3], $"{imageName}{i}.bmp", "333333", 0.7, 0, out _, out _) >= 0;
            }

        }
        private bool 点击加号开始战斗(int 第几排)
        {
            // 获取加号坐标
            var (x, y) = 获取加号坐标(第几排);
            if (x == -1 || y == -1)
                return false;

            //点此排的加号
            点击坐标(x, y, 200);

            //如果未进入战斗界面则跳过下面 以免浪费时间
            if (未进入战斗界面(第几排))
                return false;

            int 出征延迟 = Convert.ToInt32(this.其他信息["集结出征延迟"].Info);

            //单兵位置
            点击坐标(集结.单兵位置坐标[0], 集结.单兵位置坐标[1], 出征延迟);

            //战斗按钮
            点击坐标(集结.战斗按钮坐标[0], 集结.战斗按钮坐标[1], 200);

            检查战斗界面();

            return true;

            (int, int) 获取加号坐标(int 第几排)
            {
                if (第几排 >= 0 && 第几排 < 集结.加号坐标.Length)
                {
                    return (集结.加号坐标[第几排][0], 集结.加号坐标[第几排][1]);
                }
                return (-1, -1);
            }

        }
        private bool 查询队列有空闲(bool 使用亮色 = true)
        {
            var 队列数 = Convert.ToInt32(this.其他信息["队列数"].Info);

            switch (队列数)
            {
                case 2:
                    return !IsQueueFull(dm, 135, 424, 137, 434, 使用亮色);
                case 3:
                    return !IsQueueFull(dm, 135, 466, 137, 478, 使用亮色);
                case 4:
                    return !IsQueueFull(dm, 135, 508, 137, 519, 使用亮色);
                case 5:
                    return !IsQueueFull(dm, 135, 550, 137, 561, 使用亮色);
                default:
                    return false;
            }
            bool IsQueueFull(dmsoft dm, int x1, int y1, int x2, int y2, bool 使用亮色)
            {
                string imageName = 使用亮色 ? "队列蓝边亮.bmp" : "队列蓝边.bmp";
                const string color = "151515";
                const double sim = 1.0;
                return dm.FindPic(x1, y1, x2, y2, imageName, color, sim, 0, out _, out _) >= 0;
            }
        }
        private void 测试集结任务打印信息()
        {
            for (int i = 0; i < 3; i++)
            {
                bool a = IsRedLinePresent(dm, i);
                bool b = IsSlotAvailable(dm, i);
                bool c = IsAlreadyJoined(dm, i);
                string d = Get集结类型(dm, i);
                string e = c ? "已加入" : "";
                string f = b ? "有空位" : "";
                if (a)
                    Console.WriteLine($"{i + 1}    {d}    {e}    {f}");
            }

            bool IsRedLinePresent(dmsoft dm, int i)
            {
                return dm.FindPic(集结.红线范围[i][0], 集结.红线范围[i][1], 集结.红线范围[i][2], 集结.红线范围[i][3], "集结红线.bmp", "151515", 0.9, 0, out _, out _) >= 0;
            }

            bool IsSlotAvailable(dmsoft dm, int i)
            {
                return dm.FindPic(集结.加号范围[i][0], 集结.加号范围[i][1], 集结.加号范围[i][2], 集结.加号范围[i][3], "加号大.bmp", "000000", 0.8, 0, out _, out _) >= 0;
            }

            bool IsAlreadyJoined(dmsoft dm, int i)
            {
                return dm.FindPic(集结.已加入范围[i][0], 集结.已加入范围[i][1], 集结.已加入范围[i][2], 集结.已加入范围[i][3], $"已加入{i}.bmp", "000000", 0.8, 0, out _, out _) >= 0;
            }

            string Get集结类型(dmsoft dm, int i)
            {
                if (IsTypePresent(dm, i, 集结.图片名[0])) return "嘿车";
                if (IsTypePresent(dm, i, 集结.图片名[1])) return "战锤";
                if (IsTypePresent(dm, i, 集结.图片名[2])) return "难民";
                if (IsTypePresent(dm, i, 集结.图片名[3])) return "惧星";
                if (IsTypePresent(dm, i, 集结.图片名[4])) return "精卫";
                if (IsTypePresent(dm, i, 集结.图片名[5])) return "砰砰";
                return null;
            }

            bool IsTypePresent(dmsoft dm, int i, string imageName)
            {
                return dm.FindPic(集结.类型范围[i][0], 集结.类型范围[i][1], 集结.类型范围[i][2], 集结.类型范围[i][3], $"{imageName}{i}.bmp", "333333", 0.7, 0, out _, out _) >= 0;
            }
        }
        private void 集结任务(bool 是否嘿车, bool 是否难民, bool 是否战锤, bool 是否惧星, bool 是否精卫, bool 是否砰砰)
        {
            if (!查询队列有空闲(false))
                return;

            List<集结.集结信息> 集结信息 = new List<集结.集结信息>
            {
                new 集结.集结信息 { 第几排 = 0, 集结类型 = "" },
                new 集结.集结信息 { 第几排 = 1, 集结类型 = "" },
                new 集结.集结信息 { 第几排 = 2, 集结类型 = "" }
            };

            集结信息 = 返回集结信息(集结信息, 是否嘿车, 是否难民, 是否战锤, 是否惧星, 是否精卫, 是否砰砰);

            // 按照优先顺序排序
            集结信息 = 集结信息.OrderBy(info => Array.IndexOf(集结.优先顺序, info.集结类型)).ToList();

            foreach (var result in 集结信息)
            {
                if (string.IsNullOrEmpty(result.集结类型))
                    continue;

                Console.WriteLine($"第{result.第几排}排 {result.集结类型}");
                var i = result.第几排;

                if (点击加号开始战斗(i))
                {
                    if (!查询队列有空闲(false))
                        return;
                }
            }

        }
        private async Task<(int, int, int, int)> 新检查战锤难民跟车次数()
        {

            var (战锤, 砰砰, 难民营, 惧星) = (0, 0, 0, 0);

            // 点击自动加入按钮查询
            if (点击按钮("战锤查询之自动加入按钮").dm_ret < 0) goto 出口;

            //string 集结信息 = await 找字(511, 347, 643, 549, true);

            //List<DetectionResult> results = JsonConvert.DeserializeObject<List<DetectionResult>>(集结信息);

            List<DetectionResult> results = await 找字List(511, 347, 643, 549);

            if (results == null || results.Count <= 0)
            {
                Print($"{server}新检查战锤难民跟车次数 pipe返回字符串为空");
                goto 出口;
            }

            string combinedText = string.Join(" ", results.ConvertAll(result => result.Text));

            int temp战锤 = ExtractCount(combinedText, "战锤", "砰砰");
            if (temp战锤 != -1) 战锤 = temp战锤;

            int temp砰砰 = ExtractCount(combinedText, "砰砰", "难民营");
            if (temp砰砰 != -1) 砰砰 = temp砰砰;

            int temp难民营 = ExtractCount(combinedText, "难民营", "惧星");
            if (temp难民营 != -1) 难民营 = temp难民营;

            int temp惧星 = ExtractCount(combinedText, "惧星", null);
            if (temp惧星 != -1) 惧星 = temp惧星;

            //Console.WriteLine($"战锤: {战锤}");
            //Console.WriteLine($"砰砰: {砰砰}");
            //Console.WriteLine($"难民营: {难民营}");
            //Console.WriteLine($"惧星: {惧星}");

            出口:
            关闭叉叉();
            return (战锤, 砰砰, 难民营, 惧星);

            static int ExtractCount(string text, string start, string end)
            {
                string pattern = end == null ? $@"{start}.*?队员奖励次数：(\d+)/\d+" : $@"{start}.*?队员奖励次数：(\d+)/\d+.*?{end}";
                var match = Regex.Match(text, pattern);
                if (match.Success && match.Groups[1].Success)
                {
                    return int.Parse(match.Groups[1].Value);
                }
                return -1; // 返回-1表示未找到
            }

        }
        public async Task 新加入集结()
        {
            while (!this.ctsMainTask.IsCancellationRequested)
            {
                检查战斗界面();

                if (!this.ctsMainTask.IsCancellationRequested && !处于界面("集结界面"))
                {
                    if (处于界面("联盟界面"))
                    {
                        点击按钮("联盟战争按钮");
                    }
                    else if (处于界面("集结无队伍界面"))
                    {
                        关闭叉叉();
                    }
                    else
                    {
                        打开标签页("集结界面");
                    }

                    await Task.Delay(500);
                    continue;
                }

                //检测一下 战锤 难民 次数
                if (!this.ctsMainTask.IsCancellationRequested && this.任务信息["检查战锤难民次数"].是开启)
                {
                    await 更新跟车信息();
                }

                int 集结扫描频率 = Convert.ToInt32(this.其他信息["集结扫描频率"].Info);
                int 集结次数 = (int)(1 * 60 * 1000 / 集结扫描频率);
                //3分钟返回循环判断是否还在集结界面
                for (int i = 0; i < 集结次数 && !this.ctsMainTask.IsCancellationRequested; i++)
                {

                    await Task.Delay(集结扫描频率);

                    bool 嘿车 = 是否跟车("嘿车");
                    bool 难民 = 是否跟车("难民");
                    bool 战锤 = 是否跟车("战锤");
                    bool 惧星 = 是否跟车("惧星");
                    bool 精卫 = 是否跟车("精卫");
                    bool 砰砰 = 是否跟车("砰砰");

                    集结任务(嘿车, 难民, 战锤, 惧星, 精卫, 砰砰);
                    //测试集结任务打印信息(服务器);
                }

            }

            async Task 更新跟车信息()
            {
                var (检查战锤, 检查砰砰, 检查难民, 检查惧星) = await 新检查战锤难民跟车次数();
                更新跟车次数("战锤", 检查战锤, 50);
                更新跟车次数("精卫", 检查战锤, 50);
                更新跟车次数("砰砰", 检查砰砰, 50);
                更新跟车次数("难民", 检查难民, 10);
                更新跟车次数("惧星", 检查惧星, 10);
                this.写入配置();
            }
            void 更新跟车次数(string 类型, int 当前次数, int 最大次数)
            {
                this.跟车信息[类型].当前次数 = 当前次数;
                this.跟车信息[类型].是刷满 = 当前次数 >= 最大次数;
            }
            bool 是否跟车(string 类型)
            {
                var 跟车信息 = this.跟车信息[类型];
                return 跟车信息.是强制跟车 || (跟车信息.是普通跟车 && !跟车信息.是刷满);
            }

        }
        //👇👇👇 打野相关的任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        public class 打野
        {

            public class 打野类型
            {
                public string 打野军种 { set; get; } // 0陆军 1海军 2空军
                public int 军种等级 { set; get; }
                public int 搜索点击坐标x { set; get; }
                public int 搜索点击坐标y { set; get; }
            }
            public static int[] 搜索加号 = { 755, 645 };
            public static int[] 搜索减号 = { 525, 645 };
            public static int[] 搜索取消点 = { 440, 550 };
            public static int[] 搜索按钮 = { 580, 680 };
            public static int[] 中心 = { 640, 380 };
            public static int[] 攻击按钮 = { 670, 260 };
            public static int[] 一号队伍 = { 975, 635 };

        }
        private async Task<int> 获取当前体力值()
        {
            //要点开体力栏
            string result = await 找字(612, 317, 636, 337);
            return int.TryParse(result, out int number) ? number : -1;
        }
        private async Task<int> 打野补体力(bool 补充小体)
        {
            // -1 读取体力值失败 或者 补充体力值失败  0 体力充足无需补 1补了的 

            //点击体力栏
            点击坐标(130, 15);
            bool 恢复体力 = await 找字匹配(597, 219, 684, 247, "恢复体力");
            if (!恢复体力)
            { Print($"{server}体力药剂栏点击失败"); return -1; }

            //查询体力
            int 体力值 = await 获取当前体力值();

            if (体力值 < 0 || 体力值 > 125)
            { Print($"{server}当前体力值获取失败"); return -1; }

            if (体力值 >= 5)
            { Print($"{server}当前体力{体力值}"); return 0; }

            if (体力值 < 5)
            {
                int 小体 = int.TryParse(await 找字(517, 417, 564, 436), out int number) ? number : -1;
                int 大体 = int.TryParse(await 找字(628, 418, 673, 438), out number) ? number : -1;
                if (小体 < 0 || 大体 < 0)
                { Print($"{server}体力药剂数量获取失败"); return -1; }


                if (补充小体)
                    点击坐标(500, 380);//点击小体
                else
                    点击坐标(610, 380); //点击大体
                点击坐标(600, 500); //喝

                //获取当前体力值
                体力值 = await 获取当前体力值();
                if (体力值 >= 5)
                {
                    Print($"{server} 体力值补充成功 {体力值}");
                    return 1;
                }
                else
                {
                    Print($"{server} 体力值补充失败 {体力值}");
                    return -1;
                }
            }
            return -1;
        }
        private async Task<int> 调整搜索等级(int 目标等级)
        {
            for (int i = 0; i < 10; i++)
            {

                int 当前等级 = int.TryParse(await 找字(643, 615, 685, 633), out int number) ? number : -1;
                if (当前等级 < 0)
                { Print($"{server} 当前打野等级获取失败"); continue; }

                if (当前等级 == 目标等级)
                { Print($"{server}打野等级调整成功"); return 0; }
                else if (当前等级 > 目标等级)
                    点击坐标(打野.搜索减号[0], 打野.搜索减号[1]);
                else if (当前等级 < 目标等级)
                    点击坐标(打野.搜索加号[0], 打野.搜索加号[1]);
                else
                { Print($"{server}调整打野等级发生未知错误"); return -1; }
            }
            return -1;
        }
        private double 计算距离(int x1, int y1, int x2, int y2)
        {
            double distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            return distance;
        }
        private bool 搜索并点击(打野.打野类型 s)
        {
            string 图名 = s.打野军种 switch
            {
                "陆军" => "搜索陆军图",
                "海军" => "搜索海军图",
                "空军" => "搜索空军图",
                _ => null
            };

            点击坐标(s.搜索点击坐标x, s.搜索点击坐标y, 500);
            点击坐标(打野.搜索取消点[0], 打野.搜索取消点[1], 500);
            if (图名 == null || 找图(图名, "532,550,704,622").dm_ret < 0) return false;

            return true;
        }
        private bool 是打野中(string HeroName)
        {
            return 找图($"{HeroName}行军", "5,294,168,635").dm_ret >= 0;
        }
        private bool 是出征英雄上阵中(string hero1name, string hero2name)
        {
            bool hero1 = 找图Sim($"{hero1name}出征", "28,219,147,283").dm_ret >= 0;
            bool hero2 = 找图Sim($"{hero2name}出征", "28,219,147,283").dm_ret >= 0;
            return hero1 && hero2;
        }
        private async Task 正式打野()
        {
            string[] parts = null;
            bool 是打野 = false;
            int 小体 = 0;
            int 大体 = 0;

            try
            {
                parts = this.其他信息["打野"].Info.ToString().Split(',');
                是打野 = parts[0] == "1";
                小体 = int.Parse(parts[1]);
                大体 = int.Parse(parts[2]);
            }
            catch (Exception e)
            {
                Print($"{server}打野信息读取失败 {e.Message}");
            }

            if (!是打野 || (小体 == 0 && 大体 == 0))
            { Console.WriteLine("今天不打野"); return; }

            关闭箭头叉叉();
            int 人物等级 = Convert.ToInt32(this.其他信息["人物等级"].Info);
            int 等级下限 = 人物等级 - 4;
            int 等级上限 = 人物等级 + 4;
            string Hero1Name = this.编队信息["编队1"].英雄1;
            string Hero2Name = this.编队信息["编队1"].英雄2;

            List<打野.打野类型> 野怪 = new List<打野.打野类型>
            {
                Create打野类型("陆军", 等级上限, 470, 460),
                Create打野类型("海军", 等级上限, 600, 460),
                Create打野类型("空军", 等级上限, 800, 460)
            };

            打野.打野类型 Create打野类型(string 军种, int 等级, int x, int y)
            {
                return new 打野.打野类型
                {
                    打野军种 = 军种,
                    军种等级 = 等级,
                    搜索点击坐标x = x,
                    搜索点击坐标y = y
                };
            }

            //while (!服务器.通知退出)
            while (!this.ctsMainTask.IsCancellationRequested)
            {

                foreach (var s in 野怪)
                {

                    for (int i = 等级上限; i >= 等级下限; i--)
                    {

                        //if (服务器.通知退出) goto 出口;
                        if (this.ctsMainTask.IsCancellationRequested) goto 出口;

                        Print($"{server}开始打野 ---> {s.打野军种} {i} 级");

                        点击坐标(430, 700); //搜索
                        点击坐标(450, 335); //敌军

                        if (!搜索并点击(s)) continue;

                        if (await 调整搜索等级(i) < 0)
                        {
                            Print($"{server}调整等级10次内未完成");
                            点击坐标(取消点[0], 取消点[1]);
                            continue;
                        }

                        点击坐标(打野.搜索按钮[0], 打野.搜索按钮[1], 2500); //搜怪

                        var (距离x, 距离y) = await 获取此时坐标();
                        double 与基地之间距离 = 计算距离(int.Parse(this.其他信息["基地坐标"].Info.ToString().Split(',')[0]), int.Parse(this.其他信息["基地坐标"].Info.ToString().Split(',')[1]), 距离x, 距离y);

                        Print($"{server}与基地距离 ---> {(int)与基地之间距离}");
                        if (与基地之间距离 > 50) continue;

                        if (小体 > 0)
                        {
                            int r = await 打野补体力(true);
                            点击坐标(取消点[0], 取消点[1]);
                            if (r == 1)
                            {
                                小体--;
                                this.其他信息["打野"].Info = $"1,{小体},{大体}";
                                this.写入配置();
                            }
                            else if (r == 0)
                            { }
                            else
                                continue;
                        }
                        else if (大体 > 0)
                        {
                            int r = await 打野补体力(false);
                            点击坐标(取消点[0], 取消点[1]);
                            if (r == 1)
                            {
                                大体--;
                                this.其他信息["打野"].Info = $"1,{小体},{大体}";
                                this.写入配置();

                            }
                            else if (r == 0)
                            { }
                            else
                                continue;
                        }

                        while (!this.ctsMainTask.IsCancellationRequested)
                        {
                            if (!是打野中(Hero1Name) && 查询队列有空闲(true))
                            { Print($"{server} 体力瓶 {小体}个 {大体}个"); break; }
                            else
                                await Task.Delay(1000);
                        }


                        点击坐标(打野.中心[0], 打野.中心[1]);
                        if (!await 找字匹配(696, 254, 742, 273, "攻击"))
                        {
                            Print($"{server}未找到攻击按钮");
                            continue;
                        }

                        点击坐标(打野.攻击按钮[0], 打野.攻击按钮[1]);
                        点击坐标(打野.一号队伍[0], 打野.一号队伍[1]);

                        if (!是出征英雄上阵中(Hero1Name, Hero2Name)) continue;

                        //点出征
                        点击坐标(630, 290);

                        检查战斗界面();

                        if (!是打野中(Hero1Name)) continue;

                        if (小体 == 0 && 大体 == 0)
                        { Print($"{server}打野完成!"); goto 出口; }
                    }

                }

            }
        出口:
            Print($"{server} 打野已退出");
            return;
        }
        //👇👇👇 采集相关的任务 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        #region 采集的一些函数
        /*
        int 采集(dmsoft dm, string 采集类型)
        {
            var 采集队伍信息 = 返回采集队伍信息(dm);
            string 队列信息 = dm.Ocr(116, 474, 168, 508, "ffffff-000000", 0.9);
            if (采集队伍信息.dm_ret >= 0 && 队列信息 != "三队列无空闲")
            { 动作("采集队伍工作中 OR 队列没有空闲"); return 0; }
            关闭箭头叉叉(dm);
            视角拉近(dm);
            //搜索
            点击坐标(dm, 460, 720);
            //资源田
            点击坐标(dm, 590, 340);
            //农田
            if (采集类型.Contains("田"))
                点击坐标(dm, 766, 455);
            else if (采集类型.Contains("油"))
                点击坐标(dm, 666, 455);
            //5级田取消点 点击改变田搜索等级
            if (采集类型.Contains("5级"))
                点击坐标(dm, 720 - 15, 645 - 15);
            if (采集类型.Contains("4级"))
                点击坐标(dm, 680 - 15, 645 - 15);
            int[] 田坐标 = { 720, 645 };
            //点搜索
            点击坐标(dm, 633, 699);
            //屏幕中间范围查找
            var r = 找图(dm, "蓝色指向箭头", "381,166,908,541");
            if (r.dm_ret >= 0)
            {
                //y加100偏移，有时候箭头会靠上点
                点击坐标(dm, r.x, r.y + 100);
                var 采集 = 文字识别ffffff(dm, "采集字样");
                if (采集.dm_ret >= 0)
                {
                    点击坐标(dm, 采集.x, 采集.y);
                    //2#队伍
                    点击坐标(dm, 1010 - 15, 640 - 15);
                    //点击战斗
                    点击坐标(dm, 620, 280);
                }
                采集队伍信息 = 返回采集队伍信息(dm);
                return 采集队伍信息.dm_ret;
            }
            return -1;
        }
        void 收田(dmsoft dm)
        {
            var 采集队伍信息 = 返回采集队伍信息(dm);
            if (采集队伍信息.dm_ret < 0)
            { 动作("未发现采集队伍"); return; }
            // x+120   y+30
            else
            {
                //根据采集队伍头像确定箭头范围
                string 返回箭头查找范围 = 采集队伍信息.x.ToString() + "," + 采集队伍信息.y.ToString() + "," + (采集队伍信息.x + 120).ToString() + "," + (采集队伍信息.y + 30).ToString();
                var b = 找图Sim(dm, "队列返回箭头", 返回箭头查找范围);
                if (b.dm_ret >= 0)
                    点击坐标(dm, b.x - 15, b.y - 15);
            }
        }
        (int dm_ret, int x, int y) 联盟里找油田(dmsoft dm, string 田类型)
        {
            int dm_ret = dm.FindStr(432, 54, 842, 730, 田类型, "333333-666666", 0.8, out object xx, out object yy);
            return (dm_ret, (int)xx, (int)yy);
        }
        (int dm_ret, int x, int y) 文字识别ffffff(dmsoft dm, string 文字)
        {
            int dm_ret = dm.FindStr(0, 0, 1280, 760, 文字, "ffffff-555555", 0.9, out object xx, out object yy);
            return (dm_ret, (int)xx, (int)yy);
        }
        (int dm_ret, int x, int y) 油田产量(dmsoft dm, string 产量, int[] 查找范围)
        {
            //3级产量153K
            int dm_ret = dm.FindStr(查找范围[0], 查找范围[1], 查找范围[2], 查找范围[3], 产量, "444444-555555", 0.9, out object xx, out object yy);
            return (dm_ret, (int)xx, (int)yy);
        }
        int 采集机械田(dmsoft dm, string 机械田类型)
        {
            var 采集队伍信息 = 返回采集队伍信息(dm);
            string 队列信息 = dm.Ocr(116, 474, 168, 508, "ffffff-000000", 0.9);
            if (采集队伍信息.dm_ret >= 0 && 队列信息 != "三队列无空闲")
            { 动作("采集队伍工作中 OR 队列没有空闲"); return 0; }
            关闭箭头叉叉(dm);
            视角拉近(dm);
            if (点击按钮(dm, "联盟按钮").dm_ret >= 0)
            {
                string 机械田产量 = "";
                if (机械田类型.Contains("3级"))
                    机械田产量 = "产量153K";
                if (找图(dm, "联盟界面").dm_ret >= 0)
                {
                    //联盟记录
                    点击坐标(dm, 520, 700);
                    //联盟建筑
                    点击坐标(dm, 520, 650);
                    //682 103  偏移 600 110
                    var r = 联盟里找油田(dm, 机械田类型);
                    if (r.dm_ret < 0)
                        r = 联盟里找油田(dm, 机械田类型);
                    if (r.dm_ret < 0)
                        r = 联盟里找油田(dm, 机械田类型);
                    if (r.dm_ret >= 0)
                    {
                        点击坐标(dm, r.x - 82 - 15, r.y + 7 - 15);
                        r = 找图(dm, "蓝色指向箭头", "381,166,908,541");
                        if (r.dm_ret >= 0)
                        {
                            //y加100偏移，有时候箭头会靠上点
                            点击坐标(dm, r.x, r.y + 100);
                            var 驻扎 = 文字识别ffffff(dm, "驻扎采集");
                            if (驻扎.dm_ret >= 0)
                            {
                                //实例返回坐标 616,514 上方范围 591,365,812,488
                                //-25,-149,+196,-26
                                int[] 上方范围 = { 驻扎.x - 25, 驻扎.y - 149, 驻扎.x + 196, 驻扎.y - 26 };
                                var 产量 = 油田产量(dm, 机械田产量, 上方范围);
                                if (产量.dm_ret >= 0)
                                {
                                    点击坐标(dm, 驻扎.x, 驻扎.y);
                                    r = 文字识别ffffff(dm, "油田增援");
                                    if (r.dm_ret >= 0)
                                    {
                                        点击坐标(dm, r.x, r.y);
                                        //2#队伍
                                        点击坐标(dm, 1010 - 15, 640 - 15);
                                        //点击战斗
                                        点击坐标(dm, 620, 280);
                                        采集队伍信息 = 返回采集队伍信息(dm);
                                        return 采集队伍信息.dm_ret;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }
        (int dm_ret, int x, int y) 返回采集队伍信息(dmsoft dm)
        {
            //左侧队列查找
            var t = 找图Sim(dm, "采集队伍", "4,289,161,621");
            return (t.dm_ret, t.x, t.y);
        }
        */
        #endregion
        #endregion


    }
}
