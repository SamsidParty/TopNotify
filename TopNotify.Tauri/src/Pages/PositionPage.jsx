import { readFile } from "@tauri-apps/plugin-fs"
import { useState } from "react";
import "../CSS/PositionPage.css";


function CalculatePreviewScale() {
    var windowWidth = window.innerWidth;
    var screenWidth = window.currentConfig.__ScreenWidth;
    var containerWidth = (windowWidth - (15 * 16)) * 0.9; // Width Is 90% Of Window - SideBar (15rem)

    return containerWidth / screenWidth;
}

function CalculatePreviewAspectRatio() {
    return window.currentConfig.__ScreenHeight / window.currentConfig.__ScreenWidth;
}

function CalculatePreviewContainerStyle() {
    var previewWidth = CalculatePreviewScale() *  window.currentConfig.__ScreenWidth;

    return { width: previewWidth, height: (previewWidth * CalculatePreviewAspectRatio()) };
}

function CalculateNotificationPreviwStyle() { 

    //The displayed size is 364 * 109, which has a scale of 0.919191917 * 0.717105263 relative to the window

    return {
        width: CalculateNotificationWindowPreviewStyle().width * 0.919191917,
        height: CalculateNotificationWindowPreviewStyle().height * 0.717105263,
        opacity: (window.currentConfig.Opacity != undefined) ? (1 - (window.currentConfig.Opacity / 5)) : 0
    }
}

function CalculateNotificationWindowPreviewStyle() {

    //The window size (not displayed size) of windows notifications are 396 * 152
    var standardWidth = 396 * CalculatePreviewScale();
    var standardHeight = 152 * CalculatePreviewScale();

    var style = { 
        width: (standardWidth * window.currentConfig.__ScreenScale),
        height: (standardHeight * window.currentConfig.__ScreenScale)
    };

    var posX = 0;
    var posY = 0;
    var scaledMainDisplayWidth = CalculatePreviewContainerStyle().width;
    var scaledMainDisplayHeight = CalculatePreviewContainerStyle().height;

    if (!!window.currentConfig.Location) {
        if (window.currentConfig.Location == 0) { // Top left
            posX = 0;
            posY = 0;
        }
        else if (window.currentConfig.Location == 1) { // Top Right
            posX = scaledMainDisplayWidth - style.width;
            posY = 0;
        }
        else if (window.currentConfig.Location == 2) { // Bottom Left
            posX = 0;
            posY = scaledMainDisplayHeight - style.height - (50 * CalculatePreviewScale());
        }
        else if (window.currentConfig.Location == 3) { // Bottom Right
            posX = scaledMainDisplayWidth - style.width;
            posY = scaledMainDisplayHeight - style.height - (50 * CalculatePreviewScale());
        }
        else { // Custom
            posX = window.currentConfig.CustomPositionPercentX / 100 * scaledMainDisplayWidth;
            posY = window.currentConfig.CustomPositionPercentY / 100 * scaledMainDisplayHeight;

            //Make Sure Position Isn't Out Of Bounds
            posX = Math.max(0, Math.min(posX, scaledMainDisplayWidth - style.width));
            posY = Math.max(0, Math.min(posY, scaledMainDisplayHeight - style.height));
        }
    }

    style.left = posX;
    style.top = posY;

    return style;
}

export default function PositionPage() {

    var [wallpaperSource, setWallpaperSource] = useState(null);

    if (!wallpaperSource) {
        setWallpaperSource("/Image/BackgroundDecoration.png");
        setTimeout(async () => {
            var wallpaper = await readFile("C:\\Users\\Public\\Downloads\\topnotify_tempwallpaper.jpg");
            wallpaper = new Blob([wallpaper]);
            wallpaper = URL.createObjectURL(wallpaper);
            setWallpaperSource(wallpaper);
        }, 0);
    }



    return (
        <div className="splitPage positionPage">
            <div style={CalculatePreviewContainerStyle()} className="displayPreview">
                <img className="wallpaper" src={wallpaperSource || "/Image/BackgroundDecoration.png"}></img>
                <div className="notificationWindowPreview" style={CalculateNotificationWindowPreviewStyle()}>
                    <div className="notificationPreview" style={CalculateNotificationPreviwStyle()}>
                        <img src="/Image/NotificationPreview.png"></img>
                    </div>
                </div>
            </div>
        </div>
    )
}