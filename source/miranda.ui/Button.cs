using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using Point = System.Drawing.Point;

namespace miranda.ui
{
    /// <summary>
    /// Suit enumeration
    /// </summary>
    public enum ButtonTip
    {
        NOT_RECOGNIZED = 0,
        Fold,
        Check,
        Raise,
        Call,
        Bet
    }
    /// <summary>
    /// Class represents recognized playing card. Contains properties of card such as ; 
    /// Rank of card, suit of card, image of card which is extracted from source image ,
    /// corner points of card on source image
    /// </summary>
    public class Button
    {
        //Variables
        private ButtonTip tip; //Rank of card
        private Bitmap image; //Extracted(transformed) image of card
        private Point[] corners ;//Corner points of card on source image


        //Properties
        public Point[] Corners
        {
            get { return this.corners; }
        }

        public Rectangle Rect { get; set; }

        public ButtonTip Tip
        {
            set { this.tip = value; }
            get { return this.tip; }
        }
        
        public Bitmap Image
        {
            get { return this.image; }
            set
            {
                this.image = value;
            }
        }
        //Constructor
        public Button(Bitmap cardImg, IntPoint[] cornerIntPoints)
        {
            this.image = cardImg;

            //Convert AForge.IntPoint Array to System.Drawing.Point Array
            int total = cornerIntPoints.Length;
            corners = new Point[total]; 

            for(int i = 0 ; i < total ; i++)
            {
                this.corners[i].X = cornerIntPoints[i].X;
                this.corners[i].Y = cornerIntPoints[i].Y;
            }
        }

        public Button(Bitmap cardImg)
        {
            this.image = cardImg;
        }
        

        public Bitmap GetTopPart()
        {
            if (image == null)
                return null;
            Crop crop = new Crop(new Rectangle(1, 1, 15, 19));//TODO card identity
            var img = crop.Apply(image);
            return img;
        }
        /// <summary>
        /// Overrided ToString Function.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            
            string tipStr = string.Empty;

            
            //Convert Rank Value To String
            switch (tip)
            {
                case ButtonTip.NOT_RECOGNIZED:
                    tipStr = "NONE";
                    break;
                case ButtonTip.Fold:
                    tipStr = "Fold";
                    break;
                case ButtonTip.Check:
                    tipStr = "Check";
                    break;
                case ButtonTip.Raise:
                    tipStr = "Raise";
                    break;
                case ButtonTip.Bet:
                    tipStr = "Bet";
                    break;
                case ButtonTip.Call:
                    tipStr = "Call";
                    break;
                
            }
            return tipStr + "; " + Rect.ToString();
        }
    }
}
