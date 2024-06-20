
use tungstenite::{connect, Message};

#[no_mangle]
pub extern "C" fn GetIPCPort() -> i32 {
    27631
}

#[no_mangle]
pub extern "C" fn RunIPCClient() {
    let (mut socket, response) = connect("ws://127.0.0.1:27631/ipc").expect("Connection To IPC Server Failed");

    socket.send(Message::Text("Test IPC".into())).unwrap();

    loop {
        let msg = socket.read().expect("Error Reading Message");
        println!("Received: {}", msg);
    }
}