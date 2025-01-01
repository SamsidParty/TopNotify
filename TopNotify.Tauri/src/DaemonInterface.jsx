

export function OpenConnectionToDaemon() {
    if (!window.ipcSocket) {
        window.ipcSocket = new WebSocket("ws://127.0.0.1:27631/ipc");
    
        window.ipcSocket.addEventListener("open", (event) => {
            console.log("Connnected to daemon");
    
            // Ask The Daemon For A List Of Errors
            // They Will Be Displayed In The GUI
            window.ipcSocket.send(new Uint8Array([5])); // IPCPacket.RequestErrorList = 0x05
        });
    
        window.ipcSocket.addEventListener("message", async (event) => {
            var data = new Uint8Array(await event.data.arrayBuffer());
            var packetType = data[0];
            data = data.slice(1, data.length);
    
            if (packetType == 6) { // IPCPacket.FulfillErrorListRequest = 0x06
                // Parse data as JSON
                window.errorList = JSON.parse(new TextDecoder().decode(data));
    
                if (!!window.setConnectedToDaemon) {
                    window.setConnectedToDaemon(true);
                }
            }
        });
    }
}