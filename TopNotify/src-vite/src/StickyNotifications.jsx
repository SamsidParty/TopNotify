import { Button, Divider, Drawer, DrawerBody, DrawerContent, DrawerFooter, DrawerHeader, Input, Modal, ModalBody, ModalCloseButton, ModalContent, ModalHeader, ModalOverlay, NumberInput, NumberInputField, Select } from "@chakra-ui/react";
import { Fragment, useEffect, useRef, useState } from "react";
import React from "react";
import { TbChevronDown, TbClock, TbPencil, TbPlus, TbSearch, TbX } from "react-icons/tb";

import "./CSS/StickyNotifications.css";

const StickyMode = {
    Default: 0,
    WindowsDefault: 1,
    Seconds: 2,
    Permanent: 3
};

const DefaultAppIcon = "/Image/DefaultAppReferenceIcon.svg";

export default function StickyNotifications() {

    let [isOpen, _setIsOpen] = useState(false);
    let [isPickerOpen, setIsPickerOpen] = useState(false);
    let [appSearch, setAppSearch] = useState("");

    const installedApps = JSON.parse(igniteView.withReact(React).useCommandResult("FindInstalledApps") || "[]");
    const appReferences = window.Config.AppReferences || [];
    const availableApps = installedApps.filter((app) => app.Name && !appReferences.some((appReference) => appReference.ID == app.Name));
    const searchTerm = appSearch.toLowerCase();
    const filteredApps = availableApps.filter((app) => {
        return app.Name.toLowerCase().includes(searchTerm) || (app.AppID || "").toLowerCase().includes(searchTerm);
    }).slice(0, 80);

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

    let addApp = (app) => {
        if (!app) { return; }

        Config.AppReferences.push({
            ReferenceType: 0,
            ID: app.Name,
            DisplayName: app.Name,
            DisplayIcon: app.DisplayIcon || window.installedAppIcons?.[app.AppID] || DefaultAppIcon,
            SoundPath: "internal/default",
            SoundDisplayName: "Default Sound",
            StickyMode: StickyMode.Seconds,
            StickyDurationSeconds: Config.StickyDurationSeconds || 5
        });

        setAppSearch("");
        setIsPickerOpen(false);
        UploadConfig();
    };

    let removeApp = (appReference) => {
        Config.AppReferences = Config.AppReferences.filter((reference) => reference.ID != appReference.ID);
        UploadConfig();
    };

    let openPicker = () => {
        setAppSearch("");
        setIsPickerOpen(true);
    };

    let closePicker = () => {
        setAppSearch("");
        setIsPickerOpen(false);
    };

    return (
        <div className="flexx facenter fillx gap20 buttonContainer">
            <label>Notification Sticky Time</label>
            <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={() => setIsOpen(true)}>
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

                    <DrawerHeader onMouseOver={window.igniteView.dragWindow}>Notification Sticky Time</DrawerHeader>

                    <DrawerBody>
                        <div className="stickySection">
                            <h3>Default</h3>
                            <StickyControls target={Config} allowDefault={false}></StickyControls>
                        </div>

                        <Divider/>

                        <h3 className="stickySettingsTitle">Application Settings</h3>

                        <div className="stickyAddApp">
                            <Button onClick={openPicker}><TbPlus/>Add Application</Button>
                        </div>

                        <div className="stickyAppList">
                            {
                                appReferences.filter((appReference) => appReference.ID != "Other").map((appReference, i) => {
                                    return (
                                        <Fragment key={i}>
                                            <Divider/>
                                            <StickyAppItem appReference={appReference} removeApp={removeApp}></StickyAppItem>
                                        </Fragment>
                                    );
                                })
                            }
                        </div>
                    </DrawerBody>

                    <DrawerFooter>
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
            <AddApplicationPicker
                addApp={addApp}
                appSearch={appSearch}
                filteredApps={filteredApps}
                isOpen={isPickerOpen}
                setAppSearch={setAppSearch}
                setIsOpen={closePicker}
            ></AddApplicationPicker>
        </div>
    );
}

