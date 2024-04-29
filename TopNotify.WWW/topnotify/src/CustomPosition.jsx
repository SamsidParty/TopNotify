
import { useState } from "react";

addEventListener("keyup", (e) => {
    if (e.key == "Escape" && window.location.href.includes("?drag")) {
        //Exit Drag Mode
        CallCSharp("SamsidParty_TopNotify.Frontend, TopNotify", "ExitDragMode");
        setRerender(-2);
    }
})

export default function CustomPosition() {

    var [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;

    //Load Main Page
    if (rerender == -2) {
        window.location.href = "/";
    }

    if (rerender == 0) {
        CallCSharp("SamsidParty_TopNotify.Frontend, TopNotify", "EnterDragMode"); // Tell C# To Resize The Window To Be The Same Size As Notifications
    }

    document.body.style.padding = "0";
    document.body.style.paddingLeft = "15px";

    return (
        <div className={'dragMode app' + ((rerender > 0) ? " loaded" : "")}>
            <h2>Select Custom Position</h2>
            <p>Drag This Window To The Position You Would Like Notifications To Appear, Then Press Escape. Don't Close This Window.</p>
        </div>
    )
}