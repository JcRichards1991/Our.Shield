using System;

namespace Our.Shield.BackofficeAccess.Models
{
    internal class HardResetFileHandler
    {
        private const string Directory = "App_Data\\Temp\\" + nameof(Shield) + "\\" + nameof(BackofficeAccess) + "\\";

        private const string File = "HardReset.txt";

        private bool _hasInit;

        private static string DirectoryPath => AppDomain.CurrentDomain.BaseDirectory + Directory;

        public string FilePath => DirectoryPath + File;

        private string _hardLocation;
        public string HardLocation
        {
            get
            {
                if(!_hasInit)
                {
                    Load();
                }

                return _hardLocation;
            }
            set
            {
                _hasInit = true;
                _hardLocation = value;
            }
        }

        private string _softLocation;
        public string SoftLocation
        {
            get
            {
                if(!_hasInit)
                {
                    Load();
                }

                return _softLocation;
            }
            set
            {
                _hasInit = true;
                _softLocation = value;
            }
        }

        private void Load()
        {
            _hasInit = true;
            
            if(!System.IO.File.Exists(FilePath))
            {
                HardLocation = SoftLocation = null;
                return;
            }

            using (var txtFile = System.IO.File.OpenText(FilePath))
            {
                HardLocation = txtFile.ReadLine();
                SoftLocation = txtFile.ReadLine();
            }
        }

        public void Save()
        {
            System.IO.Directory.CreateDirectory(DirectoryPath);

            using (var txtFile = System.IO.File.CreateText(FilePath))
            {
                txtFile.WriteLine(HardLocation);
                txtFile.WriteLine(SoftLocation);
            }
        }

        public void Delete()
        {
            if(!System.IO.File.Exists(FilePath))
            {
                return;
            }

            System.IO.File.Delete(FilePath);
            HardLocation = SoftLocation = null;
        }
    }
}
