using System;
using System.Collections.Generic;

namespace msb_file_client
{
    public class Filecontainer
    {
        public static string target_dir = "";
        System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<FileObject>> fileEvents = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<FileObject>>();
        System.Collections.Generic.Dictionary<string, DateTime> lastEvents = new System.Collections.Generic.Dictionary<string, DateTime>();

        public void receiveFile([Fraunhofer.IPA.MSB.Client.API.Attributes.MsbFunctionParameter(Name = "fileObject")]FileObject fileObject, Fraunhofer.IPA.MSB.Client.API.Model.FunctionCallInfo info)
        {
            var id = fileObject.id;

            if (!fileEvents.ContainsKey(id))
            {
                fileEvents.Add(id, new System.Collections.Generic.List<FileObject>());                
            }

            if (!lastEvents.ContainsKey(id))
            {
                lastEvents.Add(id, DateTime.Now);
            } else {
                lastEvents[id] = DateTime.Now;
            }

            var ev = fileEvents[id];
            ev.Add(fileObject);

            if (ev.Count == ev[0].count)
            {
                var file = new File(ev);
                string target_path;
                if (target_dir != "")
                {
                    target_path = target_dir + "/" + ev[0].filename;
                    target_path.Replace("//", "/");
                } else {
                    target_path = ev[0].filename;
                }
                file.writeToFile(target_path);
                fileEvents.Remove(id);
                lastEvents.Remove(id);
            }
        }

        public void clearByTimelimit(int timelimitInMs)
        {
            var now = DateTime.Now;
            var limit = new TimeSpan(0, 0, 0, 0, timelimitInMs);

            var toRemove = new List<string>();
            try
            {
                foreach (var lastEvent in lastEvents)
                {
                    if (now - lastEvent.Value > limit)
                    {
                        toRemove.Add(lastEvent.Key);
                    }
                }

                foreach (var key in toRemove)
                {
                    fileEvents.Remove(key);
                    lastEvents.Remove(key);
                }
            } catch {

            }
        }
    }
}