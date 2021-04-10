using System;
using Serilog;

namespace msb_file_client
{    public class Service
    {
        public string uuid;
        public string name;
        public string description;
        public string token;
        public string target_interface;
        public int file_splitting_length;
        public int file_timelimit;
        public string files_sourcepath;
        public string files_targetpath;

        public static Service createService (string file)
        {
            if (!System.IO.File.Exists(file))
            {
                return null;
            } else {
                var s = System.IO.File.ReadAllText(file);
                var r = Newtonsoft.Json.JsonConvert.DeserializeObject<Service>(s);
                return r;
            }
        }
    }
}