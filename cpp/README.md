# 客户端PP协议C++ SDK

SDK文件: pp_client.hh
## 安装路径
/usr/include/pwnsky





## 安装SDK

```
make install
```



测试代码:

```c++
#include <pwnsky/pp_client.hh>
#include <iostream>

using namespace pwnsky::pp;

int main() {
    pp_client ppc;
    if(ppc.connect("127.0.0.1", 4096)) { //连接服务器
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
```



编译

```
g++ test.cc -o test
./test
```

