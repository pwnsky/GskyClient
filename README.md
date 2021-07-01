# Pwnsky Protocol SDK

提供不同语言pp协议接口


描述: 

pp 协议，全称为 pwnsky protocol, 是一款吸收http部分特性的一款二进制传输协议，主要用于游戏长连接交互协议，目前基于tcp来实现。

该协议头部只占16字节，相对与http更小，由于协议字段都在固定位置，解析起来更快速。

pp协议中定义有状态码，数据类型，数据长度，请求路由。

采用 pwnsky encryption进行数据加密，由服务端随机生成8字节密钥返回给客户端，客户端接收到之后，在断开之前传输数据都采用该密钥进行加解密。

```
--------------------------------------------------------------------------
| magic 2字节 | status 1字节| type 1字节 |  length 4 字节                  |
--------------------------------------------------------------------------
|                          route 8 字节                                  |
--------------------------------------------------------------------------
```



pp协议 c++ 定义如下：

```
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
    char route[8];           // 请求路由，代替http url中的path
};

}

```
