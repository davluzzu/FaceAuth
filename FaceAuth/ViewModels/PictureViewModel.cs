using FaceAuth.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAuth.ViewModels
{
    public class PictureViewModel : NotifyBase
    {

        private ObservableCollection<PictureModel> _pictures = new ObservableCollection<PictureModel>();
        public ObservableCollection<PictureModel> Pictures
        {
            get { return _pictures; }
            set { _pictures = value; OnChange("Pictures"); }
        }


        private int _personRecognized;
        public int PersonRecognized
        {
            get { return _personRecognized; }
            set { _personRecognized = value; OnChange("PersonRecognized"); }
        }
    }
}
