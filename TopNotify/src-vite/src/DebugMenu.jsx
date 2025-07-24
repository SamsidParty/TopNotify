
import {
    Drawer,
    DrawerBody,
    DrawerFooter,
    DrawerHeader,
    DrawerOverlay,
    DrawerContent,
    DrawerCloseButton
} from '@chakra-ui/react'

import { Button, Switch, Slider, SliderTrack, SliderFilledTrack, SliderThumb, Divider } from '@chakra-ui/react'
import { useState } from 'react'
import {TbExternalLink, TbX} from "react-icons/tb";

export function DebugMenu() {

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

    window.openDebugMenu = () => setIsOpen(true);

    return (
        <>
            <Drawer
                blockScrollOnMount={false}
                isOpen={isOpen}
                placement='bottom'
                onClose={() => setIsOpen(false)}
            >
                <DrawerContent>
                    
                    <div className="windowCloseButton">
                        <Button className="iconButton" onClick={() => setIsOpen(false)}><TbX/></Button>
                    </div>

                    <DrawerHeader>Debug Menu</DrawerHeader>

                    <DrawerBody>
                        <div className="flexx facenter fillx gap20 buttonContainer">
                            <label>Open App Folder</label>
                            <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={() => { igniteView.commandBridge.OpenAppFolder(); }}>
                                <TbExternalLink/>
                            </Button>
                        </div>

                        <Divider />

                        <div className="flexx facenter fillx gap20">
                            <label>Force Fallback Interceptor</label>
                            <Switch onChange={(e) => ChangeSwitch("EnableDebugForceFallbackMode", e)} isChecked={Config.EnableDebugForceFallbackMode} style={{ marginLeft: "auto" }} size='lg' />
                        </div>

                        <Divider />

                        <div className="flexx facenter fillx gap20">
                            <label>Disable Bounds Correction</label>
                            <Switch onChange={(e) => ChangeSwitch("EnableDebugRemoveBoundsCorrection", e)} isChecked={Config.EnableDebugRemoveBoundsCorrection} style={{ marginLeft: "auto" }} size='lg' />
                        </div>
                    </DrawerBody>

                    <DrawerFooter>
                        
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
        </>
    )
}

addEventListener("keydown", (e) => {
    if (e.key == "F2") {
        window.openDebugMenu();
        e.preventDefault();
    }
})