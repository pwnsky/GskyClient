#include "pp.hh"
#include <iostream>

using namespace pp;

int main() {
    pp_client ppc;    
    if(ppc.connect("127.0.0.1", 4096)) {
        std::cout << "连接成功!\n";
    }

    //请求路由, 6字节大小
    char route[] = {0x30, 0, 0, 0, 0};
    std::string a;

    a.resize(0x100);
    //a = "hello";
    ppc.send(route, a); // 发送0x100个数据

    // 接收数据
    std::cout << ppc.recv()->to_string();
    return 0;
}
