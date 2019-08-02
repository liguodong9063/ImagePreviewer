using System;

namespace WpfControls.ImagePreviewer
{
    public class LoactionArgs : EventArgs
    {
         public readonly double Left;
        public readonly double Top;
        public LoactionArgs(double left, double top)
        {
            Left = left;
            Top = top;
        }

    }
}
