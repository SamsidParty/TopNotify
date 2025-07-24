import { useRef } from "react";

//https://stackoverflow.com/a/66139558/18071273
export function useFirstRender() {
    const ref = useRef(true);
    const firstRender = ref.current;
    ref.current = false;
    return firstRender;
}

//https://stackoverflow.com/a/52652681/18071273
export function waitUntil(conditionFunction) {

    const poll = resolve => {
        if(conditionFunction()) resolve();
        else setTimeout(_ => poll(resolve), 400);
    };
  
    return new Promise(poll);
}