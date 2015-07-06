using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FaceAuth.Models
{
    public class PictureModel : NotifyBase
    {
        private WriteableBitmap _imgSource;
        public WriteableBitmap ImgSource
        {
            get { return _imgSource; }
            set { _imgSource = value; OnChange("ImgSource"); }
        }


        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnChange("Name"); }
        }


        private int _id;
        public int ID
        {
            get { return _id; }
            set { _id = value; OnChange("ID"); }
        }


        private string _aiID;
        public string AIID
        {
            get { return _aiID; }
            set { _aiID = value; OnChange("AIID"); }
        }


        private bool _isVerified;
        public bool IsVerified
        {
            get { return _isVerified; }
            set { _isVerified = value; OnChange("IsVerified"); }
        }


        private double _confidence;
        public double Confidence
        {
            get { return _confidence; }
            set { _confidence = value; OnChange("Confidence"); }
        }
    }
}
