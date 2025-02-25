import { Button, Divider } from '@chakra-ui/react'

import { useState, Fragment } from 'react';
import { useFirstRender } from './Helper.jsx'

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

    var setIsPickerOpen = (v, id) => {
        _setIsOpen(!v);
        _setIsPickerOpen(v);
    }

    var applySound = (sound) => {

        for (var i = 0; i < Config.AppReferences.length; i++) {
            if (Config.AppReferences[i].ID == window.soundPickerReferenceID) {
                Config.AppReferences[i].SoundPath = sound.Path;
                Config.AppReferences[i].SoundDisplayName = sound.Name;
                break;
            }
        }

        UploadConfig();
        setIsPickerOpen(false);
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
                        <div className="errorMessage medium"><h4>&#xea06;</h4>Some Apps Will Play Their Own Sounds, You May Have To Turn Them Off In The App To Prevent Multiple Sounds From Playing</div>
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
            <SoundPicker applySound={applySound} setIsPickerOpen={setIsPickerOpen} isOpen={isPickerOpen}></SoundPicker>
        </div>
    )
}

function AppReferenceSoundItem(props) {

    var pickSound = () => {
        window.soundPickerReferenceID = props.appReference.ID;
        props.setIsPickerOpen(true);
    }

    return (
        <div className="appReferenceSoundItem">
            <img src={props.appReference.DisplayIcon || "/Image/DefaultAppReferenceIcon.svg"}></img>
            <h4>{props.appReference.DisplayName}</h4>
            <div className="selectSoundButton">
                <Button onClick={pickSound}>{props.appReference.SoundDisplayName}&nbsp;&#xeb04;</Button>
            </div>
        </div>
    )
}

function SoundPicker(props) {

    var [soundPacks, setSoundPacks] = useState([]);

    if (useFirstRender()) {
        setTimeout(async () => {
            var request = await fetch("/Meta/SoundPacks.json");
            var response = await request.json();
    
            setSoundPacks(response);
        }, 0);
    }

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
                                return (<SoundPack applySound={props.applySound} soundPack={soundPack} key={i}></SoundPack>)
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

        if (sound.Path.startsWith("custom_sound_path/")) {
            var audio = new Audio("fs://" + sound.Path.replace("custom_sound_path/", ""));
            audio.play();
            return;
        }

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
                                <Button onClick={() => props.applySound(sound)} className="soundItemButton">
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
