using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using FaceAuth.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FaceAuth.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class RecognitionOnPrem : UserControl
    {        
        private Image<Bgr, Byte> currentFrame;
        private Image<Gray, byte> gray = null;
        private HaarCascade eye;
        private CascadeClassifier _faceClassifier;
        private Capture grabber;
        private Image<Gray, byte> result, TrainedFace = null;
        private MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);

        private int ContTrain = 0;

        private SolidColorBrush _gr = new SolidColorBrush(Colors.Green);
        private SolidColorBrush _rd = new SolidColorBrush(Colors.Red);
        private EigenObjectRecognizer recognizer;
        private System.Drawing.Rectangle[] _rects = null;

        public RecognitionOnPrem()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (grabber == null)
                {
                    _faceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");

                    //count number of trained faces
                    ContTrain = CommonData.TrainingImages.Count;

                    grabber = new Capture();
                    grabber.QueryFrame();                    
                }
                else
                {
                    grabber.Start();                    
                }
            };

            Unloaded += (s, e) => {
                grabber.Stop();
            };

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            _status.Fill = _rd;

            #region Recognition
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            gray = currentFrame.Convert<Gray, Byte>();

            var size = new System.Drawing.Size(20, 20);
            var window = new System.Drawing.Size(grabber.Width, grabber.Height);

            _rects = _faceClassifier.DetectMultiScale(gray, 1.2, 10, size, window);

            foreach (var f in _rects)
            {
                result = currentFrame.Copy(f).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                _status.Fill = new SolidColorBrush(Colors.Green);
                currentFrame.Draw(f, new Bgr(System.Drawing.Color.Red), 2);

                //if we have already trained
                if (CommonData.TrainingImages.Count > 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    //Eigen face recognizer
                    recognizer = new EigenObjectRecognizer(
                       CommonData.TrainingImages.ToArray(),
                       CommonData.Names.ToArray(),
                       3000,
                       ref termCrit);

                    string name = recognizer.Recognize(result);
                    currentFrame.Draw(name, ref font, new System.Drawing.Point(f.X - 2, f.Y - 2),
                        new Bgr(System.Drawing.Color.LightGreen));
                }

                //finally draw the source
                _imgCamera.Source = ImageHelper.ToBitmapSource(currentFrame);
            }
            #endregion
        }
        
    }
}
