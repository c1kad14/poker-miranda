using System.Drawing;

namespace miranda.ui
{

    public enum ShortButtonType
    {
        Button1 = 0,
        Button2 = 1,
        Button3,
        Button4
    }
    public class ShortButton
    {
        public Rectangle Rect { get; set; }
        public ShortButtonType Type { get; set; }
    }
}
