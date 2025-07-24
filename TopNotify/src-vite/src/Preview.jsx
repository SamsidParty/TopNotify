
import { Button } from "@chakra-ui/react";
import {
    TbChevronsDownLeft,
    TbChevronsDownRight,
    TbChevronsUpLeft,
    TbChevronsUpRight,
    TbCurrentLocation
} from "react-icons/tb";
import "./CSS/Preview.css";

let previewWidth = 352;
let previewScale = previewWidth / 1920; // Relative to actual scale

function CalculatePreviewContainerStyle() {
    let aspect = 0.5625; // 16:9

    if (window.Config.__ScreenWidth) {
        aspect = window.Config.__ScreenHeight / window.Config.__ScreenWidth;
    }

    return { width: previewWidth, height: (previewWidth * aspect), backgroundImage: `url('${igniteView.resolverURL + "/wallpaper.jpg"}')` };
}

function CalculateTaskbarPreviewStyle() {
    //Windows taskbar is 48px high
    let standardHeight = 48 * previewScale;

    return { 
        height: window.Config.__ScreenScale ? (standardHeight * window.Config.__ScreenScale) : standardHeight
    };
}

function CalculateNotificationWindowPreviewStyle() {

    //The window size (not displayed size) of windows notifications are 396 * 152
    let standardWidth = 396 * previewScale;
    let standardHeight = 152 * previewScale;

    let style = { 
        width: window.Config.__ScreenScale ? (standardWidth * window.Config.__ScreenScale) : standardWidth,
        height: window.Config.__ScreenScale ? (standardHeight * window.Config.__ScreenScale) : standardHeight
    };

    let posX = 0;
    let posY = 0;
    let scaledMainDisplayWidth = CalculatePreviewContainerStyle().width;
    let scaledMainDisplayHeight = CalculatePreviewContainerStyle().height;

    if (window.Config.Location) {
        if (window.Config.Location == 0) { // Top left
            posX = 0;
            posY = 0;
        }
        else if (window.Config.Location == 1) { // Top Right
            posX = scaledMainDisplayWidth - style.width;
            posY = 0;
        }
        else if (window.Config.Location == 2) { // Bottom Left
            posX = 0;
            posY = scaledMainDisplayHeight - style.height - (50 * previewScale);
        }
        else if (window.Config.Location == 3) { // Bottom Right
            posX = scaledMainDisplayWidth - style.width;
            posY = scaledMainDisplayHeight - style.height - (50 * previewScale);
        }
        else { // Custom
            posX = window.Config.CustomPositionPercentX / 100 * scaledMainDisplayWidth;
            posY = window.Config.CustomPositionPercentY / 100 * scaledMainDisplayHeight;

            //Make Sure Position Isn't Out Of Bounds
            posX = Math.max(0, Math.min(posX, scaledMainDisplayWidth - style.width));
            posY = Math.max(0, Math.min(posY, scaledMainDisplayHeight - style.height));
        }
    }

    style.left = posX;
    style.top = posY;

    return style;
}

function CalculateNotificationPreviwStyle() { 

    //The displayed size is 364 * 109, which has a scale of 0.919191917 * 0.717105263 relative to the window

    return {
        width: CalculateNotificationWindowPreviewStyle().width * 0.919191917,
        height: CalculateNotificationWindowPreviewStyle().height * 0.717105263,
        opacity: (Config.Opacity != undefined) ? (1 - (Config.Opacity / 5)) : 0
    };
}

export default function Preview() {
    return (
        <div className="previewContainer" style={CalculatePreviewContainerStyle()}>
            <div className="notificationWindowPreview" style={CalculateNotificationWindowPreviewStyle()}>
                <div className="notificationPreview" style={CalculateNotificationPreviwStyle()}>
                    <img src="/Image/NotificationPreview.png"></img>
                </div>
            </div>
            <div className="locationSelect">
                <Button className="locationSelectButton" onClick={() => SetPresetPosition(0)} ><TbChevronsUpLeft/></Button>
                <Button className="locationSelectButton" onClick={() => SetPresetPosition(1)} ><TbChevronsUpRight/></Button>
                <Button className="locationSelectButton customLocationSelectButton" onClick={() => igniteView.commandBridge.EnterDragMode()}><TbCurrentLocation/></Button>
                <Button className="locationSelectButton" onClick={() => SetPresetPosition(2)} ><TbChevronsDownLeft/></Button>
                <Button className="locationSelectButton" onClick={() => SetPresetPosition(3)} ><TbChevronsDownRight/></Button>
            </div>
            <div className="taskbarPreview" style={CalculateTaskbarPreviewStyle()}><img src="/Image/Taskbar.png"></img></div>
        </div>
    );
}


// 0 = TopLeft,
// 1 = TopRight,
// 2 = BottomLeft,
// 3 = BottomRight,
// 4 = Custom
function SetPresetPosition(position) {
    window.ChangeValue("Location", position);
}