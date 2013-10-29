using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace miranda.ui
{
    /// <summary>
    /// A Collection class that holds card objects 
    /// Derived from CollectionBase class
    /// </summary>
    public class CardCollection : List<Card>
    {

        public bool HasCards(string cards)
        {
            var pairs = cards.Split(',');
            foreach (var pair in pairs)
            {
                Rank rank1 = Rank.NOT_RECOGNIZED;
                Rank rank2 = Rank.NOT_RECOGNIZED;
                bool isSuit = false;
                bool isNotSuit = false;
                bool isAnySuit = false;

                var str = pair;
                if (pair.Length == 3)
                {
                    var s = pair.Substring(2);
                    isSuit = (s == "s");
                    isNotSuit = (s == "o");
                    str = pair.Substring(0, 2);
                }
                else
                    isAnySuit = true;
                
                if(str[0] == 'A') rank1 =Rank.Ace;
                if (str[0] == 'K') rank1 = Rank.King;
                if (str[0] == 'Q') rank1 = Rank.Queen;
                if (str[0] == 'J') rank1 = Rank.Jack;
                if (str[0] == 'T') rank1 = Rank.Ten;
                if (str[0] == '9') rank1 = Rank.Nine;
                if (str[0] == '8') rank1 = Rank.Eight;
                if (str[0] == '7') rank1 = Rank.Seven;
                if (str[0] == '6') rank1 = Rank.Six;
                if (str[0] == '5') rank1 = Rank.Five;
                if (str[0] == '4') rank1 = Rank.Four;
                if (str[0] == '3') rank1 = Rank.Three;
                if (str[0] == '2') rank1 = Rank.Two;


                if (str[1] == 'A') rank2 = Rank.Ace;
                if (str[1] == 'K') rank2 = Rank.King;
                if (str[1] == 'Q') rank2 = Rank.Queen;
                if (str[1] == 'J') rank2 = Rank.Jack;
                if (str[1] == 'T') rank2 = Rank.Ten;
                if (str[1] == '9') rank2 = Rank.Nine;
                if (str[1] == '8') rank2 = Rank.Eight;
                if (str[1] == '7') rank2 = Rank.Seven;
                if (str[1] == '6') rank2 = Rank.Six;
                if (str[1] == '5') rank2 = Rank.Five;
                if (str[1] == '4') rank2 = Rank.Four;
                if (str[1] == '3') rank2 = Rank.Three;
                if (str[1] == '2') rank2 = Rank.Two;

                if (str[1] == '.')
                {
                    if (
                        (this[0].Rank == rank1 || this[1].Rank == rank1)
                        &&
                        (
                            isSuit && this[0].Suit == this[1].Suit
                            ||
                            isNotSuit && this[0].Suit != this[1].Suit
                            ||
                            isAnySuit
                        )
                        )
                        return true;

                }

                var suit1 = this[0].Suit;
                if (
                    (
                    this[0].Rank == rank1 && this[1].Rank == rank2
                    ||
                    this[0].Rank == rank2 && this[1].Rank == rank1
                    )
                    &&
                    (
                    this.All(card => card.Suit == suit1) && isSuit
                    ||
                    this.Exists(card => card.Suit != suit1) && isNotSuit
                    ||
                    isAnySuit
                    )
                )
                {
                    return true;
                }
            }
            return false;
        }

        public void SortByRank()
        {
            this.Sort((card, card1) => card.Rank.CompareTo(card1.Rank));   
        }

        
        /// <summary>
        /// Function that returns list of images of carrds in collection. 
        /// </summary>
        /// <returns>Images of Cards</returns>
        public List<Bitmap> ToImageList()
        {
            var list = new List<Bitmap>();

            foreach (var card in this)
                list.Add(card.Image);

            return list;
        }

        public override string ToString()
        {
            var str = "";
            foreach (Card item in this)
            {
                str += item.ToShortStr() + Environment.NewLine;
            }
            return str;
        }

        public IEnumerable<Tuple<string, Color>> CardsStr
        {
            get
            {
                foreach (var card in this)
                {
                    if(card.Suit == Suit.Diamonds || card.Suit == Suit.Hearts)
                        yield return new Tuple<string, Color>(card.ToShortStr(), Color.Red);
                    else
                        yield return new Tuple<string, Color>(card.ToShortStr(), Color.Black);
                }
                
            }
        }
    }
}
