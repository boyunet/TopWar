namespace TopWar.Core.Configuration
{
    internal class 任务信息类
    {
        public bool 是开启 { get; set; }
        public int 执行间隔
        {
            get
            {
                DateTime 现在时间 = DateTime.Now;
                if (未成功运行次数 > 5)
                {
                    冻结任务();
                }
                else if (未成功运行次数 < 5 && 现在时间 >= _冻结结束时间)
                {
                    恢复任务();
                }
                return _执行间隔;
            }
            set
            {
                _执行间隔 = value;
                // 原始执行间隔 = value;
            }
        }
        private int _执行间隔;
        public int 原始执行间隔 { get; set; }
        public string 任务名称 { get; set; } = string.Empty;
        public DateTime 修改时间 { get; set; }
        private DateTime _冻结结束时间;
        public int 未成功运行次数 { get; set; }
        public bool 入口标记 { get; set; }
        public bool 出口标记 { get; set; }

        public 任务信息类(int 原始执行间隔)
        {
            this.执行间隔 = 原始执行间隔;
            this.原始执行间隔 = 原始执行间隔;
        }

        private void 冻结任务()
        {
            //动作类.print($"{任务名称} 已冻结30分钟");
            _冻结结束时间 = DateTime.Now.AddMinutes(30);
            _执行间隔 = 29; // 冻结30分钟
            未成功运行次数 = 0;
        }

        private void 恢复任务()
        {
            if (_执行间隔 == 29)
                _执行间隔 = 原始执行间隔;
        }
    }
}
