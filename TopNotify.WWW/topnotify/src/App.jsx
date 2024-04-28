import { useState } from 'react'
import * as NextUI from "@nextui-org/react"

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

            <NextUI.Spacer></NextUI.Spacer>

            <div className="locationCard">
                <div className="notifyLocation tl"><NextUI.Button onPress={() => ChangeLocation(0)} flat auto>{Config.Location == 0 ? "\uea5e" : "\ued27"}</NextUI.Button></div>
                <div className="notifyLocation tr"><NextUI.Button onPress={() => ChangeLocation(1)} flat auto>{Config.Location == 1 ? "\uea5e" : "\ued27"}</NextUI.Button></div>
                <div className="notifyLocation bl"><NextUI.Button onPress={() => ChangeLocation(2)} flat auto>{Config.Location == 2 ? "\uea5e" : "\ued27"}</NextUI.Button></div>
                <div className="notifyLocation br"><NextUI.Button onPress={() => ChangeLocation(3)} flat auto>{Config.Location == 3 ? "\uea5e" : "\ued27"}</NextUI.Button></div>
            </div>

            <div className="divider"></div>

            <div className="flexx facenter fillx gap20 buttonContainer">
                <label>Spawn Test Notification</label>
                <NextUI.Button css={{ marginLeft: "auto" }} className="iconButton" auto onPress={SpawnTestNotification}>
                    &#xea99;
                </NextUI.Button>
            </div>

            <div className="divider"></div>

            <div className="flexx facenter fillx gap20">
                <label>Run On Startup</label>
                <NextUI.Switch onChange={(e) => ChangeSwitch("RunOnStartup", e)} checked={Config.RunOnStartup} css={{ marginLeft: "auto" }}>
                    &#xea99;
                </NextUI.Switch>
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
