import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import "./CSS/App.css";
import SplashScreen from "./Pages/SplashScreen";

function App() {

    return (
        <>
            <SplashScreen></SplashScreen>
        </>
    );
}

export default App;
