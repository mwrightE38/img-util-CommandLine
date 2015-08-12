namespace TifExport
{
    using LibTIFF_NET;
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    public class Form1 : Form
    {
        private ComboBox b_box;
        public static int channel = 0;
        private Container components;
        private CheckBox compress;
        private Button dn;
        public bool drawing;
        public bool drawn;
        private CheckBox enDisplay;
        private FolderBrowserDialog fBD1;
        private CheckBox force8;
        private CheckBox full16;
        private ComboBox g_box;
        public HistoForm histo;
        private CheckBox histo_show;
        private Label label1;
        private Label label2;
        private Label label3;
        public static int max_chan = 0;
        public int mx_down;
        public int mx_up;
        public int my_down;
        public int my_up;
        private OpenFileDialog OFD;
        private OpenFileDialog OFD2;
        private PictureBox pB1;
        private PictureBox pB2;
        private ProgressBar progressBar1;
        private ComboBox r_box;
        private Button Save;
        public Bitmap screen;
        public bool showing_colors;
        public string Source;
        private ListBox statBox;
        public string Target;
        private CheckBox tenBit;
        public string this_file;
        public static IntPtr tiff = IntPtr.Zero;
        private Button up;
        public int v_scaler;
        public int v_x;
        public int v_y;
        private Button view;
        private Button view_color;
        public int view_h;
        public ushort[,,] view_pix;
        public int view_w;

        public unsafe Form1(string[] args)
        {
            string str = null;
            FileVersionInfo versionInfo = null;
            int num2 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            try
            {
                this.this_file = null;
                if (args.GetLength(0) > 0)
                {
                    this.this_file = args[0];
                }
                this.InitializeComponent();
                try
                {
                    versionInfo = FileVersionInfo.GetVersionInfo("TifExport.exe");
                    str = "Ver: " + versionInfo.FileVersion;
                    this.Text = "Tif Export Tool    " + str;
                }
                catch when (?)
                {
                    uint num = 0;
                    int num3 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num2);
                    try
                    {
                        try
                        {
                        }
                        catch when (?)
                        {
                        }
                        return;
                        if (num != 0)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        __CxxUnregisterExceptionObject((void*) num2, (int) num);
                    }
                }
            }
            fault
            {
                base.Dispose(true);
            }
        }

        private void ~Form1()
        {
            if (this.components != null)
            {
                IDisposable components = this.components;
                if (components != null)
                {
                    components.Dispose();
                }
            }
        }

        protected override void Dispose([MarshalAs(UnmanagedType.U1)] bool flag1)
        {
            if (flag1)
            {
                try
                {
                    this.~Form1();
                }
                finally
                {
                    base.Dispose(true);
                }
            }
            else
            {
                base.Dispose(false);
            }
        }

        private void dn_Click(object sender, EventArgs e)
        {
            if (tiff != IntPtr.Zero)
            {
                if (sender == this.dn)
                {
                    if (channel > 0)
                    {
                        channel--;
                    }
                    if (this.screen != null)
                    {
                        this.screen.Dispose();
                    }
                    this.screen = this.GetBitmapFromTIFFHandle(tiff, 0, 0xff);
                    this.pB1.Image = this.screen;
                    this.pB1.Update();
                    this.pB1.Invalidate();
                }
                if (sender == this.up)
                {
                    if (channel < max_chan)
                    {
                        channel++;
                    }
                    if (this.screen != null)
                    {
                        this.screen.Dispose();
                    }
                    this.screen = this.GetBitmapFromTIFFHandle(tiff, 0, 0xff);
                    this.pB1.Image = this.screen;
                    this.pB1.Update();
                    this.pB1.Invalidate();
                }
                this.showing_colors = false;
                if (((this.histo != null) && this.histo.Visible) && ((this.mx_up > this.mx_down) && (this.my_up > this.my_down)))
                {
                    this.histo.SendToHisto(this.mx_down, this.my_down, this.mx_up, this.my_up, this.v_scaler, this.tenBit.Checked, channel);
                }
            }
        }

        private unsafe void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryKey key = null;
            RegistryKey key2 = null;
            RegistryKey key3 = null;
            string str = null;
            string str2 = null;
            int num2 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            key3 = Registry.CurrentUser.CreateSubKey("Software");
            key2 = key3.CreateSubKey("Tetracam");
            key = key2.CreateSubKey("TifExport");
            str2 = base.Left.ToString();
            key.SetValue("Left", str2);
            str = base.Top.ToString();
            key.SetValue("Top", str);
            key.SetValue("Target", this.Target);
            key.SetValue("Source", this.Source);
            key.SetValue("EnableDisplay", this.enDisplay.Checked);
            key.SetValue("Force8", this.force8.Checked);
            key.SetValue("Compress", this.compress.Checked);
            key.SetValue("TenBit", this.tenBit.Checked);
            key.SetValue("StretchBits", this.full16.Checked);
            key.Close();
            key2.Close();
            key3.Close();
            try
            {
                if (tiff != IntPtr.Zero)
                {
                    LibTIFF.Close(tiff);
                    tiff = IntPtr.Zero;
                    this.view_pix = null;
                }
                if (this.pB1.Image != null)
                {
                    this.pB1.Image.Dispose();
                    this.pB1.Image = null;
                }
                if (this.screen != null)
                {
                    this.screen.Dispose();
                }
            }
            catch when (?)
            {
                uint num = 0;
                int num3 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num2);
                try
                {
                    try
                    {
                    }
                    catch when (?)
                    {
                    }
                    return;
                    if (num != 0)
                    {
                        throw;
                    }
                }
                finally
                {
                    __CxxUnregisterExceptionObject((void*) num2, (int) num);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            object up = null;
            object dn = null;
            EventArgs args = null;
            EventArgs args2 = null;
            if (e.KeyCode == Keys.Next)
            {
                dn = this.dn;
                this.dn_Click(dn, args2);
            }
            if (e.KeyCode == Keys.PageUp)
            {
                up = this.up;
                this.dn_Click(up, args);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegistryKey key = null;
            string str = null;
            string str2 = null;
            string str3 = null;
            string str4 = null;
            string str5 = null;
            string str6 = null;
            string str7 = null;
            RegistryKey key2 = null;
            RegistryKey key3 = null;
            key3 = Registry.CurrentUser.CreateSubKey("Software");
            key2 = key3.CreateSubKey("Tetracam");
            key = key2.CreateSubKey("TifExport");
            str6 = (string) key.GetValue("Top");
            if (str6 == null)
            {
                str6 = "5";
            }
            str5 = (string) key.GetValue("Left");
            if (str5 == null)
            {
                str5 = "5";
            }
            this.Source = (string) key.GetValue("Source");
            if (this.Source == null)
            {
                this.Source = " ";
            }
            this.OFD.InitialDirectory = this.Source;
            this.Target = (string) key.GetValue("Target");
            if (this.Target == null)
            {
                this.Target = " ";
            }
            this.fBD1.SelectedPath = this.Target;
            str4 = (string) key.GetValue("EnableDisplay");
            if (str4 == null)
            {
                str4 = "false";
            }
            this.enDisplay.Checked = Convert.ToBoolean(str4);
            str3 = (string) key.GetValue("Force8");
            if (str3 == null)
            {
                str3 = "false";
            }
            this.force8.Checked = Convert.ToBoolean(str3);
            str = (string) key.GetValue("Compress");
            if (str == null)
            {
                str = "false";
            }
            this.compress.Checked = Convert.ToBoolean(str);
            str7 = (string) key.GetValue("TenBit");
            if (str7 == null)
            {
                str = "false";
            }
            this.tenBit.Checked = Convert.ToBoolean(str7);
            str2 = (string) key.GetValue("StretchBits");
            if (str2 == null)
            {
                str2 = "false";
            }
            this.full16.Checked = Convert.ToBoolean(str2);
            key.Close();
            key2.Close();
            key3.Close();
            base.Top = Convert.ToInt16(str6);
            base.Left = Convert.ToInt16(str5);
            this.drawing = false;
            this.drawn = false;
            this.showing_colors = false;
            if (this.this_file != null)
            {
                this.view_Click(this.this_file, e);
            }
        }

        private unsafe Bitmap GetBitmapFromTIFFHandle(IntPtr tiff, ushort min, ushort max)
        {
            Bitmap bitmap = null;
            BitmapData bitmapdata = null;
            short[] destination = null;
            byte[] buffer = null;
            ColorPalette palette = null;
            uint val = 0;
            uint num7 = 0;
            ushort num2 = 0;
            ushort num8 = 0;
            int size = LibTIFF.ScanlineSize(tiff);
            LibTIFF.GetField(tiff, 0x100, out val);
            LibTIFF.GetField(tiff, 0x101, out num7);
            LibTIFF.GetField(tiff, 0x102, out num2);
            LibTIFF.GetField(tiff, 0x115, out num8);
            if (((num2 != 8) || (num2 != 0x10)) && (num8 > 12))
            {
                return null;
            }
            switch (num2)
            {
                case 8:
                    buffer = new byte[size * num7];
                    break;

                case 0x10:
                    destination = new short[size * num7];
                    break;
            }
            IntPtr buf = LibTIFF._malloc(size);
            for (uint i = 0; i < num7; i++)
            {
                LibTIFF.ReadScanline(tiff, buf, i);
                switch (num2)
                {
                    case 8:
                        Marshal.Copy(buf, buffer, (int) (i * size), size);
                        break;

                    case 0x10:
                        Marshal.Copy(buf, destination, (int) (i * size), size / 2);
                        break;
                }
            }
            LibTIFF._free(buf);
            bitmap = new Bitmap((int) val, (int) num7, PixelFormat.Format8bppIndexed);
            int num10 = 1;
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int num9 = 8;
            if (this.tenBit.Checked)
            {
                num9 = 2;
            }
            byte* numPtr = (byte*) bitmapdata.Scan0;
            uint num6 = 0;
            while (true)
            {
                if (num6 >= bitmap.Height)
                {
                    bitmap.UnlockBits(bitmapdata);
                    palette = bitmap.Palette;
                    for (int k = 0; k < 0x100; k++)
                    {
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(k, k, k);
                        palette.Entries[k] = color;
                    }
                    bitmap.Palette = palette;
                    this.label1.Text = "Showing band " + channel.ToString();
                    this.label1.Update();
                    buffer = null;
                    destination = null;
                    return bitmap;
                }
                for (uint j = 0; j < bitmap.Width; j++)
                {
                    switch (num2)
                    {
                        case 8:
                            numPtr[0] = buffer[((int) ((j * num8) + (num6 * size))) + channel];
                            break;

                        case 0x10:
                            numPtr[0] = (byte) (destination[(int) (((j * num8) + (num6 * size)) + channel)] >> num9);
                            break;
                    }
                    numPtr += num10;
                }
                numPtr += (byte*) (bitmapdata.Stride - (val * num10));
                num6++;
            }
        }

        private void histo_show_CheckedChanged(object sender, EventArgs e)
        {
            if (tiff != IntPtr.Zero)
            {
                if (this.histo_show.Checked)
                {
                    if ((this.histo != null) && this.histo.Visible)
                    {
                        this.histo.Close();
                    }
                    this.histo = new HistoForm();
                    this.histo.Owner = this;
                    this.histo.Show();
                    this.histo.SendArrayToHisto(this.view_pix);
                    if (!this.showing_colors)
                    {
                        this.histo.SendToHisto(this.mx_down, this.my_down, this.mx_up, this.my_up, this.v_scaler, this.tenBit.Checked, channel);
                    }
                    else
                    {
                        this.histo.SendColorsToHisto(this.mx_down, this.my_down, this.mx_up, this.my_up, this.v_scaler, this.tenBit.Checked, Convert.ToInt32(this.r_box.SelectedIndex), Convert.ToInt32(this.g_box.SelectedIndex), Convert.ToInt32(this.b_box.SelectedIndex));
                    }
                }
                else
                {
                    this.histo.Close();
                }
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = null;
            manager = new ComponentResourceManager(typeof(Form1));
            this.pB1 = new PictureBox();
            this.view = new Button();
            this.OFD = new OpenFileDialog();
            this.Save = new Button();
            this.label1 = new Label();
            this.progressBar1 = new ProgressBar();
            this.enDisplay = new CheckBox();
            this.label3 = new Label();
            this.fBD1 = new FolderBrowserDialog();
            this.force8 = new CheckBox();
            this.dn = new Button();
            this.up = new Button();
            this.view_color = new Button();
            this.OFD2 = new OpenFileDialog();
            this.statBox = new ListBox();
            this.r_box = new ComboBox();
            this.g_box = new ComboBox();
            this.b_box = new ComboBox();
            this.compress = new CheckBox();
            this.tenBit = new CheckBox();
            this.full16 = new CheckBox();
            this.label2 = new Label();
            this.pB2 = new PictureBox();
            this.histo_show = new CheckBox();
            ((ISupportInitialize) this.pB1).BeginInit();
            ((ISupportInitialize) this.pB2).BeginInit();
            base.SuspendLayout();
            this.pB1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            System.Drawing.Color transparent = System.Drawing.Color.Transparent;
            this.pB1.BackColor = transparent;
            this.pB1.Cursor = Cursors.Cross;
            Point point21 = new Point(0xec, 0x18);
            this.pB1.Location = point21;
            Size size26 = new Size(0x500, 0x400);
            this.pB1.MaximumSize = size26;
            Size size25 = new Size(640, 480);
            this.pB1.MinimumSize = size25;
            this.pB1.Name = "pB1";
            Size size24 = new Size(640, 480);
            this.pB1.Size = size24;
            this.pB1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pB1.TabIndex = 0;
            this.pB1.TabStop = false;
            this.pB1.MouseLeave += new EventHandler(this.pB1_MouseLeave);
            this.pB1.MouseMove += new MouseEventHandler(this.pB1_MouseMove);
            this.pB1.MouseDown += new MouseEventHandler(this.pB1_MouseDown);
            this.pB1.Paint += new PaintEventHandler(this.pB1_Paint);
            this.pB1.MouseUp += new MouseEventHandler(this.pB1_MouseUp);
            this.pB1.MouseEnter += new EventHandler(this.pB1_MouseEnter);
            Point point20 = new Point(4, 0x80);
            this.view.Location = point20;
            this.view.Name = "view";
            Size size23 = new Size(0x4c, 0x17);
            this.view.Size = size23;
            this.view.TabIndex = 1;
            this.view.Text = "View File";
            this.view.UseVisualStyleBackColor = true;
            this.view.Click += new EventHandler(this.view_Click);
            this.OFD.InitialDirectory = "Source";
            this.OFD.Multiselect = true;
            Point point19 = new Point(4, 4);
            this.Save.Location = point19;
            this.Save.Name = "Save";
            Size size22 = new Size(0x4b, 0x17);
            this.Save.Size = size22;
            this.Save.TabIndex = 5;
            this.Save.Text = "Convert";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new EventHandler(this.Save_Click);
            Point point18 = new Point(8, 0x108);
            this.label1.Location = point18;
            this.label1.Name = "label1";
            Size size21 = new Size(220, 13);
            this.label1.Size = size21;
            this.label1.TabIndex = 6;
            this.label1.TextAlign = ContentAlignment.MiddleLeft;
            Point point17 = new Point(8, 0x6c);
            this.progressBar1.Location = point17;
            this.progressBar1.Name = "progressBar1";
            Size size20 = new Size(220, 15);
            this.progressBar1.Size = size20;
            this.progressBar1.Step = 1;
            this.progressBar1.Style = ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 7;
            this.progressBar1.Visible = false;
            this.enDisplay.AutoSize = true;
            Point point16 = new Point(13, 0x21);
            this.enDisplay.Location = point16;
            this.enDisplay.Name = "enDisplay";
            Size size19 = new Size(0x60, 0x11);
            this.enDisplay.Size = size19;
            this.enDisplay.TabIndex = 8;
            this.enDisplay.TabStop = false;
            this.enDisplay.Text = "Enable Display";
            this.enDisplay.UseVisualStyleBackColor = true;
            Point point15 = new Point(0xec, 4);
            this.label3.Location = point15;
            this.label3.Name = "label3";
            Size size18 = new Size(640, 0x10);
            this.label3.Size = size18;
            this.label3.TabIndex = 10;
            this.fBD1.Description = "Select or Create a Target Folder";
            this.force8.AutoSize = true;
            Point point14 = new Point(13, 0x34);
            this.force8.Location = point14;
            this.force8.Name = "force8";
            Size size17 = new Size(0x55, 0x11);
            this.force8.Size = size17;
            this.force8.TabIndex = 11;
            this.force8.TabStop = false;
            this.force8.Text = "Force to 8bit";
            this.force8.UseVisualStyleBackColor = true;
            System.Drawing.Color control = SystemColors.Control;
            this.dn.BackColor = control;
            this.dn.BackgroundImageLayout = ImageLayout.Center;
            Point point13 = new Point(4, 0x98);
            this.dn.Location = point13;
            this.dn.Name = "dn";
            Size size16 = new Size(0x24, 0x17);
            this.dn.Size = size16;
            this.dn.TabIndex = 2;
            this.dn.Text = "<";
            this.dn.UseVisualStyleBackColor = true;
            this.dn.Click += new EventHandler(this.dn_Click);
            Point point12 = new Point(0x2c, 0x98);
            this.up.Location = point12;
            this.up.Name = "up";
            Size size15 = new Size(0x24, 0x17);
            this.up.Size = size15;
            this.up.TabIndex = 3;
            this.up.Text = ">";
            this.up.UseVisualStyleBackColor = true;
            this.up.Click += new EventHandler(this.dn_Click);
            Point point11 = new Point(4, 0xb0);
            this.view_color.Location = point11;
            this.view_color.Name = "view_color";
            Size size14 = new Size(0x4c, 0x17);
            this.view_color.Size = size14;
            this.view_color.TabIndex = 4;
            this.view_color.Text = "View Colors";
            this.view_color.UseVisualStyleBackColor = true;
            this.view_color.Click += new EventHandler(this.view_color_Click);
            this.OFD2.InitialDirectory = "Target";
            this.statBox.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.statBox.FormattingEnabled = true;
            this.statBox.HorizontalScrollbar = true;
            Point point10 = new Point(4, 0x180);
            this.statBox.Location = point10;
            this.statBox.Name = "statBox";
            this.statBox.SelectionMode = SelectionMode.None;
            Size size13 = new Size(0xde, 0x93);
            this.statBox.Size = size13;
            this.statBox.TabIndex = 0x12;
            this.statBox.TabStop = false;
            System.Drawing.Color color3 = System.Drawing.Color.FromArgb(0xff, 0x80, 0x80);
            this.r_box.BackColor = color3;
            this.r_box.FormattingEnabled = true;
            Point point9 = new Point(12, 200);
            this.r_box.Location = point9;
            this.r_box.Name = "r_box";
            Size size12 = new Size(0x38, 0x15);
            this.r_box.Size = size12;
            this.r_box.TabIndex = 0x13;
            this.r_box.TabStop = false;
            System.Drawing.Color color2 = System.Drawing.Color.FromArgb(0x80, 0xff, 0x80);
            this.g_box.BackColor = color2;
            this.g_box.FormattingEnabled = true;
            Point point8 = new Point(12, 220);
            this.g_box.Location = point8;
            this.g_box.Name = "g_box";
            Size size11 = new Size(0x38, 0x15);
            this.g_box.Size = size11;
            this.g_box.TabIndex = 20;
            this.g_box.TabStop = false;
            System.Drawing.Color color = System.Drawing.Color.FromArgb(0x80, 0x80, 0xff);
            this.b_box.BackColor = color;
            this.b_box.FormattingEnabled = true;
            Point point7 = new Point(12, 240);
            this.b_box.Location = point7;
            this.b_box.Name = "b_box";
            Size size10 = new Size(0x38, 0x15);
            this.b_box.Size = size10;
            this.b_box.TabIndex = 0x15;
            this.b_box.TabStop = false;
            this.compress.AutoSize = true;
            Point point6 = new Point(13, 70);
            this.compress.Location = point6;
            this.compress.Name = "compress";
            Size size9 = new Size(0x48, 0x11);
            this.compress.Size = size9;
            this.compress.TabIndex = 0x16;
            this.compress.TabStop = false;
            this.compress.Text = "Compress";
            this.compress.UseVisualStyleBackColor = true;
            this.tenBit.AutoSize = true;
            Point point5 = new Point(0x58, 0x9c);
            this.tenBit.Location = point5;
            this.tenBit.Name = "tenBit";
            Size size8 = new Size(0x7c, 0x11);
            this.tenBit.Size = size8;
            this.tenBit.TabIndex = 0x17;
            this.tenBit.TabStop = false;
            this.tenBit.Text = "Display as 10bit data";
            this.tenBit.UseVisualStyleBackColor = true;
            this.tenBit.CheckedChanged += new EventHandler(this.tenBit_CheckedChanged);
            this.full16.AutoSize = true;
            Point point4 = new Point(13, 0x58);
            this.full16.Location = point4;
            this.full16.Name = "full16";
            Size size7 = new Size(0x91, 0x11);
            this.full16.Size = size7;
            this.full16.TabIndex = 0x18;
            this.full16.TabStop = false;
            this.full16.Text = "Stretch 10bits to full word";
            this.full16.UseVisualStyleBackColor = true;
            this.label2.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            Point point3 = new Point(0xec, 0x1ff);
            this.label2.Location = point3;
            this.label2.Name = "label2";
            Size size6 = new Size(640, 0x13);
            this.label2.Size = size6;
            this.label2.TabIndex = 0x19;
            this.label2.TextAlign = ContentAlignment.MiddleLeft;
            this.pB2.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.pB2.BorderStyle = BorderStyle.FixedSingle;
            Point point2 = new Point(4, 280);
            this.pB2.Location = point2;
            this.pB2.Name = "pB2";
            Size size5 = new Size(220, 0x68);
            this.pB2.Size = size5;
            this.pB2.TabIndex = 0x1a;
            this.pB2.TabStop = false;
            this.pB2.Visible = false;
            this.pB2.Paint += new PaintEventHandler(this.pB2_Paint);
            this.histo_show.AutoSize = true;
            Point point = new Point(0x58, 0xb0);
            this.histo_show.Location = point;
            this.histo_show.Name = "histo_show";
            Size size4 = new Size(0x49, 0x11);
            this.histo_show.Size = size4;
            this.histo_show.TabIndex = 0x1b;
            this.histo_show.TabStop = false;
            this.histo_show.Text = "Histogram";
            this.histo_show.UseVisualStyleBackColor = true;
            this.histo_show.CheckedChanged += new EventHandler(this.histo_show_CheckedChanged);
            SizeF ef = new SizeF(96f, 96f);
            base.AutoScaleDimensions = ef;
           // base.AutoScaleMode = AutoScaleMode.Dpi;
            Size size3 = new Size(0x374, 0x216);
            base.ClientSize = size3;
            base.Controls.Add(this.histo_show);
            base.Controls.Add(this.pB2);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.full16);
            base.Controls.Add(this.tenBit);
            base.Controls.Add(this.compress);
            base.Controls.Add(this.b_box);
            base.Controls.Add(this.g_box);
            base.Controls.Add(this.r_box);
            base.Controls.Add(this.statBox);
            base.Controls.Add(this.view_color);
            base.Controls.Add(this.up);
            base.Controls.Add(this.dn);
            base.Controls.Add(this.force8);
            base.Controls.Add(this.label3);
            base.Controls.Add(this.enDisplay);
            base.Controls.Add(this.progressBar1);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.Save);
            base.Controls.Add(this.view);
            base.Controls.Add(this.pB1);
         //   base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.KeyPreview = true;
            Size size2 = new Size(0x5fc, 0x410);
            this.MaximumSize = size2;
            Size size = new Size(0x37c, 560);
            this.MinimumSize = size;
            base.Name = "Form1";
          //  base.SizeGripStyle = SizeGripStyle.Show;
            base.StartPosition = FormStartPosition.Manual;
            this.Text = "TIF Export Tool";
            base.Load += new EventHandler(this.Form1_Load);
            base.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            base.KeyDown += new KeyEventHandler(this.Form1_KeyDown);
            ((ISupportInitialize) this.pB1).EndInit();
            ((ISupportInitialize) this.pB2).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private int Load_pix_array(IntPtr tiff)
        {
            short[] destination = null;
            byte[] buffer = null;
            uint val = 0;
            uint num7 = 0;
            ushort num2 = 0;
            ushort num8 = 0;
            int size = LibTIFF.ScanlineSize(tiff);
            LibTIFF.GetField(tiff, 0x100, out val);
            LibTIFF.GetField(tiff, 0x101, out num7);
            LibTIFF.GetField(tiff, 0x102, out num2);
            LibTIFF.GetField(tiff, 0x115, out num8);
            this.view_w = (int) val;
            this.view_h = (int) num7;
            if (((num2 != 8) || (num2 != 0x10)) && (num8 > 12))
            {
                return 0;
            }
            switch (num2)
            {
                case 8:
                    buffer = new byte[size * num7];
                    break;

                case 0x10:
                    destination = new short[size * num7];
                    break;
            }
            IntPtr buf = LibTIFF._malloc(size);
            for (uint i = 0; i < (num7 - 1); i++)
            {
                LibTIFF.ReadScanline(tiff, buf, i);
                switch (num2)
                {
                    case 8:
                        Marshal.Copy(buf, buffer, (int) (i * size), size);
                        break;

                    case 0x10:
                        Marshal.Copy(buf, destination, (int) (i * size), size / 2);
                        break;
                }
            }
            LibTIFF._free(buf);
            this.view_pix = new ushort[num8, val, num7];
            uint num5 = 0;
        Label_0108:
            if (num5 >= num7)
            {
                int num10;
                buffer = null;
                destination = null;
                return num10;
            }
            uint num4 = 0;
        Label_0116:
            if (num4 >= val)
            {
                num5++;
                goto Label_0108;
            }
            uint num3 = 0;
            while (true)
            {
                if (num3 >= num8)
                {
                    num4++;
                    goto Label_0116;
                }
                switch (num2)
                {
                    case 8:
                        this.view_pix[num3, num4, num5] = buffer[(int) (((num4 * num8) + (num5 * size)) + num3)];
                        break;

                    case 0x10:
                        this.view_pix[num3, num4, num5] = (ushort) destination[(int) (((num4 * num8) + (num5 * size)) + num3)];
                        break;
                }
                num3++;
            }
        }

        private void pB1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(tiff == IntPtr.Zero))
            {
                this.drawn = false;
                this.drawing = true;
                this.mx_down = this.v_x;
                this.my_down = this.v_y;
            }
        }

        private void pB1_MouseEnter(object sender, EventArgs e)
        {
            if (!(tiff == IntPtr.Zero))
            {
                this.pB2.Show();
            }
        }

        private void pB1_MouseLeave(object sender, EventArgs e)
        {
            this.pB2.Hide();
        }

        private void pB1_MouseMove(object sender, MouseEventArgs e)
        {
            string str = null;
            if (tiff != IntPtr.Zero)
            {
                this.v_x = (int) ((((double) e.X) / ((double) this.pB1.Width)) * this.view_w);
                this.v_y = (int) ((((double) e.Y) / ((double) this.pB1.Height)) * this.view_h);
                if (this.v_x < 0)
                {
                    this.v_x = 0;
                }
                if (this.v_y < 0)
                {
                    this.v_y = 0;
                }
                if (this.v_x >= this.view_w)
                {
                    this.v_x = this.view_w - 1;
                }
                if (this.v_y >= this.view_h)
                {
                    this.v_y = this.view_h - 1;
                }
                for (int i = 0; i < (max_chan + 1); i++)
                {
                    str = str + this.view_pix[i, this.v_x, this.v_y].ToString("d4") + ", ";
                }
                string[] strArray = new string[] { str, " @ ", this.v_x.ToString(), ", ", this.v_y.ToString() };
                str = string.Concat(strArray);
                this.label2.Text = str;
                this.label2.Update();
                this.pB2.Invalidate();
                if ((this.drawing && (this.v_x > this.mx_down)) && (this.v_y > this.my_down))
                {
                    this.pB1.Invalidate();
                }
            }
        }

        private void pB1_MouseUp(object sender, MouseEventArgs e)
        {
            if (tiff != IntPtr.Zero)
            {
                this.mx_up = this.v_x;
                this.my_up = this.v_y;
                if ((this.mx_up == this.mx_down) || (this.my_up == this.my_down))
                {
                    this.my_down = 0;
                    this.mx_down = 0;
                    this.mx_up = this.view_w;
                    this.my_up = this.view_h;
                    if (this.drawing)
                    {
                        this.pB1.Invalidate();
                    }
                }
                this.drawing = false;
                this.drawn = true;
                if (((this.histo != null) && this.histo.Visible) && ((this.mx_up > this.mx_down) && (this.my_up > this.my_down)))
                {
                    if (!this.showing_colors)
                    {
                        this.histo.SendToHisto(this.mx_down, this.my_down, this.mx_up, this.my_up, this.v_scaler, this.tenBit.Checked, channel);
                    }
                    else
                    {
                        this.histo.SendColorsToHisto(this.mx_down, this.my_down, this.mx_up, this.my_up, this.v_scaler, this.tenBit.Checked, Convert.ToInt32(this.r_box.SelectedIndex), Convert.ToInt32(this.g_box.SelectedIndex), Convert.ToInt32(this.b_box.SelectedIndex));
                    }
                }
            }
        }

        private void pB1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = null;
            Graphics graphics2 = null;
            if (this.drawing)
            {
                graphics2 = e.Graphics;
                Rectangle rectangle2 = new Rectangle {
                    X = (int) (this.mx_down * (((double) this.pB1.Width) / ((double) this.view_w))),
                    Y = (int) (this.my_down * (((double) this.pB1.Height) / ((double) this.view_h))),
                    Width = (int) ((this.v_x - this.mx_down) * (((double) this.pB1.Width) / ((double) this.view_w))),
                    Height = (int) ((this.v_y - this.my_down) * (((double) this.pB1.Height) / ((double) this.view_h)))
                };
                System.Drawing.Color black = System.Drawing.Color.Black;
                System.Drawing.Color white = System.Drawing.Color.White;
                ControlPaint.DrawFocusRectangle(graphics2, rectangle2, white, black);
            }
            if (this.drawn)
            {
                graphics = e.Graphics;
                Rectangle rectangle = new Rectangle {
                    X = (int) (this.mx_down * (((double) this.pB1.Width) / ((double) this.view_w))),
                    Y = (int) (this.my_down * (((double) this.pB1.Height) / ((double) this.view_h))),
                    Width = (int) ((this.mx_up - this.mx_down) * (((double) this.pB1.Width) / ((double) this.view_w))),
                    Height = (int) ((this.my_up - this.my_down) * (((double) this.pB1.Height) / ((double) this.view_h)))
                };
                System.Drawing.Color backColor = System.Drawing.Color.Black;
                System.Drawing.Color foreColor = System.Drawing.Color.White;
                ControlPaint.DrawFocusRectangle(graphics, rectangle, foreColor, backColor);
            }
        }

        private void pB2_Paint(object sender, PaintEventArgs e)
        {
            float[] numArray = null;
            Pen pen = null;
            int num3;
            Pen pen2 = null;
            Graphics graphics = null;
            if (this.v_scaler == 8)
            {
                num3 = 0x100;
            }
            if (this.v_scaler == 0x10)
            {
                num3 = 0x10000;
            }
            if ((this.v_scaler == 0x10) && this.tenBit.Checked)
            {
                num3 = 0x400;
            }
            graphics = e.Graphics;
            float num4 = (float) (100.0 / ((double) num3));
            float num = 0f;
            pen2 = new Pen(System.Drawing.Color.Red, 1f);
            pen = new Pen(System.Drawing.Color.Black, 1f);
            numArray = new float[] { 4f, 2f };
            pen.DashPattern = numArray;
            for (int i = 0; i < max_chan; i++)
            {
                int num8 = (int) (100.0 - (this.view_pix[i, this.v_x, this.v_y] * num4));
                int num7 = (int) (100.0 - (this.view_pix[i + 1, this.v_x, this.v_y] * num4));
                graphics.DrawLine(pen2, (int) num, num8, ((int) num) + (this.pB2.Width / max_chan), num7);
                num += 220 / max_chan;
                graphics.DrawLine(pen, (int) num, 0, (int) num, 100);
            }
            IDisposable disposable2 = pen2;
            if (disposable2 != null)
            {
                disposable2.Dispose();
            }
            IDisposable disposable = pen;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            numArray = null;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            PropertyItem[] propertyItems = null;
            string[] fileNames = null;
            string serial = null;
            string exp = null;
            string gps = null;
            string date = null;
            string newpath = null;
            Image image = null;
            ASCIIEncoding encoding = null;
            ASCIIEncoding encoding2 = null;
            ASCIIEncoding encoding3 = null;
            ASCIIEncoding encoding4 = null;
            bool flag;
            this.OFD.Filter = "*.TIF|*.TIFF;*.TIF";
            this.OFD.Multiselect = true;
            this.OFD.InitialDirectory = this.Source;
            if (this.OFD.ShowDialog() != DialogResult.OK)
            {
                goto Label_0471;
            }
            this.fBD1.Description = "Select or create destination folder for converted files";
            this.fBD1.ShowNewFolderButton = true;
            this.fBD1.SelectedPath = this.Target;
            this.Source = Path.GetDirectoryName(this.OFD.FileNames[0]);
            if (this.fBD1.ShowDialog() != DialogResult.OK)
            {
                goto Label_0471;
            }
            this.statBox.Items.Clear();
            newpath = this.fBD1.SelectedPath;
            this.Target = newpath;
            if (this.pB1.Image != null)
            {
                this.pB1.Image.Dispose();
                this.pB1.Image = null;
            }
            this.enDisplay.Enabled = false;
            this.label1.Text = " ";
            this.label3.Text = " ";
            Application.DoEvents();
            fileNames = this.OFD.FileNames;
            this.progressBar1.Value = 0;
            this.progressBar1.Maximum = fileNames.Length;
            this.progressBar1.Show();
            int index = 0;
        Label_016D:
            if (index >= fileNames.Length)
            {
                this.label1.Text = "Done Converting Filelist";
                this.label1.Update();
                this.progressBar1.Hide();
                goto Label_0471;
            }
            int num4 = 0;
            date = "dateN/A";
            serial = "serN/A";
            gps = "gpsN/A";
            exp = "exposureN/A";
            try
            {
                image = Image.FromFile(fileNames[index]);
                propertyItems = image.PropertyItems;
                int num = 0;
                goto Label_01AE;
            Label_01AA:
                num++;
            Label_01AE:
                if (num >= propertyItems.Length)
                {
                    goto Label_037B;
                }
                if (propertyItems[num].Id != 0x102)
                {
                    goto Label_01F8;
                }
                int length = propertyItems[num].Value.Length;
                int num3 = 0;
                goto Label_01DB;
            Label_01D5:
                num3++;
            Label_01DB:
                if (num3 < length)
                {
                    num4 += Convert.ToInt32(propertyItems[num].Value[num3]);
                    goto Label_01D5;
                }
            Label_01F8:
                if (propertyItems[num].Id == 0x13b)
                {
                    encoding4 = new ASCIIEncoding();
                    string text1 = encoding4.GetString(propertyItems[num].Value, 0, propertyItems[num].Value.Length);
                    serial = text1.Substring(text1.Length - 8, 7);
                    serial = "ser" + serial;
                    IDisposable disposable4 = encoding4 as IDisposable;
                    if (disposable4 != null)
                    {
                        disposable4.Dispose();
                    }
                }
                if (propertyItems[num].Id == 0x13c)
                {
                    encoding3 = new ASCIIEncoding();
                    exp = encoding3.GetString(propertyItems[num].Value, 0, propertyItems[num].Value.Length);
                    exp = "exp" + exp;
                    IDisposable disposable3 = encoding3 as IDisposable;
                    if (disposable3 != null)
                    {
                        disposable3.Dispose();
                    }
                }
                if (propertyItems[num].Id == 0x9004)
                {
                    encoding2 = new ASCIIEncoding();
                    date = encoding2.GetString(propertyItems[num].Value, 0, propertyItems[num].Value.Length);
                    date = "date" + date;
                    IDisposable disposable2 = encoding2 as IDisposable;
                    if (disposable2 != null)
                    {
                        disposable2.Dispose();
                    }
                }
                if (propertyItems[num].Id == 0x927c)
                {
                    encoding = new ASCIIEncoding();
                    gps = encoding.GetString(propertyItems[num].Value, 0, propertyItems[num].Value.Length);
                    gps = "gps" + gps;
                    IDisposable disposable = encoding as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                goto Label_01AA;
            Label_037B:
                flag = false;
                image.Dispose();
            }
            catch (Exception)
            {
                flag = true;
                image.Dispose();
                goto Label_040B;
            }
            base.Cursor = Cursors.WaitCursor;
            if (this.force8.Checked)
            {
                this.Save8(fileNames[index], newpath, fileNames.Length, index, date, serial, gps, exp);
            }
            else
            {
                if (num4 == 8)
                {
                    this.Save8(fileNames[index], newpath, fileNames.Length, index, date, serial, gps, exp);
                }
                if (num4 > 8)
                {
                    this.Save16(fileNames[index], newpath, fileNames.Length, index, date, serial, gps, exp);
                }
            }
            base.Cursor = Cursors.Default;
        Label_040B:
            if (flag)
            {
                this.label1.Text = Path.GetFileName(fileNames[index]) + " is not a MultiPage file";
                this.label1.Update();
                Sleep(0x7d0);
            }
            index++;
            goto Label_016D;
        Label_0471:
            this.enDisplay.Enabled = true;
        }

        private unsafe void Save16(string filename, string newpath, int length, int pos, string date, string serial, string gps, string exp)
        {
            Bitmap bitmap = null;
            IntPtr zero;
            Image im = null;
            short[] source = null;
            string str = null;
            short[,,] numArray2 = null;
            string val = null;
            IntPtr ptr2;
            BitmapData bitmapdata = null;
            Graphics graphics = null;
            char[] trimChars = null;
            int[] wavelength = null;
            int[] order = null;
            FrameDimension dimension = null;
            string str3 = null;
            int num13 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            im = Image.FromFile(filename);
            int frameCount = 0;
            dimension = new FrameDimension(im.FrameDimensionsList[0]);
            frameCount = im.GetFrameCount(dimension);
            order = new int[frameCount];
            wavelength = new int[frameCount];
            this.SortChannels(im, order, wavelength);
            val = "descTetracam MCA:";
            for (int i = 0; i < frameCount; i++)
            {
                val = val + " " + wavelength[i].ToString();
            }
            trimChars = new char[] { ' ' };
            val.Trim(trimChars);
            date.Trim(trimChars);
            int width = im.Width;
            int height = im.Height;
            numArray2 = new short[frameCount, width, height];
            int index = 0;
            while (true)
            {
                if (index >= frameCount)
                {
                    if (im != null)
                    {
                        im.Dispose();
                        im = null;
                    }
                    this.progressBar1.Value++;
                    this.progressBar1.Update();
                    str = "_10.TIF";
                    if (this.full16.Checked)
                    {
                        str = "_16.TIF";
                    }
                    filename = Path.GetFileNameWithoutExtension(filename);
                    str3 = newpath + @"\" + filename + str;
                    if (tiff != IntPtr.Zero)
                    {
                        LibTIFF.Close(tiff);
                        tiff = IntPtr.Zero;
                    }
                    zero = LibTIFF.Open(str3, "w");
                    ptr2 = new IntPtr();
                    short num15 = (short) height;
                    try
                    {
                        string[] strArray = new string[] { "Writing to 16bit: ", filename, str, " ", (pos + 1).ToString(), " of ", length.ToString() };
                        this.label1.Text = string.Concat(strArray);
                        this.label1.Update();
                        LibTIFF.SetField(zero, 0x13c, exp);
                        LibTIFF.SetField(zero, 0x100, (short) width);
                        LibTIFF.SetField(zero, 0x101, (short) height);
                        LibTIFF.SetField(zero, 0x102, 0x10);
                        LibTIFF.SetField(zero, 0x115, (short) frameCount);
                        LibTIFF.SetField(zero, 0x116, num15);
                        LibTIFF.SetField(zero, 0x106, (short) 1);
                        LibTIFF.SetField(zero, 0x11c, (short) 1);
                        LibTIFF.SetField(zero, 0x132, date);
                        LibTIFF.SetField(zero, 270, val);
                        LibTIFF.SetField(zero, 0xc62f, serial);
                        if (this.compress.Checked)
                        {
                            LibTIFF.SetField(zero, 0x103, 0x80b2);
                        }
                        LibTIFF.SetField(zero, 0x13b, gps);
                        ptr2 = LibTIFF._malloc((width * frameCount) * 2);
                        source = new short[width * frameCount];
                        int num5 = 0;
                        goto Label_04EA;
                    Label_04E4:
                        num5++;
                    Label_04EA:
                        if (num5 >= height)
                        {
                            goto Label_054C;
                        }
                        int num4 = 0;
                        goto Label_04FB;
                    Label_04F5:
                        num4 += frameCount;
                    Label_04FB:
                        if (num4 >= (width * frameCount))
                        {
                            goto Label_0532;
                        }
                        int num3 = 0;
                        goto Label_050E;
                    Label_0508:
                        num3++;
                    Label_050E:
                        if (num3 >= frameCount)
                        {
                            goto Label_04F5;
                        }
                        int num14 = num4 / frameCount;
                        source[num4 + num3] = numArray2[num3, num14, num5];
                        goto Label_0508;
                    Label_0532:
                        Marshal.Copy(source, 0, ptr2, source.Length);
                        LibTIFF.WriteScanline(zero, ptr2, (uint) num5);
                        goto Label_04E4;
                    Label_054C:
                        source = null;
                        numArray2 = null;
                    }
                    catch when (?)
                    {
                        uint num9 = 0;
                        int num16 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num13);
                        try
                        {
                            try
                            {
                                this.label1.Text = "Error writing" + filename + str;
                                this.label1.Update();
                                break;
                            }
                            catch when (?)
                            {
                            }
                            if (num9 != 0)
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            __CxxUnregisterExceptionObject((void*) num13, (int) num9);
                        }
                    }
                    break;
                }
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
                im.SelectActiveFrame(dimension, order[index]);
                bitmap = new Bitmap(im.Width, im.Height, PixelFormat.Format24bppRgb);
                graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(im, 0, 0, im.Width, im.Height);
                graphics.Dispose();
                this.label1.Text = Path.GetFileName(filename) + " reading page " + index.ToString();
                this.label1.Update();
                if (this.enDisplay.Checked)
                {
                    if (this.screen != null)
                    {
                        this.screen.Dispose();
                    }
                    Rectangle rectangle2 = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    this.screen = bitmap.Clone(rectangle2, bitmap.PixelFormat);
                    this.pB1.Image = this.screen;
                    this.pB1.Invalidate();
                    this.pB1.Update();
                }
                int num12 = 3;
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                byte* numPtr = (byte*) bitmapdata.Scan0;
                uint num7 = 0;
                while (true)
                {
                    if (num7 >= bitmap.Height)
                    {
                        bitmap.UnlockBits(bitmapdata);
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }
                        break;
                    }
                    for (uint j = 0; j < bitmap.Width; j++)
                    {
                        if (this.full16.Checked)
                        {
                            numArray2[index, j, num7] = (short) (((numPtr[1] * 4) + (numPtr[1] - numPtr[0])) << 6);
                        }
                        else
                        {
                            numArray2[index, j, num7] = (short) ((numPtr[1] << 2) + Math.Abs((int) (numPtr[1] - numPtr[0])));
                        }
                        numPtr += num12;
                    }
                    numPtr += bitmapdata.Stride - (bitmap.Width * num12);
                    num7++;
                }
                index++;
            }
            LibTIFF._free(ptr2);
            LibTIFF.Close(zero);
            zero = IntPtr.Zero;
            Application.DoEvents();
        }

        private unsafe void Save8(string filename, string newpath, int length, int pos, string date, string serial, string gps, string exp)
        {
            Bitmap bitmap = null;
            IntPtr zero;
            Image im = null;
            byte[] source = null;
            string val = null;
            IntPtr ptr2;
            BitmapData bitmapdata = null;
            byte[,,] buffer2 = null;
            Graphics graphics = null;
            int[] wavelength = null;
            int[] order = null;
            FrameDimension dimension = null;
            string str2 = null;
            int num13 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            im = Image.FromFile(filename);
            int frameCount = 0;
            dimension = new FrameDimension(im.FrameDimensionsList[0]);
            frameCount = im.GetFrameCount(dimension);
            order = new int[frameCount];
            wavelength = new int[frameCount];
            this.SortChannels(im, order, wavelength);
            val = "descTetracam MCA:";
            for (int i = 0; i < frameCount; i++)
            {
                val = val + " " + wavelength[i].ToString();
            }
            val.Trim();
            int width = im.Width;
            int height = im.Height;
            buffer2 = new byte[frameCount, width, height];
            int index = 0;
            while (true)
            {
                if (index >= frameCount)
                {
                    if (im != null)
                    {
                        im.Dispose();
                        im = null;
                    }
                    this.progressBar1.Value++;
                    this.progressBar1.Update();
                    filename = Path.GetFileNameWithoutExtension(filename);
                    str2 = newpath + @"\" + filename + "_8.TIF";
                    if (tiff != IntPtr.Zero)
                    {
                        LibTIFF.Close(tiff);
                        tiff = IntPtr.Zero;
                    }
                    zero = LibTIFF.Open(str2, "w");
                    ptr2 = new IntPtr();
                    short num15 = (short) height;
                    try
                    {
                        string[] strArray = new string[] { "Writing to 8bit: ", filename, "_8.TIF ", (pos + 1).ToString(), " of ", length.ToString() };
                        this.label1.Text = string.Concat(strArray);
                        this.label1.Update();
                        LibTIFF.SetField(zero, 0x13c, exp);
                        LibTIFF.SetField(zero, 0x100, (short) width);
                        LibTIFF.SetField(zero, 0x101, (short) height);
                        LibTIFF.SetField(zero, 0x102, 8);
                        LibTIFF.SetField(zero, 0x115, (short) frameCount);
                        LibTIFF.SetField(zero, 0x116, num15);
                        LibTIFF.SetField(zero, 0x106, (short) 1);
                        LibTIFF.SetField(zero, 0x11c, (short) 1);
                        LibTIFF.SetField(zero, 0x132, date);
                        LibTIFF.SetField(zero, 270, val);
                        LibTIFF.SetField(zero, 0xc62f, serial);
                        if (this.compress.Checked)
                        {
                            LibTIFF.SetField(zero, 0x103, 0x80b2);
                        }
                        LibTIFF.SetField(zero, 0x13b, gps);
                        ptr2 = LibTIFF._malloc(width * frameCount);
                        source = new byte[width * frameCount];
                        int num5 = 0;
                        goto Label_0456;
                    Label_0450:
                        num5++;
                    Label_0456:
                        if (num5 >= height)
                        {
                            goto Label_04B8;
                        }
                        int num4 = 0;
                        goto Label_0467;
                    Label_0461:
                        num4 += frameCount;
                    Label_0467:
                        if (num4 >= (width * frameCount))
                        {
                            goto Label_049E;
                        }
                        int num3 = 0;
                        goto Label_047A;
                    Label_0474:
                        num3++;
                    Label_047A:
                        if (num3 >= frameCount)
                        {
                            goto Label_0461;
                        }
                        int num14 = num4 / frameCount;
                        source[num4 + num3] = buffer2[num3, num14, num5];
                        goto Label_0474;
                    Label_049E:
                        Marshal.Copy(source, 0, ptr2, source.Length);
                        LibTIFF.WriteScanline(zero, ptr2, (uint) num5);
                        goto Label_0450;
                    Label_04B8:
                        source = null;
                        buffer2 = null;
                    }
                    catch when (?)
                    {
                        uint num7 = 0;
                        int num16 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num13);
                        try
                        {
                            try
                            {
                                this.label1.Text = "Error writing" + filename + "_8.TIF ";
                                this.label1.Update();
                                break;
                            }
                            catch when (?)
                            {
                            }
                            if (num7 != 0)
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            __CxxUnregisterExceptionObject((void*) num13, (int) num7);
                        }
                    }
                    break;
                }
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
                im.SelectActiveFrame(dimension, order[index]);
                bitmap = new Bitmap(im.Width, im.Height, PixelFormat.Format24bppRgb);
                graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(im, 0, 0, im.Width, im.Height);
                graphics.Dispose();
                this.label1.Text = Path.GetFileName(filename) + " reading page " + index.ToString();
                this.label1.Update();
                if (this.enDisplay.Checked)
                {
                    if (this.screen != null)
                    {
                        this.screen.Dispose();
                    }
                    Rectangle rectangle2 = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    this.screen = bitmap.Clone(rectangle2, bitmap.PixelFormat);
                    this.pB1.Image = this.screen;
                    this.pB1.Invalidate();
                    this.pB1.Update();
                }
                int num12 = 3;
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                byte* numPtr = (byte*) bitmapdata.Scan0;
                uint num9 = 0;
                while (true)
                {
                    if (num9 >= bitmap.Height)
                    {
                        bitmap.UnlockBits(bitmapdata);
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }
                        break;
                    }
                    for (uint j = 0; j < bitmap.Width; j++)
                    {
                        buffer2[index, j, num9] = numPtr[0];
                        numPtr += num12;
                    }
                    numPtr += bitmapdata.Stride - (bitmap.Width * num12);
                    num9++;
                }
                index++;
            }
            LibTIFF._free(ptr2);
            LibTIFF.Close(zero);
            zero = IntPtr.Zero;
            Application.DoEvents();
        }

        private unsafe Bitmap show_colors()
        {
            Bitmap bitmap = null;
            short[] destination = null;
            byte[] buffer = null;
            BitmapData bitmapdata = null;
            uint val = 0;
            uint num11 = 0;
            ushort num6 = 0;
            ushort num4 = 0;
            int size = LibTIFF.ScanlineSize(tiff);
            LibTIFF.GetField(tiff, 0x100, out val);
            LibTIFF.GetField(tiff, 0x101, out num11);
            LibTIFF.GetField(tiff, 0x102, out num6);
            LibTIFF.GetField(tiff, 0x115, out num4);
            if (num4 < 3)
            {
                return null;
            }
            if (((num6 != 8) || (num6 != 0x10)) && (num4 > 12))
            {
                return null;
            }
            buffer = new byte[size * num11];
            destination = new short[size * num11];
            IntPtr buf = LibTIFF._malloc(size);
            for (uint i = 0; i < num11; i++)
            {
                LibTIFF.ReadScanline(tiff, buf, i);
                switch (num6)
                {
                    case 8:
                        Marshal.Copy(buf, buffer, (int) (i * size), size);
                        break;

                    case 0x10:
                        Marshal.Copy(buf, destination, (int) (i * size), size / 2);
                        break;
                }
            }
            LibTIFF._free(buf);
            bitmap = new Bitmap((int) val, (int) num11, PixelFormat.Format24bppRgb);
            int num12 = 3;
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int num10 = Convert.ToInt32(this.r_box.SelectedIndex);
            int num9 = Convert.ToInt32(this.g_box.SelectedIndex);
            int num8 = Convert.ToInt32(this.b_box.SelectedIndex);
            int num7 = 8;
            if (this.tenBit.Checked)
            {
                num7 = 2;
            }
            byte* numPtr = (byte*) bitmapdata.Scan0;
            uint num3 = 0;
            while (true)
            {
                if (num3 >= bitmap.Height)
                {
                    bitmap.UnlockBits(bitmapdata);
                    string[] strArray = new string[] { "Showing bands: ", num10.ToString(), " ", num9.ToString(), " ", num8.ToString() };
                    this.label1.Text = string.Concat(strArray);
                    this.label1.Update();
                    this.showing_colors = true;
                    if ((this.histo != null) && this.histo.Visible)
                    {
                        this.histo.SendColorsToHisto(this.mx_down, this.my_down, this.mx_up, this.my_up, this.v_scaler, this.tenBit.Checked, num10, num9, num8);
                    }
                    return bitmap;
                }
                for (uint j = 0; j < bitmap.Width; j++)
                {
                    switch (num6)
                    {
                        case 8:
                            numPtr[2] = buffer[((int) ((j * num4) + (num3 * size))) + num10];
                            numPtr[1] = buffer[((int) ((j * num4) + (num3 * size))) + num9];
                            numPtr[0] = buffer[((int) ((j * num4) + (num3 * size))) + num8];
                            break;

                        case 0x10:
                            numPtr[2] = (byte) (destination[(int) (((j * num4) + (num3 * size)) + num10)] >> num7);
                            numPtr[1] = (byte) (destination[(int) (((j * num4) + (num3 * size)) + num9)] >> num7);
                            numPtr[0] = (byte) (destination[(int) (((j * num4) + (num3 * size)) + num8)] >> num7);
                            break;
                    }
                    numPtr += num12;
                }
                numPtr += (byte*) (bitmapdata.Stride - (val * num12));
                num3++;
            }
        }

        private unsafe void SortChannels(Image im, int[] order, int[] wavelength)
        {
            PropertyItem[] propertyItems = null;
            string[] strArray = null;
            char[] separator = null;
            ASCIIEncoding encoding = null;
            string str = null;
            FrameDimension dimension = null;
            int num7 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            int frameCount = 0;
            dimension = new FrameDimension(im.FrameDimensionsList[0]);
            frameCount = im.GetFrameCount(dimension);
            int frameIndex = 0;
            goto Label_0053;
        Label_004F:
            frameIndex++;
        Label_0053:
            if (frameIndex < frameCount)
            {
                im.SelectActiveFrame(dimension, frameIndex);
                try
                {
                    order[frameIndex] = frameIndex;
                    propertyItems = im.PropertyItems;
                    str = " ";
                    int num2 = 0;
                    goto Label_007F;
                Label_007B:
                    num2++;
                Label_007F:
                    if (num2 < propertyItems.Length)
                    {
                        encoding = new ASCIIEncoding();
                        if (propertyItems[num2].Id == 800)
                        {
                            str = encoding.GetString(propertyItems[num2].Value, 0, propertyItems[num2].Len - 1);
                            separator = new char[] { '-' };
                            strArray = str.Split(separator);
                            wavelength[frameIndex] = Convert.ToInt32(strArray[0].Substring(strArray[0].Length - 3, 3));
                            IDisposable disposable = encoding as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }
                        goto Label_007B;
                    }
                }
                catch when (?)
                {
                    uint num4 = 0;
                    int num9 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num7);
                    try
                    {
                        try
                        {
                            goto Label_004F;
                        }
                        catch when (?)
                        {
                        }
                        if (num4 != 0)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        __CxxUnregisterExceptionObject((void*) num7, (int) num4);
                    }
                }
                goto Label_004F;
            }
            int num6 = 0;
        Label_0175:
            if (num6 >= frameCount)
            {
                return;
            }
            int index = frameCount - 2;
            while (true)
            {
                if (index <= -1)
                {
                    num6++;
                    goto Label_0175;
                }
                if (wavelength[index + 1] < wavelength[index])
                {
                    Array.Reverse(order, index, 2);
                    Array.Reverse(wavelength, index, 2);
                }
                index--;
            }
        }

        private void tenBit_CheckedChanged(object sender, EventArgs e)
        {
            if (tiff != IntPtr.Zero)
            {
                if (this.screen != null)
                {
                    this.screen.Dispose();
                }
                this.screen = this.GetBitmapFromTIFFHandle(tiff, 0, 0xff);
                this.pB1.Image = this.screen;
                this.pB1.Update();
            }
        }

        private unsafe void view_Click(object sender, EventArgs e)
        {
            byte[] bytes = null;
            ASCIIEncoding encoding = null;
            string[] strArray = null;
            BinaryReader reader = null;
            ushort[] numArray = null;
            string item = null;
            string str2 = null;
            string str3 = null;
            ushort num14;
            char[] separator = null;
            string str4 = null;
            string str5 = null;
            string str6 = null;
            ushort num16;
            long length;
            FileInfo info = null;
            ushort num19;
            int num18 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            try
            {
                str3 = (string) sender;
                str3 = str3.Replace("^", " ");
                this.OFD2.FileName = str3;
                goto Label_00CB;
            }
            catch (Exception exception)
            {
                if (!exception.GetType().FullName.Equals("System.InvalidCastException"))
                {
                    return;
                }
            }
            this.OFD2.InitialDirectory = this.Target;
            this.OFD2.Filter = "*.TIF|*.TIFF;*.TIF";
            this.OFD2.Multiselect = false;
            if (this.OFD2.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        Label_00CB:
            this.label1.Text = " ";
            this.label3.Text = " ";
            this.Target = Path.GetDirectoryName(this.OFD2.FileName);
            if (tiff != IntPtr.Zero)
            {
                LibTIFF.Close(tiff);
                tiff = IntPtr.Zero;
                this.view_pix = null;
            }
            if (this.pB1.Image != null)
            {
                this.pB1.Image.Dispose();
                this.pB1.Image = null;
            }
            str6 = "";
            str2 = "";
            str5 = "";
            item = "";
            str4 = "";
            int byteIndex = 300;
            int num10 = 300;
            int num9 = 300;
            int num15 = 300;
            int num12 = 300;
            try
            {
                numArray = new ushort[0x97];
                bytes = new byte[0x12d];
                info = new FileInfo(this.OFD2.FileName);
                reader = new BinaryReader(info.OpenRead());
                length = reader.BaseStream.Length;
                reader.BaseStream.Position = length - 0x12dL;
                int index = 0;
                int num4 = 0;
                goto Label_0209;
            Label_0203:
                num4++;
            Label_0209:
                if (num4 < 150)
                {
                    numArray[num4] = reader.ReadUInt16();
                    bytes[index] = (byte) (numArray[num4] & 0xff);
                    index++;
                    bytes[index] = (byte) ((numArray[num4] >> 8) & 0xff);
                    index++;
                    goto Label_0203;
                }
                int num = 4;
                goto Label_0254;
            Label_0250:
                num++;
            Label_0254:
                if (num < 300)
                {
                    if (((bytes[num - 4] == 100) && (bytes[num - 3] == 0x65)) && ((bytes[num - 2] == 0x73) && (bytes[num - 1] == 0x63)))
                    {
                        byteIndex = num;
                    }
                    if (((bytes[num - 4] == 100) && (bytes[num - 3] == 0x61)) && ((bytes[num - 2] == 0x74) && (bytes[num - 1] == 0x65)))
                    {
                        num10 = num;
                    }
                    if (((bytes[num - 3] == 0x73) && (bytes[num - 2] == 0x65)) && (bytes[num - 1] == 0x72))
                    {
                        num9 = num;
                    }
                    if (((bytes[num - 3] == 0x67) && (bytes[num - 2] == 0x70)) && (bytes[num - 1] == 0x73))
                    {
                        num15 = num;
                    }
                    if (((bytes[num - 3] == 0x65) && (bytes[num - 2] == 120)) && (bytes[num - 1] == 0x70))
                    {
                        num12 = num;
                    }
                    goto Label_0250;
                }
                reader.Close();
                encoding = new ASCIIEncoding();
                str2 = encoding.GetString(bytes, byteIndex, num10 - byteIndex);
                str6 = encoding.GetString(bytes, num10, num9 - num10);
                str5 = encoding.GetString(bytes, num9, bytes.Length - num9);
                item = encoding.GetString(bytes, num15, bytes.Length - num15);
                str4 = encoding.GetString(bytes, num12, bytes.Length - num12);
                numArray = null;
                bytes = null;
            }
            catch when (?)
            {
                uint num11 = 0;
                int num26 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num18);
                try
                {
                    try
                    {
                        goto Label_03C9;
                    }
                    catch when (?)
                    {
                    }
                    if (num11 != 0)
                    {
                        throw;
                    }
                }
                finally
                {
                    __CxxUnregisterExceptionObject((void*) num18, (int) num11);
                }
            }
        Label_03C9:
            channel = 0;
            if (Path.GetFileName(this.OFD2.FileName).Contains("_10.TIF"))
            {
                this.tenBit.Checked = true;
                this.tenBit.Enabled = true;
            }
            else
            {
                this.tenBit.Checked = false;
                this.tenBit.Enabled = false;
            }
            tiff = LibTIFF.Open(this.OFD2.FileName, "r");
            LibTIFF.GetField(tiff, 0x115, out num14);
            LibTIFF.GetField(tiff, 0x102, out num16);
            LibTIFF.GetField(tiff, 0x103, out num19);
            this.v_scaler = num16;
            this.statBox.Items.Clear();
            this.statBox.Items.Add("Image properties for:");
            this.statBox.Items.Add(Path.GetFileName(this.OFD2.FileName));
            if (num9 < 300)
            {
                this.statBox.Items.Add("Camera# " + str5);
            }
            if (num10 < 300)
            {
                this.statBox.Items.Add(str6);
            }
            if (num12 < 300)
            {
                this.statBox.Items.Add(str4);
            }
            ushort num25 = num14;
            this.statBox.Items.Add(num25.ToString() + " bands");
            ushort num24 = num16;
            this.statBox.Items.Add(num24.ToString() + " bits per band");
            long num23 = length / 0x3e8L;
            this.statBox.Items.Add(num23.ToString() + " KB");
            if (num19 > 1)
            {
                this.statBox.Items.Add("This file is compressed");
            }
            if (item != "")
            {
                this.statBox.Items.Add(item);
            }
            max_chan = num14 - 1;
            if (byteIndex >= 300)
            {
                this.r_box.Items.Clear();
                this.g_box.Items.Clear();
                this.b_box.Items.Clear();
                for (int i = 0; i < num14; i++)
                {
                    int num22 = i;
                    this.r_box.Items.Add(num22.ToString());
                    int num21 = i;
                    this.g_box.Items.Add(num21.ToString());
                    int num20 = i;
                    this.b_box.Items.Add(num20.ToString());
                }
                this.r_box.SelectedIndex = 0;
                this.g_box.SelectedIndex = 0;
                this.b_box.SelectedIndex = 0;
            }
            else
            {
                this.label3.Text = str2;
                this.label3.Update();
                separator = new char[] { ' ' };
                strArray = str2.Split(separator);
                int selectedIndex = this.r_box.SelectedIndex;
                if (selectedIndex < 0)
                {
                    selectedIndex = 0;
                }
                int num7 = this.g_box.SelectedIndex;
                if (num7 < 0)
                {
                    num7 = 0;
                }
                int num6 = this.b_box.SelectedIndex;
                if (num6 < 0)
                {
                    num6 = 0;
                }
                this.r_box.Items.Clear();
                this.g_box.Items.Clear();
                this.b_box.Items.Clear();
                for (int j = 2; j < strArray.Length; j++)
                {
                    this.r_box.Items.Add(strArray[j]);
                    this.g_box.Items.Add(strArray[j]);
                    this.b_box.Items.Add(strArray[j]);
                }
                if ((selectedIndex > -1) && (selectedIndex <= max_chan))
                {
                    this.r_box.SelectedIndex = selectedIndex;
                }
                else
                {
                    this.r_box.SelectedIndex = 0;
                }
                if ((num7 > -1) && (num7 <= max_chan))
                {
                    this.g_box.SelectedIndex = num7;
                }
                else
                {
                    this.g_box.SelectedIndex = 0;
                }
                if ((num6 > -1) && (num6 <= max_chan))
                {
                    this.b_box.SelectedIndex = num6;
                }
                else
                {
                    this.b_box.SelectedIndex = 0;
                }
            }
            if (tiff != IntPtr.Zero)
            {
                if (this.screen != null)
                {
                    this.screen.Dispose();
                }
                this.screen = this.GetBitmapFromTIFFHandle(tiff, 0, 0xff);
                this.Load_pix_array(tiff);
                this.pB1.Image = this.screen;
                this.mx_down = 0;
                this.my_down = 0;
                this.mx_up = this.view_w;
                this.my_up = this.view_h;
                this.drawn = false;
                this.pB1.Invalidate();
                this.showing_colors = false;
                if (this.histo_show.Checked)
                {
                    if ((this.histo != null) && this.histo.Visible)
                    {
                        this.histo.Close();
                    }
                    this.histo = new HistoForm();
                    this.histo.Owner = this;
                    this.histo.Show();
                    this.histo.SendArrayToHisto(this.view_pix);
                    this.histo.SendToHisto(0, 0, this.view_w, this.view_h, this.v_scaler, this.tenBit.Checked, channel);
                }
            }
        }

        private void view_color_Click(object sender, EventArgs e)
        {
            if (tiff != IntPtr.Zero)
            {
                this.screen = this.show_colors();
                if (this.screen != null)
                {
                    this.pB1.Image = this.screen;
                    this.pB1.Update();
                }
            }
        }
    }
}

