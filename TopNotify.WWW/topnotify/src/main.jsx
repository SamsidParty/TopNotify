import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import DragMode from './DragMode.jsx'
import { ChakraProvider } from '@chakra-ui/react'
import './index.css'
import { useFirstRender, waitUntil } from './Helper.jsx'

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



ReactDOM.createRoot(document.getElementById('root')).render(RootComponent());

function RootComponent(params) {
    return (
        <ChakraProvider>
            <Dispatcher/>
        </ChakraProvider>
    )
}

function Dispatcher() {
    
    var MainMethod = App;

    if (window.location.href.includes("?drag")) {
        MainMethod = DragMode;
    }

    if (useFirstRender()) {
        setTimeout(async () => {
            await waitUntil(() => !!window.RequestConfig); // Wait Until The RequestConfig Function Is Available To Use
            RequestConfig(); // Tells C# React Is Ready, And To Send The Config File
        }, 0);
    }

    return (
        <MainMethod></MainMethod>
    )
}