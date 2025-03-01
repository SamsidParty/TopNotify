using IgniteView.Core;
using MimeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Common;
using WatsonWebserver.Core;

namespace TopNotify.GUI
{
    internal class WallpaperFinder
    {
        public static async Task WallpaperRoute(HttpContextBase ctx)
        {
            if (
                ctx.Request.Url != null &&
                CopyWallpaper() != null
            )
            {

                // Send The Current Wallpaper
                var wallpaperFile = CopyWallpaper(); 

                var fileStream = new FileStream(wallpaperFile, FileMode.Open, FileAccess.Read);

                if (fileStream.CanSeek)
                {
                    ctx.Response.ContentLength = fileStream.Length;
                }

                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "image/jpeg";
                await ctx.Response.Send(fileStream.Length, fileStream);

                await fileStream.DisposeAsync();
            }

            ctx.Response.StatusCode = 404;
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
