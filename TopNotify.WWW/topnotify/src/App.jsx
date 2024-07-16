import { useState } from 'react'
import { Button, Switch, Slider, SliderTrack, SliderFilledTrack, SliderThumb, Divider } from '@chakra-ui/react'
import { DebugMenu } from './DebugMenu'

import {
    AlertDialog,
    AlertDialogBody,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogContent,
    AlertDialogOverlay,
    AlertDialogCloseButton,
} from '@chakra-ui/react'

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

    var [isCTWarningOpen, setIsCTWarningOpen] = useState(false);

    //Load Drag Page
    if (rerender == -1) {
        window.EnterDragMode();
        window.location.href = "?drag";
    }
    
    return (
        <div className={'app' + ((rerender > 0) ? " loaded" : "")}>

            <DebugMenu></DebugMenu>

            <h2>TopNotify</h2>

            <div className="locationCard">
                <div className="notifyLocations">
                    <div className="notifyLocation tl"><Button onClick={() => ChangeLocation(0)}>{Config.Location == 0 ? "\uea5e" : "\ued27"}</Button></div>
                    <div className="notifyLocation tr"><Button onClick={() => ChangeLocation(1)}>{Config.Location == 1 ? "\uea5e" : "\ued27"}</Button></div>
                    <div className="notifyLocation bl"><Button onClick={() => ChangeLocation(2)}>{Config.Location == 2 ? "\uea5e" : "\ued27"}</Button></div>
                    <div className="notifyLocation br"><Button onClick={() => ChangeLocation(3)}>{Config.Location == 3 ? "\uea5e" : "\ued27"}</Button></div>
                    <div className="notifyLocation custom"><Button onClick={() => ChangeLocation(4)}>{Config.Location == 4 ? "\uea5e" : "\ued27"}&nbsp;<p>Custom</p></Button></div>
                </div>
                <div className="notifyTaskbar"><img src="/Image/Taskbar.png"></img></div>
            </div>

            <Divider />

            <div className="flexx facenter fillx gap20 buttonContainer">
                <label>Spawn Test Notification</label>
                <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={SpawnTestNotification}>
                    &#xede3;
                </Button>
            </div>

            <Divider />

            {
                window.isUWP ? //Don't Show Run On Startup Switch On UWP, It's Managed By Windows
                (<></>) : (
                    <>
                        <div className="flexx facenter fillx gap20">
                            <label>Run On Startup</label>
                            <Switch onChange={(e) => ChangeSwitch("RunOnStartup", e)} isChecked={Config.RunOnStartup} style={{ marginLeft: "auto" }} size='lg' />
                        </div>
        
                        <Divider />
                    </>
                )
            }

            <div className="flexx facenter fillx gap20">
                <label>Enable Click-Through</label>
                <Switch onChange={(e) => { ChangeSwitch("EnableClickThrough", e); setIsCTWarningOpen(e.target.checked); }} isChecked={Config.EnableClickThrough} style={{ marginLeft: "auto" }} size='lg' />
                <AlertDialog
                    blockScrollOnMount={false}
                    motionPreset='slideInBottom'
                    onClose={() => setIsCTWarningOpen(false)}
                    isOpen={isCTWarningOpen}
                    isCentered
                >
                    <AlertDialogOverlay />

                    <AlertDialogContent>
                    <AlertDialogHeader>Warning</AlertDialogHeader>
                    <AlertDialogCloseButton />
                    <AlertDialogBody>
                        Click-Through Currently Can't Be Disabled Unless You Restart Your Computer.
                        Additionally, Note That You Can't Interact With Notifications When Click-Through Is Enabled
                    </AlertDialogBody>
                    <AlertDialogFooter>
                    </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
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

            <br></br>

            <div className="footer">
                <img className="footerLogo" src="/Image/PartyWordmarkIconMono.png"></img>
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

export default App
