
var previewScale = 0.241666667; // Relative to actual scale

function CalculatePreviewContainerStyle() {
    var previewWidth = 464;
    var aspect = 0.5625; // 16:9

    if (!!window.Config.__ScreenWidth) {
        aspect = window.Config.__ScreenHeight / window.Config.__ScreenWidth;
    }

    return { width: previewWidth, height: (previewWidth * aspect) };
}

function CalculateTaskbarPreviewStyle() {
    //Windows taskbar is 48px high
    var standardHeight = 48 * previewScale;

    return { 
        height: !!window.Config.__ScreenScale ? (standardHeight * window.Config.__ScreenScale) : standardHeight
    };
}

function CalculateNotificationWindowPreviewStyle() {

    //The window size (not displayed size) of windows notifications are 396 * 152
    var standardWidth = 396 * previewScale;
    var standardHeight = 152 * previewScale;

    var style = { 
        width: !!window.Config.__ScreenScale ? (standardWidth * window.Config.__ScreenScale) : standardWidth,
        height: !!window.Config.__ScreenScale ? (standardHeight * window.Config.__ScreenScale) : standardHeight
    };

    var posX = 0;
    var posY = 0;
    var scaledMainDisplayWidth = CalculatePreviewContainerStyle().width;
    var scaledMainDisplayHeight = CalculatePreviewContainerStyle().height;

    if (!!window.Config.Location) {
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
    }
}

export default function Preview() {
    return (
        <div className="previewContainer" style={CalculatePreviewContainerStyle()}>
            <div className="notificationWindowPreview" style={CalculateNotificationWindowPreviewStyle()}>
                <div className="notificationPreview" style={CalculateNotificationPreviwStyle()}></div>
            </div>
            <div className="taskbarPreview" style={CalculateTaskbarPreviewStyle()}><img src="/Image/Taskbar.png"></img></div>
        </div>
    )
}