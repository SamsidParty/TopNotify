
use std::thread;
use std::str;
use crate::ipc_packet::{self, IPCPacketType};
use crate::settings::Settings;

use tungstenite::{connect, Message};
use serde::{Deserialize, Serialize};
use serde_json::Result;

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

    //Tell The Daemon To Send Us The Config File
    let getConfigHeader: Vec<u8> = vec![IPCPacketType::RequestConfig as u8];
    socket.send(Message::binary::<Message>(getConfigHeader.into())).unwrap();

    loop {
        let message = socket.read().expect("Error Reading IPC Message From Server").into_data();
        let mut packet = message.clone();
        let header = message.get(0);
        packet.remove(0); // Removes The First (IPCPacketType) Byte
        
        match header {
            Some(packetType) => {
                if (*packetType == IPCPacketType::FulfillConfigRequest as u8) {

                    let json = str::from_utf8(&packet).unwrap();
                    let settings: Settings = serde_json::from_str(&json).expect("JSON Parse Error");
                }
            },
            None => {}
        }


    }
}