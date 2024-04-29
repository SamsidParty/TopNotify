import { useState } from 'react'
import * as NextUI from "@nextui-org/react"
import { Button, Switch } from '@chakra-ui/react'

var Config = {
    Location: -1,
    RunOnStartup: true,
    Opacity: 0
}

window.SetConfig = (e) => {
    Config = JSON.parse(e);
    window.setRerender(Math.random());
}

function UploadConfig() {

    if (Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    var ev = new CustomEvent("uploadConfig");
    ev.newConfig = JSON.stringify(Config);
    document.body.dispatchEvent(ev);
    window.setRerender(Math.random());
}

function App() {

    var [rerender, setRerender] = useState(Math.random());
    window.setRerender = setRerender;

    
    return (
        <>
            <h2>Settings</h2>

            <div className="locationCard">
                <div className="notifyLocation tl"><Button onClick={() => ChangeLocation(0)} flat auto>{Config.Location == 0 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation tr"><Button onClick={() => ChangeLocation(1)} flat auto>{Config.Location == 1 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation bl"><Button onClick={() => ChangeLocation(2)} flat auto>{Config.Location == 2 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation br"><Button onClick={() => ChangeLocation(3)} flat auto>{Config.Location == 3 ? "\uea5e" : "\ued27"}</Button></div>
            </div>

            <div className="divider"></div>

            <div className="flexx facenter fillx gap20 buttonContainer">
                <label>Spawn Test Notification</label>
                <Button style={{ marginLeft: "auto" }} className="iconButton" auto onClick={SpawnTestNotification}>
                    &#xea99;
                </Button>
            </div>

            <div className="divider"></div>

            <div className="flexx facenter fillx gap20">
                <label>Run On Startup</label>
                <Switch onChange={(e) => ChangeSwitch("RunOnStartup", e)} isChecked={Config.RunOnStartup} style={{ marginLeft: "auto" }} size='lg' />
            </div>

            <div className="divider"></div>

            <div className="flexy fillx gap20">
                <label>Notification Opacity</label>
                <NextUI.Pagination onChange={ChangeOpacity} page={Math.round(6 - Config.Opacity)} rounded onlyDots total={6} />
            </div>
        </>
    )
}

function SpawnTestNotification() {
    document.body.dispatchEvent(new Event("spawnTestNotification"));
}

function ChangeLocation(location) {
    Config.Location = location;
    UploadConfig();
    window.setRerender(Math.random());
}

function ChangeOpacity(opacity) {
    Config.Opacity = (6 - opacity);
    UploadConfig();
    window.setRerender(Math.random());
}

function ChangeSwitch(key, e) {
    Config[key] = e.target.checked;
    UploadConfig();
    window.setRerender(Math.random());
}

export default App
