import { useState } from 'react'

import {
    AlertDialog,
    AlertDialogBody,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogContent,
    AlertDialogOverlay,
    AlertDialogCloseButton,
} from '@chakra-ui/react'

import { Button, Switch, Divider } from '@chakra-ui/react'

export default function ClickThrough() {

    
    var [isCTWarningOpen, setIsCTWarningOpen] = useState(false);

    return (
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
    )
}