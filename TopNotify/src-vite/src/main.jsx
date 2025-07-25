import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.jsx";
import { ChakraProvider } from "@chakra-ui/react";
import "./CSS/App.css";
import "./CSS/ChakraOverrides.css";
import { useFirstRender, waitUntil } from "./Helper.jsx";
import About from "./About.jsx";

window.serverURL = "http://" + window.location.host + "/";

//Chakra UI Color Mode
let defaultTheme = (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) ? "dark" : "light";
localStorage.setItem("chakra-ui-color-mode", defaultTheme);
document.documentElement.setAttribute("data-theme", defaultTheme);
document.body.setAttribute("chakra-ui-theme", "chakra-ui-" + defaultTheme);

window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", event => {
    location.reload();
});



ReactDOM.createRoot(document.getElementById("root")).render(RootComponent());

function RootComponent(params) {
    return (
        <ChakraProvider>
            <Dispatcher/>
        </ChakraProvider>
    );
}

function Dispatcher() {
    let MainMethod = App;

    if (window.location.search.includes("about")) {
        MainMethod = About;
    }
    
    return (
        <MainMethod></MainMethod>
    );
}