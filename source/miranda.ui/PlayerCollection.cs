using System;
using System.Collections.Generic;
using System.Drawing;

namespace miranda.ui
{
    /// <summary>
    /// A Collection class that holds card objects 
    /// Derived from CollectionBase class
    /// </summary>
    public class PlayerCollection : List<Player>
    {
        
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
            foreach (Player item in this)
            {
                str += item.ToString() + Environment.NewLine;
            }
            return str;
        }
    }
}
