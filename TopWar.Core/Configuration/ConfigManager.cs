using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using System.Xml;

namespace TopWar.Core.Configuration
{
    internal class ConfigManager
    {
        public string 存档路径 { get; set; }
        public int Server { get; set; }
        public dmsoft Dm0 { get; set; } = new dmsoft();
        // 定义只读属性作为别名
        private dmsoft dm => this.Dm0;
        private int server => this.Server;
        public bool 主进程运行中 { get; set; } = false;
        public CancellationTokenSource ctsMainTask = new CancellationTokenSource();
        public bool 通知退出 { get; set; } = false;

        public int 图色句柄 { get; set; }
        public int 键鼠句柄 { get; set; }
        public ConfigManager(int srv)
        {
            this.Server = srv;
            设置任务名称();
            SetConfigPath();
            SetDmPath();
            读取配置();
        }
        private void SetConfigPath()
        {
            string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string path = System.IO.Path.Combine(userDirectory, "TopWar");
            this.存档路径 = path;
        }
        public class 服务器配置Converter : JsonConverter<ConfigManager>
        {
            private readonly ConfigManager _existingInstance;
            public 服务器配置Converter(ConfigManager existingInstance)
            {
                _existingInstance = existingInstance;
            }

            public override ConfigManager ReadJson(JsonReader reader, Type objectType, ConfigManager existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var myClass = _existingInstance; // 使用现有实例
                JObject obj = JObject.Load(reader);

                try
                {
                    //if (obj["当前勾选打螺丝物品"] != null)
                    //    myClass.当前勾选打螺丝物品 = obj["当前勾选打螺丝物品"].ToString();

                    if (obj["跟车信息"] != null)
                    {
                        var new跟车信息 = obj["跟车信息"].ToObject<Dictionary<string, 跟车信息类>>(serializer);
                        foreach (var entry in new跟车信息)
                        {
                            if (myClass.跟车信息.ContainsKey(entry.Key))
                            {
                                myClass.跟车信息[entry.Key] = entry.Value;
                            }
                        }
                    }

                    if (obj["任务信息"] != null)
                    {
                        var new任务信息 = obj["任务信息"].ToObject<Dictionary<string, 任务信息类>>(serializer);
                        foreach (var entry in new任务信息)
                        {
                            if (myClass.任务信息.ContainsKey(entry.Key))
                            {
                                myClass.任务信息[entry.Key] = entry.Value;
                            }
                        }
                    }

                    if (obj["打螺丝信息"] != null)
                    {
                        var new打螺丝信息 = obj["打螺丝信息"].ToObject<Dictionary<string, 打螺丝信息类>>(serializer);
                        foreach (var entry in new打螺丝信息)
                        {
                            if (myClass.打螺丝信息.ContainsKey(entry.Key))
                            {
                                myClass.打螺丝信息[entry.Key] = entry.Value;
                            }
                        }
                    }

                    if (obj["其他信息"] != null)
                    {
                        var new其他信息 = obj["其他信息"].ToObject<Dictionary<string, 其他信息类>>(serializer);
                        foreach (var entry in new其他信息)
                        {
                            if (myClass.其他信息.ContainsKey(entry.Key))
                            {
                                myClass.其他信息[entry.Key] = entry.Value;
                            }
                        }
                    }

                    if (obj["编队信息"] != null)
                    {
                        var new编队信息 = obj["编队信息"].ToObject<Dictionary<string, 编队信息类>>(serializer);
                        foreach (var entry in new编队信息)
                        {
                            if (myClass.编队信息.ContainsKey(entry.Key))
                            {
                                myClass.编队信息[entry.Key] = entry.Value;
                            }
                        }
                    }

                    if (obj["战争之源信息"] != null)
                    {
                        var new战争之源信息 = obj["战争之源信息"].ToObject<Dictionary<int, 战争之源信息类>>(serializer);
                        foreach (var entry in new战争之源信息)
                        {
                            if (myClass.战争之源信息.ContainsKey(entry.Key))
                            {
                                myClass.战争之源信息[entry.Key] = entry.Value;
                            }
                        }
                    }


                }
                catch (JsonSerializationException ex)
                {
                    // Handle JSON serialization errors
                    Console.WriteLine($"Serialization error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle other potential errors
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                return myClass;
            }

            public override void WriteJson(JsonWriter writer, ServerSetting value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                //writer.WritePropertyName("当前勾选打螺丝物品");
                //writer.WriteValue(value.当前勾选打螺丝物品);
                writer.WritePropertyName("跟车信息");
                serializer.Serialize(writer, value.跟车信息);
                writer.WritePropertyName("任务信息");
                serializer.Serialize(writer, value.任务信息);
                writer.WritePropertyName("打螺丝信息");
                serializer.Serialize(writer, value.打螺丝信息);
                writer.WritePropertyName("其他信息");
                serializer.Serialize(writer, value.其他信息);
                writer.WritePropertyName("编队信息");
                serializer.Serialize(writer, value.编队信息);
                writer.WritePropertyName("战争之源信息");
                serializer.Serialize(writer, value.战争之源信息);
                writer.WriteEndObject();
            }

            public override bool CanRead => true;
            public override bool CanWrite => true;
        }
        // 私有方法来读取配置
        public void 读取配置()
        {
            string filePath = null;
            try
            {
                filePath = Path.Combine(存档路径, $"{this.Server}.json");

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new 服务器配置Converter(this) }
                };

                string jsonInput = File.ReadAllText(filePath);

                if (string.IsNullOrEmpty(jsonInput))
                {
                    MessageBox.Show($"{this.Server}本地Json文件为空");
                    return;
                }

                ConfigManager 服务器 = JsonConvert.DeserializeObject<ConfigManager>(jsonInput, settings);
                this.任务信息 = 服务器.任务信息;
                this.跟车信息 = 服务器.跟车信息;
                this.打螺丝信息 = 服务器.打螺丝信息;
                this.其他信息 = 服务器.其他信息;
                this.编队信息 = 服务器.编队信息;
                this.战争之源信息 = 服务器.战争之源信息;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Error: File {filePath} not found.");
                MessageBox.Show($"{this.Server}Error: File {filePath} not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: An error occurred while reading or parsing the JSON file. {ex.Message}");
                MessageBox.Show($"{this.Server}Error: An error occurred while reading or parsing the JSON file. {ex.Message}");
            }
        }
        //public void 读取配置2(ref ServerSetting 服务器)
        //{
        //    string filePath = $"{this.存档路径}{this.Server}.json";

