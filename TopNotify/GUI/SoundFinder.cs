using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.GUI
{
    public class SoundFinder
    {

        public static bool InterceptSoundRequest(HttpListenerContext context)
        {

            // Check If The Request Is For List Of Sound Packs
            if (
                context.Request.Url != null &&
                context.Request.Url.LocalPath.EndsWith("SoundPacks.json")
            )
            {
                // Read The Current List Of Sound Packs
                var jsonFile = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WWW", "Meta", "SoundPacks.json"));
                var soundPacks = JsonConvert.DeserializeObject<List<ExpandoObject>>(jsonFile);

                // Inject Files From Music Folder Into The JSON File
                dynamic packToInject = soundPacks.Where((dynamic pack) => pack.ID == "custom_sound_path").FirstOrDefault();
                var wavFiles = GetWAVFilesInMusicFolder();

                foreach (var wavFile in wavFiles)
                {
                    dynamic soundToInject = new ExpandoObject();
                    soundToInject.Path = "custom_sound_path/" + wavFile;
                    soundToInject.Name = Path.GetFileNameWithoutExtension(wavFile);
                    soundToInject.Icon = "/Image/Sound.svg";
                    packToInject.Sounds.Add(soundToInject);
                }

                // Send To GUI
                jsonFile = JsonConvert.SerializeObject(soundPacks);
                var input = Encoding.UTF8.GetBytes(jsonFile);

                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Content-Type", "application/json");
                context.Response.OutputStream.Write(input, 0, input.Length);
                context.Response.OutputStream.Flush();

                context.Response.StatusCode = (int)HttpStatusCode.OK;

                return true;
            }

                return false;
        }

        /// <summary>
        /// Returns A List Of WAV Files In The Music Folder
        /// </summary>
        public static string[] GetWAVFilesInMusicFolder()
        {
            return Directory.GetFiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Music"), "*.wav", SearchOption.AllDirectories);
        }
    }
}
