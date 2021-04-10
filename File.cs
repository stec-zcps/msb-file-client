using System;
using System.IO;

namespace msb_file_client
{
    
    public class File
    {
        public static bool IsFileLocked(string filename)
        {
            bool Locked = false;
            try
            {
                System.IO.FileStream fs =
                    System.IO.File.Open(filename, System.IO.FileMode.OpenOrCreate,
                    System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                fs.Close();
            }
            catch (System.IO.IOException ex)
            {
                Locked = true;
            }
            return Locked;
        }

        public System.Collections.Generic.List<FileObject> fileObjects;            

        public void writeToFile(string path)
        {
            fileObjects.Sort();

            var file__ = "";

            foreach (var fileObject in fileObjects)
            {
                file__ += fileObject.content;
            }

            //var file_ = Packaging.DecodeBase64(file__);
            //var file = Packaging.Unzip(file_);
            //System.IO.File.WriteAllText(path, file);

            var file = Packaging.DecodeBase64(file__);
            System.IO.File.WriteAllBytes(path, file);
        }

        public File(System.Collections.Generic.List<FileObject> fileObjects_)
        {
            fileObjects = fileObjects_;
        }

        public File(string path, int maxSizeFilePart)
        {
            while(IsFileLocked(path))
            {
                System.Threading.Thread.Sleep(10);
            }
            var file = System.IO.File.ReadAllBytes(path);
            var file__ = Packaging.EncodeBase64(file);

            //var file = System.IO.File.ReadAllText(path);
            //var file_ = Packaging.Zip(file);
            //var file__ = Packaging.EncodeBase64(file_);

            fileObjects = new System.Collections.Generic.List<FileObject>();

            var count = (uint)System.Math.Ceiling((double)file__.Length / maxSizeFilePart);

            int runner = 0;

            var id_ = System.Guid.NewGuid().ToString();
            var fname_ = System.IO.Path.GetFileName(path);

            for(uint i = 0; i < count; ++i)
            {
                var fileObject = new FileObject(){
                        id = id_,
                        filename = fname_,
                        index = i, 
                        count = count
                    };

                if (i < count - 1)
                {
                    fileObject.content = file__.Substring(runner, maxSizeFilePart);
                    runner += maxSizeFilePart;
                } else {
                    fileObject.content = file__.Substring(runner);
                }

                fileObjects.Add(fileObject);                    
            }
        }
    }
}