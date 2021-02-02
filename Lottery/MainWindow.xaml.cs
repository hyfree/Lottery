using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

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
        private volatile int specialPrizeSize = 0;
        private volatile int firstPrizeSize = 0;
        private volatile int secondPrizeSize = 0;
        private volatile int thirdPrizeSize = 0;
        private volatile int sunPrizeSize = 0;

        private string specialPrizeName = string.Empty;
        private string firstPrizeName = string.Empty;
        private string secondPrizeName = string.Empty;
        private string thirdPrizeName = string.Empty;
        private string sunPrizeName = string.Empty;

        private int selectSize = 0;

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
                this.StartButton.IsEnabled = false;
                
            }
            else
            {
                var result = DoCheck();
                if (!result)
                {
                    return;
                }
                startFlag = true;
                DoStart();
                this.SelectPrizeComboBox.IsEnabled = false;
                this.StartButton.Content = "停止";
            }
        }

        public bool DoCheck()
        {
            try
            {
                selectSize = Convert.ToInt32(SelectSizeTextBox.Text);
                if (selectSize < 1)
                {
                    MessageBox.Show("抽取人数错误");
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("抽取人数错误");
                return false;
            }
            //检查人员是否设置
            if (peoples.Count == 0)
            {
                MessageBox.Show("抽奖参与者=0，无法抽奖");
                return false;
            }
            if (peoples.Count < selectSize)
            {
                MessageBox.Show($"抽奖参与者数量={peoples.Count}小于抽取人数={selectSize}，无法抽奖");
                return false;
            }

            prizeNameIndex = SelectPrizeComboBox.SelectedIndex;
            //检查奖品是否抽完了
            switch (SelectPrizeComboBox.SelectedIndex)
            {
                case 0:
                    if (specialPrizeSize < 1 || specialPrizeSize < selectSize)
                    {
                        MessageBox.Show("特等奖已经被抽完了或者奖品的数量已经不足");
                        return false;
                    }
                    prizeName = SpecialPrizeNameTextBox.Text;
                    break;

                case 1:
                    if (FirstPrizeSize < 1 || FirstPrizeSize < selectSize)
                    {
                        MessageBox.Show("一等奖已经被抽完了或者奖品的数量已经不足");
                        return false;
                    }
                    prizeName = FirstPrizeNameTextBox.Text;
                    break;

                case 2:
                    if (SecondPrizeSize < 1 || SecondPrizeSize < selectSize)
                    {
                        MessageBox.Show("二等奖已经被抽完了或者奖品的数量已经不足");
                        return false;
                    }
                    prizeName = SecondPrizeNameTextBox.Text;
                    break;

                case 3:
                    if (ThirdPrizeSize < 1 || ThirdPrizeSize < selectSize)
                    {
                        MessageBox.Show("三等奖已经被抽完了或者奖品的数量已经不足");
                        return false;
                    }
                    prizeName = ThirdPrizeNameTextBox.Text;
                    break;

                case 4:
                    if (sunPrizeSize < 1 || sunPrizeSize < selectSize)
                    {
                        MessageBox.Show("阳光奖已经被抽完了或者奖品的数量已经不足");
                        return false;
                    }
                    prizeName = SunPrizeNameTextBox.Text;
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
            thread.Priority = ThreadPriority.Highest;
            thread.Start();

            firstPlayer.Pause();
            bgPlayer.Play();

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
                            Thread.Sleep(20);
                            SetPeopleName(item);
                        }
                        else
                        {
                         
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
            Stop();
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
            specialPrizeName = this.SpecialPrizeNameTextBox.Text;
            string p0SizeText = this.SpecialPrizeSizeTextBox.Text;
            specialPrizeSize = Convert.ToInt32(p0SizeText);
            this.P0.Content = $"特等奖剩余数量：{p0SizeText}";

            firstPrizeName = this.FirstPrizeNameTextBox.Text;
            string p1SizeText = this.FirstPrizeSizeTextBox.Text;
            firstPrizeSize = Convert.ToInt32(p1SizeText);
            this.P1.Content = $"一等奖剩余数量：{p1SizeText}";

            secondPrizeName = this.SecondPrizeNameTextBox.Text;
            string p2SizeText = this.SecondPrizeSizeTextBox.Text;
            SecondPrizeSize = Convert.ToInt32(p2SizeText);
            this.P2.Content = $"二等奖剩余数量：{p2SizeText}";

            thirdPrizeName = this.ThirdPrizeNameTextBox.Text;
            string p3SizeText = this.ThirdPrizeSizeTextBox.Text;
            thirdPrizeSize = Convert.ToInt32(p3SizeText);
            this.P3.Content = $"三等奖剩余数量：{p3SizeText}";

            sunPrizeName = this.SunPrizeNameTextBox.Text;
            string p4SizeText = this.SunPrizeSizeTextBox.Text;
            sunPrizeSize = Convert.ToInt32(p4SizeText);
            this.P4.Content = $"阳光奖剩余数量：{p4SizeText}";
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

        private void Stop()
        {
            //使用RNGCryptoServiceProvider产生一个随机性足够好的随机数
            //RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            //SecureRandom random = new SecureRandom(rngCsp);

            //Random simpleRandom = new Random();
            //simpleRandom.Next(0, 5);
             
            Random random = new Random();
            int index = 0;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < selectSize; i++)
            {
                if (peoples.Count != 1)
                {
                    index = random.Next(0, peoples.Count - 1);
                }
                string tempPeopleName = peoples[index];
                stringBuilder.Append($" {tempPeopleName}  ");
                string log = $"恭喜{tempPeopleName}获得{prizeName}";
                File.AppendAllText("log.txt", log + "\r\n");
                AppendLog(log);
                peoples.RemoveAt(index);
            }
            if (selectSize == 1)
            {
                SetPeopleName(stringBuilder.ToString(), false);

            }
            else
            {
                SetPeopleName(stringBuilder.ToString(), false);
            }
            //SetPeopleName(stringBuilder.ToString(),false);
            // SetPeopleName(tempPeopleName, false);
            
  
            //更新界面
            this.Dispatcher.Invoke(()=>{
                firstPlayer.Pause();
                bgPlayer.Pause();
                TextWindow textWindow = new TextWindow();
                textWindow.People.Text = stringBuilder.ToString();
                textWindow.Owner = this;
                textWindow.ShowDialog();

                firstPlayer.Play();
                bgPlayer.Stop();


                SelectPrizeComboBox.IsEnabled = true;

                this.StartButton.Content = "开始";
                SelectPrizeComboBox.IsEnabled = true;
                this.StartButton.IsEnabled = true;

                switch (prizeNameIndex)
                {

                    case 0:
                        specialPrizeSize = specialPrizeSize - selectSize;
                        this.P0.Content = $"特等奖剩余数量：{specialPrizeSize}";
                        break;

                    case 1:
                        firstPrizeSize = firstPrizeSize - selectSize;
                        this.P1.Content = $"一等奖剩余数量：{firstPrizeSize}";
                        break;

                    case 2:
                        secondPrizeSize = secondPrizeSize - selectSize;
                        this.P2.Content = $"二等奖剩余数量：{secondPrizeSize}";
                        break;

                    case 3:
                        thirdPrizeSize = thirdPrizeSize - selectSize;
                        this.P3.Content = $"三等奖剩余数量：{thirdPrizeSize}";
                        break;

                    case 4:
                        sunPrizeSize = sunPrizeSize - selectSize;
                        this.P4.Content = $"阳光奖剩余数量：{sunPrizeSize}";
                        break;
                    default:
                        break;
                }
               
          
            });
            
        }
        private void AppendLog(string log)
        {
            this.logTextBox.Dispatcher.Invoke(()=>{

                this.logTextBox.AppendText($"{log}\r\n");

            });
        }

        MediaPlayer firstPlayer = null;
        MediaPlayer bgPlayer = null;
        private void Window_Initialized(object sender, EventArgs e)
        {
            string file = File.ReadAllText("抽奖人名单.txt");
            this.PeoplesTextBox.Text = file;

            firstPlayer = new MediaPlayer();
            bgPlayer = new MediaPlayer();

            bgPlayer.Open(new Uri("抽奖背景.wav", UriKind.Relative));
            bgPlayer.Volume = 0.5;
            bgPlayer.MediaEnded += (senderx, ex) =>
            {//播放结束后 又重新播放
                bgPlayer.Position = new TimeSpan(0);
            };

            firstPlayer.Open(new Uri("通用音乐.wav", UriKind.Relative));
            firstPlayer.Volume = 0.5;
            firstPlayer.MediaEnded += (senderx, ex) =>
            {//播放结束后 又重新播放
                firstPlayer.Position = new TimeSpan(0);
            };

            firstPlayer.Play();
        }
    }
}