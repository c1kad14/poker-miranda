using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace miranda
{
    public partial class AreaSelectorForm : Form
    {
        public AreaSelectorForm()
        {
            InitializeComponent();
            _ZoomFactor = tbZoomScale.Value;
            cbZoom.Checked = true;

        }

        private double ZOOMFACTOR = 1.25;	// = 25% smaller or larger
        private int MINMAX = 5;				// 5 times bigger or smaller than the ctrl

        public string SelectedImageFileName
        {
            get { return tbFile.Text; }
        }

        #region Zooming Methods

        /// <summary>
        /// Make the PictureBox dimensions larger to effect the Zoom.
        /// </summary>
        /// <remarks>Maximum 5 times bigger</remarks>
        private void ZoomIn(PictureBox box, Panel panel)
        {
            if ((box.Width < (MINMAX * panel.Width)) &&
                (box.Height < (MINMAX * panel.Height)))
            {
                box.Width = Convert.ToInt32(box.Width * ZOOMFACTOR);
                box.Height = Convert.ToInt32(box.Height * ZOOMFACTOR);
                box.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        /// <summary>
        /// Make the PictureBox dimensions smaller to effect the Zoom.
        /// </summary>
        /// <remarks>Minimum 5 times smaller</remarks>
        private void ZoomOut(PictureBox box, Panel panel)
        {
            if ((box.Width > (panel.Width / MINMAX)) &&
                (box.Height > (panel.Height / MINMAX)))
            {
                box.SizeMode = PictureBoxSizeMode.StretchImage;
                box.Width = Convert.ToInt32(box.Width / ZOOMFACTOR);
                box.Height = Convert.ToInt32(box.Height / ZOOMFACTOR);
            }
        }

        #endregion


        protected override void OnPaint(PaintEventArgs e)
        {
            

            base.OnPaint(e);
        }

        public AreaSelectorForm(Rectangle rect):this()
        {
            
            Rect = rect;
        }

        public void SetImage(Bitmap bmp)
        {
            //pictureBox1.Image = bmp;
            SetImage(bmp, 0);
        }

        public void SetImage(Bitmap bmp, int zoom)
        {
            if (zoom != 0)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Height = bmp.Height * zoom;
                pictureBox1.Width = bmp.Width * zoom;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            }
            pictureBox1.Image = bmp;
        }

        public Bitmap GetSelectedPicture()
        {
            var crop = new Crop(Rect);
            var newBmp = crop.Apply(pictureBox1.Image as Bitmap);
            return newBmp;
        }

        private Point RectStartPoint;
        public Rectangle Rect = new Rectangle();
        private Brush selectionBrush = new SolidBrush(Color.FromArgb(128, 72, 145, 220));

        // Start Rectangle
        //

        private void pictureBox1_MouseDown_1(object sender, MouseEventArgs e)
        {
            // Determine the initial rectangle coordinates...
            RectStartPoint = e.Location;
            Invalidate();
        }

        // Draw Rectangle
        //
        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (cbZoom.Checked)
                UpdateZoomedImage(e);

            if (e.Button != MouseButtons.Left)
                return;
            Point tempEndPoint = e.Location;
            Rect.Location = new Point(
                Math.Min(RectStartPoint.X, tempEndPoint.X),
                Math.Min(RectStartPoint.Y, tempEndPoint.Y));
            Rect.Size = new Size(
                Math.Abs(RectStartPoint.X - tempEndPoint.X),
                Math.Abs(RectStartPoint.Y - tempEndPoint.Y));
            pictureBox1.Invalidate();

            label1.Text = Rect.ToString();

            

            
        }

        private int _ZoomFactor;
        private Color _BackColor = Color.Black;

        public Bitmap GetAreaFromScreen(Rectangle area)
        {
            var pt = pictureBox1.Location;
            pt = pictureBox1.PointToScreen(pt);
            var rect = new Rectangle(pt.X + area.X, pt.Y + area.Y, area.Width, area.Height);

            var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            ;

            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            return bmp;
        }

        private void UpdateZoomedImage(MouseEventArgs e)
        {
            if (pictureBox1.Image == null)
                return;

            // Calculate the width and height of the portion of the image we want
            // to show in the picZoom picturebox. This value changes when the zoom
            // factor is changed.
            int zoomWidth = picZoom.Width / _ZoomFactor;
            int zoomHeight = picZoom.Height / _ZoomFactor;


            // Calculate the horizontal and vertical midpoints for the crosshair
            // cursor and correct centering of the new image
            int halfWidth = zoomWidth / _ZoomFactor;
            int halfHeight = zoomHeight / _ZoomFactor;

            // Create a new temporary bitmap to fit inside the picZoom picturebox
            Bitmap tempBitmap = new Bitmap(zoomWidth, zoomHeight, PixelFormat.Format24bppRgb);

            // Create a temporary Graphics object to work on the bitmap
            Graphics bmGraphics = Graphics.FromImage(tempBitmap);

            // Clear the bitmap with the selected backcolor
            bmGraphics.Clear(_BackColor);

            // Set the interpolation mode
            bmGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw the portion of the main image onto the bitmap
            // The target rectangle is already known now.
            // Here the mouse position of the cursor on the main image is used to
            // cut out a portion of the main image.

            //var image = GetAreaFromScreen(pictureBox1.ClientRectangle);
            //bmGraphics.DrawImage(image,
            //                     new Rectangle(0, 0, zoomWidth, zoomHeight),
            //                     new Rectangle(e.X - halfWidth, e.Y - halfHeight, zoomWidth, zoomHeight),
            //                     GraphicsUnit.Pixel);

            
            bmGraphics.DrawImage(pictureBox1.Image,
                                 new Rectangle(0, 0, zoomWidth, zoomHeight),
                                 new Rectangle(e.X - halfWidth, e.Y - halfHeight, zoomWidth, zoomHeight),
                                 GraphicsUnit.Pixel);

            // Draw the bitmap on the picZoom picturebox
            picZoom.Image = tempBitmap;

            // Draw a crosshair on the bitmap to simulate the cursor position
            //bmGraphics.DrawLine(Pens.White, halfWidth + 1, halfHeight - 4, halfWidth + 1, halfHeight - 1);
            //bmGraphics.DrawLine(Pens.White, halfWidth + 1, halfHeight + 6, halfWidth + 1, halfHeight + 3);
            //bmGraphics.DrawLine(Pens.White, halfWidth - 4, halfHeight + 1, halfWidth - 1, halfHeight + 1);
            //bmGraphics.DrawLine(Pens.White, halfWidth + 6, halfHeight + 1, halfWidth + 3, halfHeight + 1);

            bmGraphics.DrawLine(Pens.White, halfWidth, 0, halfWidth, halfHeight * _ZoomFactor);
            bmGraphics.DrawLine(Pens.White, 0, halfHeight, halfWidth * _ZoomFactor, halfHeight);

            //bmGraphics.DrawLine(Pens.White, zoomWidth, 0, zoomWidth, zoomHeight * 2);
            //bmGraphics.DrawLine(Pens.White, 0, zoomHeight, zoomWidth * 2, zoomHeight);
            //bmGraphics.DrawLine(Pens.White, halfWidth - 4, halfHeight + 1, halfWidth - 1, halfHeight + 1);
            //bmGraphics.DrawLine(Pens.White, halfWidth + 6, halfHeight + 1, halfWidth + 3, halfHeight + 1);

            ////Draw rectangle
            //if (e.Button != MouseButtons.Left)
            //{
            //    _startPoint = Point.Empty;
            //    _startPoint.X = e.X;
            //    _startPoint.Y = e.Y;
            //}

            //if (e.Button == MouseButtons.Left)
            //{
            //    var currentPoint = Point.Empty;
            //    currentPoint.X = e.X;
            //    currentPoint.Y = e.Y;

            //    var begPoint = Point.Empty;

            //    if (currentPoint.X - _startPoint.X > picZoom.Width)
            //        begPoint.X = 0;
            //    else
            //        begPoint.X = currentPoint.X - _startPoint.X;

            //    if (currentPoint.Y - _startPoint.Y > picZoom.Height/2)
            //        begPoint.Y = 0;
            //    else
            //        begPoint.Y = currentPoint.Y - _startPoint.Y;

            //    var rect = new Rectangle(begPoint, new Size(picZoom.Width / 2, picZoom.Height/2));
            //    if (rect != Rectangle.Empty && rect.Width > 0 && rect.Height > 0)
            //    {
            //        bmGraphics.FillRectangle(selectionBrush, rect);
            //    }
            //}

            //if (pictureBox1.Image != null)
            //{
            //    if (Rect != null && Rect.Width > 0 && Rect.Height > 0)
            //    {
            //        bmGraphics.FillRectangle(selectionBrush, Rect);
            //    }
            //}

            // Dispose of the Graphics object
            bmGraphics.Dispose();

            // Refresh the picZoom picturebox to reflect the changes
            picZoom.Refresh();
        }

        private Point _startPoint;
        // Draw Area
        //
        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            

            // Draw the rectangle...
            if (pictureBox1.Image != null)
            {
                if (Rect != null && Rect.Width > 0 && Rect.Height > 0)
                {
                    e.Graphics.FillRectangle(selectionBrush, Rect);
                }
            }

            //if (cbZoom.Checked)
            //{
            //    var pt = Cursor.Position;
            //    var ptNew = pictureBox1.PointToClient(pt);
            //    UpdateZoomedImage(new MouseEventArgs(MouseButtons.None, 0, ptNew.X, ptNew.Y, 0));
            //}
        }

        private void pictureBox1_MouseUp_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (Rect.Contains(e.Location))
                {
                    Debug.WriteLine("Right click");
                }
            }
        }


        private Bitmap CaptureScreenShot()
        {
            // get the bounding area of the screen containing (0,0)
            // remember in a multidisplay environment you don't know which display holds this point
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            // create the bitmap to copy the screen shot to
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            // now copy the screen image to the graphics device from the bitmap
            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                gr.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }

            return bitmap;
        }

        private void tbZoomScale_ValueChanged(object sender, EventArgs e)
        {
            _ZoomFactor = tbZoomScale.Value;
            tbZoomScale.Text = string.Format("x{0}", _ZoomFactor);
        }

        private void btSelectDir_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbFile.Text = openFileDialog1.FileName;
                var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
                SetImage(image);
            }
        }

       
        
    }
}
