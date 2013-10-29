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
    public class ButtonCollection : CollectionBase
    {
        public void Add(Button button)
        {
            this.List.Add(button);
        }
        public Button this[int index]
        {
            get { return this.List[index] as Button; }
            set { this.List[index] = value; }
        }
        /// <summary>
        /// Function that returns list of images of carrds in collection. 
        /// </summary>
        /// <returns>Images of buttons</returns>
        public List<Bitmap> ToImageList()
        {
            List<Bitmap> list = new List<Bitmap>();

            foreach (Button button in this.List)
                list.Add(button.Image);

            return list;
        }

        public override string ToString()
        {
            var str = "";
            foreach (Button item in this)
            {
                str += item.ToString() + Environment.NewLine;
            }
            return str;
        }
    }
}
