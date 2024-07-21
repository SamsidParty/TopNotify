
import { Switch } from '@chakra-ui/react'

export default function ClickThrough() {


    return (
        <div className="flexx facenter fillx gap20">
            <label>Enable Click-Through</label>
            <Switch onChange={(e) => ChangeSwitch("EnableClickThrough", e)} isChecked={Config.EnableClickThrough} style={{ marginLeft: "auto" }} size='lg' />
        </div>
    )
}