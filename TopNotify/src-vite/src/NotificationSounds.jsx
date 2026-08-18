import { Button, Checkbox, Divider } from "@chakra-ui/react";

import { Fragment, useEffect, useState } from "react";

import "./CSS/NotificationSounds.css";

import {
    Drawer,
    DrawerBody,
    DrawerContent,
    DrawerFooter,
    DrawerHeader
} from "@chakra-ui/react";
import React from "react";
import { TbAlertTriangle, TbCheck, TbChevronDown, TbFolder, TbMusicPlus, TbPencil, TbTrash, TbVolume } from "react-icons/tb";

export default function ManageNotificationSounds() {

    let [isOpen, _setIsOpen] = useState(false);
    let [isPickerOpen, _setIsPickerOpen] = useState(false);

    let setIsOpen = (v) => {

        if (v && rerender < 0) { return; }

        if (v) {
            setTimeout(() => setRerender(-9999999), 0);
        }
        else {
            setTimeout(() => setRerender(2), 0);
        }

        _setIsOpen(v);
    };

    let setIsPickerOpen = (v, id) => {
        _setIsOpen(!v);
        _setIsPickerOpen(v);
    };

    // Applies The Chosen Sound And Keeps The Picker Open So The User Can See Which
    // Sound Is Now Active (Highlighted) And Keep Previewing/Browsing Others Instead Of
    // Being Bounced Back To The App List On Every Click. UploadConfig() Already Triggers
    // A Global Rerender, So The Highlight Updates Immediately Without An Extra Call.
    let applySound = (sound) => {

        for (let i = 0; i < Config.AppReferences.length; i++) {
            if (Config.AppReferences[i].ID == window.soundPickerReferenceID) {
                Config.AppReferences[i].SoundPath = sound.Path;
                Config.AppReferences[i].SoundDisplayName = sound.Name;
                break;
            }
        }

        UploadConfig();
    };

    return (
        <div className="flexx facenter fillx gap20 buttonContainer">
            <label data-greyed-out={(!window.isInterceptionEnabled).toString()}>Edit Notification Sounds</label>
            <Button data-greyed-out={(!window.isInterceptionEnabled).toString()} style={{ marginLeft: "auto" }} className="iconButton" onClick={() => setIsOpen(true)}>
                <TbPencil/>
            </Button>
            <Drawer
                blockScrollOnMount={false}
                isOpen={isOpen}
                placement='bottom'
                onClose={() => setIsOpen(false)}
            >
                <DrawerContent>
                    
                    <div className="windowCloseButton">
                        <Button className="iconButton" onClick={() => setIsOpen(false)}><TbChevronDown/></Button>
                    </div>

                    <DrawerHeader onMouseOver={window.igniteView.dragWindow}>Notification Sounds</DrawerHeader>

                    <DrawerBody>
                        <div className="errorMessage medium"><TbAlertTriangle/>Some apps will play their own sounds, you may have to turn them off in-app to prevent overlapping audio.</div>
                        {
                            window.Config.AppReferences.map((appReference, i) => {
                                return (
                                    <Fragment key={i}>
                                        <Divider/>
                                        <AppReferenceSoundItem setIsPickerOpen={setIsPickerOpen} appReference={appReference}></AppReferenceSoundItem>
                                    </Fragment>
                                );
                            })
                        }
                        <Divider/>
                        <p>When an app sends a notification, TopNotify will capture it and it will show up here for you to modify the sounds.</p>
                    </DrawerBody>

                    <DrawerFooter>
                        
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
            <SoundPicker applySound={applySound} setIsPickerOpen={setIsPickerOpen} key={window.soundPickerReferenceID + isPickerOpen || "soundPicker"} isOpen={isPickerOpen}></SoundPicker>
        </div>
    );
}

