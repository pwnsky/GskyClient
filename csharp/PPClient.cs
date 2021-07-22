using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Runtime.InteropServices; //  StructLayoutAttribute
using System;
using System.Linq;

/*
 * Pwnsky Protocl C# Client API
 * For Unity3d Game
 * */
namespace Pwnsky {
namespace PP
{
    public class Pe {
        private byte[] xorTable = new byte[]{
    0xbe, 0xd1, 0x90, 0x88, 0x57, 0x00, 0xe9, 0x53, 0x10, 0xbd, 0x2a, 0x34, 0x51, 0x84, 0x07, 0xc4,
    0x33, 0xc5, 0x3b, 0x53, 0x5f, 0xa8, 0x5d, 0x4b, 0x6d, 0x22, 0x63, 0x5d, 0x3c, 0xbd, 0x47, 0x6d,
    0x22, 0x3f, 0x38, 0x4b, 0x7a, 0x4c, 0xb8, 0xcc, 0xb8, 0x37, 0x78, 0x17, 0x73, 0x23, 0x27, 0x71,
    0xb1, 0xc7, 0xa6, 0xd1, 0xa0, 0x48, 0x21, 0xc4, 0x1b, 0x0a, 0xad, 0xc9, 0xa5, 0xe6, 0x14, 0x18,
    0xfc, 0x7b, 0x53, 0x59, 0x8b, 0x0d, 0x07, 0xcd, 0x07, 0xcc, 0xbc, 0xa5, 0xe0, 0x28, 0x0e, 0xf9,
    0x31, 0xc8, 0xed, 0x78, 0xf4, 0x75, 0x60, 0x65, 0x52, 0xb4, 0xfb, 0xbf, 0xac, 0x6e, 0xea, 0x5d,
    0xca, 0x0d, 0xb5, 0x66, 0xac, 0xba, 0x06, 0x30, 0x95, 0xf4, 0x96, 0x42, 0x7a, 0x7f, 0x58, 0x6d,
    0x83, 0x8e, 0xf6, 0x61, 0x7c, 0x0e, 0xfd, 0x09, 0x6e, 0x42, 0x6b, 0x1e, 0xb9, 0x14, 0x22, 0xf6,

    0x16, 0xd2, 0xd2, 0x60, 0x29, 0x23, 0x32, 0x9e, 0xb4, 0x82, 0xee, 0x58, 0x3a, 0x7d, 0x1f, 0x74,
    0x98, 0x5d, 0x17, 0x64, 0xe4, 0x6f, 0xf5, 0xad, 0x94, 0xaa, 0x89, 0xe3, 0xbe, 0x98, 0x91, 0x38,
    0x70, 0xec, 0x2f, 0x5e, 0x9f, 0xc9, 0xb1, 0x26, 0x3a, 0x64, 0x48, 0x13, 0xf1, 0x1a, 0xc5, 0xd5,
    0xe5, 0x66, 0x11, 0x11, 0x3a, 0xaa, 0x79, 0x45, 0x42, 0xb4, 0x57, 0x9d, 0x3f, 0xbc, 0xa3, 0xaa,
    0x98, 0x4e, 0x6b, 0x7a, 0x4a, 0x2f, 0x3e, 0x10, 0x7a, 0xc5, 0x33, 0x8d, 0xac, 0x0b, 0x79, 0x33,
    0x5d, 0x09, 0xfc, 0x9d, 0x9b, 0xe5, 0x18, 0xcd, 0x1c, 0x7c, 0x8b, 0x0a, 0xa8, 0x95, 0x56, 0xcc,
    0x4e, 0x34, 0x31, 0x33, 0xf5, 0xc1, 0xf5, 0x03, 0x0a, 0x4a, 0xb4, 0xd1, 0x90, 0xf1, 0x8f, 0x57,
    0x20, 0x05, 0x0d, 0xa0, 0xcd, 0x82, 0xb3, 0x25, 0xd8, 0xd2, 0x20, 0xf3, 0xc5, 0x96, 0x35, 0x35,
    };
        public void Encode(byte[] key, byte[] data) {
            byte[] keys = new byte[8];
            Array.Copy(key, keys, keys.Length);
            for(int i = 0; i < data.Length; i++) {
                int n = ((keys[i & 7] + keys[(i + 1) & 7]) * keys[(i + 2) & 7] + keys[(i + 3) & 7]) & 0xff;
                data[i] = (byte)(data[i] ^ (byte)n ^ xorTable[n]);
                keys[i & 7] = (byte)((n * 2 + 3) & 0xff);
                if((i & 0xf) == 0) {
                    KeyRandom(key, keys, xorTable[i & 0xff]);
                }
            }
        }

