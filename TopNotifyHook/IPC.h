#pragma once

#define WIN32_LEAN_AND_MEAN

#include "Settings.h"
#include <boost/asio.hpp>
#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <cstdlib>
#include <iostream>
#include <vector>
#include <string>


enum IPCPacketType
{
    RequestConfig, // Client To Daemon, Asks For Config
    FulfillConfigRequest, // Daemon To Client, Returns Config
    RequestHandle, // Daemon To Client, Asks For The Handle Of The Notification Window
    FulfillHandleRequest, // Client To Daemon, Tells The Interceptor The Notification Window Handle
    UpdateConfig, // GUI To Daemon, Tells The Interceptor That The Config Has Changed
    RequestErrorList, // GUI To Daemon, Asks For A List Of Errors To Display
    FulfillErrorListRequest, // Daemon To GUI, Returns A List Of DaemonErrors in JSON Format
};

class IPC {
	public:
		static void RunIPC();
};