function AppReferenceSoundItem(props) {

    let pickSound = () => {
        window.soundPickerReferenceID = props.appReference.ID;
        props.setIsPickerOpen(true);
    };

    // "internal/default" already means "inherit Other's sound" all the way down in
    // GetSoundPath's fallback (live at playback time, not a snapshot) - this just surfaces
    // that state visually instead of silently defaulting with no indication. Excludes
    // "Other" itself, since it's the thing being inherited from, not an inheritor.
    let isInherited = props.appReference.ID != "Other" && props.appReference.SoundPath == "internal/default";

    // While inherited, show what it actually resolves to right now (Other's current
    // sound) rather than the stored SoundDisplayName, which would go stale the moment
    // Other's own sound changes.
    let otherAppReference = window.Config.AppReferences.find((a) => a.ID == "Other");
    let displayedSoundName = (isInherited && otherAppReference) ? otherAppReference.SoundDisplayName : props.appReference.SoundDisplayName;

    return (
        <div className="appReferenceSoundItem" data-inherited={isInherited.toString()}>
            <img src={props.appReference.DisplayIcon || "/Image/DefaultAppReferenceIcon.svg"}></img>
            <h4>{props.appReference.DisplayName}</h4>
            <div className="selectSoundButton">
                <Button onClick={pickSound}>{displayedSoundName}&nbsp;<TbPencil/></Button>
            </div>
        </div>
    );
}

function SoundPicker(props) {

    // useCommandResult resolves asynchronously - a useState lazy initializer only runs on
    // the very first render, before the result has actually arrived, which permanently
    // seeded an empty list and left the picker blank. useEffect re-syncs local state
    // whenever fetchedPacks actually changes, while still letting removeSoundLocally/
    // addSoundLocally mutate on top afterward without a fetchedPacks change stomping them.
    const fetchedPacks = igniteView.withReact(React).useCommandResult("FindSounds");
    let [soundPacks, setSoundPacks] = useState([]);

    useEffect(() => {
        setSoundPacks(JSON.parse(fetchedPacks || "[]"));
    }, [fetchedPacks]);

    let removeSoundLocally = (soundPath) => {
        setSoundPacks((prev) => prev.map((pack) => ({
            ...pack,
            Sounds: pack.Sounds.filter((s) => s.Path != soundPath)
        })));
    };

    let addSoundLocally = (sound) => {
        setSoundPacks((prev) => prev.map((pack) => {
            if (pack.Name != "Your Collection") { return pack; }
            return { ...pack, Sounds: [...pack.Sounds, sound] };
        }));
    };

    return (
        <Drawer
            blockScrollOnMount={false}
            isOpen={props.isOpen}
            placement='bottom'
            onClose={() => props.setIsPickerOpen(false)}
        >
            <DrawerContent>
                
                <div className="windowCloseButton">
                    <Button className="iconButton" onClick={() => props.setIsPickerOpen(false)}><TbChevronDown/></Button>
                </div>

                <DrawerHeader>Select Sound</DrawerHeader>

                <DrawerBody>
                    <div className="soundPackList">
                        {
                            soundPacks.map((soundPack, i) => {
                                return (<SoundPack applySound={props.applySound} onSoundDeleted={removeSoundLocally} onSoundImported={addSoundLocally} soundPack={soundPack} key={i}></SoundPack>);
                            })
                        }
                    </div>
                </DrawerBody>

                <DrawerFooter>
                    
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
    );
}

