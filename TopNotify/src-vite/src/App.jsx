import { useState } from 'react'
import { Button, Switch, Divider, Container } from '@chakra-ui/react'
import NotificationTransparency from './Transparency'
import ClickThrough from './ClickThrough'
import TestNotification from './TestNotification'
import ManageNotificationSounds from './NotificationSounds'
import Preview from './Preview'
import { DebugMenu } from './DebugMenu'
import ReadAloud from './ReadAloud'
import MonitorSelect from './MonitorSelect'
import SoundInterceptionToggle from './SoundInterceptionToggle'
import { useFirstRender } from './Helper'


window.Config = {
    Location: -1,
    Opacity: 0,
    ReadAloud: false,
    AppReferences: []
}

// Called By C#, Sets The window.Config Object To The Saved Config File
window.SetConfig = async (config) => {
    Config = JSON.parse(config);
    window.setRerender(rerender + 1);
}

window.UploadConfig = () => {

    if (Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    igniteView.commandBridge.WriteConfigFile(JSON.stringify(Config));
    window.setRerender(rerender + 1);
}

window.ChangeSwitch = function (key, e) {
    Config[key] = e.target.checked;
    UploadConfig();
    window.setRerender(rerender + 1);
}

window.ChangeValue = function (key, e) {
    Config[key] = e;
    UploadConfig();
    window.setRerender(rerender + 1);
}

function App() {

    var [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;
    
    if (useFirstRender()) {
        igniteView.commandBridge.invoke("RequestConfig");
    }

    return (
        <div className={'app' + ((rerender > 0) ? " loaded" : "")}>

            <DebugMenu></DebugMenu>

            <div onMouseOver={window.igniteView.dragWindow} className="draggableHeader">
                <img src="/Image/IconTiny.png"></img>
                <h2>TopNotify</h2>
            </div>

            <div className="windowCloseButton">
                <Button className="iconButton" onClick={() => window.close()}>&#xeb55;</Button>
            </div>

            <TestNotification></TestNotification>

            <Preview></Preview>

            <MonitorSelect></MonitorSelect>

            {
                window.errorList?.map((error, i) => {
                    return (<ErrorMessage key={i} error={error}></ErrorMessage>)
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

            <Button onClick={() => igniteView.commandBridge.About()}>&#xeac5; About TopNotify</Button>
        </div>
    )
}

function ErrorMessage(props) {
    return (
        <div className="errorMessage"><h4>&#xea06;</h4>{props.error.Text}</div>
    )
}


export default App
