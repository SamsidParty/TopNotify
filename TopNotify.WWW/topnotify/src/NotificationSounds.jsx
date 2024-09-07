import { Button, Divider } from '@chakra-ui/react'

import { useState, Fragment } from 'react';

import "./CSS/NotificationSounds.css";

import {
    Drawer,
    DrawerBody,
    DrawerFooter,
    DrawerHeader,
    DrawerOverlay,
    DrawerContent,
    DrawerCloseButton
} from '@chakra-ui/react'

export default function ManageNotificationSounds() {

    var [isOpen, _setIsOpen] = useState(false);
    var [isPickerOpen, _setIsPickerOpen] = useState(false);

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

    var setIsPickerOpen = (v) => {
        _setIsOpen(!v);
        _setIsPickerOpen(v);
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
                                        <AppReferenceSoundItem setIsPickerOpen={setIsPickerOpen} appReference={appReference}></AppReferenceSoundItem>
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
            <SoundPicker setIsPickerOpen={setIsPickerOpen} isOpen={isPickerOpen}></SoundPicker>
        </div>
    )
}

function AppReferenceSoundItem(props) {

    var setSoundPath = (soundPath) => {
        props.appReference.SoundPath = soundPath;
        UploadConfig();
    }

    var pickSound = () => {
        props.setIsPickerOpen(true);
    }

    return (
        <div className="appReferenceSoundItem">
            <img src={props.appReference.DisplayIcon || "/Image/DefaultAppReferenceIcon.svg"}></img>
            <h4>{props.appReference.DisplayName}</h4>
            <div className="selectSoundButton">
                <Button onClick={pickSound}>{props.appReference.SoundPath}</Button>
            </div>
        </div>
    )
}

function SoundPicker(props) {

    var [soundPacks, setSoundPacks] = useState([]);

    setTimeout(async () => {
        var request = await fetch("/Meta/SoundPacks.json");
        var response = await request.json();

        setSoundPacks(response);
    }, 0);

    return (
        <Drawer
            blockScrollOnMount={false}
            isOpen={props.isOpen}
            placement='top'
            onClose={() => props.setIsPickerOpen(false)}
        >
            <DrawerContent>
                <DrawerCloseButton />
                <DrawerHeader>Select Sound</DrawerHeader>

                <DrawerBody>
                    {
                        soundPacks.map((soundPack) => {
                            return (<h4>{soundPack.Name}</h4>)
                        })
                    }
                </DrawerBody>

                <DrawerFooter>
                    
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
    )
}