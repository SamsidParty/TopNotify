
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import './CSS/DragMode.css'

addEventListener("mouseup", (e) => {
    if (e.button == 0) {
        //Exit Drag Mode
        window.ExitDragMode();
    }
})

export default function DragMode() {

    var [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;

    document.body.classList.add("dragModeBody");

    return (
        <div className={'dragMode app' + ((rerender > 0) ? " loaded" : "")}>
            <h4>Select Custom Position</h4>
            <p>Move Your Mouse To The Desired Notification Position, Then Left Click To Confirm.</p>
        </div>
    )
}