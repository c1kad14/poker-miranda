using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TableTester.Properties;
using miranda.tools;

namespace TableTester
{
    public partial class Form1 : Form
    {
        private SynchronizationContext _sync;
        public Form1()
        {
            InitializeComponent();
            _sync = SynchronizationContext.Current;
            HookManager.MouseMove += new MouseEventHandler(HookManager_MouseMove);

            tbDirTestFiles.Text = Settings.Default.TestFolder;
            if (File.Exists(Application.StartupPath + @"\blank.jpg"))
            {
                var image = Bitmap.FromFile(Application.StartupPath + @"\blank.jpg") as Bitmap;
                pictureBox1.Image = image;
            }
            this.Location = new Point(0, 0);
        }

        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            this.Text = e.Location.ToString();
        }

        private void btnSelectTestDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDirTestFiles.Text = folderBrowserDialog1.SelectedPath;
                Settings.Default.TestFolder = tbDirTestFiles.Text;
                Settings.Default.Save();
            }
        }

        private void btnNextFile_Click(object sender, EventArgs e)
        {
            var di = new DirectoryInfo(tbDirTestFiles.Text);

            if (_files.Count == 0)
            {
                var files = new List<FileInfo>(di.GetFiles("*.bmp"));
                files.AddRange(di.GetFiles("*.jpeg"));

                var orderedFiles = files.OrderByDescending(f => f.LastWriteTime);
                foreach (var fileInfo in orderedFiles)
                {
                    _files.Push(fileInfo);
                }
            }

            var fi = _files.Pop();
            lblCurrentFile.Text = fi.Name;
            var image = Bitmap.FromFile(fi.FullName) as Bitmap;
            pictureBox1.Image = image;
        }

        Stack<FileInfo> _files = new Stack<FileInfo>();

        private void btnRenew_Click(object sender, EventArgs e)
        {
            _files.Clear();
        }

        private Point _pt;
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var args = (MouseEventArgs) e;
            textBox1.Text += args.X + ", " + args.Y + ", " + args.Button + Environment.NewLine;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
            _pt = args.Location;
            pictureBox1.Invalidate();
            //var tr = new Thread(() =>
            //    {
            //        Thread.Sleep(1500);
            //        _pt = Point.Empty;
            //        _sync.Send(state => pictureBox1.Invalidate(), null);
            //    });
            //tr.Start();
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.KeyCode == Keys.NumPad0)
            textBox1.Text += e.KeyCode + Environment.NewLine;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();

            if (e.KeyCode == Keys.F2)
            {
                btnNextFile_Click(this, EventArgs.Empty);
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (_pt != Point.Empty)
            {
                var pen = new Pen(Color.Red);
                pen.Width = 2;

                
                e.Graphics.DrawLine(pen, _pt, _pt + new Size(10, 0));
                e.Graphics.DrawLine(pen, _pt, _pt - new Size(10, 0));
                e.Graphics.DrawLine(pen, _pt, _pt + new Size(0, 10));
                e.Graphics.DrawLine(pen, _pt, _pt - new Size(0, 10));
                
                //e.Graphics.FillRectangle(selectionBrush, Rect);
            }

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
