using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceAuth.Helpers;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FaceAuth.ViewModels;
using FaceAuth.Models;
using System.Windows.Media.Imaging;

namespace FaceAuth.Pages
{
    /// <summary>
    /// Interaction logic for AIRecognition.xaml
    /// </summary>
    public partial class AIRecognition : UserControl
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("5a6bc138df1e4823be192f0103f788bb");

        private Image<Bgr, Byte> currentFrame;
        private Image<Gray, byte> gray = null;
        private CascadeClassifier _faceClassifier;
        private Capture grabber;
        private Image<Gray, byte> TrainedFace = null;

        private System.Drawing.Rectangle[] _faces = null;
        private int _countFaces = 0;
        private System.Drawing.Rectangle[] _rects = null;
        private Face[] _detectedFaceFromAI = null;
        private List<Face> _trainedFacesAI = new List<Face>();
        private PictureViewModel _vmodel = new PictureViewModel();

        public AIRecognition()
        {
            InitializeComponent();

            _faceClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");

            Loaded += (s, e) =>
            {
                _vmodel.Pictures.Clear();
                _vmodel.PersonRecognized = 0;
                this.DataContext = _vmodel;

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

        private async void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);            

            if (_rects == null || _rects.Length == 0)
            {
                gray = currentFrame.Convert<Gray, Byte>();

                var size = new System.Drawing.Size(20, 20);
                var window = new System.Drawing.Size(grabber.Width, grabber.Height);
                _rects = _faceClassifier.DetectMultiScale(gray, 1.2, 10, size, window);

                _vmodel.PersonRecognized = _rects.Length;

                if (_rects.Length > 0)
                {                    
                    /*
                    1) save the current rendered faces
                    2) upload the current frame to detect
                    3) verify with trained images
                    */
                    string snapshot = CommonData.TARGET_SNAPSHOT_PAHT + DateTime.Now.ToString().Replace('/', '_').Replace(':', '_') + "." + CommonData.IMAGE_EXT;
                    currentFrame.Save(snapshot);

                    _progressRec.IsIndeterminate = false;
                    _progressRec.IsEnabled = true;

                    var fr = await UploadAndDetectFaces(snapshot);

                    //detect all faces
                    foreach (var trainedFile in CommonData.ImageFiles)
                    {
                        var fileStream = File.OpenRead(trainedFile);
                        var f = await faceServiceClient.DetectAsync(fileStream);
                        _trainedFacesAI.AddRange(f.ToList());
                    }

                    int i = 0;
                    //verify reading from all db
                    foreach (var face in _detectedFaceFromAI)
                    {
                        foreach (var secondFace in _trainedFacesAI)
                        {
                            var res = await faceServiceClient.VerifyAsync(face.FaceId, secondFace.FaceId);

                            PictureModel _model = new PictureModel();
                            _model.ImgSource = new WriteableBitmap(new BitmapImage(new Uri(CommonData.ImageFiles[i])));
                            _model.AIID = face.FaceId.ToString();

                            if (res.IsIdentical)
                            {
                                _model.Name = CommonData.Names[i];
                                _model.ID = i;
                                _model.IsVerified = true;
                                _model.Confidence = res.Confidence;
                            }
                            else
                            {
                                _model.Name = "Unkonwn";
                                _model.IsVerified = false;
                                _model.Confidence = res.Confidence;
                            }

                            var c = _vmodel.Pictures.Where(x => x.AIID == _model.AIID).Count();
                            if (!(c > 0))//adds only if is not already added
                            {
                                _vmodel.Pictures.Add(_model);
                            }

                            i++;
                        }
                    }

                    _progressRec.IsIndeterminate = false;
                    _progressRec.IsEnabled = false;
                }                
            }            

            _imgSource.Source = ImageHelper.ToBitmapSource(currentFrame);
        }


        /// <summary>
        /// Detect faces from by uploading an image asynchronously
        /// </summary>
        /// <param name="imageFilePath">The file path of the image file</param>
        /// <returns>
        /// A Task object represents the future result of detection.
        /// The result will be an array of FaceRectangle objects contains the locations 
        /// of detected faces
        /// </returns>
        private async Task<FaceRectangle[]> UploadAndDetectFaces(string imageFilePath)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    _detectedFaceFromAI = await faceServiceClient.DetectAsync(imageFileStream);

                    var faceRects = _detectedFaceFromAI.Select(face => face.FaceRectangle);

                    return faceRects.ToArray();
                }
            }
            catch (Exception)
            {

                return new FaceRectangle[0];
            }

        }
    }
}
