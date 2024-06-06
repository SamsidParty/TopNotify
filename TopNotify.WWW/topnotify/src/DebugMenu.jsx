
import {
    Drawer,
    DrawerBody,
    DrawerFooter,
    DrawerHeader,
    DrawerOverlay,
    DrawerContent,
    DrawerCloseButton,
    Button
} from '@chakra-ui/react'

import { useState } from 'react'

export function DebugMenu() {

    var [isOpen, setIsOpen] = useState(false);
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
                    <DrawerCloseButton />
                    <DrawerHeader>Debug Menu</DrawerHeader>

                    <DrawerBody>
                        <div className="flexx facenter fillx gap20 buttonContainer">
                            <label>Open App Folder</label>
                            <Button style={{ marginLeft: "auto" }} className="iconButton" onClick={() => {}}>
                                &#xea99;
                            </Button>
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