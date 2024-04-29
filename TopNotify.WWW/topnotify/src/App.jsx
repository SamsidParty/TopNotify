import { useState } from 'react'
import { Button, Switch, Slider, SliderTrack, SliderFilledTrack, SliderThumb, Divider } from '@chakra-ui/react'

var Config = {
    Location: -1,
    RunOnStartup: true,
    Opacity: 0
}

window.SetConfig = (e) => {
    Config = JSON.parse(e);
    window.setRerender(rerender + 1);
}

function UploadConfig() {

    if (Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    var ev = new CustomEvent("uploadConfig");
    ev.newConfig = JSON.stringify(Config);
    document.body.dispatchEvent(ev);
    window.setRerender(rerender + 1);
}

function App() {

    var [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;

    //Load Drag Page
    if (rerender == -1) {
        CallCSharp("SamsidParty_TopNotify.Frontend, TopNotify", "EnterDragMode");
        window.location.href = "?drag";
    }
    
    return (
        <div className={'app' + ((rerender > 0) ? " loaded" : "")}>
            <h2>Settings</h2>

            <div className="locationCard">
                <div className="notifyLocation tl"><Button onClick={() => ChangeLocation(0)}>{Config.Location == 0 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation tr"><Button onClick={() => ChangeLocation(1)}>{Config.Location == 1 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation bl"><Button onClick={() => ChangeLocation(2)}>{Config.Location == 2 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation br"><Button onClick={() => ChangeLocation(3)}>{Config.Location == 3 ? "\uea5e" : "\ued27"}</Button></div>
                <div className="notifyLocation custom"><Button onClick={() => ChangeLocation(4)}>{Config.Location == 4 ? "\uea5e" : "\ued27"}&nbsp;<p>Custom</p></Button></div>
            </div>

            <Divider />

            <div className="flexx facenter fillx gap20 buttonContainer">
                <label>Spawn Test Notification</label>
                <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={SpawnTestNotification}>
                    &#xea99;
                </Button>
            </div>

            <Divider />

            <div className="flexx facenter fillx gap20">
                <label>Run On Startup</label>
                <Switch onChange={(e) => ChangeSwitch("RunOnStartup", e)} isChecked={Config.RunOnStartup} style={{ marginLeft: "auto" }} size='lg' />
            </div>

            <Divider />

            <div className="flexy fillx gap20">
                <label>Notification Transparency</label>
                {
                    //Slider Is In Uncontrolled Mode For Performance Reasons
                    //So We Need To Wait For The Config To Load Before Setting The Default Value
                    (rerender == 0) ? 
                    (<></>) :
                    (
                        <Slider size="lg" onChangeEnd={ChangeOpacity} defaultValue={Config.Opacity * 20}>
                            <SliderTrack>
                                <SliderFilledTrack />
                            </SliderTrack>
                            <SliderThumb />
                        </Slider>
                    )
                }
            </div>
        </div>
    )
}

function SpawnTestNotification() {
    document.body.dispatchEvent(new Event("spawnTestNotification"));
}

function ChangeLocation(location) {
    Config.Location = location;
    UploadConfig();
    window.setRerender(rerender + 1);

    //Open Position Picker Page
    if (location == 4) {
        setRerender(-1);
    }
}

function ChangeOpacity(opacity) {
    Config.Opacity = (opacity * 0.05);
    UploadConfig();
    window.setRerender(rerender + 1);
}

function ChangeSwitch(key, e) {
    Config[key] = e.target.checked;
    UploadConfig();
    window.setRerender(rerender + 1);
}

export default App
