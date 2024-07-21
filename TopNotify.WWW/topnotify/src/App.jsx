import { useState } from 'react'
import { Button, Switch, Divider } from '@chakra-ui/react'
import NotificationTransparency from './Transparency'
import RunOnStartup from './RunOnStartup'
import ClickThrough from './ClickThrough'
import TestNotification from './TestNotification'
import Preview from './Preview'
import { DebugMenu } from './DebugMenu'


window.Config = {
    Location: -1,
    RunOnStartup: true,
    Opacity: 0,
    SoundPath: "windows/win11"
}

window.SetConfig = (e) => {
    Config = JSON.parse(e);
    window.setRerender(rerender + 1);
}

window.isUWP = false;
window.SetIsUWP = (e) => {
    console.log("Setting UWP Mode To " + e);
    window.isUWP = (e == "True" ? true : false);
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


    //Load Drag Page
    if (rerender == -1) {
        window.EnterDragMode();
        window.location.href = "?drag";
    }
    
    return (
        <div className={'app' + ((rerender > 0) ? " loaded" : "")}>

            <DebugMenu></DebugMenu>

            <h2>TopNotify</h2>

            <Preview></Preview>

            <Divider />

            <TestNotification></TestNotification>

            <Divider />

            <RunOnStartup></RunOnStartup>
            <ClickThrough></ClickThrough>

            <Divider />

            <NotificationTransparency></NotificationTransparency>

            <br></br>

            <div className="footer">
                <img className="footerLogo" src="/Image/PartyWordmarkIconMono.png"></img>
            </div>
        </div>
    )
}




export default App
