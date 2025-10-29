import { Switch } from "@chakra-ui/react";

export default function ForceForeground() {
    return (
        <div className="flexx facenter fillx gap20">
            <label>Force Foreground on Click</label>
            <Switch onChange={(e) => ChangeSwitch("ForceForegroundOnClick", e)} isChecked={Config.ForceForegroundOnClick} style={{ marginLeft: "auto" }} size='lg' />
        </div>
    );
}

