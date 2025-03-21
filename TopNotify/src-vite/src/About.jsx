import { Button } from "@chakra-ui/react";
import "./CSS/About.css";
import { useState } from "react";

export default function About() {

    var [version, setVersion] = useState("");

    if (version == "") {
        setVersion(" ...");

        setTimeout(async () => {
            setVersion(await igniteView.commandBridge.GetVersion());
        });
    }

    return (
        <div className={'app loaded about'}>
            <div onMouseOver={window.igniteView.dragWindow} className="draggableHeader">
                <h2>About</h2>
            </div>

            <div className="windowCloseButton">
                <Button className="iconButton" onClick={() => window.close()}>&#xeb55;</Button>
            </div>

            <img src="/Image/IconSmall.png"></img>
            <h4>TopNotify{version}</h4>
            <h6>Developed by SamsidParty • Powered by IgniteView</h6>

            <div className="aboutButtons">
                <Button onClick={() => window.open("https://www.samsidparty.com/software/topnotify")}>&#xeb54; Official Website</Button>
                <Button onClick={() => window.open("https://github.com/SamsidParty/TopNotify")}>&#xec1c; GitHub</Button>
                <Button onClick={() => window.open("ms-windows-store://pdp/?ProductId=9pfmdk0qhkqj")}>&#xecd8; Microsoft Store</Button>
                <Button onClick={() => window.open("ms-windows-store://review/?ProductId=9pfmdk0qhkqj")}>&#xf6a6; Leave A Review</Button>
                <Button onClick={() => window.open("https://github.com/SamsidParty/TopNotify/blob/main/LICENSE")}>&#xea7b; License</Button>
            </div>
        </div>
    )
}