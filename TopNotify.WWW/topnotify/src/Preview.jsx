
function CalculatePreviewContainerStyle() {
    var previewWidth = 464;
    var aspect = 0.5625; // 16:9

    if (!!window.Config.__ScreenWidth) {
        aspect = window.Config.__ScreenHeight / window.Config.__ScreenWidth;
    }

    return { width: previewWidth, height: (previewWidth * aspect) };
}

function CalculateTaskbarPreviewStyle() {
    //Windows Taskbar Is 48px
    //In The Small Preview, It Should Be 11.6px, Scaled
    return { 
        height: !!window.Config.__ScreenScale ? (11.6 * window.Config.__ScreenScale) : 11.6
    };
}

function CalculateNotificationPreviewStyle() {
    return {};
}

export default function Preview() {
    return (
        <div className="previewContainer" style={CalculatePreviewContainerStyle()}>
            <div className="notificationPreview" style={CalculateNotificationPreviewStyle()}></div>
            <div className="taskbarPreview" style={CalculateTaskbarPreviewStyle()}><img src="/Image/Taskbar.png"></img></div>
        </div>
    )
}