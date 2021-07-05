/*
 * Author: I0gan
 * Date: 2021-07-05
 * C# pp client example
 * */

using System;
using pwnsky.pp;

class Test {
    public static PeTest() {
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
        pp.Connect("127.0.0.1", 4096);
    }
}
