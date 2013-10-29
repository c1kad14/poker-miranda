using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace miranda.ui
{
    /// <summary>
    /// A Collection class that holds card objects 
    /// Derived from CollectionBase class
    /// </summary>
    public class CardCollectionOld : CollectionBase
    {
        public void Add(Card card)
        {
            this.List.Add(card);
        }
        public Card this[int index]
        {
            get { return this.List[index] as Card; }
            set { this.List[index] = value; }
        }
        /// <summary>
        /// Function that returns list of images of carrds in collection. 
        /// </summary>
        /// <returns>Images of Cards</returns>
        public List<Bitmap> ToImageList()
        {
            List<Bitmap> list = new List<Bitmap>();

            foreach (Card card in this.List)
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
    }
}
