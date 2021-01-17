using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;

namespace Lottery
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private volatile bool startFlag = false;
        private volatile List<string> peoples = new List<string>();
        private volatile Thread thread = null;
        private volatile string prizeName = string.Empty;
        private volatile int prizeNameIndex = 0;
        private bool firstInit = false;

        //抽奖结果
        private volatile Dictionary<string, string> keys = new Dictionary<string, string>();

        private Object lockObj = new Object();
        private volatile int firstPrizeSize = 0;
        private volatile int secondPrizeSize = 0;
        private volatile int thirdPrizeSize = 0;

        private string firstPrizeName = string.Empty;
        private string secondPrizeName = string.Empty;
        private string thirdPrizeName = string.Empty;

        public int FirstPrizeSize
        {
            get
            {
                lock (lockObj)
                {
                    return firstPrizeSize;
                }
            }
            set
            {
                lock (lockObj)
                {
                    this.firstPrizeSize = value;
                }
            }
        }

        public int SecondPrizeSize
        {
            get
            {
                lock (lockObj)
                {
                    return secondPrizeSize; ;
                }
            }
            set
            {
                lock (lockObj)
                {
                    this.secondPrizeSize = value;
                }
            }
        }

        public int ThirdPrizeSize
        {
            get
            {
                lock (lockObj)
                {
                    return thirdPrizeSize; ;
                }
            }
            set
            {
                lock (lockObj)
                {
                    this.thirdPrizeSize = value;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (startFlag)
            {
                startFlag = false;
                DoStopAsync();
                this.StartButton.Content = "开始";
                SelectPrizeComboBox.IsEnabled = true;
            }
            else
            {
                var result = DoCheck();
                if (!result)
                {
                    return;
                }
                DoStart();
                startFlag = true;
                SelectPrizeComboBox.IsEnabled = false;

                this.StartButton.Content = "停止";
            }
        }

        public bool DoCheck()
        {
            //检查人员是否设置
            if (peoples.Count == 0)
            {
                MessageBox.Show("抽奖参与者=0，无法抽奖");
                return false;
            }
            prizeNameIndex = SelectPrizeComboBox.SelectedIndex;
            //检查奖品是否抽完了
            switch (SelectPrizeComboBox.SelectedIndex)
            {
                case 0:
                    if (FirstPrizeSize < 1)
                    {
                        MessageBox.Show("一等奖已经被抽完了");
                        return false;
                    }
                    prizeName = FirstPrizeNameTextBox.Text;
                    break;

                case 1:
                    if (SecondPrizeSize < 1)
                    {
                        MessageBox.Show("二等奖已经被抽完了");
                        return false;
                    }
                    prizeName = SecondPrizeNameTextBox.Text;
                    break;

                case 2:
                    if (ThirdPrizeSize < 1)
                    {
                        MessageBox.Show("三等奖已经被抽完了");
                        return false;
                    }
                    prizeName = ThirdPrizeNameTextBox.Text;
                    break;

                default:
                    break;
            }
            return true;
        }

        /// <summary>
        ///  开始循环抽奖
        /// </summary>
        private bool DoStart()
        {
            //开始抽奖 开启一个线程循环显示名字
            thread = new Thread(CycleShow);
            thread.Start();
            return true;
        }

        /// <summary>
        /// 循环显示名字
        /// </summary>
        public void CycleShow()
        {
            while (startFlag)
            {
                try
                {
                    foreach (string item in peoples)
                    {
                        if (startFlag)
                        {
                            SetPeopleName(item);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void DoSet(object sender, RoutedEventArgs e)
        {
            try
            {
                //设置参与人数
                InitPeople();
                //设置抽奖奖品
                InitPrize();
                MessageBox.Show($"设置成功，参与抽奖人数为:{peoples.Count}");
                firstInit = true;
                this.StartButton.Content = "开始";
                this.StartButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化抽奖参数失败，请检查抽奖参数");
            }
        }

        /// <summary>
        /// 初始化抽奖人
        /// </summary>
        public bool InitPeople()
        {
            peoples.Clear();
            string text = this.PeoplesTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("抽奖人名单填写错误");
                return false;
            }
            else
            {
                string[] array = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (var item in array)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (peoples.Contains(item))
                        {
                            MessageBox.Show("抽奖人名单冲突：" + item);
                            return false;
                        }
                        else
                        {
                            peoples.Add(item);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void InitPrize()
        {
            firstPrizeName = this.FirstPrizeNameTextBox.Text;
            string p1SizeText = this.FirstPrizeSizeTextBox.Text;
            firstPrizeSize = Convert.ToInt32(p1SizeText);
            this.P1.Content=$"一等奖剩余数量：{p1SizeText}";

            secondPrizeName = this.SecondPrizeNameTextBox.Text;
            string p2SizeText = this.SecondPrizeSizeTextBox.Text;
            SecondPrizeSize = Convert.ToInt32(p2SizeText);
                this.P2.Content=$"二等奖剩余数量：{p2SizeText}";


            thirdPrizeName = this.ThirdPrizeNameTextBox.Text;
            string p3SizeText = this.ThirdPrizeSizeTextBox.Text;
            thirdPrizeSize = Convert.ToInt32(p3SizeText);
             this.P3.Content=$"三等奖剩余数量：{p3SizeText}";
        }

        public void SetPeopleName(string tempPeopleName, bool needCheck = true)
        {
            Console.WriteLine(tempPeopleName);
            this.PeopleNameTextBlock.Dispatcher.Invoke(() =>
                  {
                      if (needCheck && !startFlag)
                      {
                          return;
                      }
                      this.PeopleNameTextBlock.Text = tempPeopleName;
                  });
        }

        private void DoStopAsync()
        {
            lock (lockObj)
            {
                if (thread != null)
                {
                    thread.Abort();
                }
            }

            //使用RNGCryptoServiceProvider产生一个随机性足够好的随机数
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            SecureRandom random = new SecureRandom(rngCsp);
             int index=0;
            if (peoples.Count!=1)
            {
                index = random.NextInt(0, peoples.Count - 1);
            }
            string tempPeopleName = peoples[index];
            SetPeopleName(tempPeopleName, false);

            string log = $"恭喜{tempPeopleName}获得{prizeName}";
            File.AppendAllText("log.txt",log+"\r\n");
            this.logTextBox.AppendText($"{log}\r\n");
            Console.WriteLine(log);
            MessageBox.Show(log);

            //扣除奖项
            peoples.RemoveAt(index);
            switch (prizeNameIndex)
            {
                case 0:
                    firstPrizeSize--;
                    this.P1.Content=$"一等奖剩余数量：{firstPrizeSize}";
                    break;

                case 1:
                    secondPrizeSize--;
                    this.P2.Content=$"二等奖剩余数量：{secondPrizeSize}";
                    break;

                case 2:
                    thirdPrizeSize--;
                    this.P3.Content=$"三等奖剩余数量：{thirdPrizeSize}";
                    break;
                default:
                    break;
            }
            //释放控件
            SelectPrizeComboBox.IsEnabled = true;

        }
    }
}