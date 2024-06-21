#pragma once

#define WIN32_LEAN_AND_MEAN

#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <cstdlib>
#include <iostream>
#include <string>


class IPC {
	public:
		static void RunIPC();
};