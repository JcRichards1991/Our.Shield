namespace Our.Shield.BackofficeAccess.Models
{
    using System;

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

        private int? environmentId;
        public int? EnvironmentId
        {
            get
            {
                if(!hasInit)
                {
                    Load();
                }

                return environmentId;
            }
            set
            {
                environmentId = value;
            }
        }

        private void Load()
        {
            hasInit = true;
            
            if(!System.IO.File.Exists(DirectoryPath + File))
            {
                HardLocation = SoftLocation = null;
                EnvironmentId = null;
                return;
            }

            using (var txtFile = System.IO.File.OpenText(DirectoryPath + File))
            {
                HardLocation = txtFile.ReadLine();
                SoftLocation = txtFile.ReadLine();
                int tempId;
                EnvironmentId = int.TryParse(txtFile.ReadLine(), out tempId) ? tempId : (int?) null;
            }
        }

        public void Save()
        {
            System.IO.Directory.CreateDirectory(DirectoryPath);

            using (var txtFile = System.IO.File.CreateText(DirectoryPath + File))
            {
                txtFile.WriteLine(HardLocation);
                txtFile.WriteLine(SoftLocation);
                txtFile.WriteLine(EnvironmentId);
            }
        }

        public void Delete()
        {
            if(!System.IO.File.Exists(DirectoryPath + File))
            {
                return;
            }

            System.IO.File.Delete(DirectoryPath + File);
            HardLocation = SoftLocation = null;
            EnvironmentId = null;
        }
    }
}
