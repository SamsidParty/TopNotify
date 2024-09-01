import { Button, Divider } from '@chakra-ui/react'

import { useState, Fragment } from 'react';

import {
    Drawer,
    DrawerBody,
    DrawerFooter,
    DrawerHeader,
    DrawerOverlay,
    DrawerContent,
    DrawerCloseButton,
    Select
} from '@chakra-ui/react'

window.NotificationSoundList = [
    "windows/win11",
    "windows/win10",
    "windows/win7",
    "fortnite/levelup_c5s1",
]

export default function ManageNotificationSounds() {

    var [isOpen, _setIsOpen] = useState(false);

    var setIsOpen = (v) => {

        if (v && rerender < 0) { return; }

        if (v) {
            setTimeout(() => setRerender(-9999999), 0);
        }
        else {
            setTimeout(() => setRerender(2), 0);
        }

        _setIsOpen(v);
    }

    return (
        <div className="flexx facenter fillx gap20 buttonContainer">
            <label>Edit Notification Sounds</label>
            <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={() => setIsOpen(true)}>
                &#xeb04;
            </Button>
            <Drawer
                blockScrollOnMount={false}
                isOpen={isOpen}
                placement='bottom'
                onClose={() => setIsOpen(false)}
            >
                <DrawerContent>
                    <DrawerCloseButton />
                    <DrawerHeader>Notification Sounds</DrawerHeader>

                    <DrawerBody>
                        {
                            window.Config.AppReferences.map((appReference, i) => {
                                return (
                                    <Fragment key={i}>
                                        <Divider/>
                                        <AppReferenceSoundItem appReference={appReference}></AppReferenceSoundItem>
                                    </Fragment>
                                )
                            })
                        }
                        <Divider/>
                    </DrawerBody>

                    <DrawerFooter>
                        
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
        </div>
    )
}

function AppReferenceSoundItem(props) {

    var setSoundPath = (soundPath) => {
        props.appReference.SoundPath = soundPath;
        UploadConfig();
    }

    return (
        <div className="appReferenceSoundItem">
            <img src={props.appReference.DisplayIcon || "/Image/DefaultAppReferenceIcon.svg"}></img>
            <h4>{props.appReference.DisplayName}</h4>
            <div className="selectSoundButton">
                <Select value={props.appReference.SoundPath} onChange={(e) => setSoundPath(e.target.value)}>
                    {
                        NotificationSoundList.map((sound, i) => {
                            return (<option key={i} value={sound}>{sound}</option>)
                        })
                    }
                </Select>
            </div>
        </div>
    )
}