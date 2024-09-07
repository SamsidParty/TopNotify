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
            placement='bottom'
            onClose={() => props.setIsPickerOpen(false)}
        >
            <DrawerContent>
                <DrawerCloseButton />
                <DrawerHeader>Select Sound</DrawerHeader>

                <DrawerBody>
                    <div className="soundPackList">
                        {
                            soundPacks.map((soundPack, i) => {
                                return (<SoundPack soundPack={soundPack} key={i}></SoundPack>)
                            })
                        }
                    </div>
                </DrawerBody>

                <DrawerFooter>
                    
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
    )
}

function SoundPack(props) {

    var playSound = (sound) => {
        var audio = new Audio("/Audio/" + sound.Path + ".wav");
        audio.play();
    }

    return (
        <div className="soundPack">
            <h3>{props.soundPack.Name}</h3>
            <h4>{props.soundPack.Description}</h4>
            <Divider></Divider>
            <div className="soundList">
                {
                    props.soundPack.Sounds.map((sound, i) => {
                        return (
                            <div className="soundItem" key={i}>
                                <Button className="soundItemButton">
                                    <img src={sound.Icon}></img>
                                </Button>
                                <h5>{sound.Name}&nbsp;<Button onClick={() => playSound(sound)} className="iconButton">&#xeb51;</Button></h5>
                            </div>
                        )
                    })
                }
            </div>
        </div>
    )
}
