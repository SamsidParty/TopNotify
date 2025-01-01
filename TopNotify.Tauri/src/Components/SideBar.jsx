import { IconCurrentLocation } from "@tabler/icons-react";
import "../CSS/SideBar.css";
import { Button } from 'primereact/button';

export default function SideBar() {
    return (
        <div className="sideBar">
            <Button className="sideBarButton selectedSideBarButton">
                <IconCurrentLocation/> Position
            </Button>
        </div>
    )
}