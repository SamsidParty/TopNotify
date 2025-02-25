import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import DragMode from './DragMode.jsx'
import { ChakraProvider } from '@chakra-ui/react'
import './CSS/App.css'
import { useFirstRender, waitUntil } from './Helper.jsx'

window.serverURL = "http://" + window.location.host + "/";

//Chakra UI Color Mode
var defaultTheme = (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) ? "dark" : "light";
localStorage.setItem("chakra-ui-color-mode", defaultTheme);
document.documentElement.setAttribute("data-theme", defaultTheme);

window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
    location.reload();
});



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
            await waitUntil(() => !!igniteView.commandBridge.RequestConfig); // Wait Until The RequestConfig Function Is Available To Use
            igniteView.commandBridge.RequestConfig(); // Tells C# React Is Ready, And To Send The Config File
        }, 0);
    }

    return (
        <MainMethod></MainMethod>
    )
}