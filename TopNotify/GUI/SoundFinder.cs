using IgniteView.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using TopNotify.Daemon;

namespace TopNotify.GUI
{
    public class SoundFinder
    {
        [Command("FindSounds")]
        public static string FindSounds()
        {
            // Read The Current List Of Sound Packs
            var jsonFile = Util.GetFileResolver().ReadFileAsText("/Meta/SoundPacks.json");
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
            return JsonConvert.SerializeObject(soundPacks);
        }

        /// <summary>
        /// Plays the provided sound ID
        /// </summary>
        [Command("PreviewSound")]
        public static void PreviewSound(string soundID)
        {
            SoundInterceptor.PlaySoundWithoutTimeout(soundID);
        }

        /// <summary>
        /// Returns A List Of WAV Files In The Music Folder
        /// </summary>
        public static string[] GetWAVFilesInMusicFolder()
        {
            try
            {
                var musicFolder = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Music");

                // Music folder doesn't always exist https://github.com/SamsidParty/TopNotify/issues/40#issuecomment-2692353622
                if (Directory.Exists(musicFolder))
                {
                    return Directory.GetFiles(musicFolder, "*.wav", SearchOption.AllDirectories);
                }
            }
            catch { }

            return new string[0];
        }
    }
}
