using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lottery
{
    /// <summary>
    /// TextWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TextWindow : Window
    {
        MediaPlayer player = null;
        public TextWindow()
        {
            InitializeComponent();
            player = new MediaPlayer();
            player.Volume = 1;
            player.Open(new Uri("颁奖.wav", UriKind.Relative));
            player.MediaEnded += (senderx, ex) =>
            {//播放结束后 又重新播放
                player.Position = new TimeSpan(0);
            };
            player.Play();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            player.Stop();

        }
    }
}
