import { useState } from 'react'
import { Button, Switch, Divider, Select } from '@chakra-ui/react'
import NotificationTransparency from './Transparency'
import ClickThrough from './ClickThrough'
import TestNotification from './TestNotification'
import ManageNotificationSounds from './NotificationSounds'
import Preview from './Preview'
import { DebugMenu } from './DebugMenu'
import './IPCClient'
import ReadAloud from './ReadAloud'
import MonitorSelect from './MonitorSelect'


window.Config = {
    Location: -1,
    Opacity: 0,
    ReadAloud: false,
    AppReferences: []
}

// Called By C#, Sets The window.Config Object To The Saved Config File
window.SetConfig = (e) => {
    Config = JSON.parse(e);
    window.setRerender(rerender + 1);
}

window.UploadConfig = () => {

    if (Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    window.WriteConfigFile(JSON.stringify(Config));
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
    
    return (
        <div className={'app' + ((rerender > 0) ? " loaded" : "")}>

            <DebugMenu></DebugMenu>

            <h2>TopNotify</h2>

            <Preview></Preview>

            <Divider />

            <MonitorSelect></MonitorSelect>

            {
                window.errorList.map((error, i) => {
                    return (<ErrorMessage key={i} error={error}></ErrorMessage>)
                })
            }

            <Divider />

            <TestNotification></TestNotification>

            <Divider />

            <ClickThrough></ClickThrough>

            <Divider />

            <NotificationTransparency></NotificationTransparency>

            <Divider />

            <ReadAloud></ReadAloud>

            <Divider />
            
            <ManageNotificationSounds></ManageNotificationSounds>

            <br></br>

            <div className="footer">
                <img className="footerLogo" src="/Image/PartyWordmarkIconMono.png"></img>
            </div>
        </div>
    )
}

function ErrorMessage(props) {
    return (
        <div className="errorMessage"><h4>&#xea06;</h4>{props.error.Text}</div>
    )
}


export default App
