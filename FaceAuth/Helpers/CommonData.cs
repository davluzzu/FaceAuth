using Emgu.CV;
using Emgu.CV.Structure;
using FaceAuth.Models;
using FaceAuth.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FaceAuth.Helpers
{
    public static class CommonData
    {
        public static string TARGET_SNAPSHOT_FOLDER = "Snapshots";
        public static string TARGET_FOLDER = "TrainedFaces";
        public static string LABELS_FILE = "TrainedLabels.txt";
        public static string TARGET_PATH = Environment.CurrentDirectory + "/" + TARGET_FOLDER + "/";
        public static string TARGET_SNAPSHOT_PAHT = Environment.CurrentDirectory + "/" + TARGET_SNAPSHOT_FOLDER + "/";
        public static string IMAGE_EXT = "jpg";

        public static PictureViewModel PicturesVM = new PictureViewModel();
        public static List<Image<Gray, byte>> TrainingImages = new List<Image<Gray, byte>>();
        public static List<string> Names = new List<string>();
        public static string[] ImageFiles = null;

        public static void LoadSavedData()
        {
            if (File.Exists(TARGET_PATH + LABELS_FILE) && PicturesVM.Pictures.Count <= 0)
            {
                ImageFiles = Directory.GetFiles(TARGET_PATH, "*."+ IMAGE_EXT);
                Names = File.ReadAllText(TARGET_PATH + LABELS_FILE).Split('%')
                    .TakeWhile(x => !string.IsNullOrEmpty(x))
                    .ToList();
                int countLabels = 0;

                foreach (var f in ImageFiles)
                {
                    #region filling VModel
                    PictureModel pm = new PictureModel();
                    pm.ImgSource = new WriteableBitmap(new BitmapImage(new Uri(f)));
                    pm.Name = Names[countLabels];
                    pm.ID = countLabels;

                    PicturesVM.Pictures.Add(pm);
                    #endregion

                    #region Filling Training Images
                    TrainingImages.Add(new Image<Gray, byte>(f));
                    #endregion

                    countLabels++;
                }
            }
        }

    }
}
