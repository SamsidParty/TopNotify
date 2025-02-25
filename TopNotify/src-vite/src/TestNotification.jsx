import { Button } from '@chakra-ui/react'

export default function TestNotification() {
    return (
        <div className="flexx facenter fillx gap20 buttonContainer">
            <label>Spawn Test Notification</label>
            <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={() => igniteView.commandBridge.SpawnTestNotification()}>
                &#xede3;
            </Button>
        </div>
    )
}