import { Button, Switch, Slider, SliderTrack, SliderFilledTrack, SliderThumb, Divider } from "@chakra-ui/react";
import { useState } from "react";

export default function SoundInterceptionToggle() {

    let [isInterceptionEnabled, setIsInterceptionEnabled] = useState(false);
    let [checkState, setCheckState] = useState("none");

    if (checkState == "none") {
        setCheckState("loading");
        setTimeout(async () => { 
            window.isInterceptionEnabled = await igniteView.commandBridge.IsSoundInstalledInRegistry();
            setIsInterceptionEnabled(window.isInterceptionEnabled);
            setCheckState("success");
            window.setRerender(rerender + 1); 
        }, 0);
    }

    let setEnabled = async (isChecked) => {
        if (isChecked) {
            await igniteView.commandBridge.InstallSoundInRegistry();
        }
        else {
            await igniteView.commandBridge.UninstallSoundInRegistry();
        }

        UploadConfig();

        setCheckState("none");
    };

    return (
        <div className="flexx facenter fillx gap20">
            <label>Enable Custom Sounds</label>
            <Switch onChange={(e) => setEnabled(e.target.checked)} isChecked={isInterceptionEnabled} style={{ marginLeft: "auto" }} size='lg' />
        </div>
    );
}
