using Emgu.CV;
using Emgu.CV.Structure;
using FaceAuth.Helpers;
using FaceAuth.Models;
using FaceAuth.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FaceAuth.Pages
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : UserControl
    {
        private Image<Bgr, Byte> currentFrame;
        private Image<Gray, byte> gray = null;
        private HaarCascade face;
        private Capture grabber;
        private Image<Gray, byte> TrainedFace = null;

        private System.Drawing.Rectangle[] _faces = null;
        private int _countFaces = 0;
        private MCvAvgComp[][] facesDetected;

        public Admin()
        {
            InitializeComponent();

            face = new HaarCascade("haarcascade_frontalface_default.xml");

            Loaded += (s, e) => 
            {
                this.DataContext = CommonData.PicturesVM;
                if (grabber == null)
                {
                    CommonData.LoadSavedData();
                    //check how many faces we already have
                    _countFaces = CommonData.PicturesVM.Pictures.Count;

                    grabber = new Capture();
                    grabber.QueryFrame();
                    grabber.Start();
                }
                else
                {
                    grabber.Start();
                }

            };
            Unloaded += (s, e) => 
            {
                grabber.Stop();
            };

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            _imgSource.Source = ImageHelper.ToBitmapSource(currentFrame);
        }

        int count = 0;
        private void RegisterFaces()
        {
            //iterates till detects a face in the image
            while (facesDetected == null || facesDetected[0].Length == 0)
            {
                gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                //Face Detector
                facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new System.Drawing.Size(20, 20));

                foreach (var f in TrainFaces(facesDetected))
                {
                    var t = f.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    t.Save(CommonData.TARGET_PATH + "face" + _countFaces + "." + CommonData.IMAGE_EXT);
                    //File.AppendAllText(Environment.CurrentDirectory + "/TrainedFaces/TrainedLabels.txt", "names");

                    PictureModel _model = new PictureModel();
                    _model.ImgSource = ImageHelper.ToBitmapSource(t).ToWriteableBitmap();
                    _model.ID = _countFaces;

                    CommonData.PicturesVM.Pictures.Add(_model);
                    _countFaces++;
                }

                //Debug.WriteLine("d " + ++count);
                Thread.Sleep(100);//give it some time to elaborate if the CPU is full
            }            
        }

        private void ModernButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            facesDetected = null;
            RegisterFaces();
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string text = ((TextBox)sender).Text;
                File.AppendAllText(Environment.CurrentDirectory + "/TrainedFaces/TrainedLabels.txt", text+"%");
            }
        }

        private IEnumerable<Image<Gray, byte>> TrainFaces(MCvAvgComp[][] facesDetected)
        {
            foreach (MCvAvgComp f in facesDetected[0])
            {
                TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                yield return TrainedFace;
			}
        }
    }
}
