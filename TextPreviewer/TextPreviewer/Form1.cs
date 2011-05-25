using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TextPreviewer
{
    public partial class Form1 : Form
    {
        const string appName = "TextPreviewer";
        int cur_x = 0;
        int cur_y = 4;
        Image fontImage = null;
        Image textboxImage = null;
        Bitmap finalImg = null;

        byte[] WidthFile;
        Dictionary<string, string> table;
        Encoding encoding = Encoding.GetEncoding(932); //SJIS

        int numNewLines = 0;
        int curLine = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private int GetCharacter(char character)
        {
            /*for (int i = 0; i < table.Keys.Count; i++)
            {
                string j = i.ToString("X").PadLeft(2, '0');
                if (table[j] == character.ToString())
                {
                    return i;
                }
            }*/

            try
            {
                return Convert.ToInt32(table[character.ToString()],16);
            }
            catch
            {
                //System.Diagnostics.Trace.WriteLine("Byte not found in table: " + j);
                //tmp += "{" + j + "}";
                MessageBox.Show("Byte not found in table: " + character, appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //return "Error";
            }
            return 0;
        }

        private int GetCharacterWidth(int character)
        {
            return WidthFile[character];
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        private void DrawText()
        {
            string text = "";

            if (textBox1.Lines.Length > curLine)
            {
                text += textBox1.Lines[curLine];
            }

            if (textBox1.Lines.Length > curLine + 1)
            {
                text += Environment.NewLine;
                text += textBox1.Lines[curLine + 1];
            }

            if (textBox1.Lines.Length > (curLine + 2))
            {
                button2.Enabled = true;
            }

            GraphicsUnit units = GraphicsUnit.Pixel;
            // Draw image to screen.
            Graphics gfx = Graphics.FromImage(finalImg);

            //Textbox stuff
            Rectangle tdRect = new Rectangle(0, 0, 206, 40);
            // Create rectangle for source image.
            Rectangle tsRect = new Rectangle(0, 0, 206, 40);
            gfx.DrawImage(textboxImage, tdRect, tsRect, units);

            cur_x = 15;
            cur_y = 8;
            text = text.Replace("<LINE>", "");
            text = text.Replace("<END>", "");
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' && text[i+1] == '\n') //New Line
                {
                    cur_x = 15;
                    cur_y += 16;
                    i += 2;

                    if (i >= text.Length) return;
                }

                if (cur_x < 191)
                {
                    int cur_char = GetCharacter(text[i]);
                    int cur_width = GetCharacterWidth(cur_char);
                    int src_x = (cur_char % 0x10) * 8;
                    int src_y = (cur_char / 0x10) * 8;
                    // Create rectangle for displaying image.
                    Rectangle destRect = new Rectangle(cur_x, cur_y, cur_width, 8);
                    // Create rectangle for source image.
                    Rectangle srcRect = new Rectangle(src_x, src_y, cur_width, 8);

                    gfx.DrawImage(fontImage, destRect, srcRect, units);

                    cur_x += cur_width;

                }
                //else
                //    MessageBox.Show("The line is too long.");
            }

            pictureBox1.Image = ResizeImage(finalImg, finalImg.Width * 3, finalImg.Height * 3);
        }

        /// <summary>
        /// Function to get byte array from a file
        /// </summary>
        /// <param name="_FileName">File name to get byte array</param>
        /// <returns>Byte Array</returns>
        public byte[] FileToByteArray(string _FileName)
        {
            byte[] _Buffer = null;

            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                // attach filestream to binary reader
                System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);

                // get total byte length of the file
                long _TotalBytes = new System.IO.FileInfo(_FileName).Length;

                // read entire file into buffer
                _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);

                // close file reader
                _FileStream.Close();
                _FileStream.Dispose();
                _BinaryReader.Close();
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            return _Buffer;
        }


        bool loadTable(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs, encoding);
                string[] lines = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                table = new Dictionary<string, string>();
                int cur_line = 0;
                foreach (string str in lines)
                {
                    if (str == "") break;

                    int i = str.IndexOf("=");
                    string sByte;
                    string sVal;
                    if (i >= 0)
                    {
                        sByte = str.Substring(0, i).ToUpper();
                        sVal = str.Substring(i + 1);
                        //System.Diagnostics.Trace.WriteLine(sVal);
                        table.Add(sVal, sByte);
                        cur_line++;
                    }
                    else
                    {
                        MessageBox.Show("Invalid table file.", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        fs.Dispose();
                        return false;
                    }
                }
                fs.Dispose();
                return true;
            }
            catch
            {
                MessageBox.Show("Error accessing file.", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            /*numNewLines = 0;
            int firstIndex = 0;
            while(true)
            {
                int nextIndex = textBox1.Text.IndexOf(Environment.NewLine,firstIndex);
                if (nextIndex == -1) break;

                firstIndex = nextIndex+1;
                numNewLines++;
            }*/
            DrawText();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fontImage = Image.FromFile("font.png");
            textboxImage = Image.FromFile("textbox.png");
            loadTable("melissa8x8.tbl");
            WidthFile = FileToByteArray("smallWidthTable.bin");
            finalImg = new Bitmap(206, 40);

            DrawText();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            curLine -= 2;
            if (curLine < 0)
            {
                curLine = 0;
            }
            DrawText();
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            curLine += 2;
            if (curLine >= textBox1.Lines.Length)
            {
                curLine = textBox1.Lines.Length - 2;
            }
            DrawText();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;

            cur_x = 15;
            cur_y = 8;
            text = text.Replace(Environment.NewLine, " ");
            text = text.Replace("<LINE>", "");
            text = text.Replace("<END>", "");
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\r' && text[i + 1] == '\n') //New Line
                {
                    cur_x = 15;
                    cur_y += 16;
                    i += 2;

                    if (i >= text.Length) return;
                }

                cur_x += GetCharacterWidth(GetCharacter(text[i]));

                if (cur_x > 191)
                {
                    if (i > text.Length) i = text.Length - 1;
                    int lastSpace = text.LastIndexOf(" ", i);
                    text = text.Remove(lastSpace, 1);
                    textBox1.Text = text.Insert(lastSpace, Environment.NewLine);
                    return;
                    //MessageBox.Show("The line is too long.");
                    //return;
                }
            }
        }

        
    }
}
