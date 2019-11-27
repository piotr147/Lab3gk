using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lab3gk.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private WriteableBitmap _baseImage;
        private WriteableBitmap _baseImage2;
        private WriteableBitmap _image1;
        private WriteableBitmap _image2;
        private WriteableBitmap _image3;

        public int SelectedMode{ get; set; }
        public RelayCommand LoadBaseImage2Command { get; set; }
        public RelayCommand ApplyCommand { get; set; }
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

        public MainViewModel()
        {
            SelectedMode = 0;



            LoadBaseImage2Command = new RelayCommand(LoadBaseImage2);
            ApplyCommand = new RelayCommand(Apply);
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
        }

        private void HSV()
        {
            for (int i = 0; i < Image1.PixelWidth; i++)
            {
                for (int j = 0; j < Image1.PixelHeight; j++)
                {
                    //var col = 
                }
            }
        }

        private void YCbCr()
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

    }
}