        public void Decode(byte[] key, byte[] data) {
            byte[] keys = new byte[8];
            Array.Copy(key, keys, keys.Length);
            for(int i = 0; i < data.Length; i++) {
                int n = ((keys[i & 7] + keys[(i + 1) & 7]) * keys[(i + 2) & 7] + keys[(i + 3) & 7]) & 0xff;
                data[i] = (byte)(data[i] ^ (byte)n ^ xorTable[n]);
                keys[i & 7] = (byte)((n * 2 + 3) & 0xff);
                if((i & 0xf) == 0) {
                    KeyRandom(key, keys, xorTable[i & 0xff]);
                }
            }
        }

        private void KeyRandom(byte[] raw_key, byte[] out_key, byte seed) {
            for(int i = 0; i < 8; i ++)  {
                out_key[i] = (byte)((raw_key[i] ^ xorTable[raw_key[i]]) & 0xff);
                out_key[i] = (byte)(out_key[i] ^ seed + i);
            } 
        }
    }

    public class PPClient {
    private byte[] key = new byte[8] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};  // pp key
    private byte[] code = new byte[2]; // pp code
    private string host;
    private int port;
    private Socket tcpSock;
    bool disconnected = true;
    public byte[] router = new byte[6];

    public bool Connect(string host, int port) {
        if(this.host == host && this.port == port && disconnected == false) {
            return true;
        }

        this.host = host;
        this.port = port;
        bool result = false;
        IPAddress ipAddress;
        try {
            ipAddress = IPAddress.Parse(host); //解析失败采用dns解析
        } catch(Exception e) {
            IPHostEntry ipHostInfo = Dns.Resolve(host);
            ipAddress = ipHostInfo.AddressList[0];  
        }

        tcpSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            tcpSock.Connect(new IPEndPoint(ipAddress , port));
            result = true;
            disconnected = false;
        } catch (SocketException se) {
            Console.WriteLine("SocketException : " + se.ToString());  
            return false;
        } catch(Exception e) {  
            Console.WriteLine("Unexpected exception : " + e.ToString());  
            return false;
        }

        // 连接成功后，获取key请求
        byte[] headerBytes = new byte[] {
            0x50, 0x50, 0x10, 0x00, // 0x5050 标识头， 0x10 请求密钥, 0x00内容类型
            0x00, 0x00, 0x00, 0x00, // 长度
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // route
            0x00, 0x00 // code
        };
        try {
            tcpSock.Send(headerBytes);
        }catch (SocketException se) {
            disconnected = true;
        }

        byte[] ppHeaderData = new byte[16];
        byte[] ppMagicBytes = new byte[2]; 
        byte[] ppStatusBytes = new byte[1]; 
        //byte[] ppTypeBytes = new byte[1]; 
        byte[] ppLengthBytes = new byte[4]; 
        byte[] ppCodeBytes = new byte[2]; 

        byte[] ppHeaderLeftBytes = new byte[8]; 

        // 接收数据
        tcpSock.Receive(ppHeaderData, 0, ppHeaderData.Length, 0);

        ppMagicBytes = ppHeaderData.Skip(0).Take(2).ToArray();   // 标识
        ppStatusBytes = ppHeaderData.Skip(2).Take(1).ToArray();  // 状态
        //ppTypeBytes = ppHeaderData.Skip(3).Take(1).ToArray(); // 类型
        ppLengthBytes = ppHeaderData.Skip(4).Take(4).ToArray(); // 获取长度
        ppHeaderLeftBytes = ppHeaderData.Skip(8).Take(8).ToArray(); // 获取头部剩余部分

        int ppLength = IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(ppLengthBytes, 0));
        byte ppStatus = ppStatusBytes[0];

        if(ppStatus != 0x31) { // 密钥返回标识
            Console.WriteLine("PP Connecte Failed!");
            tcpSock.Close();
            disconnected = true;
            return false;
        }

        //接受数据部分
        byte[] ppData = new byte[ppLength];
        int recvedLen = 0;
        while(ppLength > recvedLen) {
            int recvLen = 0;
            try {
                recvLen = tcpSock.Receive(ppData, recvedLen, ppLength - recvedLen, 0);
            }catch (SocketException se) {
                Console.WriteLine("Unexpected exception : " + se.ToString());  
                disconnected = true;
            }
            recvedLen += recvLen;
            if(recvLen == 0) {
                Console.WriteLine("Disconnected!");
                tcpSock.Close();
                disconnected = true;
                return false;
                //break;
            }
        }

        /*
        //对密钥头部进行解密
        */
        Pe pe = new Pe();

        pe.Decode(key, ppHeaderLeftBytes);

        ppCodeBytes = ppHeaderLeftBytes.Skip(6).Take(2).ToArray();
        Array.Copy(ppCodeBytes, code, code.Length);

