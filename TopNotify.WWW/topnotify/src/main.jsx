import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import { ChakraProvider } from '@chakra-ui/react'
import './index.css'

window.serverURL = "http://" + window.location.host + "/";

//Chakra UI Color Mode
var defaultTheme = (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) ? "dark" : "light";
localStorage.setItem("chakra-ui-color-mode", defaultTheme);
document.documentElement.setAttribute("data-theme", defaultTheme);

window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
    location.reload();
});

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


ReactDOM.createRoot(document.getElementById('root')).render(
    <ChakraProvider>
        <App />
    </ChakraProvider>
,
)

//TODO: Come Up With A Better Solution Than This
setTimeout(() => {
    document.body.dispatchEvent(new Event("reactReady"));
}, 250);
