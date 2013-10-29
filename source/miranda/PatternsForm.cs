using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace miranda
{
    public partial class PatternsForm : Form
    {
        public PatternsForm()
        {
            InitializeComponent();
            _ZoomFactor = tbZoomScale.Value;
        }

        private Rectangle _rect;
        private int _ZoomFactor;
        private Color _BackColor = Color.Black;
        private string _lastFileName = "";
        private void btnSelectPicture_Click(object sender, EventArgs e)
        {
            var f = new AreaSelectorForm(Rectangle.Empty);
            f.Text = "Select rect";

            if (_lastFileName != "")
            {
                var image = Bitmap.FromFile(_lastFileName) as Bitmap;
                f.SetImage(image);
            }
            if (f.ShowDialog() == DialogResult.OK)
            {
                _rect = f.Rect;
                pbSelectedPicture.Image = f.GetSelectedPicture();
                _lastFileName = f.SelectedImageFileName;
                lblLastFileName.Text = "last file: " + _lastFileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var source = pbSelectedPicture.Image as Bitmap;
            var seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);  //First add  grayScaling filter
            seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            var temp = seq.Apply(source);
            pbProcessedPicture.Image = temp;

        }

        private void tbZoomScale_ValueChanged(object sender, EventArgs e)
        {
            _ZoomFactor = tbZoomScale.Value;
            tbZoomScale.Text = string.Format("x{0}", _ZoomFactor);
        }

        private void UpdateZoomedImage(MouseEventArgs e, PictureBox pictureBox)
        {
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


            bmGraphics.DrawImage(pictureBox.Image,
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

        private void pbSelectedPicture_MouseMove(object sender, MouseEventArgs e)
        {
            if (((PictureBox) sender).Image != null)
                UpdateZoomedImage(e, (PictureBox) sender);
        }

        private void pbProcessedPicture_MouseMove(object sender, MouseEventArgs e)
        {
            if (((PictureBox)sender).Image != null)
                UpdateZoomedImage(e, (PictureBox)sender);
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pbProcessedPicture.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
        }
        
    }
}