        /*
        for(int i = 0; i < code.Length; i++) {
            Console.WriteLine("code: " + code[i]);
        }
        */

        pe.Decode(key, ppData);
        key = ppData;
        Array.Copy(key, ppData, key.Length);
        /*
        for(int i = 0; i < ppData.Length; i++) {
            Console.WriteLine("Key: " + key[i]);
        }
        */
        //Console.WriteLine("Key: Deubg");
        return result;
    }

    public bool isConnected() {
        return !disconnected;
    }

    public void Close() {
        Array.Copy(new byte[8] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}, key, key.Length); //密钥初始化
        disconnected = true;
        tcpSock.Close();
    }

    public bool Send(byte[] route, byte[] data) {
        if(route.Length > 4 || disconnected == true) {
            return false;

        }
        /*
         * magic: 0x5050
         * transfer: 0x11
         * binary_stream: 0x00
         * */

        byte[] headerBytes = new byte[] {0x50, 0x50, 0x11, 0x00};
        byte[] lengthBytes = BitConverter.GetBytes((int)IPAddress.HostToNetworkOrder((int)data.Length));
        byte[] headerEncodyBytes = new byte[8];

        if(route.Length > 6) {
            Console.WriteLine("Route Length > 6");
            return false;
        }
        for(int i = 0; i < route.Length; i++) //复制route
        {
            headerEncodyBytes[i] = route[i];
        }

        // 复制code
        headerEncodyBytes[6] = code[0];
        headerEncodyBytes[7] = code[1];
        Pe pe = new Pe();
        pe.Encode(key, headerEncodyBytes); //加密头部
        pe.Encode(key, data);              //加密内容

        Array.Resize(ref headerBytes, headerBytes.Length + lengthBytes.Length + headerEncodyBytes.Length);
        lengthBytes.CopyTo(headerBytes, headerBytes.Length - lengthBytes.Length - headerEncodyBytes.Length); //复制到header Length位置
        headerEncodyBytes.CopyTo(headerBytes, headerBytes.Length - headerEncodyBytes.Length); //复制到headerBytes中 Route
        try {
            tcpSock.Send(headerBytes);
        }catch (SocketException se) {
            disconnected = true;
            return false;
        }
        
        int sendedLen = 0;
        int totalLen = data.Length;
        while(sendedLen < totalLen) {
            int sendLen = tcpSock.Send(data, sendedLen, totalLen - sendedLen, 0);
            sendedLen += sendLen;
        }
        return true;
    }

    public byte[] Recv() {
        if(disconnected) {
            return null;
        }

        byte[] ppHeaderData = new byte[16];
        byte[] ppMagicBytes = new byte[2]; 
        byte[] ppStatusBytes = new byte[1]; 
        byte[] ppTypeBytes = new byte[1]; 
        byte[] ppLengthBytes = new byte[4]; 
        byte[] ppCodeBytes = new byte[2]; 
        byte[] ppHeaderLeftBytes = new byte[8]; 

        // 接收数据
        tcpSock.Receive(ppHeaderData, 0, ppHeaderData.Length, 0);

        ppMagicBytes = ppHeaderData.Skip(0).Take(2).ToArray();   // 标识
        ppStatusBytes = ppHeaderData.Skip(2).Take(1).ToArray();  // 状态
        ppTypeBytes = ppHeaderData.Skip(3).Take(1).ToArray(); // 类型
        ppLengthBytes = ppHeaderData.Skip(4).Take(4).ToArray(); // 获取长度
        ppHeaderLeftBytes = ppHeaderData.Skip(8).Take(8).ToArray(); // 获取头部剩余部分

        int ppLength = IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(ppLengthBytes, 0));
        byte ppStatus = ppStatusBytes[0];

        //接受数据部分
        byte[] ppData = new byte[ppLength];
        int recvedLen = 0;
        while(ppLength > recvedLen) {
            int recvLen = 0;
            try {
                recvLen = tcpSock.Receive(ppData, recvedLen, ppLength - recvedLen, 0);
            }catch (SocketException se) {
                Console.WriteLine("Unexpected exception : " + se.ToString());  
                disconnected = true;
            }
            recvedLen += recvLen;
            if(recvLen == 0) {
                Console.WriteLine("Disconnected!");
                tcpSock.Close();
                disconnected = true;
                return null;
                //break;
            }
        }
        /*
        //对密钥头部进行解密
        */
        Pe pe = new Pe();
        pe.Decode(key, ppHeaderLeftBytes);
        pe.Decode(key, ppData);
        //ppCodeBytes = ppHeaderLeftBytes.Skip(6).Take(2).ToArray();
        Array.Copy(ppHeaderLeftBytes, router, router.Length);
        return ppData;
    }
}

}
}
