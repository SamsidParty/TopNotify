#include "ipc.h"

namespace beast = boost::beast;
namespace http = beast::http;
namespace websocket = beast::websocket;
namespace net = boost::asio;
using tcp = boost::asio::ip::tcp;

void IPC::RunIPC() {
    try
    {
        net::io_context ioc;

        tcp::resolver resolver { ioc };
        websocket::stream<tcp::socket> ws { ioc };

        auto results = resolver.resolve(std::string("127.0.0.1"), "27631");
        auto ep = net::connect(ws.next_layer(), results);

        std::string host = "127.0.0.1:" + std::to_string(27631);


        ws.handshake(host, "/ipc");
        beast::flat_buffer buffer;

        //Send Request For Config File
        const char packet = 0;
        ws.write(net::buffer(&packet, 1));

        while (ws.read(buffer)) {
            MessageBox(NULL, std::to_wstring(buffer.size()).c_str(), L"Got Data", 0);
            buffer.clear();
        }

        ws.close(websocket::close_code::normal);
        
        MessageBox(NULL, L"Disconnected From TopNotify Daemon", L"TopNotify Hook", 0);
        IPC::RunIPC();
    }
    catch (std::exception const& e)
    {
        MessageBox(NULL, L"Failed To Connect To TopNotify Daemon", L"TopNotify Hook", 0);
        IPC::RunIPC();
    }
}