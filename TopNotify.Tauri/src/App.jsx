import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import "./CSS/App.css";
import "./CSS/PrimeReact.css";
import SplashScreen from "./Pages/SplashScreen";
import { OpenConnectionToDaemon } from "./DaemonInterface";
import SideBar from "./Components/SideBar";

function App() {

    var [rerender, _setRerender] = useState(0);
    var [connectedToDaemon, setConnectedToDaemon] = useState(false);

    window.setRerender = () => _setRerender(rerender + 1);
    window.setConnectedToDaemon = setConnectedToDaemon;

    if (rerender == 0) {
        setTimeout(OpenConnectionToDaemon, 1000); // Show the splash screen for at least 1 second
        setTimeout(setRerender, 0); // Increase the rerender count to prevent infinite loop
    }

    return (
        <>

            <SplashScreen shown={!connectedToDaemon}></SplashScreen>

            {
                // Show the app only if we are connected to the daemon
                connectedToDaemon ? <AppSplitView></AppSplitView> : null
            }
            
        </>
    );
}

function AppSplitView() {
    return (
        <div className="appSplitView">
            <SideBar></SideBar>
        </div>
    )
}


export default App;
