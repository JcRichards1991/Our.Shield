using System;
using System.IO;

namespace Our.Shield.BackofficeAccess.Models
{
    internal class HardResetFileHandler
    {
        private const string DirectoryLocation = "App_Data\\Temp\\" + nameof(Shield) + "\\" + nameof(BackofficeAccess) + "\\";

        private const string FileName = "HardReset.txt";

        private bool _hasInit;

        private static string DirectoryPath => AppDomain.CurrentDomain.BaseDirectory + DirectoryLocation;

        public string FilePath => DirectoryPath + FileName;

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
            
            if(!File.Exists(FilePath))
            {
                HardLocation = SoftLocation = null;
                return;
            }

            using (var txtFile = File.OpenText(FilePath))
            {
                HardLocation = txtFile.ReadLine();
                SoftLocation = txtFile.ReadLine();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(DirectoryPath);

            using (var txtFile = File.CreateText(FilePath))
            {
                txtFile.WriteLine(HardLocation);
                txtFile.WriteLine(SoftLocation);
            }
        }

        public void Delete()
        {
            if(!File.Exists(FilePath))
            {
                return;
            }

            File.Delete(FilePath);
            HardLocation = SoftLocation = null;
        }
    }
}
