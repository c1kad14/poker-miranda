using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TableLogCleaner
{
    public partial class MainForm : Form
    {
        private SynchronizationContext _sync;
        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            _sync = SynchronizationContext.Current;
        }

        private void LoadSettings()
        {
            _settings = AppSettingsManager.Load("none");

            tbFile.Text = _settings.TemplateFile;
            tbDirTestFiles.Text = _settings.TestFolder;


            foreach (var coord in _settings.AllCoords)
            {
                var newRow = (DataGridViewRow)dataGridView1.RowTemplate.Clone();
                newRow.CreateCells(dataGridView1, new object[] { coord.Id, coord.Value, "" });
                dataGridView1.Rows.Add(newRow);
            }
        }

        private AppSettings _settings;
        Stack<FileInfo> _files = new Stack<FileInfo>();

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["btn"].Index)
            {
                var row = dataGridView1.Rows[e.RowIndex];

                if (row.IsNewRow)
                {
                    var f = new AreaSelectorForm(Rectangle.Empty);
                    f.Text = "Select rect";
                    var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
                    f.SetImage(image);

                    var maxId = _settings.AllCoords.Count;
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        var newRow = (DataGridViewRow)dataGridView1.RowTemplate.Clone();
                        newRow.CreateCells(dataGridView1, new object[]{maxId, f.Rect, ""});
                        dataGridView1.Rows.Add(newRow);

                        _settings.AllCoords.Add(new Coord{Id = maxId, Value = f.Rect});
                        AppSettingsManager.Save(_settings);
                        
                    }
                }
                else
                {
                    var oldRect = (Rectangle) row.Cells["Coords"].Value;
                    var id = (int)row.Cells["id"].Value;
                    var f = new AreaSelectorForm(oldRect);
                    f.Text = "Select rect";
                    var image = Bitmap.FromFile(tbFile.Text) as Bitmap;
                    f.SetImage(image);
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        row.Cells["Coords"].Value = f.Rect;

                        var item = _settings.AllCoords.Find(coord => coord.Id == id);
                        item.Value = f.Rect;
                        AppSettingsManager.Save(_settings);

                    }
                }

                
            }
        }

        private void btSelectDir_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbFile.Text = openFileDialog1.FileName;
                _settings.TemplateFile = tbFile.Text;
                AppSettingsManager.Save(_settings);
            }
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            var row = e.Row;
            _settings.AllCoords.RemoveAll(coord => coord.Id == (int)row.Cells["id"].Value);
            AppSettingsManager.Save(_settings);
        }

        private void btnSelectTestDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDirTestFiles.Text = folderBrowserDialog1.SelectedPath;
                _settings.TestFolder = tbDirTestFiles.Text;
                AppSettingsManager.Save(_settings);
            }
        }

        private void btnNextFile_Click(object sender, EventArgs e)
        {

            var di = new DirectoryInfo(tbDirTestFiles.Text);

            if (_files.Count == 0)
            {
                var files = new List<FileInfo>(di.GetFiles("*.jpeg"));
                files.AddRange(di.GetFiles("*.bmp"));
                var orderedFiles = files.OrderByDescending(f => f.LastWriteTime);
                //files.Sort((info, fileInfo) => fileInfo.Name.CompareTo(info.Name));

                foreach (var fileInfo in orderedFiles)
                {
                    _files.Push(fileInfo);
                }
            }

            var fi = _files.Pop();

            lblCurrentFile.Text = fi.Name;
            var image = Bitmap.FromFile(fi.FullName) as Bitmap;
            var imgProcessed = ProcessImage(image, null);
            pictureBox1.Image = imgProcessed;
        }

        private Image ProcessImage(Bitmap image, string fileName)
        {
            using (var g = Graphics.FromImage(image))
            {
                foreach (var coord in _settings.AllCoords)
                {
                    g.FillRectangle(Brushes.White, coord.Value);
                }

                if(!string.IsNullOrEmpty(fileName))
                    image.Save(fileName, ImageFormat.Jpeg);

                return image;
            }
            
        }

        private void btnRenew_Click(object sender, EventArgs e)
        {
            _files.Clear();
        }

        private void btnProcessAll_Click(object sender, EventArgs e)
        {
            var tr = new Thread(obj => ProcessAll((string)obj));
            tr.Start(tbDirTestFiles.Text);
        }

        private void ProcessAll(string path)
        {
            var di = new DirectoryInfo(path);

            var files = new List<FileInfo>(di.GetFiles("*.jpeg"));
            files.AddRange(di.GetFiles("*.bmp"));
            var orderedFiles = files.OrderByDescending(f => f.LastWriteTime);
            //files.Sort((info, fileInfo) => fileInfo.Name.CompareTo(info.Name));

            int i = 0;
            while (Directory.Exists(di.FullName + "_" + i))
            {
                i++;
            }

            Directory.CreateDirectory(di.FullName + "_" + i);

            foreach (var fi in orderedFiles)
            {
                using (var image = Bitmap.FromFile(fi.FullName) as Bitmap)
                {
                    var newName = di.FullName + "_" + i + @"\" + fi.Name;
                    ProcessImage(image, newName);
                }
                _sync.Send(state => lblCurrentFile.Text = fi.Name, null);
                GC.Collect();
            }
        }

        private void btnGetDefault_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show("Overwrite settings with default values?", "Info", MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button3) == DialogResult.Yes)
            {
                var txt = "none";
                var file = Application.StartupPath + @"\default-settings\" + txt + ".xml";
                if (File.Exists(file))
                {
                    File.Copy(file, Application.StartupPath + @"\" + txt + ".xml", true);
                    LoadSettings();
                }
            }
        }
    }
}
