/*
 * Author: I0gan
 * Date: 2021-07-05
 * C# pp client example
 * */

using System;
using Pwnsky.PP;
using System.Text;

class Test {
    public static void PeTest() {
        Pe pe = new Pe();
        byte[] key = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        string msg = "Hello World!";
        byte[] data = System.Text.Encoding.Default.GetBytes (msg);
        pe.Encode(key, data);
        msg = System.Text.Encoding.Default.GetString (data);

        Console.WriteLine("加密后: " + msg);

        pe.Decode(key, data);
        msg = System.Text.Encoding.Default.GetString (data);
        Console.WriteLine("解密后: " + msg);
    }

    public static void Main(string[] args) {
        System.Console.WriteLine("连接服务器中...");

        PPClient pp = new PPClient();
        if(pp.Connect("pwnsky.com", 4096)) {
            Console.WriteLine("连接成功!");
        }else {
            Console.WriteLine("连接失败!");
        }
        pp.Send(new byte[]{0x30}, Encoding.Default.GetBytes("Hello"));
        while(true) {
            byte[] data = pp.Recv();
            if(data == null) {
                Console.WriteLine("断开连接...");
                break;
            }
            string msg;
            msg = System.Text.Encoding.Default.GetString (data);
            Console.WriteLine("接收: " + msg);
        }
    }
}
