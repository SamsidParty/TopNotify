
use std::thread;

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

    socket.send(Message::Text("Test IPC".into())).unwrap();

    loop {
        let msg = socket.read().expect("Error Reading Message");
        println!("Received: {}", msg);
    }
}