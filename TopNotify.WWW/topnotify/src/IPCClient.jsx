
window.errorList = [];

if (!window.ipcSocket) {
    window.ipcSocket = new WebSocket("ws://127.0.0.1:27631/ipc");


    window.ipcSocket.addEventListener("open", (event) => {
        console.log("Connnected To Daemon");

        // Ask The Daemon For A List Of Errors
        // They Will Be Displayed In The GUI
        window.ipcSocket.send(new Uint8Array([5])); // IPCPacket.RequestErrorList = 0x05
    });

    window.ipcSocket.addEventListener("message", async (event) => {
        var data = new Uint8Array(await event.data.arrayBuffer());
        var packetType = data[0];
        data = data.slice(1, data.length);

        if (packetType == 6) { // IPCPacket.FulfillErrorListRequest = 0x06
            //Parse data as JSON
            window.errorList = JSON.parse(new TextDecoder().decode(data));

            if (!!window.setRerender) {
                setRerender(rerender + 1);
            }
        }
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