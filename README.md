# Pwnsky Protocol Client SDK

提供不同语言pp协议接口
目前语言支持:

1. c++
2. c#
3. python
4. java

## 描述

pp 协议，全称为 pwnsky protocol, 是一款吸收http部分特性的一款二进制传输协议，主要用于游戏长连接交互协议，目前基于tcp来实现。

该协议头部只占16字节，相对与http更小，由于协议字段都在固定位置，解析起来更快速。

pp协议中定义有状态码，数据类型，数据长度，请求路由。

采用pe (pwnsky encryption)进行数据加密，由服务端随机生成8字节密钥返回给客户端，客户端接收到之后，在断开之前传输数据都采用该密钥进行加解密。

```
--------------------------------------------------------------------------
| magic 2字节 | status 1字节| type 1字节 |  length 4 字节                  |
--------------------------------------------------------------------------
|                   route 6 字节             |        code 2字节          |
--------------------------------------------------------------------------
```



pp协议 c++ 定义如下：

```cpp
namespace pwnsky {
namespace pp {
enum class status {
    
    // 客户端请求码
    connect = 0x10, // 建立连接，请求密钥的过程
    data_transfer = 0x11,  // 传输数据

    // 服务端响应码
    protocol_error = 0x20, // 协议解析错误
    too_big = 0x21, // 传输数据过长
    invalid_transfer = 0x22, // 无效传输
    
    ok = 0x30, // 请求成功
    send_key = 0x31, // 发送密钥

    redirct = 0x40, // 重置路由

};

// 数据类型
enum class data_type {
    binary_stream = 0x00, // 二进制数据流
    image = 0x01, // 图片
    video = 0x02, // 视频
    music = 0x03, // 音乐

    text = 0x10, // 文本
    json = 0x11, // json 数据
    xml  = 0x12, // xml 数据
};

// 协议头，只占16 字节
struct header {
    unsigned short magic;    // 协议标识，"PP" 值为0x5050,
    unsigned char status;    // 客户端请求码与服务端响应码
    unsigned char type;      // 数据类型
    unsigned int length;     // 数据长度
    char route[6];           // 请求路由，代替http url中的path
    char code[6];            // 校验code
};

}
}
```



## 开发者

i0gan

happi0

daniel



## PP协议连接过程

在客户端与服务端进行TCP连接之后，客户端需要向服务端请求密钥，请求数据包如下

```
magic: 0x5050 (pp协议标识)
status: 0x10 (请求连接)
type: 0x00
length: 0x00 0x00 0x00 0x00
route: 0x00 0x00 0x00 0x00 0x00 0x00
code: 0x00 0x00
```



发送该数据包给给服务端后，服务端会随机生成8字节密钥与2字节code并采用pe以8字节全0进行加密后返回给客户端。

客户端接收数据包如下:

```
magic: 0x5050 (pp协议标识)
status: 0x31 (发送密钥)
type: 0x00
length: 0x08 0x00 0x00 0x00
route: 0x00 0x00 0x00 0x00 0x00 0x00
code: 0x00 0x00 (这里会为服务端随机生成值)
data: 内容为密钥
```



客户端收到24字节数据包后，16字节头部后8字节进行pe解密，初始密钥为{0x00, 0x00, 0x00, 0x00, ,0x00 0x00, 0x00, 0x00}，解密得到pp的code，再对接收内容解密密钥也是8字节全0x00，解密后得到密钥，至此，获取密钥完成。



## PP协议发送数据过程

客户端发送数据包时需要对协议头部后8字节与内容分别进行pe加密，密钥为建立连接时获取的密钥，在头部加密之前，需要将之前获取的code值放入pp协议的code中，用于服务端校验。pp协议头部status也需要改为0x11，代表传输数据，length4字节采用大端设置，type默认为0x00，route为请求路由。





## PP协议接受数据过程

客户端先接收16字节头部信息，判断magic是否为0x5050，再获取length，需转换为host字节序，根据长度获取数据内容。

采用密钥解密头部后8字节获取请求route和code信息，再对数据包进系pe解密，密钥为之前连接时获取的密钥。

