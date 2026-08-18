using IgniteView.Core;
using IgniteView.FileDialogs;
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
        public static string ImportedSoundFolder => Path.Join(Settings.GetAppDataFolder(), "NotificationSounds", "imported");

        // Formats Windows Media Foundation decodes natively on a stock install - what
        // MediaPlayer in SoundInterceptor can actually play. Deliberately excludes OGG,
        // which needs a separate Store codec extension on some systems and isn't reliably
        // available out of the box.
        static readonly string[] SupportedExtensions = { "wav", "mp3", "aac", "m4a", "wma", "flac" };

        [Command("FindSounds")]
        public static string FindSounds()
        {
            // Read The Current List Of Sound Packs
            var jsonFile = Util.GetFileResolver().ReadFileAsText("/Meta/SoundPacks.json");
            var soundPacks = JsonConvert.DeserializeObject<List<ExpandoObject>>(jsonFile);

            // Inject Files From Music Folder Into The JSON File
            dynamic packToInject = soundPacks.Where((dynamic pack) => pack.ID == "custom_sound_path").FirstOrDefault();
            var soundFiles = GetImportedSoundFiles();

            foreach (var soundFile in soundFiles)
            {
                dynamic soundToInject = new ExpandoObject();
                soundToInject.Path = "custom_sound_path/" + soundFile;
                soundToInject.Name = Path.GetFileNameWithoutExtension(soundFile);
                soundToInject.Icon = "/Image/Sound.svg";
                packToInject.Sounds.Add(soundToInject);
            }

            // Send To GUI
            return JsonConvert.SerializeObject(soundPacks);
        }

        /// <summary>
        /// Opens a file dialog to select a sound file and imports it
        /// </summary>
        [Command("ImportSound")]
        public static string[] ImportSound()
        {
            // A single combined filter, not one FileFilter per extension - keeps the dialog's
            // dropdown to one "Audio Files" entry instead of listing each format separately.
            // The underlying dialog is Native File Dialog Extended (NFDe, via IgniteView's
            // NFDBindings) - confirmed by decompiling IgniteView.FileDialogs.dll that Pattern
            // is passed through to NFDFilterU8.Spec verbatim, with zero transformation. NFDe's
            // spec format is a COMMA-separated extension list, not semicolon-separated - a
            // semicolon-joined pattern silently matches nothing.
            var filterName = $"Audio Files ({string.Join(", ", SupportedExtensions.Select(ext => ext.ToUpper()))})";
            var filters = new[] { new FileFilter(filterName, string.Join(",", SupportedExtensions)) };
            var soundPath = FileDialog.PickFile(filters);

            if (!string.IsNullOrEmpty(soundPath) && File.Exists(soundPath))
            {
                var extension = Path.GetExtension(soundPath).ToLower().TrimStart('.');

                if (!SupportedExtensions.Contains(extension))
                {
                    return new string[0];
                }

                GetImportedSoundFiles(); // Makes sure ImportedSoundFolder exists
                var soundName = Path.GetFileNameWithoutExtension(soundPath);

                // Prevent duplicate files
                while (File.Exists(Path.Join(ImportedSoundFolder, soundName + "." + extension)))
                {
                    soundName += "_";
                }

                var copiedSoundPath = Path.Join(ImportedSoundFolder, soundName + "." + extension);
                File.Copy(soundPath, copiedSoundPath, true);
                return new string[] { "custom_sound_path/" + copiedSoundPath, Path.GetFileNameWithoutExtension(soundPath) };
            }

            return new string[0];
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
        /// Returns A List Of Supported-Format Sound Files In The Music Folder And The Imported-Sounds Folder
        /// </summary>
        public static string[] GetImportedSoundFiles()
        {
            try
            {
                var musicFolder = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Music");

                if (!Directory.Exists(ImportedSoundFolder)) { Directory.CreateDirectory(ImportedSoundFolder); }

                var importedFiles = SupportedExtensions
                    .SelectMany(ext => Directory.GetFiles(ImportedSoundFolder, "*." + ext, SearchOption.AllDirectories))
                    .ToArray();

                // Music folder doesn't always exist https://github.com/SamsidParty/TopNotify/issues/40#issuecomment-2692353622
                if (Directory.Exists(musicFolder))
                {
                    var musicFiles = SupportedExtensions
                        .SelectMany(ext => Directory.GetFiles(musicFolder, "*." + ext, SearchOption.AllDirectories));
                    return musicFiles.Concat(importedFiles).ToArray();
                }

                return importedFiles;
            }
            catch { }

            return new string[0];
        }
    }
}
