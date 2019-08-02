using ImagePreviewer.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfControls.ImagePreviewer;

namespace ImagePreviewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private bool _isBackgroundDragMove;

        public MainWindow()
        {
            InitializeComponent();


            var image1=new BitmapImage(new Uri("pack://application:,,,/Resources/Images/testImage1.jpg"));
            var byte1 = image1.ToByteArray();

            var image2 = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/testImage2.jpg"));
            var byte2 = image2.ToByteArray();

            var image3 = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/testImage3.jpeg"));
            var byte3 = image3.ToByteArray();

            var image4 = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/testImage4.jpg"));
            var byte4 = image4.ToByteArray();

            var imageSources = new List<byte[]>
            {
                byte1,byte2,byte3,byte4
            };

            
            ImagePreviewer.SourceList = new ImageListIndex { Index = 2, ImageSources = imageSources };
            ImagePreviewer.CloseDelegate = isClose =>
            {
                if (isClose)
                {
                    Close();
                }
            };
            ImagePreviewer.DragMoveDelegate = isDragMove =>
            {
                _isBackgroundDragMove = isDragMove;
            };
        }

        private void BackGround_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isBackgroundDragMove) return;
            try
            {
                DragMove();
            }
            catch
            {
                // ignored
            }
        }
    }
}