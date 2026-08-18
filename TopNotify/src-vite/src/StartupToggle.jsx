import { Switch } from "@chakra-ui/react";
import { useState } from "react";

export default function StartupToggle() {

    let [isEnabled, setIsEnabled] = useState(false);
    let [checkState, setCheckState] = useState("none");

    if (checkState == "none") {
        setCheckState("loading");
        setTimeout(async () => {
            let state = await igniteView.commandBridge.GetStartupState();

            // The manifest declares Enabled="true" as the default, but Windows still wants
            // an explicit RequestEnableAsync() call to actually turn it on for a new
            // install. Only auto-enable on a genuine never-configured "Disabled" state -
            // never override a state the user explicitly chose (DisabledByUser).
            if (state == "Disabled") {
                state = await igniteView.commandBridge.SetStartupEnabled(true);
            }

            setIsEnabled(state == "Enabled" || state == "EnabledByPolicy");
            setCheckState("success");
        }, 0);
    }

    let setEnabled = async (isChecked) => {
        let newState = await igniteView.commandBridge.SetStartupEnabled(isChecked);
        setIsEnabled(newState == "Enabled" || newState == "EnabledByPolicy");
    };

    return (
        <div className="flexx facenter fillx gap20">
            <label>Run At Startup</label>
            <Switch onChange={(e) => setEnabled(e.target.checked)} isChecked={isEnabled} style={{ marginLeft: "auto" }} size='lg' />
        </div>
    );
}
