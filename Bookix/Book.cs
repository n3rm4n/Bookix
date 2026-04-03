using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Bookix
{
    public class Book : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string FilePath { get; set; }

        public string CoverPath { get; set; }

        private int _progress;

        private int _readProgress;
        public int Progress
        {
            get => _progress;
            set
            {
                var clamped = Math.Clamp(value, 0, 100);
                if (_progress != clamped)
                {
                    _progress = clamped;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
