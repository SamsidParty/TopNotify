import { Button, Switch, Divider } from '@chakra-ui/react'

export default function RunOnStartup() {
    return window.isUWP ? (<></>) :  //Don't Show Run On Startup Switch On UWP, It's Managed By Windows
    (
        <>
            <div className="flexx facenter fillx gap20">
                <label>Run On Startup</label>
                <Switch onChange={(e) => ChangeSwitch("RunOnStartup", e)} isChecked={Config.RunOnStartup} style={{ marginLeft: "auto" }} size='lg' />
            </div>

            <Divider />
        </>
    )

}