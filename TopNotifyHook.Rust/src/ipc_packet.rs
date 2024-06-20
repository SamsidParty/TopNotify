pub enum IPCPacketType {
    RequestConfig, // Client To Daemon, Asks For Config
    FulfillConfigRequest // Daemon To Client, Returns Config
}