function AddApplicationPicker(props) {
    const searchInput = useRef(null);

    useEffect(() => {
        if (!props.isOpen) { return; }
        setTimeout(() => searchInput.current?.focus(), 0);
    }, [props.isOpen]);

    return (
        <Modal initialFocusRef={searchInput} isOpen={props.isOpen} onClose={props.setIsOpen} isCentered={true}>
            <ModalOverlay />
            <ModalContent className="stickyAppPicker">
                <ModalHeader>Add Application</ModalHeader>
                <ModalCloseButton />
                <ModalBody>
                    <div className="stickySearchBox">
                        <TbSearch/>
                        <Input ref={searchInput} value={props.appSearch} onChange={(e) => props.setAppSearch(e.target.value)} placeholder="Search Windows Apps"/>
                    </div>
                    <div className="stickySearchResults">
                        {
                            props.filteredApps.length > 0 ?
                                props.filteredApps.map((app, i) => {
                                    return (<StickySearchResult app={app} addApp={props.addApp} key={i}></StickySearchResult>);
                                }) :
                                (<div className="stickySearchEmpty">No applications found.</div>)
                        }
                    </div>
                </ModalBody>
            </ModalContent>
        </Modal>
    );
}

function StickySearchResult(props) {
    let [icon, setIcon] = useState(props.app.DisplayIcon || window.installedAppIcons?.[props.app.AppID] || DefaultAppIcon);

    useEffect(() => {
        if (!window.installedAppIcons) {
            window.installedAppIcons = {};
        }

        if (window.installedAppIcons[props.app.AppID]) {
            setIcon(window.installedAppIcons[props.app.AppID]);
            return;
        }

        let isMounted = true;
        igniteView.commandBridge.FindInstalledAppIcon(props.app.Name, props.app.AppID || "").then((result) => {
            window.installedAppIcons[props.app.AppID] = result;
            props.app.DisplayIcon = result;
            if (isMounted) {
                setIcon(result);
            }
        });

        return () => {
            isMounted = false;
        };
    }, [props.app]);

    let addApp = () => {
        props.app.DisplayIcon = icon;
        props.addApp(props.app);
    };

    return (
        <button className="stickySearchResult" onClick={addApp}>
            <img src={icon || DefaultAppIcon}></img>
            <StickyOverflowText text={props.app.Name}></StickyOverflowText>
            <TbPlus/>
        </button>
    );
}

function StickyAppItem(props) {
    let icon = props.appReference.DisplayIcon || DefaultAppIcon;

    return (
        <div className="stickyAppItem">
            <div className="stickyAppIdentity">
                <img src={icon}></img>
                <StickyOverflowText tag="h4" text={props.appReference.DisplayName}></StickyOverflowText>
            </div>
            <StickyControls target={props.appReference} allowDefault={true}></StickyControls>
            <Button className="iconButton" onClick={() => props.removeApp(props.appReference)}><TbX/></Button>
        </div>
    );
}

function StickyOverflowText(props) {
    let Component = props.tag || "span";
    let textRef = useRef(null);
    let [title, setTitle] = useState("");

    let updateTitle = () => {
        let element = textRef.current;
        setTitle(element && element.scrollWidth > element.clientWidth ? props.text : "");
    };

    return (
        <Component ref={textRef} title={title} onMouseEnter={updateTitle}>
            {props.text}
        </Component>
    );
}

function StickyControls(props) {

    if (props.target.StickyMode == undefined) {
        props.target.StickyMode = props.allowDefault ? StickyMode.Default : StickyMode.WindowsDefault;
    }

    if (props.target.StickyDurationSeconds == undefined) {
        props.target.StickyDurationSeconds = 5;
    }

    let setMode = (mode) => {
        props.target.StickyMode = parseInt(mode);
        UploadConfig();
    };

    let setDuration = (value) => {
        props.target.StickyDurationSeconds = Math.max(1, parseInt(value || "1"));
        UploadConfig();
    };

    return (
        <div className="stickyControls">
            <Select value={props.target.StickyMode} onChange={(e) => setMode(e.target.value)}>
                {
                    props.allowDefault && (<option value={StickyMode.Default}>Default</option>)
                }
                {
                    !props.allowDefault && (<option value={StickyMode.WindowsDefault}>Windows Default</option>)
                }
                <option value={StickyMode.Seconds}>Custom Seconds</option>
                <option value={StickyMode.Permanent}>Permanent</option>
            </Select>
            <div className={"stickySeconds" + (props.target.StickyMode == StickyMode.Seconds ? "" : " hidden")}>
                <NumberInput min={1} max={86400} value={props.target.StickyDurationSeconds} onChange={setDuration}>
                    <NumberInputField />
                </NumberInput>
            </div>
        </div>
    );
}
