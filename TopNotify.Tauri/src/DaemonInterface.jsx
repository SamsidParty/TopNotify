window.currentConfig = null;

export function OpenConnectionToDaemon() {
    if (!window.ipcSocket) {
        window.ipcSocket = new WebSocket("ws://127.0.0.1:27631/ipc");
    
        window.ipcSocket.addEventListener("open", (event) => {
            console.log("Connnected to daemon");
    
            // Ask The Daemon For A List Of Errors
            // They Will Be Displayed In The GUI
            window.ipcSocket.send(new Uint8Array([5])); // IPCPacket.RequestErrorList = 0x05

            RequestConfig();
        });
    
        window.ipcSocket.addEventListener("message", async (event) => {
            var data = new Uint8Array(await event.data.arrayBuffer());
            var packetType = data[0];
            data = data.slice(1, data.length);
    
            console.log("Recieved Packet Of Type: " + packetType);

            if (packetType == 6) { // IPCPacket.FulfillErrorListRequest = 0x06
                // Parse data as JSON
                window.errorList = JSON.parse(new TextDecoder().decode(data));
            }
            else if (packetType == 1) { // IPCPacket.FullfillConfigRequest
                // Parse data as JSON
                window.currentConfig = JSON.parse(new TextDecoder().decode(data));

                setTimeout(window.setRerender, 0);

                if (!!window.setConnectedToDaemon) {
                    window.setConnectedToDaemon(true);
                }
            }
        });
    }
}

export function RequestConfig() {
    // Ask The Daemon For The Config File
    window.ipcSocket.send(new Uint8Array([0])); // IPCPacket.RequestConfig = 0x00
}

export async function ApplyConfig() {

    if (!window.currentConfig) { return; }

    // TODO: Send The File To The Daemon

    // Tell The Daemon To Reload The Config File
    window.ipcSocket.send(new Uint8Array([4])); // IPCPacket.UpdateConfig = 0x04
}

export async function SpawnTestNotification() {
    window.ipcSocket.send(new Uint8Array([7])); // IPCPacket.SpawnTestNotification = 0x07
}