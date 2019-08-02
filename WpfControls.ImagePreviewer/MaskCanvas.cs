using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Size = System.Drawing.Size;

namespace WpfControls.ImagePreviewer
{
    public class MaskCanvas : Canvas
    {
        private readonly Brush _selectionBorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        private readonly Brush _maskWindowBackground = new SolidColorBrush(Color.FromArgb(5, 0, 0, 0));

        public MaskCanvas()
        {
            Loaded += OnLoaded;
        }

        

        public event EventHandler<LoactionArgs> LoationChanged;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            maskRectLeft.Fill = maskRectRight.Fill = maskRectTop.Fill = maskRectBottom.Fill = _maskWindowBackground;

            SetLeft(maskRectLeft, 0);
            SetTop(maskRectLeft, 0);
            SetRight(maskRectRight, 0);
            SetTop(maskRectRight, 0);
            SetTop(maskRectTop, 0);
            SetBottom(maskRectBottom, 0);
            maskRectLeft.Height = ActualHeight;

            Children.Add(maskRectLeft);
            Children.Add(maskRectRight);
            Children.Add(maskRectTop);
            Children.Add(maskRectBottom);

            selectionBorder.Stroke = _selectionBorderBrush;
            selectionBorder.StrokeThickness = 1;

            Children.Add(selectionBorder);

            indicator = new IndicatorObject(this);
            indicator.Visibility = Visibility.Hidden;
            Children.Add(indicator);

            CompositionTarget.Rendering += OnCompositionTargetRendering;

        }

        private void UpdateSelectionBorderLayout()
        {
            if (!selectionRegion.IsEmpty)
            {
                SetLeft(selectionBorder, selectionRegion.Left);
                SetTop(selectionBorder, selectionRegion.Top);
                selectionBorder.Width = selectionRegion.Width;
                selectionBorder.Height = selectionRegion.Height;
            }
        }

        private void UpdateMaskRectanglesLayout()
        {
            var actualHeight = ActualHeight;
            var actualWidth = ActualWidth;

            if (selectionRegion.IsEmpty)
            {
                SetLeft(maskRectLeft, 0);
                SetTop(maskRectLeft, 0);
                maskRectLeft.Width = actualWidth;
                maskRectLeft.Height = actualHeight;

                maskRectRight.Width = maskRectRight.Height = maskRectTop.Width = maskRectTop.Height = maskRectBottom.Width = maskRectBottom.Height = 0;
            }
            else
            {
                var temp = selectionRegion.Left;
                if (maskRectLeft.Width != temp)
                {
                    maskRectLeft.Width = temp < 0 ? 0 : temp; //Math.Max(0, selectionRegion.Left);
                }

                temp = ActualWidth - selectionRegion.Right;
                if (maskRectRight.Width != temp)
                {
                    maskRectRight.Width = temp < 0 ? 0 : temp; //Math.Max(0, ActualWidth - selectionRegion.Right);
                }

                if (maskRectRight.Height != actualHeight)
                {
                    maskRectRight.Height = actualHeight;
                }

                SetLeft(maskRectTop, maskRectLeft.Width);
                SetLeft(maskRectBottom, maskRectLeft.Width);

                temp = actualWidth - maskRectLeft.Width - maskRectRight.Width;
                if (maskRectTop.Width != temp)
                {
                    maskRectTop.Width = temp < 0 ? 0 : temp; //Math.Max(0, ActualWidth - maskRectLeft.Width - maskRectRight.Width);
                }

                temp = selectionRegion.Top;
                if (maskRectTop.Height != temp)
                {
                    maskRectTop.Height = temp < 0 ? 0 : temp; //Math.Max(0, selectionRegion.Top);
                }

                maskRectBottom.Width = maskRectTop.Width;

                temp = actualHeight - selectionRegion.Bottom;
                if (maskRectBottom.Height != temp)
                {
                    maskRectBottom.Height = temp < 0 ? 0 : temp; //Math.Max(0, ActualHeight - selectionRegion.Bottom);
                }
            }
        }


        #region Fileds & Props
        private Rect selectionRegion = Rect.Empty;
        private bool isMaskDraging;
        public bool MoveState;
        private IndicatorObject indicator;
        private Point? selectionStartPoint;
        private Point? selectionEndPoint;

        private readonly Rectangle selectionBorder = new Rectangle();

        private readonly Rectangle maskRectLeft = new Rectangle();
        private readonly Rectangle maskRectRight = new Rectangle();
        private readonly Rectangle maskRectTop = new Rectangle();
        private readonly Rectangle maskRectBottom = new Rectangle();


        public Size? DefaultSize
        {
            get;
            set;
        }
        #endregion

        #region Mouse Managment

