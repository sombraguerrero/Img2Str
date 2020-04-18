using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Imazen.WebP;
namespace Img2Str
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();

        private void Button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                button2.Enabled = true;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Str2Img();
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private int branchConvert()
        {
            FileInfo fileInfo = new FileInfo(textBox1.Text);
            int mode;
            switch (fileInfo.Extension.ToUpper())
            {
                case ".TXT":
                    mode = 2;
                    break;
                case ".WEBP":
                    mode = 3;
                    break;
                case ".WEBT":
                    mode = 4;
                    break;
                default:
                    mode = 1;
                    break;
            }
            button2.Enabled = true;
            return mode;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fn = (string[])e.Data.GetData(DataFormats.FileDrop);
            textBox1.Text = fn[0];
            button2.Enabled = true;
            
        }
        private void Str2Img()
        {
            switch (branchConvert())
            {
                case 1:
                    saveFileDialog1.Title = "Output Text File";
                    saveFileDialog1.DefaultExt = "txt";
                    saveFileDialog1.Filter = @"Base64 text file|*.txt";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                        {
                            sw.Write(Convert.ToBase64String(File.ReadAllBytes(textBox1.Text)));
                        }
                        MessageBox.Show("Written to " + textBox1.Text, "Complete");
                    }
                    break;
                case 2:
                    try
                    {
                        MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(File.ReadAllText(textBox1.Text)));
                        Bitmap bitmap = new Bitmap(memoryStream);
                        saveFileDialog1.Title = "Output Image File";
                        saveFileDialog1.DefaultExt = "webp";
                        saveFileDialog1.Filter = @"Webp image file|*.webp";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            using (FileStream fs = File.Create(saveFileDialog1.FileName))
                            {
                                SimpleEncoder simpleEncoder = new SimpleEncoder();
                                simpleEncoder.Encode(bitmap, fs, 85);
                            }
                            MessageBox.Show("Saved to " + saveFileDialog1.FileName);
                        }
                        memoryStream.Close();
                    }
                    catch (FormatException f)
                    {
                        MessageBox.Show(f.Message, "Invalid input file.");
                    }
                    break;
                case 3:
                    saveFileDialog1.Title = "Output Text File";
                    saveFileDialog1.DefaultExt = "webt";
                    saveFileDialog1.Filter = @"Webp Base64 text file|*.webt";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                        {
                            //sw.Write(Convert.ToBase64String(File.ReadAllBytes(textBox1.Text)));
                            sw.Write(Convert.ToBase64String(File.ReadAllBytes(textBox1.Text)));
                        }
                        //MessageBox.Show("Written to " + saveFileDialog1.FileName + "p.", "Complete");
                        MessageBox.Show("Written to " + saveFileDialog1.FileName, "Complete");
                    }
                    break;
                case 4:
                    try
                    {
                        byte[] imageData = Convert.FromBase64String(File.ReadAllText(textBox1.Text));
                        SimpleDecoder simpleDecoder = new SimpleDecoder();
                        saveFileDialog1.Title = "Output Image File";
                        saveFileDialog1.DefaultExt = "bmp";
                        saveFileDialog1.Filter = @"Bitmap image file|*.bmp";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bitmap = simpleDecoder.DecodeFromBytes(imageData, imageData.Length);
                            bitmap.Save(saveFileDialog1.FileName);
                            MessageBox.Show("Written to " + saveFileDialog1.FileName, "Complete");
                        }
                    }
                    catch (FormatException f)
                    {
                        MessageBox.Show(f.Message, "Invalid input file.");
                    }
                    break;
            }
        }
    }
}
