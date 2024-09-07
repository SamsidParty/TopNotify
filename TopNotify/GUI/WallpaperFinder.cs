using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;

namespace TopNotify.GUI
{
    internal class WallpaperFinder
    {
        public static bool InterceptWallpaperRequest(HttpListenerContext context)
        {

            // Check If The Request Is For The Wallpaper
            if (
                context.Request.Url != null &&
                context.Request.Url.LocalPath.EndsWith("BackgroundDecoration.jpg") &&
                CopyWallpaper() != null
            )
            {
                // Send The Current Wallpaper
                var wallpaperFile = CopyWallpaper(); 

                var input = new FileStream(wallpaperFile, FileMode.Open, FileAccess.Read);

                if (input.CanSeek)
                {
                    context.Response.ContentLength64 = input.Length;
                }

                context.Response.AddHeader("Content-Type", "image/jpeg");

                byte[] buffer = new byte[1024 * 32];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                context.Response.OutputStream.Flush();

                context.Response.StatusCode = (int)HttpStatusCode.OK;

                return true;
            }

            return false;
        }

        public static string CopyWallpaper()
        {
            //Workaround For File System Write Virtualization
            //The UWP Runtime Won't Let Us Read From AppData
            //So Call CMD To Copy It Into A Location That We Can Access

            var copiedWallpaperPath = "C:\\Users\\Public\\Downloads\\topnotify_tempwallpaper.jpg";

            if (File.Exists(copiedWallpaperPath))
            {
                return copiedWallpaperPath;
            }

            Util.SimpleCMD("copy /b/v/y \"%APPDATA%\\Microsoft\\Windows\\Themes\\TranscodedWallpaper\" \"C:\\Users\\Public\\Downloads\\topnotify_tempwallpaper.jpg\"");

            return File.Exists(copiedWallpaperPath) ? copiedWallpaperPath : null;
        }

        public static void CleanUp()
        {
            var copiedWallpaperPath = "C:\\Users\\Public\\Downloads\\topnotify_tempwallpaper.jpg";

            if (File.Exists(copiedWallpaperPath))
            {
                File.Delete(copiedWallpaperPath);
            }
        }
    }
}
