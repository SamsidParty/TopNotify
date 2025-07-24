
import { Switch } from "@chakra-ui/react";

export default function ReadAloud() {


    return (
        <div className="flexx facenter fillx gap20">
            <label>Read Notifications To Me</label>
            <Switch onChange={(e) => ChangeSwitch("ReadAloud", e)} isChecked={Config.ReadAloud} style={{ marginLeft: "auto" }} size='lg' />
        </div>
    );
}