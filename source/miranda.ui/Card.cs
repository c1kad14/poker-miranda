using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using Point = System.Drawing.Point;

namespace miranda.ui
{
    /// <summary>
    /// Rank enumeration
    /// </summary>
    public enum Rank
    {
        NOT_RECOGNIZED = 0,
        Two = 1,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }
    /// <summary>
    /// Suit enumeration
    /// </summary>
    public enum Suit
    {
        NOT_RECOGNIZED = 0,
        Hearts,
        Diamonds,
        Spades,
        Clubs
    }
    /// <summary>
    /// Class represents recognized playing card. Contains properties of card such as ; 
    /// Rank of card, suit of card, image of card which is extracted from source image ,
    /// corner points of card on source image
    /// </summary>
    public class Card
    {
        //TODO fucking StackOverflow
        //public static bool operator ==(Card c1, Card c2)
        //{
        //    if (c1 == null || c2 == null)
        //    {
        //        return false;
        //    }
        //    return c1.Rank == c2.Rank;
        //}

        //public static bool operator !=(Card c1, Card c2)
        //{
        //    if (c1 == null || c2 == null)
        //    {
        //        return true;
        //    }
        //    return c1.Rank != c2.Rank;
        //}

        //Variables
        private Rank rank; //Rank of card
        private Suit suit; //Suit of card
        private Bitmap image; //Extracted(transformed) image of card
        

        
        public Rank Rank
        {
            set { this.rank = value; }
            get { return this.rank; }
        }
        public Suit Suit
        {
            set { this.suit = value; }
            get { return this.suit; }
        }
        public Bitmap Image
        {
            get { return this.image; }
            set
            {
                this.image = value;
            }
        }

        public Card()
        {
            
        }
        //Constructor
        public Card(Bitmap cardImg)
        {
            this.image = cardImg;

            
        }

        public Bitmap GetPart(Rectangle rect)
        {
            if (image == null)
                return null;
            Crop crop = new Crop(rect);//TODO card identity
            var img = crop.Apply(image);
            return img;
        }
        public Bitmap GetTopRightPart()
        {
            if (image == null)
                return null;
            Crop crop = new Crop(new Rectangle(0, 17, 15, 20));//TODO card identity
            var img = crop.Apply(image);
            return img;
        }

        public Bitmap GetTopPart()
        {
            if (image == null)
                return null;
            Crop crop = new Crop(new Rectangle(1, 1, 15, 19));//TODO card identity
            var img = crop.Apply(image);
            return img;
        }

        public string ToShortStr()
        {
            string suitStr = string.Empty;
            string rankStr = string.Empty;

            //Convert suit value to string 
            switch (suit)
            {
                case Suit.Clubs:
                    suitStr = "♣";
                    break;
                case Suit.Diamonds:
                    suitStr = "♦";
                    break;
                case Suit.Hearts:
                    suitStr = "♥";
                    break;
                case Suit.Spades:
                    suitStr = "♠";
                    break;
            }
            //Convert Rank Value To String
            switch (rank)
            {
                case Rank.Ace:
                    rankStr = "A";
                    break;
                case Rank.Two:
                    rankStr = "2";
                    break;
                case Rank.Three:
                    rankStr = "3";
                    break;
                case Rank.Four:
                    rankStr = "4";
                    break;
                case Rank.Five:
                    rankStr = "5";
                    break;
                case Rank.Six:
                    rankStr = "6";
                    break;
                case Rank.Seven:
                    rankStr = "7";
                    break;
                case Rank.Eight:
                    rankStr = "8";
                    break;
                case Rank.Nine:
                    rankStr = "9";
                    break;
                case Rank.Ten:
                    rankStr = "10";
                    break;
                case Rank.Jack:
                    rankStr = "J";
                    break;
                case Rank.Queen:
                    rankStr = "Q";
                    break;
                case Rank.King:
                    rankStr = "K";
                    break;
            }
            return rankStr + suitStr;
        }

        /// <summary>
        /// Overrided ToString Function.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string suitStr = string.Empty;
            string rankStr = string.Empty;

            //Convert suit value to string 
            switch (suit)
            {
                case Suit.Clubs:
                    suitStr = "Clubs";
                    break;
                case Suit.Diamonds:
                    suitStr = "Diamonds";
                    break;
                case Suit.Hearts:
                    suitStr = "Hearts";
                    break;
                case Suit.Spades:
                    suitStr = "Spades";
                    break;
            }
            //Convert Rank Value To String
            switch (rank)
            {
                case Rank.Ace:
                    rankStr = "Ace";
                    break;
                case Rank.Two:
                    rankStr = "Two";
                    break;
                case Rank.Three:
                    rankStr = "Three";
                    break;
                case Rank.Four:
                    rankStr = "Four";
                    break;
                case Rank.Five:
                    rankStr = "Five";
                    break;
                case Rank.Six:
                    rankStr = "Six";
                    break;
                case Rank.Seven:
                    rankStr = "Seven";
                    break;
                case Rank.Eight:
                    rankStr = "Eight";
                    break;
                case Rank.Nine:
                    rankStr = "Nine";
                    break;
                case Rank.Ten:
                    rankStr = "Ten";
                    break;
                case Rank.Jack:
                    rankStr = "Jack";
                    break;
                case Rank.Queen:
                    rankStr = "Queen";
                    break;
                case Rank.King:
                    rankStr = "King";
                    break;
            }
            return rankStr + " of " + suitStr;
        }
    }
}
