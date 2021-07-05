using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Runtime.InteropServices; //  StructLayoutAttribute
using System;

/*
 * Pwnsky Protocl C# Client API
 * For Unity3d Game
 * */

namespace PP
{
    public class PPT {
    private string host;
    private int port;
    private Socket tcpSock;
    bool disconnected = true;
    public byte[] RecvRouter = new byte[4];
    public bool ConnectServer(string host, int port) {
        if(this.host == host && this.port == port && disconnected == false) {
            return true;
        }

        this.host = host;
        this.port = port;
        bool result = false;
        IPAddress ipAddress;
        try {
            ipAddress = IPAddress.Parse(host);
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
            Console.WriteLine("Unexpected exception : " + se.ToString());  
        } catch(Exception e) {  
            Console.WriteLine("Unexpected exception : " + e.ToString());  
        }
        return result;
    }

    public bool isConnected() {
        return !disconnected;
    }

    public void Close() {
        disconnected = true;
        tcpSock.Close();
    }

    public bool SendData(byte[] router, byte[] data) {
        if(router.Length > 4 || disconnected == true) {
            return false;

        }
        byte[] headerBytes = new byte[] {0x50, 0x53, 0x50, 0x00};
        byte[] routerBytes = new byte[] {0x00, 0x00, 0x00, 0x00};
        byte[] lengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)data.Length));
        for(int i = 0; i < router.Length; i++) //复制router
        {
            routerBytes[i] = router[i];
        }
            
        //headerBytes = headerBytes.Concat(lengthBytes).ToArray();
        Array.Resize(ref headerBytes, headerBytes.Length + routerBytes.Length + lengthBytes.Length);
        routerBytes.CopyTo(headerBytes, headerBytes.Length - routerBytes.Length - lengthBytes.Length);
        lengthBytes.CopyTo(headerBytes, headerBytes.Length - lengthBytes.Length);
        try {
            tcpSock.Send(headerBytes);
        }catch (SocketException se) {
            disconnected = true;
        }
        
        int sendedLen = 0;
        int totalLen = data.Length;
        while(sendedLen < totalLen) {
            int sendLen = tcpSock.Send(data, sendedLen, totalLen - sendedLen, 0);
            sendedLen += sendLen;
        }
        return true;
    }

    public byte[] RecvData() {
        byte[] headerMagic = new byte[4];
        byte[] headerLength = new byte[4];
        tcpSock.Receive(headerMagic, 0, headerMagic.Length, 0);
        tcpSock.Receive(RecvRouter, 0, RecvRouter.Length, 0);
        tcpSock.Receive(headerLength, 0, headerLength.Length, 0);
        int length = IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(headerLength, 0));
        Console.WriteLine("length: " + length.ToString());
        byte[] data = new byte[length];
        int recvedLen = 0;
        while(length > recvedLen) {
            int recvLen = 0;
            try {
                tcpSock.Receive(data, recvedLen, length - recvedLen, 0);
            }catch (SocketException se) {
                disconnected = true;
            }
            length -= recvLen;
            recvedLen += recvLen;
            if(recvLen == 0) {
                disconnected = true;
                break;
            }
        }
        return data;
    }
}
}
