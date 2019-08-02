using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace WpfControls.ImagePreviewer
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImagePreviewView
    {
        int thumbWidth;
        int thumbHeight;
        int moving = 0;
        int movingX = 0;
        int movingY = 0;
        static  int? ImageIndex=null;
        double scale=1;
        private bool SourceChange = false;
        double imgWidth;
        double imgHeight;
        double hOffSetRate = 0;//滚动条横向位置横向百分比
        double vOffSetRate = 0;//滚动条位置纵向百分比
        public delegate void UpdateDelegate(bool IsClose);
        public UpdateDelegate CloseDelegate;
        public UpdateDelegate DragMoveDelegate;
        //Storyboard sb_ShowTools;
        //Storyboard sb_HideTools;
        //Storyboard sb_close;
        //Storyboard sb_right;
        //Storyboard sb_left;
        //Storyboard sb_sclose;
        //Storyboard sb_sright;
        //Storyboard sb_sleft;
        Storyboard sb_Tip;
        TransformGroup tfGroup;
        private Point pStart;

        private bool _isDown = false;//是否控件上按下鼠标

        private TransformGroup _transformGroup = null;

        private RotateTransform _rotateTransform = null;//旋转

        private ScaleTransform _scaleTransform = null;

        private TranslateTransform _translateTransform = null;

        #region 依赖属性

        public static readonly DependencyProperty SourceListProperty = DependencyProperty.Register("SourceList", typeof(ImageListIndex),typeof(ImagePreviewView),new PropertyMetadata(OnSourceListPropertyChanged));

        public ImageListIndex SourceList
        {
            get => (ImageListIndex)GetValue(SourceListProperty);
            set => SetValue(SourceListProperty, value);
        }
        static void OnSourceListPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender != null && sender is ImagePreviewView)
            {
                ImagePreviewView view = (ImagePreviewView)sender;
                if (args != null && args.NewValue != null)
                {

                    ImageListIndex lstimgSource = args.NewValue as ImageListIndex;
                    if (ImagePreviewView.ImageIndex != null)
                    {
                        lstimgSource.Index = (int)ImagePreviewView.ImageIndex;
                    }
                    view.imgBig.Source = ByteArrayToBitmapImage(lstimgSource.ImageSources[lstimgSource.Index]);
                    view.imgless.Source = ByteArrayToBitmapImage(lstimgSource.ImageSources[lstimgSource.Index]);
                    ImagePreviewView.ImageIndex = lstimgSource.Index;
                    view.GetImageWidthHeight();
                    
                }
            }
        }
      

        #endregion



        public ImagePreviewView()
        {
            InitializeComponent();
            

            //sb_ShowTools = this.FindResource("Sb_ShowTools") as Storyboard;
            //sb_HideTools = this.FindResource("Sb_HideTools") as Storyboard;
            //sb_sright = this.FindResource("rg_ShowTools") as Storyboard;
            //sb_sleft = this.FindResource("le_ShowTools") as Storyboard;
            //sb_sclose = this.FindResource("cl_ShowTools") as Storyboard;
            //sb_right = this.FindResource("rg_HideTools") as Storyboard;
            //sb_left = this.FindResource("le_HideTools") as Storyboard;
            //sb_close = this.FindResource("cl_HideTools") as Storyboard;
            sb_Tip = this.FindResource("sb_Tips") as Storyboard;
            tfGroup = this.FindResource("TfGroup") as TransformGroup;
            this.Loaded += MainWindow_Loaded;
            this.MouseEnter += ImageViews_MouseEnter;
            this.MouseLeave += ImageViews_MouseLeave;
            svImg.ScrollChanged += svImg_ScrollChanged;
            gridMouse.MouseWheel += svImg_MouseWheel;
            gridMouse.MouseLeftButtonDown += control_MouseLeftButtonDown;
            gridMouse.MouseLeftButtonUp += control_MouseLeftButtonUp;
            gridMouse.MouseMove += control_MouseMove;
            btnActualsize.Click += btnActualsize_Click;
            btnEnlarge.Click += btnEnlarge_Click;
            btnNarrow.Click += btnNarrow_Click;
            btnRotate.Click += btnRotate_Click;
            btnLeft.Click += btnLeft_Click;
            next.MouseLeftButtonUp += Next_Click;
            last.MouseLeftButtonUp += Last_Click;
            Close.Click += Close_Click1;
            SaveAs.Click += SaveAs_Click;
            border.MouseMove += Border_GotFocus;
            border.MouseLeave += Border_LostFocus;
            stkLeft.MouseMove += StkLeft_MouseMove;
            stkLeft.MouseLeave += StkLeft_MouseLeave;
            stkRight.MouseMove += StkRight_MouseMove;
            stkRight.MouseLeave += StkRight_MouseLeave;
            stkClose.MouseMove += stkClose_MouseMove;
            stkClose.MouseLeave += stkClose_MouseLeave;
            this.MouseMove += ImageViews_MouseMove;
        
            SetbtnActualsizeEnable();
        }

        private void ImageViews_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgBig.ActualHeight >= 720 || imgBig.ActualWidth >= 1400)
            {
                DragMoveDelegate(false);
            }
            else
            {
                DragMoveDelegate(true);
                if (SourceList.ImageSources.Count == 1)
                {
                    stkRight.Visibility = Visibility.Collapsed;
                    stkLeft.Visibility = Visibility.Collapsed;
                }
            }
            last.IsEnabled = SourceList.Index != 0;
            next.IsEnabled = SourceList.Index != SourceList.ImageSources.Count - 1;
        }

        private void stkClose_MouseMove(object sender, MouseEventArgs e)
        {
            Close.Visibility = Visibility.Visible;
        }
        private void stkClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Close.Visibility = Visibility.Collapsed;
        }
        private void StkRight_MouseMove(object sender, MouseEventArgs e)
        {
            next.Visibility = Visibility.Visible;
        }
        private void StkRight_MouseLeave(object sender, MouseEventArgs e)
        {
            next.Visibility = Visibility.Collapsed;
        }
        private void StkLeft_MouseMove(object sender, MouseEventArgs e)
        {
            last.Visibility = Visibility.Visible;
        }
        private void StkLeft_MouseLeave(object sender, MouseEventArgs e)
        {
            last.Visibility = Visibility.Collapsed;
        }
        private void Border_LostFocus(object sender, RoutedEventArgs e)
        {
            spl_gj.Visibility = Visibility.Collapsed;
        }

        private void Border_GotFocus(object sender, RoutedEventArgs e)
        {
            spl_gj.Visibility = Visibility.Visible;
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {Filter = "Files(*.jpg)|*.jpg|Files (*.png)|*.png"};
            var result = saveFileDialog.ShowDialog();
            if (result != true) return;
            var fs = new FileStream(saveFileDialog.FileName, FileMode.Create);
            var w = new BinaryWriter(fs);
            try
            {
                w.Write(SourceList.ImageSources[SourceList.Index]);
            }
            finally
            {
                fs.Close();
                w.Close();
            }
        }

        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] bytearray = null;
            try
            {
                var smarket = bmp.StreamSource; ;
                if (smarket != null && smarket.Length > 0)
                {
                    //设置当前位置
                    smarket.Position = 0;
                    using (BinaryReader br = new BinaryReader(smarket))
                    {
                        bytearray = br.ReadBytes((int)smarket.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return bytearray;
        }


        // byte[] --> BitmapImage
        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
        private void Close_Click1(object sender, RoutedEventArgs e)
        {
            CloseDelegate(true);
            ImageIndex = null;
        }

        private void Last_Click(object sender, RoutedEventArgs e)
        {

         
            if (SourceList.Index == 0)
            {
                //imgBig.Source = ByteArrayToBitmapImage(SourceList.imageSources[SourceList.imageSources.Count-1]);
                //imgless.Source = ByteArrayToBitmapImage(SourceList.imageSources[SourceList.imageSources.Count - 1]);
                //imgBig.Height = ByteArrayToBitmapImage((SourceList.imageSources[SourceList.imageSources.Count - 1])).Height;
                //imgBig.Width = ByteArrayToBitmapImage((SourceList.imageSources[SourceList.imageSources.Count - 1])).Width;
                //if (tfGroup != null)
                //{
                //    var rotate = tfGroup.Children[2] as RotateTransform;
                //    rotate.Angle = 0;
                //}
                //SourceList.Index = SourceList.imageSources.Count-1;
                //SourceChange = true;
                //scale = 1;
                //mouseXY.X = 0;
                //mouseXY.Y = 0;
                last.IsEnabled = false;

            }
        
            else
            {
                
                imgBig.Source = ByteArrayToBitmapImage(SourceList.ImageSources[SourceList.Index-1]);
                imgless.Source = ByteArrayToBitmapImage(SourceList.ImageSources[SourceList.Index-1]);
                imgBig.Width = ByteArrayToBitmapImage((SourceList.ImageSources[SourceList.Index - 1])).Width;
                imgBig.Height = ByteArrayToBitmapImage((SourceList.ImageSources[SourceList.Index - 1])).Height;
                //imgless.Width = (SourceList.imageSources[SourceList.Index - 1]).Width;
                //imgless.Height = (SourceList.imageSources[SourceList.Index - 1]).Height;

                if (tfGroup != null)
                {
                    var rotate = tfGroup.Children[2] as RotateTransform;
                    rotate.Angle = 0;
                }

                SourceList.Index--;
                SourceChange = true;
                scale = 1;
                mouseXY.X=0 ;
                mouseXY.Y = 0;
                last.IsEnabled = true;
                if (SourceList.Index == 0)
                {
                    last.IsEnabled = false;

                }

            }
            this.UpdateLayout();
            GetImageWidthHeight();
            //thumbWidth = (int)mask.ActualWidth;
            //thumbHeight = (int)mask.ActualHeight;
            //double timeH = svImg.ViewportHeight / (svImg.ViewportHeight + svImg.ScrollableHeight);
            //double timeW = svImg.ViewportWidth / (svImg.ViewportWidth + svImg.ScrollableWidth);
            //double w = thumbWidth * timeW;
            //double h = thumbHeight * timeH;
            //Rect rect = new Rect(0,0, w, h);
            //mask.UpdateSelectionRegion(rect);
            SourceChange = false;
       

        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {

            if (SourceList.Index == SourceList.ImageSources.Count-1)
            {
                //imgBig.Source = ByteArrayToBitmapImage(SourceList.imageSources[0]);
                //imgless.Source = ByteArrayToBitmapImage(SourceList.imageSources[0]);
                //imgBig.Height = ByteArrayToBitmapImage((SourceList.imageSources[0])).Height;
                //imgBig.Width = ByteArrayToBitmapImage((SourceList.imageSources[0])).Width;
                //if (tfGroup != null)
                //{
                //    var rotate = tfGroup.Children[2] as RotateTransform;
                //    rotate.Angle = 0;
                //}

                //SourceList.Index = 0;
                //SourceChange = true;
                //scale = 1;
                //mouseXY.X = 0;
                //mouseXY.Y = 0;
                next.IsEnabled = false;

            }

            else
            {

                imgBig.Source = ByteArrayToBitmapImage(SourceList.ImageSources[SourceList.Index + 1]);
                imgless.Source = ByteArrayToBitmapImage(SourceList.ImageSources[SourceList.Index + 1]);
                imgBig.Width = ByteArrayToBitmapImage((SourceList.ImageSources[SourceList.Index + 1])).Width;
                imgBig.Height = ByteArrayToBitmapImage((SourceList.ImageSources[SourceList.Index + 1])).Height;
                //imgless.Width = (SourceList.imageSources[SourceList.Index - 1]).Width;
                //imgless.Height = (SourceList.imageSources[SourceList.Index - 1]).Height;

                if (tfGroup != null)
                {
                    var rotate = tfGroup.Children[2] as RotateTransform;
                    rotate.Angle = 0;
                }

                SourceList.Index++;
                SourceChange = true;
                scale = 1;
                mouseXY.X = 0;
                mouseXY.Y = 0;
                next.IsEnabled = true;
                if (SourceList.Index == SourceList.ImageSources.Count - 1)
                {
                    next.IsEnabled = false;
                }

            }
            this.UpdateLayout();
            GetImageWidthHeight();
            //thumbWidth = (int)mask.ActualWidth;
            //thumbHeight = (int)mask.ActualHeight;
            //double timeH = svImg.ViewportHeight / (svImg.ViewportHeight + svImg.ScrollableHeight);
            //double timeW = svImg.ViewportWidth / (svImg.ViewportWidth + svImg.ScrollableWidth);
            //double w = thumbWidth * timeW;
            //double h = thumbHeight * timeH;
            //Rect rect = new Rect(0,0, w, h);
            //mask.UpdateSelectionRegion(rect);
            SourceChange = false;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GetImageWidthHeight();
        }

        public void GetImageWidthHeight()
        {
            this.UpdateLayout();
            if (imgWidth == 0.0 || imgHeight == 0.0|| SourceChange)
            {
                imgWidth = imgBig.ActualWidth;
                imgHeight = imgBig.ActualHeight;
            }
        }
        void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;
            scale = 1;
            this.txtZoom.Text = ((int)(scale * 100)).ToString() + "%";
            if (sb_Tip != null)
                sb_Tip.Begin();
            SetbtnActualsizeEnable();
            btnNarrow.IsEnabled = true;
            btnEnlarge.IsEnabled = true;
            hOffSetRate = 0;
            vOffSetRate = 0;
            imgBig.Width = imgWidth;
            imgBig.Height = imgHeight;

            if (tfGroup != null)
            {
                var rotate = tfGroup.Children[2] as RotateTransform;
                rotate.Angle -= 90;
            }

        }
        void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;
            scale = 1;
            this.txtZoom.Text = ((int)(scale * 100)).ToString() + "%";
            if (sb_Tip != null)
                sb_Tip.Begin();
            SetbtnActualsizeEnable();
            btnNarrow.IsEnabled = true;
            btnEnlarge.IsEnabled = true;
            hOffSetRate = 0;
            vOffSetRate = 0;
            imgBig.Width = imgWidth;
            imgBig.Height = imgHeight;

            if (tfGroup != null)
            {
                var rotate = tfGroup.Children[2] as RotateTransform;
                rotate.Angle += 90;
            }

        }

        void btnNarrow_Click(object sender, RoutedEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            btnEnlarge.IsEnabled = true;
            if (scale < 0.15 )
                return;
            scale = scale * (1 / 1.2);

            SetbtnActualsizeEnable();
            if (scale < 0.15)
            {
                btnNarrow.IsEnabled = false;
            }
            this.txtZoom.Text = ((int)(scale * 100)).ToString()+"%";
            if (sb_Tip != null)
                sb_Tip.Begin();
            SetImageByScale();
            if (imgBig.ActualHeight >= 720 || imgBig.ActualWidth >= 1400)
            {
                lessimage.Visibility = Visibility.Visible;
            }
            else
            {
                lessimage.Visibility = Visibility.Collapsed;
            }

        }

        void btnEnlarge_Click(object sender, RoutedEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            btnNarrow.IsEnabled = true;
            if (scale > 16 )
                return;
            scale = scale * 1.2;
            SetbtnActualsizeEnable();

            if (scale > 16)
            {
                btnEnlarge.IsEnabled = false;
            }
            this.txtZoom.Text = ((int)(scale * 100)).ToString() + "%";
            if (sb_Tip != null)
                sb_Tip.Begin();
            SetImageByScale();

        }

        void btnActualsize_Click(object sender, RoutedEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            scale = 1;
            imgBig.Width = imgWidth;
            imgBig.Height = imgHeight;
            SetbtnActualsizeEnable();
        }

        void ImageViews_MouseLeave(object sender, MouseEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            //if (sb_ShowTools != null)
            //    sb_ShowTools.Pause();
            //if (sb_HideTools != null)
            //    sb_HideTools.Begin();
            //if (sb_sclose != null)
            //    sb_sclose.Pause();
            //if (sb_close != null)
            //    sb_close.Begin();
            //if (sb_sright != null)
            //    sb_sright.Pause();
            //if (sb_right != null)
            //    sb_right.Begin();
            //if (sb_sleft != null)
            //    sb_sleft.Pause();
            //if (sb_left != null)
            //    sb_left.Begin();

        }

        void ImageViews_MouseEnter(object sender, MouseEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            //if (sb_HideTools != null)
            //    sb_HideTools.Pause();
            //if (sb_ShowTools != null)
            //    sb_ShowTools.Begin();
            //if (sb_sclose != null)
            //    sb_sclose.Begin();
            //if (sb_close != null)
            //    sb_close.Pause();
            //if (sb_sright != null)
            //    sb_sright.Begin();
            //if (sb_right != null)
            //    sb_right.Pause();
            //if (sb_sleft != null)
            //    sb_sleft.Begin();
            //if (sb_left != null)
            //    sb_left.Pause();

        }

        private bool mouseDown;
        private Point mouseXY;

        void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            var img = sender as Grid;
            if (img == null)
            {
                return;
            }
            if (mouseDown)
            {
                Domousemove(img, e);
            }
        }

        private void Domousemove(Grid img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var position = e.GetPosition(img);
            double X = mouseXY.X - position.X;
            double Y = mouseXY.Y - position.Y;
            mouseXY = position;
            if (X != 0)
                svImg.ScrollToHorizontalOffset(svImg.HorizontalOffset + X);
            if (Y != 0)
                svImg.ScrollToVerticalOffset(svImg.VerticalOffset + Y);

            GetOffSetRate();
     
            if (imgBig.ActualHeight>=720|| imgBig.ActualWidth>=1400)
            {
                DragMoveDelegate(false);
            }
            else
            {
                DragMoveDelegate(true);
            }
        }
        void control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Grid;
            if (img == null)
            {
                return;
            }
            img.ReleaseMouseCapture();
            mouseDown = false;
        }

        void control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Grid;
            if (img == null)
            {
                return;
            }
            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
        }

        void svImg_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (imgWidth == 0 || imgHeight == 0)
                return;

            if (scale < 0.15 && e.Delta < 0)
                return;
            if (scale > 16 && e.Delta > 0)
                return;
            scale = scale * (e.Delta > 0 ? 1.2 : 1 / 1.2);

            SetbtnActualsizeEnable();
            btnNarrow.IsEnabled = scale > 0.15;
            btnEnlarge.IsEnabled = scale < 16;

            this.txtZoom.Text = ((int)(scale * 100)).ToString() + "%";
            if (sb_Tip != null)
                sb_Tip.Begin();
            SetImageByScale();
        }

        private void SetImageByScale()
        {
            GetOffSetRate();
            imgBig.Width = scale * imgWidth;
            imgBig.Height = scale * imgHeight;
            SetOffSetByRate();
        }

        /// <summary>
        /// 通过滚动条位置百分比在放大缩小时设置滚动条位置
        /// </summary>
        private void SetOffSetByRate()
        {
            this.UpdateLayout();
            if (svImg.ScrollableWidth > 0)
            {
                double hOffSet = hOffSetRate * svImg.ScrollableWidth;
                svImg.ScrollToHorizontalOffset(hOffSet);
            }
            if (svImg.ScrollableHeight > 0)
            {
                double vOffSet = vOffSetRate * svImg.ScrollableHeight;
                svImg.ScrollToVerticalOffset(vOffSet);
            }
        }

        /// <summary>
        /// 获取滚动条滚动位置百分比
        /// </summary>
        private void GetOffSetRate()
        {
            if (svImg.ScrollableWidth > 0)
            {
                if (svImg.HorizontalOffset != 0)
                    hOffSetRate = svImg.HorizontalOffset / svImg.ScrollableWidth;
            }
            if (svImg.ScrollableHeight > 0)
            {
                if (svImg.VerticalOffset != 0)
                    vOffSetRate = svImg.VerticalOffset / svImg.ScrollableHeight;
            }
        }




        void svImg_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            if (imgWidth == 0 || imgHeight == 0)
                return;

            thumbWidth = (int)mask.ActualWidth;
            thumbHeight = (int)mask.ActualHeight;

            double timeH = svImg.ViewportHeight / (svImg.ViewportHeight + svImg.ScrollableHeight);
            double timeW = svImg.ViewportWidth / (svImg.ViewportWidth + svImg.ScrollableWidth);

            double w = thumbWidth * timeW;
            double h = thumbHeight * timeH;

            double offsetx = 0;
            double offsety = 0;
            if (svImg.ScrollableWidth == 0)
            {
                offsetx = 0;
            }
            else
            {
                offsetx = (w - thumbWidth) / svImg.ScrollableWidth * svImg.HorizontalOffset;
            }

            if (svImg.ScrollableHeight == 0)
            {
                offsety = 0;
            }
            else
            {
                offsety = (h - thumbHeight) / svImg.ScrollableHeight * svImg.VerticalOffset;
            }


            Rect rect = new Rect(-offsetx, -offsety, w, h);

            mask.UpdateSelectionRegion(rect);
            if (imgBig.ActualHeight >= 720 || imgBig.ActualWidth >= 1400)
            {
                lessimage.Visibility = Visibility.Visible;
            }
            else
            {
                lessimage.Visibility = Visibility.Collapsed;
            }
            //throw new NotImplementedException();
        }

        #region 设置工具栏按钮可用
        /// <summary>
        /// 设置1:1按钮可用
        /// </summary>
        private void SetbtnActualsizeEnable()
        {
            btnActualsize.IsEnabled = (int)(scale * 100) != 100;
            if (btnActualsize.IsEnabled==true)
            {
                imgBig.Cursor = Cursors.Hand;
            }
        }
        #endregion

        private void Close_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void Close_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Close_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Close_GotFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
