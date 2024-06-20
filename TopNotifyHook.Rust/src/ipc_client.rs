
use std::thread;
use crate::ipc_packet::{self, IPCPacketType};
use crate::settings;

use tungstenite::{connect, Message};

#[no_mangle]
pub extern "C" fn GetIPCPort() -> i32 {
    27631
}

#[no_mangle]
pub extern "C" fn RunIPCClient() {

    thread::spawn(|| {
        RunIPCClientThreaded();
    });

}

fn RunIPCClientThreaded() {

    let (mut socket, response) = connect(format!("{}{}{}", "ws://127.0.0.1:", GetIPCPort().to_string(), "/ipc")).expect("Connection To IPC Server Failed");

    let getConfigHeader: Vec<u8> = vec![IPCPacketType::RequestConfig as u8];
    socket.send(Message::binary::<Message>(getConfigHeader.into())).unwrap();

    loop {
        let packet = socket.read().expect("Error Reading IPC Message From Server").into_data();
        let header = packet.get(0);
        
        match header {
            Some(packetType) => {
                if (*packetType == IPCPacketType::FulfillConfigRequest as u8) {
                    
                }
            },
            None => {}
        }


    }
}