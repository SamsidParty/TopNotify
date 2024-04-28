import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import { NextUIProvider, createTheme } from '@nextui-org/react';
import './index.css'

window.serverURL = "http://" + window.location.host + "/";

//For UWP
if (navigator.gamepadInputEmulation) {
    navigator.gamepadInputEmulation = "gamepad";
}

//Dev Mode
if (window.location.hostname != "localhost") {
    serverURL = "http://localhost:25631/";

    //Interop Injection Stub
    if (window.external.receiveMessage == undefined) {
        window.chrome.webview.addEventListener('message', message => eval.call(window, message));
        window.chrome.webview.postMessage(`{"Type":"load"}`);
    }
    else {
        window.external.receiveMessage(message => eval.call(window, message));
        window.external.sendMessage(`{"Type":"load"}`);
    }
}

const lightTheme = createTheme({
    type: 'light'
})
  
const darkTheme = createTheme({
    type: 'dark'
})

ReactDOM.createRoot(document.getElementById('root')).render(
    <NextUIProvider theme={(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) ? darkTheme : lightTheme}>
        <App />
    </NextUIProvider>,
)

//TODO: Come Up With A Better Solution Than This
setTimeout(() => {
    document.body.dispatchEvent(new Event("reactReady"));
}, 250);
