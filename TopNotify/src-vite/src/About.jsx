import { Button } from "@chakra-ui/react";
import "./CSS/About.css";
import { useState } from "react";
import {TbBrandGithub, TbGlobe, TbLicense, TbShoppingBag, TbStar, TbStarFilled, TbWorld, TbX} from "react-icons/tb";

export default function About() {

    let [version, setVersion] = useState("");

    if (version == "") {
        setVersion(" ...");

        setTimeout(async () => {
            setVersion(await igniteView.commandBridge.GetVersion());
        });
    }

    return (
        <div className={"app loaded about"}>
            <div data-webview-drag className="draggableHeader">
                <h2>About</h2>
            </div>

            <div className="windowCloseButton">
                <Button className="iconButton" onClick={() => window.close()}><TbX/></Button>
            </div>

            <img src="/Image/IconSmall.png"></img>
            <h4>TopNotify{version}</h4>
            <h6>Developed by SamsidParty • Powered by IgniteView</h6>

            <div className="aboutButtons">
                <Button onClick={() => window.open("https://www.samsidparty.com/software/topnotify")}><TbWorld/> Official Website</Button>
                <Button onClick={() => window.open("https://github.com/SamsidParty/TopNotify")}><TbBrandGithub/> GitHub</Button>
                <Button onClick={() => window.open("ms-windows-store://pdp/?ProductId=9pfmdk0qhkqj")}><TbShoppingBag/> Microsoft Store</Button>
                <Button onClick={() => window.open("ms-windows-store://review/?ProductId=9pfmdk0qhkqj")}><TbStar/> Leave A Review</Button>
                <Button onClick={() => window.open("https://github.com/SamsidParty/TopNotify/blob/main/LICENSE")}><TbLicense/> License</Button>
            </div>
        </div>
    );
}