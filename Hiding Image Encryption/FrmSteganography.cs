using System;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.IO;


namespace Text2Image
{
    public partial class FrmSteganography : Form
    {
        public FrmSteganography()
        {
            InitializeComponent();
            Encrypt_btn.Visible = false;
            label16.Visible = false;
            label17.Visible = false;
          

        }

        //public values
        string ImagePath, FilePath, saveImage;
        int height, width;
        long fileSize, fileNameSize;
        Image loadTrueImage, loadSecretImage,AfterEncryption;
        Bitmap TrueBitmap;
        Rectangle showImage = new Rectangle(900,85,140,140);
        Rectangle showSecretImage = new Rectangle(900, 250, 140, 140);//x,y,greater the number moves right,greater the number moves down,hight and width of the images
        bool canPaint = false, EncriptionDone = false;
        byte[] fileContainer;
        string hash = "@sfhg";

        private void EnImageBrowse_btn_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try {

                    ImagePath = openFileDialog1.FileName;

                    EnImage_tbx.Text = ImagePath;
                    loadTrueImage = Image.FromFile(ImagePath);
                    height = loadTrueImage.Height;
                    width = loadTrueImage.Width;
                    TrueBitmap = new Bitmap(loadTrueImage);
                    //File Info 
                    FileInfo imginf = new FileInfo(ImagePath);
                    float fs = (float)imginf.Length / 1024;
                    ImageSize_lbl.Text = smalldecimal(fs.ToString(), 2) + " KB";
                    ImageHeight_lbl.Text = loadTrueImage.Height.ToString() + " Pixel";
                    ImageWidth_lbl.Text = loadTrueImage.Width.ToString() + " Pixel";
                    //storing capacity
                    double cansave = (8.0 * ((height * (width / 3) * 3) / 3 - 1)) / 1024;
                    CanSave_lbl.Text = smalldecimal(cansave.ToString(), 2) + " KB";

                    canPaint = true;
                    //draw Image1
                    this.Invalidate();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Select Image");
                }
                }
        }

        private string smalldecimal(string inp, int dec)
        {
            int i;
            for (i = inp.Length - 1; i > 0; i--)
                if (inp[i] == '.')
                   break;
            try
            {
                return inp.Substring(0, i + dec + 1);
            }
            catch
            {
                return inp;
            }
        }

        private void EnFileBrowse_btn_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                FilePath = openFileDialog2.FileName;
              
                EnFile_tbx.Text = FilePath;
                loadSecretImage = Image.FromFile(FilePath);
                height = loadSecretImage.Height;
                width = loadSecretImage.Width;
                TrueBitmap = new Bitmap(loadSecretImage);
                //get File Path 
                FileInfo finfo = new FileInfo(FilePath);
                fileSize = finfo.Length;
                fileNameSize = FName(FilePath).Length;
                canPaint =true;
                //draw Image2
                this.Invalidate();
            }
        }

        private void Encrypt_btn_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveImage = saveFileDialog1.FileName;
            }
            else
                return;
            if (EnImage_tbx.Text == "" || EnFile_tbx.Text == "")
            {
                MessageBox.Show("Fields are Empty...!");
            }
            if (8*((height * (width/3)*3)/3 - 1) < fileSize + fileNameSize)
            {
                MessageBox.Show("File size is too large!\nPlease use a larger image to hide this file.");
                return;
            }
            fileContainer = File.ReadAllBytes(FilePath);
            EncryptLayer();
            label14.Visible = false;

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void EnImage_tbx_TextChanged(object sender, EventArgs e)
        {

        }

        private void EncryptLayer()
        {
            toolStripStatusLabel1.Text ="Encrypting... Please wait";
            Application.DoEvents();
            long FSize = fileSize;
            Bitmap changedBitmap = EncryptLayer(8, TrueBitmap, 0, (height * (width/3)*3) / 3 - fileNameSize - 1, true);

            FSize -= (height * (width / 3) * 3) / 3 - fileNameSize - 1;
            if(FSize > 0)

            {
                for (int i = 7; i >= 0 && FSize > 0; i--)
                {
                    changedBitmap = EncryptLayer(i, changedBitmap, (((8 - i) * height * (width / 3) * 3) / 3 - fileNameSize - (8 - i)), (((9 - i) * height * (width / 3) * 3) / 3 - fileNameSize - (9 - i)), false);
                    FSize -= (height * (width / 3) * 3) / 3 - 1;
                }
            }
            changedBitmap.Save(saveImage);
            toolStripStatusLabel1.Text = "Encryption Done image has been successfully saved.";
            EncriptionDone = true;
            AfterEncryption = Image.FromFile(saveImage);
            this.Invalidate();
        }


        private Bitmap EncryptLayer(int layer, Bitmap inputBitmap, long startPosition, long endPosition, bool writeFileName)
        {
            Bitmap outputBitmap = inputBitmap;
            layer--;
            int i = 0, j = 0;
            long Size = 0;
            bool[] t = new bool[8];
            bool[] rb = new bool[8];
            bool[] gb = new bool[8];
            bool[] bb = new bool[8];
            Color pixel = new Color();
            byte r, g, b;

            if (writeFileName)
                
            {
                Size = fileNameSize;
                string fileName = FName(FilePath);

                //write fileName:
                for (i = 0; i < height && i * (height / 3) < fileNameSize; i++)
                    for (j = 0; j < (width / 3) * 3 && i * (height / 3) + (j / 3) < fileNameSize; j++)
                    {
                        bytetobool((byte)fileName[i * (height / 3) + j / 3], ref t);
                        //get pixel of  specified color
                        pixel = inputBitmap.GetPixel(j, i);
                        r = pixel.R;
                        g = pixel.G;
                        //rb=pixerl.B;

                        b = pixel.B;
                        bytetobool(r, ref rb);
                        bytetobool(g, ref gb);
                        bytetobool(b, ref bb);
                        //row1 change bit
                        if (j % 3 == 0)
                        {
                            rb[7] = t[0];

                            gb[7] = t[1];
                            bb[7] = t[2];
                        }
                        //row2 change bit
                        else if (j % 3 == 1)
                        {
                            rb[7] = t[3];
                            gb[7] = t[4];

                            bb[7] = t[5];
                        }

                        else
                        {
                            rb[7] = t[6];
                            gb[7] = t[7];
                        }
                        Color result = Color.FromArgb((int)booltobyte(rb), (int)booltobyte(gb), (int)booltobyte(bb));
                        outputBitmap.SetPixel(j, i, result);
                    }
                i--;
            }
            /*
private void KeyProcess()
{
String s1;
byte[] data = UTF8Encoding.UTF8.GetBytes(textBox1.Text);//
using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())

}

}
*/
            // file after file name
            int tempj = j;

            for (; i < height && i * (height / 3) < endPosition - startPosition + Size && startPosition + i * (height / 3) < fileSize + Size; i++)
                for ( j = 0; j < (width / 3) * 3 && i * (height / 3) + (j / 3) < endPosition - startPosition + Size && startPosition + i * (height / 3) + (j / 3) < fileSize + Size; j++)
                {

                    if (tempj != 0)
                    {
                        j = tempj;
                        tempj = 0;
                    }


                    bytetobool((byte)fileContainer[startPosition + i * (height / 3) + j / 3 - Size], ref t);
                    pixel = inputBitmap.GetPixel(j, i);
                    r = pixel.R;

                    g = pixel.G;
                    b = pixel.B;
                    bytetobool(r, ref rb);
                    bytetobool(g, ref gb);
                    bytetobool(b, ref bb);
                    if (j % 3 == 0)
                    {
                        rb[layer] = t[0];

                        gb[layer] = t[1];
                        bb[layer] = t[2];
                        
                    }
                    else if (j % 3 == 1)
                    {
                        rb[layer] = t[3];
                        gb[layer] = t[4];
                        bb[layer] = t[5];
                    }
                    else
                    {
                        rb[layer] = t[6];
                        gb[layer] = t[7];
                    }
                    Color result = Color.FromArgb((int)booltobyte(rb), (int)booltobyte(gb), (int)booltobyte(bb));
                    outputBitmap.SetPixel(j, i, result);

                }


            long temp1 = fileSize, temp2 = fileNameSize;
            r = (byte)(temp1 % 100);
            temp1 /= 100;
            g = (byte)(temp1 % 100);
            temp1 /= 100;
            b = (byte)(temp1 % 100);
            Color flenColor = Color.FromArgb(r,g,b);
            outputBitmap.SetPixel(width - 1, height - 1, flenColor);

            r = (byte)(temp2 % 100);
            temp2 /= 100;
            g = (byte)(temp2 % 100);
            temp2 /= 100;
            b = (byte)(temp1 % 100);
            Color fnlenColor = Color.FromArgb(r,g,b);
            outputBitmap.SetPixel(width - 2, height - 1, fnlenColor);

            return outputBitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            EnImage_tbx.Text = "";
            textBox1.Text = "";
            EnFile_tbx.Text = "";
            label16.Visible = false;
            label17.Visible=false;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Enter Password or Secret Key to Encrypt...");
            }
            else { 
                byte[] data = UTF8Encoding.UTF8.GetBytes(textBox1.Text);//
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                    using (TripleDESCryptoServiceProvider triples = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                    {
                        ICryptoTransform transform = triples.CreateEncryptor();
                        byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                        //display in label 
                        label16.Text = Convert.ToBase64String(results, 0, results.Length);
                        label16.Visible = true;
                        label17.Visible = true;
                        Encrypt_btn.Visible = true;
                    }
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void FrmSteganography_Load(object sender, EventArgs e)
        {
            
        }

        private void bytetobool(byte inp, ref bool[] outp)
        {
            if(inp>=0 && inp<=255)
                for (short i = 7; i >= 0; i--)
                {
                    if (inp % 2 == 1)
                        outp[i] = true;
                    else
                        outp[i] = false;
                    inp /= 2;
                }
            else
                throw new Exception("Input number is wrong.");
        }

        private byte booltobyte(bool[] inp)
        {
            byte outp = 0;
            for (short i = 7; i >= 0; i--)
            {
                if (inp[i])
                    outp += (byte)Math.Pow(2.0, (double)(7-i));
            }
           
            return outp;
        }
        
       
      

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(canPaint)
                try
                {
                    if (!EncriptionDone)
                        e.Graphics.DrawImage(loadTrueImage, showImage);
                     if (!EncriptionDone)
                        e.Graphics.DrawImage(loadSecretImage, showSecretImage);
                    else
                        e.Graphics.DrawImage(AfterEncryption, showImage);
                }
                catch
                {
                   
                        e.Graphics.DrawImage(loadTrueImage, showImage);
                }
        }
        

        private string FName(string path)
        {
            string output;
            int i;
            if (path.Length == 3)   // i.e: "C:\\"
                return path.Substring(0, 1);
            for (i = path.Length - 1; i > 0; i--)
                if (path[i] == '\\')
                    break;
            output = path.Substring(i + 1);
            return output;
        }

    }
}