        private bool IsMouseOnThis(RoutedEventArgs e)
        {
            return e.Source.Equals(this) || e.Source.Equals(maskRectLeft) || e.Source.Equals(maskRectRight) || e.Source.Equals(maskRectTop) || e.Source.Equals(maskRectBottom);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            indicator.Visibility = Visibility.Visible;

            if (e.Source.Equals(indicator))
            {
                HandleIndicatorMouseDown(e);
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseOnThis(e))
            {
                UpdateSelectionRegion(e, UpdateMaskType.ForMouseMoving);

                e.Handled = true;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseOnThis(e))
            {
                UpdateSelectionRegion(e, UpdateMaskType.ForMouseLeftButtonUp);
                FinishShowMask();
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            indicator.Visibility = Visibility.Collapsed;
            selectionRegion = Rect.Empty;
            selectionBorder.Width = selectionBorder.Height = 0;
            // ClearSelectionData();
            UpdateMaskRectanglesLayout();

            base.OnMouseRightButtonUp(e);
        }


        internal void HandleIndicatorMouseDown(MouseButtonEventArgs e)
        {
            MoveState = true;
        }

        internal void HandleIndicatorMouseUp(MouseButtonEventArgs e)
        {
            MoveState = false;
        }

        private void PrepareShowMask(System.Drawing.Point mouseLoc)
        {
            indicator.Visibility = Visibility.Collapsed;
            selectionBorder.Visibility = Visibility.Visible;

        }

        private void UpdateSelectionRegion()
        {
            var startPoint = new System.Drawing.Point(0, 0);
            var endPoint = new System.Drawing.Point(190, 130);
            var sX = startPoint.X;
            var sY = startPoint.Y;
            var eX = endPoint.X;
            var eY = endPoint.Y;

            var deltaX = eX - sX;
            var deltaY = eY - sY;

            if (Math.Abs(deltaX) >= SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(deltaX) >= SystemParameters.MinimumVerticalDragDistance)
            {

                double x = sX < eX ? sX : eX;//Math.Min(sX, eX);
                double y = sY < eY ? sY : eY;//Math.Min(sY, eY);
                double w = deltaX < 0 ? -deltaX : deltaX;//Math.Abs(deltaX);
                double h = deltaY < 0 ? -deltaY : deltaY;//Math.Abs(deltaY);

                selectionRegion = new Rect(x, y, w, h);
            }
            else
            {
                selectionRegion = new Rect(startPoint.X, startPoint.Y, DefaultSize.Value.Width, DefaultSize.Value.Height);
            }
        }

        private void UpdateSelectionRegion(MouseEventArgs e, UpdateMaskType updateType)
        {
            if (updateType == UpdateMaskType.ForMouseMoving && e.LeftButton != MouseButtonState.Pressed)
            {
                selectionStartPoint = null;
            }

            if (selectionStartPoint.HasValue)
            {
                selectionEndPoint = e.GetPosition(this);

                var startPoint = (Point)selectionEndPoint;
                var endPoint = (Point)selectionStartPoint;
                var sX = startPoint.X;
                var sY = startPoint.Y;
                var eX = endPoint.X;
                var eY = endPoint.Y;

                var deltaX = eX - sX;
                var deltaY = eY - sY;

                if (Math.Abs(deltaX) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(deltaX) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    isMaskDraging = true;

                    double x = sX < eX ? sX : eX;//Math.Min(sX, eX);
                    double y = sY < eY ? sY : eY;//Math.Min(sY, eY);
                    double w = deltaX < 0 ? -deltaX : deltaX;//Math.Abs(deltaX);
                    double h = deltaY < 0 ? -deltaY : deltaY;//Math.Abs(deltaY);

                    selectionRegion = new Rect(x, y, w, h);
                }
                else
                {
                    if (DefaultSize.HasValue && updateType == UpdateMaskType.ForMouseLeftButtonUp)
                    {
                        isMaskDraging = true;

                        selectionRegion = new Rect(startPoint.X, startPoint.Y, DefaultSize.Value.Width, DefaultSize.Value.Height);
                    }
                    else
                    {
                        isMaskDraging = false;
                    }
                }
            }

            UpdateIndicator(selectionRegion);
        }

        internal void UpdateSelectionRegion(Rect region, bool flag = false)
        {
            selectionRegion = region;
            UpdateIndicator(selectionRegion);
            if (LoationChanged != null && flag)
            {
                LoationChanged(this, new LoactionArgs(region.Left / Width, region.Top / Height));
            }
        }


        private void FinishShowMask()
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            if (isMaskDraging)
            {

                UpdateIndicator(selectionRegion);

                ClearSelectionData();
            }
        }

        private void ClearSelectionData()
        {
            isMaskDraging = false;
            selectionBorder.Visibility = Visibility.Collapsed;
            selectionStartPoint = null;
            selectionEndPoint = null;
        }

        private void UpdateIndicator(Rect region)
        {
            if (indicator == null)
                return;

            if (region.Width < indicator.MinWidth || region.Height < indicator.MinHeight)
            {
                return;
            }
            indicator.Visibility = Visibility.Visible;
            indicator.Width = region.Width;
            indicator.Height = region.Height;
            SetLeft(indicator, region.Left);
            SetTop(indicator, region.Top);


        }

        private Rect GetIndicatorRegion()
        {
            return new Rect(GetLeft(indicator), GetTop(indicator), indicator.ActualWidth, indicator.ActualHeight);
        }

        #endregion

        #region Render

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            UpdateSelectionBorderLayout();
            UpdateMaskRectanglesLayout();
        }

        #endregion

        #region inner types

        private enum UpdateMaskType
        {
            ForMouseMoving,
            ForMouseLeftButtonUp
        }

        #endregion

    }
}
