using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliDown
{
    public class DownloadListElement:INotifyPropertyChanged
    {
        public DownloadListElement(string fileName, string fileUrl)
        {
            _willDownload = true;
            _canDownload = false;
            _downloadPercentage = 0;
            _fileName = fileName;
            this.fileUrl = fileUrl;
        }
        private bool _willDownload;

        public bool WillDownload
        {
            get => _willDownload;
            set
            {
                _willDownload = value;
                OnPropertyChanged(this, "WillDownload");
            }
        }
        private bool _canDownload;

        public bool CanDownload
        {
            get => _canDownload;
            set
            {
                _canDownload = value;
                OnPropertyChanged(this, "CanDownload");
            }
        }
        private string _fileName;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(this,"FileName");
            }
        }

        public string fileUrl { get; set; }

        private int _downloadPercentage;

        public int DownloadPercentage
        {
            get => _downloadPercentage;
            set
            {
                _downloadPercentage = value;
                OnPropertyChanged(this, "DownloadPercentage");
            }
        }
        private void OnPropertyChanged(object sender, string propertyName)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