        //    try
        //    {
        //        var settings = new JsonSerializerSettings
        //        {
        //            Formatting = Formatting.Indented,
        //            Converters = new List<JsonConverter> { new 服务器配置Converter(服务器) }
        //        };

        //        string jsonInput = File.ReadAllText(filePath);

        //        if (string.IsNullOrEmpty(jsonInput))
        //        {
        //            MessageBox.Show($"{this.Server}本地Json文件为空");
        //            return;
        //        }

        //        服务器 = JsonConvert.DeserializeObject<ServerSetting>(jsonInput, settings);
        //        JsonConvert.PopulateObject(jsonInput, this, settings);
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine($"Error: File {filePath} not found.");
        //        MessageBox.Show($"{this.Server}Error: File {filePath} not found.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: An error occurred while reading or parsing the JSON file. {ex.Message}");
        //        MessageBox.Show($"{this.Server}Error: An error occurred while reading or parsing the JSON file. {ex.Message}");
        //    }
        //}
        // 提供一个方法来手动重新加载配置
        public void 重新加载配置()
        {
            读取配置();
        }
        public void 写入配置()
        {
            try
            {
                string outputPath = Path.Combine(存档路径, $"{this.Server}.json");

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new 服务器配置Converter(this) }
                };

                string jsonOutput = JsonConvert.SerializeObject(this, settings);
                File.WriteAllText(outputPath, jsonOutput);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: An error occurred while writing the JSON file. {ex.Message}");
                MessageBox.Show($"{this.Server}Error: An error occurred while writing the JSON file. {ex.Message}");
                //throw;
            }
        }
        private void SetDmPath()
        {
            var dmList = new[] { this.Dm0, this.Dm1 };
            foreach (var dm in dmList)
            {
                dm.SetPath(存档路径);
                dm.SetDict(0, @"ziku.txt");
            }
        }



        #region  👇👇👇 字典相关 👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇
        public Dictionary<string, 任务信息类> 任务信息 = new Dictionary<string, 任务信息类>
            {
                { "检查战锤难民次数", new 任务信息类(5) },

                { "联盟遗迹道具", new 任务信息类(60) },
                { "联盟科技捐献", new 任务信息类(180) },
                { "联盟机甲捐献", new 任务信息类(60) },
                { "联盟帮助与请求", new 任务信息类(3) },
                { "联盟礼物", new 任务信息类(300) },

                { "联盟科技捐献之遗迹", new 任务信息类(180) },
                { "英雄招募", new 任务信息类(5) },
                { "英雄高级招募", new 任务信息类(5) },
                { "礼包商城", new 任务信息类(300) },
                { "军级奖励", new 任务信息类(60) },
                { "收取金币", new 任务信息类(180) },
                { "模块研究", new 任务信息类(30) },
                { "打螺丝", new 任务信息类(150) },
                { "战争之源", new 任务信息类(3) },

                { "特殊任务", new 任务信息类(1440) },

                { "每日军情荒野行动", new 任务信息类(180) },
                { "每日军情沙盘演习", new 任务信息类(180) },
                { "每日军情远征行动", new 任务信息类(180) },
                { "每日军情跨战区演习", new 任务信息类(180) },
                { "每日军情次元矿洞", new 任务信息类(5) },
                { "每日军情岛屿作战", new 任务信息类(180) },

                { "金融中心套装商店", new 任务信息类(180) }
            };
        private void 设置任务名称()
        {
            foreach (var key in 任务信息.Keys.ToList())
            {
                任务信息[key].任务名称 = key;
            }
        }
        public int 战争之源今日攻击次数
        {
            get
            {
                int count = 0;
                int todayDay = DateTime.Now.Day;
                foreach (var entry in 战争之源信息)
                {
                    if (entry.Value.攻击时间.Day == todayDay)
                        count++;
                }
                return count;
            }
        }
        public Dictionary<int, 战争之源信息类> 战争之源信息 = new Dictionary<int, 战争之源信息类>
            {
                { 0, new 战争之源信息类(DateTime.MinValue) },
                { 1, new 战争之源信息类(DateTime.MinValue) },
                { 2, new 战争之源信息类(DateTime.MinValue) },
                { 3, new 战争之源信息类(DateTime.MinValue) },
                { 4, new 战争之源信息类(DateTime.MinValue) }
            };
        public Dictionary<string, 打螺丝信息类> 打螺丝信息 = new Dictionary<string, 打螺丝信息类>
            {
                { "钢铁", new 打螺丝信息类(true, 470, 540) },
                { "螺丝", new 打螺丝信息类(false, 560, 540) },
                { "晶体管", new 打螺丝信息类(false, 660, 540) },
                { "橡胶", new 打螺丝信息类(false, 760, 540) },
                { "钨", new 打螺丝信息类(false, 470, 630) },
                { "电池", new 打螺丝信息类(false, 560, 630) },
                { "玻璃", new 打螺丝信息类(false, 660, 630) }
            };
        public KeyValuePair<string, 打螺丝信息类>? 螺丝选中项
        {
            get
            {
                return 打螺丝信息.FirstOrDefault(item => item.Value.是选中);
            }
        }
        public Dictionary<string, 跟车信息类> 跟车信息 { get; set; } = new Dictionary<string, 跟车信息类>
            {
                { "嘿车", new 跟车信息类(true, false, false, 0) },
                { "难民", new 跟车信息类(true, false, false, 0) },
                { "战锤", new 跟车信息类(true, false, false, 0) },
                { "惧星", new 跟车信息类(true, false, false, 0) },
                { "精卫", new 跟车信息类(false, false, false, 0) },
                { "砰砰", new 跟车信息类(false, false, false, 0) },
            };
        public Dictionary<string, 其他信息类> 其他信息 { get; set; } = new Dictionary<string, 其他信息类>
            {
                { "打野", new 其他信息类("0,0,0") },  //第一位0为不打野 3:补充几个小体 4:大体
                { "队列数", new 其他信息类(3) },
                { "人物等级", new 其他信息类(80) },
                { "基地坐标", new 其他信息类("0,0") },
                { "遗迹坐标", new 其他信息类("0,0") },
                { "集结扫描频率", new 其他信息类(1000) },
                { "集结出征延迟", new 其他信息类(200) },
                { "购买齿轮", new 其他信息类(0) },
                { "采矿星级", new 其他信息类(0) },
                { "采矿等级", new 其他信息类(0) },
                { "采矿类型", new 其他信息类(string.Empty) },
                { "战争之源攻击次数", new 其他信息类(0) }
            };
        public Dictionary<string, 编队信息类> 编队信息 { get; set; } = new Dictionary<string, 编队信息类>
            {
                { "编队1", new 编队信息类("打野","戴安娜","鲍勃") },
                { "编队2", new 编队信息类("","","") },
                { "编队3", new 编队信息类("","","") },
                { "编队4", new 编队信息类("","","") },
                { "编队5", new 编队信息类("","","") },
                { "编队6", new 编队信息类("","","") },
                { "编队7", new 编队信息类("","","") },
                { "编队8", new 编队信息类("","","") }
            };
        #endregion


    }
}
