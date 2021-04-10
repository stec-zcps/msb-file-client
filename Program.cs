using System;
using System.Linq;
using Serilog;

namespace msb_file_client
{    
    class Program
    {
        public static void msbCallback_Connected(object sender, System.EventArgs e)
        {
            msbClient.RegisterAsync(msbApplication);
        }
        public static void msbCallback_Disconnected(object sender, System.EventArgs e)
        {
            msbActive = false;
        }
        public static void msbCallback_Registered(object sender, System.EventArgs e)
        {
            msbActive = true;
        }

        static Fraunhofer.IPA.MSB.Client.API.Model.Event fileEvent;
        static Fraunhofer.IPA.MSB.Client.Websocket.MsbClient msbClient;
        static Fraunhofer.IPA.MSB.Client.API.Model.Application msbApplication;
        static bool msbActive = false;
        static void Main(string[] args)
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:yyyy-MM-dd - HH:mm:ss}] [{SourceContext:s}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var service = Service.createService(args[0]);

            Filecontainer.target_dir = service.files_targetpath;

            msbClient = new Fraunhofer.IPA.MSB.Client.Websocket.MsbClient(service.target_interface);
            msbClient.Connected += msbCallback_Connected;
            msbClient.Disconnected += msbCallback_Disconnected;
            msbClient.Registered += msbCallback_Registered;
            msbClient.AutoReconnect = true;
            msbClient.AutoReconnectIntervalInMilliseconds = 10000;

            msbApplication = new Fraunhofer.IPA.MSB.Client.API.Model.Application(service.uuid, service.name, service.description, service.token);

            fileEvent = new Fraunhofer.IPA.MSB.Client.API.Model.Event("fEv", "FileEvent", "File event", typeof(FileObject));
            msbApplication.AddEvent(fileEvent);

            var filecontainer = new Filecontainer();

            var methodInfo = filecontainer.GetType().GetMethod("receiveFile");
            var receiveFunction = new Fraunhofer.IPA.MSB.Client.API.Model.Function("fF", "FileFunction", "File function", methodInfo, filecontainer);
            msbApplication.AddFunction(receiveFunction);

            msbClient.ConnectAsync();

            System.Collections.Generic.Dictionary<string, System.IO.FileInfo> oldFiles = new System.Collections.Generic.Dictionary<string, System.IO.FileInfo>();
            var newFiles = new System.Collections.Generic.Dictionary<string, System.IO.FileInfo>();

            try {
                var oldstatus = System.IO.File.ReadAllText(service.files_sourcepath + "/oldfiles.json");
                oldFiles = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, System.IO.FileInfo>>(oldstatus);
            } catch {
                oldFiles = new System.Collections.Generic.Dictionary<string, System.IO.FileInfo>();
            }

            while(!msbActive)
            {
                System.Threading.Thread.Sleep(5000);
            }

            while(true)
            {
                var fileInfos = new System.IO.DirectoryInfo(service.files_sourcepath).GetFiles();

                foreach (var fileInfo in fileInfos)
                {
                    if (!newFiles.ContainsKey(fileInfo.Name))
                    {
                        if (oldFiles.ContainsKey(fileInfo.Name))
                        {
                            if (oldFiles[fileInfo.Name].LastWriteTime >= fileInfo.LastWriteTime)
                            {
                                continue;
                            }
                        }
                        newFiles.Add(fileInfo.Name, fileInfo);
                    }
                }

                if (msbActive)
                {
                    var toRemove = new System.Collections.Generic.List<string>();
                    foreach (var newFile in newFiles)
                    {
                        var file = new File(newFile.Value.FullName, service.file_splitting_length);
                        var fileEventInstance = new Fraunhofer.IPA.MSB.Client.API.Model.EventData(fileEvent);

                        foreach (var d_ in file.fileObjects)
                        {
                            fileEventInstance.Value = d_;
                            msbClient.PublishAsync(msbApplication, fileEventInstance);
                            System.Threading.Thread.Sleep(100);
                        }

                        toRemove.Add(newFile.Key);

                        if (!msbActive)
                        {
                            break;
                        }
                    }

                    foreach (var rem in toRemove)
                    {
                        oldFiles.Add(rem, newFiles[rem]);
                        newFiles.Remove(rem);
                    }

                    if (toRemove.Count > 0)
                    {
                        var status = Newtonsoft.Json.JsonConvert.SerializeObject(oldFiles);
                        System.IO.File.WriteAllText(service.files_sourcepath + "/oldfiles.json", status);
                    }
                }

                filecontainer.clearByTimelimit(service.file_timelimit);
                
                while(!msbActive)
                {
                    System.Threading.Thread.Sleep(10000);
                }
            }
        }
    }
}