function SoundPack(props) {

    let playSound = (sound) => igniteView.commandBridge.PreviewSound(sound.Path);

    // The AppReference Currently Being Edited (Set By AppReferenceSoundItem.pickSound Before
    // The Picker Opens) - Used To Figure Out Which Sound, If Any, Is Already Active So It
    // Can Be Shown Highlighted Instead Of The User Having To Guess.
    let currentAppReference = window.Config.AppReferences.find((a) => a.ID == window.soundPickerReferenceID);

    // If This App Hasn't Been Given Its Own Preference, It's Inheriting Other's Sound -
    // Highlight Whatever That Actually Resolves To Right Now, Not The Literal
    // "internal/default" Tile, So The Picker Matches What's Shown (And Actually Plays).
    let effectiveSoundPath = currentAppReference?.SoundPath;
    if (effectiveSoundPath == "internal/default" && currentAppReference?.ID != "Other") {
        let otherAppReference = window.Config.AppReferences.find((a) => a.ID == "Other");
        effectiveSoundPath = otherAppReference ? otherAppReference.SoundPath : effectiveSoundPath;
    }

    let [trimSilence, setTrimSilence] = useState(true);

    // Only Confirms When Deleting Would Actually Break Something - Checked Against EVERY
    // AppReference, Not Just The One The Picker Was Opened For, Since The Underlying File
    // Is Shared Across All Of Them.
    let deleteSound = async (sound) => {
        let usedBy = window.Config.AppReferences.filter((a) => a.SoundPath == sound.Path);

        if (usedBy.length > 0) {
            let confirmed = window.confirm(
                `"${sound.Name}" is currently used by ${usedBy.length} notification sound${usedBy.length > 1 ? "s" : ""}. Delete it anyway?`
            );
            if (!confirmed) { return; }
        }

        let success = await igniteView.commandBridge.DeleteSound(sound.Path);
        if (!success) { return; }

        if (usedBy.length > 0) {
            for (let i = 0; i < Config.AppReferences.length; i++) {
                if (Config.AppReferences[i].SoundPath == sound.Path) {
                    Config.AppReferences[i].SoundPath = "internal/default";
                    Config.AppReferences[i].SoundDisplayName = "Default Sound";
                }
            }
            UploadConfig();
        }

        props.onSoundDeleted(sound.Path);
    };

    return (
        <div className="soundPack">
            <h3>{props.soundPack.Name}</h3>
            <h4>{props.soundPack.Description}</h4>
            <Divider></Divider>
            <div className="soundList">
                {
                    props.soundPack.Sounds.map((sound, i) => {
                        let isActive = effectiveSoundPath != null && effectiveSoundPath == sound.Path;
                        return (
                            <div className="soundItem" data-active={isActive.toString()} key={sound.Path}>
                                <Button onClick={() => props.applySound(sound)} className="soundItemButton">
                                    <img src={sound.Icon}></img>
                                </Button>
                                {
                                    isActive && (
                                        <div className="activeCheckbox"><TbCheck/></div>
                                    )
                                }
                                {
                                    sound.IsDeletable && (
                                        <Button onClick={() => deleteSound(sound)} className="deleteSoundButton iconButton"><TbTrash/></Button>
                                    )
                                }
                                <h5><span>{sound.Name}</span><Button onClick={() => playSound(sound)} className="iconButton"><TbVolume/></Button></h5>
                            </div>
                        );
                    })
                }
                {
                    props.soundPack.Name == "Your Collection" && (
                        <div className="soundItem" key={"add"}>
                            <Button onClick={async () => {
                                let result = await igniteView.commandBridge.ImportSound(trimSilence);
                                if (result.length == 2) {
                                    // Freshly imported, so it's always inside ImportedSoundFolder -
                                    // matches what FindSounds would report on a real re-scan.
                                    props.onSoundImported({ Path: result[0], Name: result[1], Icon: "/Image/Sound.svg", IsDeletable: true });
                                }
                            }} className="soundItemButton">
                                <TbMusicPlus/>
                            </Button>
                            <h5><span>Import</span><Button onClick={() => igniteView.commandBridge.OpenSoundFolder()} className="iconButton"><TbFolder/></Button></h5>
                        </div>
                    )
                }
            </div>
            {
                props.soundPack.Name == "Your Collection" && (
                    <Checkbox className="trimSilenceOption" isChecked={trimSilence} onChange={(e) => setTrimSilence(e.target.checked)}>
                        Trim leading silence when importing
                    </Checkbox>
                )
            }
        </div>
    );
}
