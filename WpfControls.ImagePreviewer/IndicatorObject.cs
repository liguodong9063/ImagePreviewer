using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfControls.ImagePreviewer
{
    internal class IndicatorObject : ContentControl
    {
        private readonly MaskCanvas _canvasOwner;

        public IndicatorObject(MaskCanvas canvasOwner)
        {
            _canvasOwner = canvasOwner;
        }

        static IndicatorObject()
        {
            var ownerType = typeof(IndicatorObject);

            FocusVisualStyleProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(null));
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
            MinWidthProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(5.0));
            MinHeightProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(5.0));
        }

        public void Move(System.Windows.Point offset)
        {
            var x = Canvas.GetLeft(this) + offset.X;
            var y = Canvas.GetTop(this) + offset.Y;

            x = x < 0 ? 0 : x;
            y = y < 0 ? 0 : y;

            x = Math.Min(x, this._canvasOwner.Width - this.Width);
            y = Math.Min(y, this._canvasOwner.Height - this.Height);

            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

            _canvasOwner.UpdateSelectionRegion(new Rect(x, y, Width, Height), true);
        }
    }
}