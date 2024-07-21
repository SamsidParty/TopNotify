
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import './DragMode.css'

addEventListener("keyup", (e) => {
    if (e.key == "Escape" && window.location.href.includes("?drag")) {
        //Exit Drag Mode
        window.ExitDragMode();
        setRerender(-2);
    }
})

export default function DragMode() {

    var [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;

    //Load Main Page
    if (rerender == -2) {
        window.location.href = "/";
    }

    document.body.classList.add("dragModeBody");

    return (
        <div className={'dragMode app' + ((rerender > 0) ? " loaded" : "")}>
            <h2>Reposition Notifications</h2>
            <Button className="iconButton exitDragModeButton">
                &#xede3;
            </Button>
        </div>
    )
}