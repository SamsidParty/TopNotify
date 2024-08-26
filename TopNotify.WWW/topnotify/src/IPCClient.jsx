
if (!window.ipcSocket) {
    window.ipcSocket = new WebSocket("ws://127.0.0.1:27631/ipc");


    window.ipcSocket.addEventListener("open", (event) => {
        console.log("Connnected To Daemon");
    });
}


// Tells The Daemon The Config Has Changed
function UpdateConfig() {
    if (!!window.ipcSocket) {
        console.log("Updating Config");
        window.ipcSocket.send(new Uint8Array([4])); // IPCPacket.UpdateConfig = 0x04
    }
}
window.UpdateConfig = UpdateConfig;