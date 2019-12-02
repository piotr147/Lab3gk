using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Lab3gk.Helpers;

namespace Lab3gk.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private WriteableBitmap _baseImage;
        private WriteableBitmap _baseImage2;
        private WriteableBitmap _image1;
        private WriteableBitmap _image2;
        private WriteableBitmap _image3;
        private double _redX;
        private double _gamma;
        private double _whiteY;
        private double _whiteX;
        private double _blueY;
        private double _blueX;
        private double _greenY;
        private double _greenX;
        private double _redY;
        private int _predCol;
        private int _predIlu;

        public int SelectedMode{ get; set; }
        public int PredCol
        {
            get { return _predCol; }
            set { _predCol = value; RaisePropertyChanged(nameof(PredCol)); }
        }
        public int PredIlu
        {
            get { return _predIlu; }
            set { _predIlu = value; RaisePropertyChanged(nameof(PredIlu)); }
        }

        public RelayCommand LoadBaseImage2Command { get; set; }
        public RelayCommand ApplyCommand { get; set; }
        public AsyncCommand ApplyAsync { get; set; }
        public RelayCommand SelectionChangedColCommand { get; set; }
        public RelayCommand SelectionChangedIluCommand { get; set; }
        public RelayCommand SaveImage1Command { get; set; }
        public RelayCommand SaveImage2Command { get; set; }
        public RelayCommand SaveImage3Command { get; set; }

        //public BitmapImage BaseImage2 { get; set; }
        public WriteableBitmap BaseImage
        {
            get { return _baseImage; }
            set { _baseImage = value; RaisePropertyChanged(nameof(BaseImage)); }
        }
        public WriteableBitmap BaseImage2
        {
            get { return _baseImage2; }
            set { _baseImage2 = value; RaisePropertyChanged(nameof(BaseImage2)); }
        }

        public WriteableBitmap Image1
        {
            get { return _image1; }
            set { _image1 = value; RaisePropertyChanged(nameof(Image1)); }
        }

        public WriteableBitmap Image2
        {
            get { return _image2; }
            set { _image2 = value; RaisePropertyChanged(nameof(Image2)); }
        }

        public WriteableBitmap Image3
        {
            get { return _image3; }
            set { _image3 = value; RaisePropertyChanged(nameof(Image3)); }
        }

        public (byte r, byte g, byte b)[,] Pixels { get; set; }
        public bool IsBusy { get; private set; }

        public double RedX
        {
            get { return _redX; }
            set { _redX = value; RaisePropertyChanged(nameof(RedX)); }
        }

        public double RedY
        {
            get { return _redY; }
            set { _redY = value; RaisePropertyChanged(nameof(RedY)); }
        }

        public double GreenX
        {
            get { return _greenX; }
            set { _greenX = value; RaisePropertyChanged(nameof(GreenX)); }
        }

        public double GreenY
        {
            get { return _greenY; }
            set { _greenY = value; RaisePropertyChanged(nameof(GreenY)); }
        }

        public double BlueX
        {
            get { return _blueX; }
            set { _blueX = value; RaisePropertyChanged(nameof(BlueX)); }
        }

        public double BlueY
        {
            get { return _blueY; }
            set { _blueY = value; RaisePropertyChanged(nameof(BlueY)); }
        }

        public double WhiteX
        {
            get { return _whiteX; }
            set { _whiteX = value; RaisePropertyChanged(nameof(WhiteX)); }
        }

        public double WhiteY
        {
            get { return _whiteY; }
            set { _whiteY = value; RaisePropertyChanged(nameof(WhiteY)); }
        }

        public double Gamma
        {
            get { return _gamma; }
            set { _gamma = value; RaisePropertyChanged(nameof(Gamma)); }
        }







        public MainViewModel()
        {
            SelectedMode = 0;
            PredCol = 0;
            PredIlu = 5;

            RedX = 0.64;
            RedY = 0.33;
            GreenX = 0.3;
            GreenY = 0.6;
            BlueX = 0.15;
            BlueY = 0.06;
            WhiteX = 0.313;
            WhiteY = 0.329;
            Gamma = 2.2;


            LoadBaseImage2Command = new RelayCommand(LoadBaseImage2);
            ApplyCommand = new RelayCommand(Apply);
            ApplyAsync = new AsyncCommand(ApplyAsyncFunc, () => !IsBusy);
            SelectionChangedColCommand = new RelayCommand(SelectionChangedCol);
            SelectionChangedIluCommand = new RelayCommand(SelectionChangedIlu);
            SaveImage1Command = new RelayCommand(() => SaveImage(Image1));
            SaveImage2Command = new RelayCommand(() => SaveImage(Image2));
            SaveImage3Command = new RelayCommand(() => SaveImage(Image3));
        }

        private void SaveImage(WriteableBitmap imageBitmap)
        {
            if (imageBitmap == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "png (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream stream5 = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(imageBitmap));
                    encoder5.Save(stream5);
                }
            }
        }

        private async Task ApplyAsyncFunc()
        {
            try
            {
                IsBusy = true;


                if (SelectedMode == 0)
                {
                    await YCbCr();
                }
                else if (SelectedMode == 1)
                {
                    await HSV();
                }
                else if (SelectedMode == 2)
                {
                    await Lab();
                }
                else if (SelectedMode == 3)
                {
                    await RGB();
                }

            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Apply()
        {
            if(SelectedMode == 0)
            {
                YCbCr();
            }
            else if (SelectedMode == 1)
            {
                HSV();
            }
            else if (SelectedMode == 2)
            {
                Lab();
            }
            else if (SelectedMode == 3)
            {
                RGB();
            }
        }

        private async Task Lab()
        {
            var Pixels1 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels2 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels3 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];

            //int Y = 100;

            var M = XYZMatrix();

            for (int i = 0; i < Image1.PixelWidth; i++)
            {
                for (int j = 0; j < Image1.PixelHeight; j++)
                {
                    var col = Pixels[i, j];

                    double R = ((double)col.r / 255.0);
                    double G = ((double)col.g / 255.0);
                    double B = ((double)col.b / 255.0);

                    Vector<double> RGB = Vector<double>.Build.DenseOfArray(new double[] { R, G, B });
                    Vector<double> XYZ = M.Multiply(RGB);

                    var Lab = XYZ2Lab(XYZ[0], XYZ[1], XYZ[2]);

                    Pixels1[i, j] = ((byte)(int)Lab.L, (byte)(int)Lab.L, (byte)(int)Lab.L);
                    Pixels2[i, j] = ((byte) (int) (127 + Lab.a), (byte) (int) (127 - Lab.a), (byte) 127);
                    Pixels3[i, j] = ((byte)(int)(127 + Lab.b), (byte)127, (byte)(int)(127 - Lab.b));
                }
            }

            Pixels2Bitmap(Image1, Pixels1);
            Pixels2Bitmap(Image2, Pixels2);
            Pixels2Bitmap(Image3, Pixels3);
        }

        private (double L, double a, double b) XYZ2Lab(double X, double Y, double Z)
        {
            var xr = X / WhiteX;
            var yr = Y / WhiteY;
            var zr = Z / (1 - WhiteX - WhiteY);

            var eps = 0.008856;
            var kappa = 903.3;

            var fx = xr > eps ? Math.Pow(xr, 1.0 / 3.0) : (kappa * xr + 16.0) / 116.0;
            var fy = yr > eps ? Math.Pow(yr, 1.0 / 3.0) : (kappa * yr + 16.0) / 116.0;
            var fz = zr > eps ? Math.Pow(zr, 1.0 / 3.0) : (kappa * zr + 16.0) / 116.0;

            var L = 116.0 * fy - 16.0;
            var a = 500.0 * (fx - fy);
            var b = 200.0 * (fy - fz);

            return (L, a, b);
        }

        private Matrix<double> XYZMatrix()
        {
            var XR = RedX / RedY;
            var YR = 1;
            var ZR = (1 - RedX - RedY) / RedY;

            var XG = GreenX / GreenY;
            var YG = 1;
            var ZG = (1 - GreenX - GreenY) / GreenY;

            var XB = BlueX / BlueY;
            var YB = 1;
            var ZB = (1 - BlueX - BlueY) / BlueY;


            var mat1 = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {XR, XG, XB },
                {YR, YG, YB },
                {ZR, ZG, ZB }
            });

            var white = Vector<double>.Build.DenseOfArray(new double[] { WhiteX, WhiteY, 1 - WhiteX - WhiteY });
            Vector<double> S = mat1.Inverse().Multiply(white);

            var M = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {S[0] * XR, S[1] * XG, S[2] * XB },
                {S[0] * YR, S[1] * YG, S[2] * YB },
                {S[0] * ZR, S[1] * ZG, S[2] * ZB }
            });

            return M;
        }

        private async Task RGB()
        {
            var Pixels1 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels2 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels3 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];

            for (int i = 0; i < Image1.PixelWidth; i++)
            {
                for (int j = 0; j < Image1.PixelHeight; j++)
                {
                    var col = Pixels[i, j];

                    Pixels1[i, j] = (col.r, 0, 0);
                    Pixels2[i, j] = (0, col.g, 0);
                    Pixels3[i, j] = (0, 0, col.b);
                }
            }

            Pixels2Bitmap(Image1, Pixels1);
            Pixels2Bitmap(Image2, Pixels2);
            Pixels2Bitmap(Image3, Pixels3);
        }

        private async Task HSV()
        {
            var Pixels1 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels2 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels3 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)]; 

            for (int i = 0; i < Image1.PixelWidth; i++)
            {
                for (int j = 0; j < Image1.PixelHeight; j++)
                {
                    double h = 0;
                    double s = 0;
                    double v = 0;

                    var col = Pixels[i, j];
                    double r = (double)(int)col.r / 255.0;
                    double g = (double)(int)col.g / 255.0;
                    double b = (double)(int)col.b / 255.0;

                    double Cmax = Math.Max(r, Math.Max(g, b));
                    double Cmin = Math.Min(Math.Min(r, g), b);

                    double delta = Cmax - Cmin;

                    if (delta == 0)
                    {
                        h = 0;
                    }
                    else if (Cmax == r)
                    {
                        h = 60 * (((g - b) / delta) % 6.0);
                    }
                    else if (Cmax == g)
                    {
                        h = 60 * (((b - r) / delta) + 2.0);
                    }
                    else if (Cmax == b)
                    {
                        h = 60 * (((r - g) / delta) + 4.0);
                    }

                    s = Cmax == 0 ? 0 : delta / Cmax;

                    v = Cmax;

                    if (h < 0)
                        h += 360;

                    h = h / 360;

                    h *= 255;
                    s *= 255;
                    v *= 255;

                    if (h < 0 || h > 255 || s < 0 || s > 255 || v < 0 || v > 255)
                        v = v;

                    Pixels1[i, j] = ((byte)(int)h, (byte)(int)h, (byte)(int)h);
                    Pixels2[i, j] = ((byte)(int)s, (byte)(int)s, (byte)(int)s);
                    Pixels3[i, j] = ((byte)(int)v, (byte)(int)v, (byte)(int)v);

                }
            }

            Pixels2Bitmap(Image1, Pixels1);
            Pixels2Bitmap(Image2, Pixels2);
            Pixels2Bitmap(Image3, Pixels3);     
        }

        private async Task YCbCr()
        {
            var kb = 0.114;
            var kr = 0.299;
            var Pixels1 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels2 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];
            var Pixels3 = new (byte r, byte g, byte b)[Pixels.GetLength(0), Pixels.GetLength(1)];

            for (int i = 0; i < Pixels.GetLength(0); i++)
            {
                for(int j = 0; j < Pixels.GetLength(1); j++)
                {
                    var col = Pixels[i, j];
                    var y = (((double)col.r * kr) + ((double)col.g * (1 - kr - kb)) + ((double)col.b * kb));

                    var cb = (col.b / 255.0 - y / 255.0) / 1.772 + 0.5;
                    var g2 = (int)(255.0 - cb * 255.0);
                    var b2 = (int)(cb * 255.0);

                    var cr = (col.r / 255.0 - y / 255.0) / 1.402 + 0.5;

                    var r3 = (int)(255.0 * cr);
                    var g3 = (int)(255.0 - cr * 255.0);
                    Pixels1[i, j] = ((byte)y, (byte)y, (byte)y);
                    Pixels2[i, j] = ((byte)(127), (byte)(g2), (byte)(b2));
                    Pixels3[i, j] = ((byte)(r3), (byte)(g3), (byte)127);
                }
            }

            Pixels2Bitmap(Image1, Pixels1);
            Pixels2Bitmap(Image2, Pixels2);
            Pixels2Bitmap(Image3, Pixels3);
        }

        private void Pixels2Bitmap(WriteableBitmap bitmap, (byte r, byte g, byte b)[,] pixels)
        {


            unsafe
            {
                using (var context = bitmap.GetBitmapContext())
                {
                    for (int i = 0; i < pixels.GetLength(0); i++)
                    {
                        for (int j = 0; j < pixels.GetLength(1); j++)
                        {
                            context.Pixels[i * pixels.GetLength(1) + j] = (255 << 24) | (pixels[i, j].r << 16) | (pixels[i, j].g << 8) | pixels[i, j].b;
                        }
                    }
                }
            }

        }

        private void LoadBaseImage2()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                var BaseImage2xd = new BitmapImage(fileUri);
                BaseImage2 = new WriteableBitmap(BaseImage2xd);
                Image1 = new WriteableBitmap(BaseImage2xd);
                Image2 = new WriteableBitmap(BaseImage2xd);
                Image3 = new WriteableBitmap(BaseImage2xd);

                Pixels = new (byte r, byte g, byte b)[BaseImage2.PixelWidth , BaseImage2.PixelHeight];

                unsafe
                {
                    using (var context = BaseImage2.GetBitmapContext(ReadWriteMode.ReadOnly))
                    {
                        for (int i = 0; i < BaseImage2.PixelWidth; i++)
                        {
                            for(int j = 0; j < BaseImage2.PixelHeight; j++)
                            {
                                if (i * BaseImage2.PixelHeight + j > context.Length - 1)
                                    break;
                                var c = context.Pixels[i * BaseImage2.PixelHeight + j];
                                
                                var a = (byte)(c >> 24);
                                int ai = a;
                                if (ai == 0)
                                {
                                    ai = 1;
                                }
                                ai = ((255 << 8) / ai);
                                Pixels[i, j] = ((byte)((((c >> 16) & 0xFF) * ai) >> 8), (byte)((((c >> 8) & 0xFF) * ai) >> 8), (byte)((((c & 0xFF) * ai) >> 8)));
                            }
                        }
                    }
                }
            }
        }

        private void SelectionChangedIlu()
        {
            if (PredIlu == 0)
            {
                WhiteX = 0.448; WhiteY = 0.407;
            }
            else if (PredIlu == 1)
            {
                WhiteX = 0.348; WhiteY = 0.351;
            }
            else if (PredIlu == 2)
            {
                WhiteX = 0.31; WhiteY = 0.316;
            }
            else if (PredIlu == 3)
            {
                WhiteX = 0.346; WhiteY = 0.359;
            }
            else if (PredIlu == 4)
            {
                WhiteX = 0.332; WhiteY = 0.347;
            }
            else if (PredIlu == 5)
            {
                WhiteX = 0.312; WhiteY = 0.329;
            }
            else if (PredIlu == 6)
            {
                WhiteX = 0.299; WhiteY = 0.315;
            }
            else if (PredIlu == 7)
            {
                WhiteX = 0.285; WhiteY = 0.293;
            }
            else if (PredIlu == 8)
            {
                WhiteX = 0.333; WhiteY = 0.333;
            }
            else if (PredIlu == 9)
            {
                WhiteX = 0.372; WhiteY = 0.375;
            }
            else if (PredIlu == 10)
            {
                WhiteX = 0.313; WhiteY = 0.329;
            }
            else if (PredIlu == 11)
            {
                WhiteX = 0.381; WhiteY = 0.377;
            }
        }

        private void SelectionChangedCol()
        {
            if (PredCol == 0) //sRGB
            {
                RedX = 0.64;
                RedY = 0.33;
                GreenX = 0.3;
                GreenY = 0.6;
                BlueX = 0.15;
                BlueY = 0.06;
                WhiteX = 0.313;
                WhiteY = 0.329;
                Gamma = 2.2;
            }
            else if (PredCol == 1) // Adobe RGB
            {
                RedX = 0.64;
                RedY = 0.33;
                GreenX = 0.21;
                GreenY = 0.71;
                BlueX = 0.15;
                BlueY = 0.06;
                WhiteX = 0.313;
                WhiteY = 0.329;
                Gamma = 2.2;
            }
            else if (PredCol == 2) // Apple RGB 
            {
                RedX = 0.625;
                RedY = 0.34;
                GreenX = 0.28;
                GreenY = 0.595;
                BlueX = 0.155;
                BlueY = 0.07;
                WhiteX = 0.313;
                WhiteY = 0.329;
                Gamma = 1.8;
            }
            else if (PredCol == 3) // CIE RGB
            {
                RedX = 0.735;
                RedY = 0.26;
                GreenX = 0.274;
                GreenY = 0.717;
                BlueX = 0.167;
                BlueY = 0.009;
                WhiteX = 0.333;
                WhiteY = 0.333;
                Gamma = 2.2;
            }
            else if (PredCol == 4) // Wide Gamut
            {
                RedX = 0.735;
                RedY = 0.265;
                GreenX = 0.115;
                GreenY = 0.826;
                BlueX = 0.157;
                BlueY = 0.018;
                WhiteX = 0.346;
                WhiteY = 0.356;
                Gamma = 1.2;
            }
            else if (PredCol == 5) // PAL/SECAM
            {
                RedX = 0.64;
                RedY = 0.33;
                GreenX = 0.29;
                GreenY = 0.6;
                BlueX = 0.15;
                BlueY = 0.06;
                WhiteX = 0.3127;
                WhiteY = 0.06;
                Gamma = 1.95;
            }

        }

    }
}
