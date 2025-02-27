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
                    
                    <div className="windowCloseButton">
                        <Button className="iconButton" onClick={() => setIsOpen(false)}>&#xea5f;</Button>
                    </div>

                    <DrawerHeader onMouseOver={window.igniteView.dragWindow}>Notification Sounds</DrawerHeader>

                    <DrawerBody>
                        <div className="errorMessage medium"><h4>&#xea06;</h4>Some apps will play their own sounds, you may have to turn them off in-app to prevent overlapping audio.</div>
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
                            {
                                window.Config.AppReferences.length == 0 ? (<p>When an app sends a notification, TopNotify will capture it and it will show up here for you to modify the sounds.</p>) : null
                            }
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
            var newSounds = JSON.parse(await igniteView.commandBridge.FindSounds());
            setSoundPacks(newSounds);
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

    var playSound = (sound) => igniteView.commandBridge.PreviewSound(sound.Path);

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
