#include "ipc.h"
#define BOOST_JSON_STACK_BUFFER_SIZE 1024
#include <boost/json.hpp>
#include <boost/json/src.hpp>


using namespace boost::asio;
namespace beast = boost::beast;
namespace http = beast::http;
namespace websocket = beast::websocket;
using tcp = boost::asio::ip::tcp;
using namespace boost::json;

void IPC::RunIPC() {
    try
    {
        io_context ioc;

        tcp::resolver resolver { ioc };
        websocket::stream<tcp::socket> ws { ioc };

        auto results = resolver.resolve(std::string("127.0.0.1"), "27631");
        auto ep = connect(ws.next_layer(), results);

        std::string host = "127.0.0.1:" + std::to_string(27631);


        ws.handshake(host, "/ipc");
        beast::flat_buffer buffer;

        //Send Request For Config File
        const char packet = IPCPacketType::RequestConfig;
        ws.write(boost::asio::buffer(&packet, 1));

        while (ws.read(buffer)) {

            try {
                std::string json = beast::buffers_to_string(buffer.data() + 1);
                value settingsFile = parse(json);

                Settings* newSettings = new Settings();
                //Can't Get Direct Struct Deserialization Working
                //Manually Fill In Fields
                newSettings->Location = static_cast<NotifyLocation>(settingsFile.at("Location").as_int64());
                newSettings->SoundPath = settingsFile.at("SoundPath").as_string().c_str();
                newSettings->CustomPositionPercentX = settingsFile.at("CustomPositionPercentX").as_double();
                newSettings->CustomPositionPercentY = settingsFile.at("CustomPositionPercentY").as_double();
                newSettings->__ScreenWidth = settingsFile.at("__ScreenWidth").as_int64();
                newSettings->__ScreenHeight = settingsFile.at("__ScreenHeight").as_int64();
                newSettings->__ScreenScale = settingsFile.at("__ScreenScale").as_double();

                GlobalSettings::SetSettings(newSettings);
            }
            catch (...) { }

            buffer.clear();
        }

        ws.close(websocket::close_code::normal);
        
        std::this_thread::sleep_for(std::chrono::seconds(2));
        IPC::RunIPC();
    }
    catch (...)
    {
        std::this_thread::sleep_for(std::chrono::seconds(2));
        IPC::RunIPC();
    }
}