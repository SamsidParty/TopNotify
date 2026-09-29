
import { Switch } from "@chakra-ui/react";

// Only Shown For Custom Positions
export default function GrowUpward() {

    return (
        <div className="flexx facenter fillx gap20">
            <label>Grow Upward</label>
            <Switch onChange={(e) => ChangeSwitch("CustomPositionGrowUpward", e)} isChecked={Config.CustomPositionGrowUpward} style={{ marginLeft: "auto" }} size='lg' />
        </div>
    );
}
