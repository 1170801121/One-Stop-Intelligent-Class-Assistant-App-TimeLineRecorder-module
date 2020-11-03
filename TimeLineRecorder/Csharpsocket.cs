using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebSocketSharp;
using TimeLineRecorder;
using System.Windows.Forms;

namespace TimeLineRecorder
{
    class Csharpsocket
    {
        String url = "ws://127.0.0.1:8082/sw2020/wsserver/teacher/01/16";

        //String url = "ws://dragonsnest.far/Laputa";
        //String url = "127.0.0.1::8082";
        public void SendHeart()
        {
            
            var ws = new WebSocket(url);
            ws.OnOpen += (sender, e) =>
                MessageBox.Show("建立socket");
            ws.OnMessage += (sender, e) =>
                MessageBox.Show("接受消息: " + e.Data);
            ws.OnError += (sender, e) => {
                MessageBox.Show("error: ");
            };
            MessageBox.Show("进入方法");
            ws.Connect();
            while (true)
            {
                System.Threading.Thread.Sleep(1 * 1000);
                ws.Send("BALUS");
                MessageBox.Show("建立socket");
            }
        }
        public static void main(string[] args)
        {
            MessageBox.Show("1");
            Csharpsocket websocket = new Csharpsocket();
            websocket.SendHeart();
        }


        public void SendFIle()
        {
            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
                Console.WriteLine("接受消息: " + e.Data);
            ws.ConnectAsync();

            //读取选择的文件
            //using (FileStream fsRead = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
            //{
            //    byte[] buffer = new byte[1024 * 1024 * 2];
            //    int r = 0;
            //    while ((r = fsRead.Read(buffer, 0, buffer.Length)) != 0)
            //    {
            //        //获得发送的信息
            //        List<byte> list = new List<byte>();
            //        list.AddRange(buffer);
            //        byte[] newBuffer = list.ToArray();
            //        //将字节流转成Base64编码的字符串
            //        string encryptBase64 = Convert.ToBase64String(newBuffer);
            //        //将字符串转成字节流
            //        byte[] encryptoByte = Encoding.ASCII.GetBytes(encryptBase64);
            //        //将了标识字符的字节数组传递给客户端
            //        ws.Send(encryptoByte);
            //    }
                
            //}
        }

    }
}
