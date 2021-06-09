using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Imazen.WebP;
namespace Img2Str
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();
        public  enum FileTypes { TxtSrc = 2, WebP = 3, WebP_Text = 4, General = 1 }

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

        private FileTypes branchConvert()
        {
            FileInfo fileInfo = new FileInfo(textBox1.Text);
            FileTypes mode;
            switch (fileInfo.Extension.ToUpper())
            {
                case ".TXT":
                    mode = FileTypes.TxtSrc;
                    break;
                case ".WEBP":
                    mode = FileTypes.WebP;
                    break;
                case ".WEBT":
                    mode = FileTypes.WebP_Text;
                    break;
                default:
                    mode = FileTypes.General;
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
        private async void Str2Img() //method must be asynchronous so we can properly await the Discord post and not block the UI thread (hangs otherwise)
        {
            switch (branchConvert())
            {
                case FileTypes.General:
                    saveFileDialog1.Title = "Output Text File";
                    saveFileDialog1.DefaultExt = "txt";
                    saveFileDialog1.Filter = @"Base64 text file|*.txt";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                        {
                            sw.Write(Convert.ToBase64String(File.ReadAllBytes(textBox1.Text)));
                        }
                        MessageBox.Show("Written to " + saveFileDialog1.FileName, "Complete");
                    }
                    break;
                case FileTypes.TxtSrc:
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
                            memoryStream.Close();
                            if (MessageBox.Show("Do you want to post to Discord?", "#lis-media", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Task<string> postArt = PostImage();
                                await postArt; // must await the task so as not to block the UI thread and cause hanging
                                using (StreamWriter sw = new StreamWriter("fanart_response.json"))
                                {
                                    sw.Write(postArt.Result);
                                }
                            }
                        }
                    }
                    catch (FormatException f)
                    {
                        MessageBox.Show(f.Message, "Invalid input file.");
                    }
                    break;
                case FileTypes.WebP:
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
                case FileTypes.WebP_Text:
                    try
                    {
                        byte[] imageData = Convert.FromBase64String(File.ReadAllText(textBox1.Text));
                        SimpleDecoder simpleDecoder = new SimpleDecoder();
                        saveFileDialog1.Title = "Output Image File";
                        saveFileDialog1.DefaultExt = "webp";
                        saveFileDialog1.Filter = @"WebP image file|*.webp";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bitmap = simpleDecoder.DecodeFromBytes(imageData, imageData.Length);
                            using (FileStream fs = File.Create(saveFileDialog1.FileName))
                            {
                                SimpleEncoder simpleEncoder = new SimpleEncoder();
                                simpleEncoder.Encode(bitmap, fs, 85);
                            }
                            MessageBox.Show("Saved to " + saveFileDialog1.FileName);
                        }
                    }
                    catch (FormatException f)
                    {
                        MessageBox.Show(f.Message, "Invalid input file.");
                    }
                    break;
            }
        }

        private async Task<string> PostImage()
        {
            HttpClient httpClient = new HttpClient();
            string shortname = saveFileDialog1.FileName.Substring(saveFileDialog1.FileName.LastIndexOf('\\') + 1);
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new StringContent("Decoded fanart: " + shortname), "content" },
                { new ByteArrayContent(File.ReadAllBytes(saveFileDialog1.FileName)), "file", shortname }
            };
            HttpResponseMessage response = await httpClient.PostAsync($"https://discordapp.com/api/webhooks/768447672758566919/{Environment.GetEnvironmentVariable("wolfbros_token", EnvironmentVariableTarget.User)}", form);
            httpClient.Dispose();
            try
            {
                return (await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex)
            {
                return $"{ex.Message}{Environment.NewLine}From: {ex.Source}";
            }
        }
    }
}
