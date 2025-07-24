import { Button, Container, Divider } from "@chakra-ui/react";
import { useState } from "react";
import {TbAlertTriangle, TbCurrencyDollar, TbInfoCircle, TbX} from "react-icons/tb";
import ClickThrough from "./ClickThrough";
import { DebugMenu } from "./DebugMenu";
import { useFirstRender } from "./Helper";
import MonitorSelect from "./MonitorSelect";
import ManageNotificationSounds from "./NotificationSounds";
import Preview from "./Preview";
import ReadAloud from "./ReadAloud";
import SoundInterceptionToggle from "./SoundInterceptionToggle";
import TestNotification from "./TestNotification";
import NotificationTransparency from "./Transparency";

window.Config = {
    Location: -1,
    Opacity: 0,
    ReadAloud: false,
    AppReferences: []
};

// Called By C#, Sets The window.Config Object To The Saved Config File
window.SetConfig = async (config) => {
    Config = JSON.parse(config);
    window.setRerender(rerender + 1);
};

window.UploadConfig = () => {

    if (Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    igniteView.commandBridge.WriteConfigFile(JSON.stringify(Config));
    window.setRerender(rerender + 1);
};

window.ChangeSwitch = function (key, e) {
    Config[key] = e.target.checked;
    UploadConfig();
    window.setRerender(rerender + 1);
};

window.ChangeValue = function (key, e) {
    Config[key] = e;
    UploadConfig();
    window.setRerender(rerender + 1);
};

function App() {

    let [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;
    
    if (useFirstRender()) {
        igniteView.commandBridge.invoke("RequestConfig");
    }

    return (
        <div className={"app" + ((rerender > 0) ? " loaded" : "")}>

            <DebugMenu></DebugMenu>

            <div data-webview-drag className="draggableHeader">
                <img src="/Image/IconTiny.png"></img>
                <h2>TopNotify</h2>
            </div>

            <div className="windowCloseButton">
                <Button className="iconButton" onClick={() => window.close()}><TbX /></Button>
            </div>

            <TestNotification></TestNotification>

            <Preview></Preview>

            <MonitorSelect></MonitorSelect>

            {
                window.errorList?.map((error, i) => {
                    return (<ErrorMessage key={i} error={error}></ErrorMessage>);
                })
            }

            <Container>
                <ClickThrough></ClickThrough>
                <Divider />
                <NotificationTransparency></NotificationTransparency>
            </Container>

            <Container>
                <ReadAloud></ReadAloud>
                <Divider />
                <SoundInterceptionToggle></SoundInterceptionToggle>
                <Divider />
                <ManageNotificationSounds></ManageNotificationSounds>
            </Container>

            <div className='aboutButtons'>
                <Button onClick={() => igniteView.commandBridge.About()}><TbInfoCircle/>About TopNotify</Button>
                <Button onClick={() => igniteView.commandBridge.Donate()}><TbCurrencyDollar/>Donate</Button>
            </div>
        </div>
    );
}

function ErrorMessage(props) {
    return (
        <div className="errorMessage"><TbAlertTriangle/>{props.error.Text}</div>
    );
}


export default App;
