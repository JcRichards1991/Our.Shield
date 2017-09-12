using System;

namespace Our.Shield.BackofficeAccess.Models
{
    internal class HardResetFileHandler
    {
        private const string Directory = "App_Data\\Temp\\" + nameof(Shield) + "\\" + nameof(BackofficeAccess) + "\\";

        private const string File = "HardReset.txt";

        private bool hasInit = false;

        private string DirectoryPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + Directory;
            }
        }

        public string FilePath => DirectoryPath + File;

        private string hardLocation;
        public string HardLocation
        {
            get
            {
                if(!hasInit)
                {
                    Load();
                }

                return hardLocation;
            }
            set
            {
                hasInit = true;
                hardLocation = value;
            }
        }

        private string softLocation;
        public string SoftLocation
        {
            get
            {
                if(!hasInit)
                {
                    Load();
                }

                return softLocation;
            }
            set
            {
                hasInit = true;
                softLocation = value;
            }
        }

        private void Load()
        {
            hasInit = true;
            
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
