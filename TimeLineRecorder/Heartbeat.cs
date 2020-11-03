using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeLineRecorder
{
    
    class Heartbeat
    {
        static ClientWebSocket cln;
        static string urlPrefix = "ws://192.168.0.100:8082/sw2020/wsserver/teacher/";
        static string url;
        static Thread childThread;
        static string classID;
        static string courseId;
        static string userId;
        public static void sendHeart()
        {
            cln = new ClientWebSocket();
            cln.ConnectAsync(new Uri(url), new CancellationToken()).Wait();
            while (true)
            {
                Thread.Sleep(100);
                byte[] bs = Encoding.UTF8.GetBytes("{\"state\": " + (Ribbon1.recorder.GetState()? "true":"false") + ", \"systemTime\": 1234567, \"recordTime\": " + Ribbon1.recorder.GetRecordTime() + ", \"classId\": \""+ classID +"\"}");

                cln.SendAsync(new ArraySegment<byte>(bs), System.Net.WebSockets.WebSocketMessageType.Text, true, new CancellationToken()).Wait();
            }
        }

        public static void start(string classID, string courseId, string userId)
        {
            Heartbeat.classID = classID;
            Heartbeat.courseId = courseId;
            Heartbeat.userId = userId;
            Heartbeat.url = Heartbeat.urlPrefix + "/" + Heartbeat.userId + "/" + Heartbeat.courseId;
            ThreadStart childref = new ThreadStart(sendHeart);
            childThread = new Thread(childref);
            childThread.Start();
        }
        public static void stop()
        {
            cln.CloseAsync(WebSocketCloseStatus.Empty, "", new CancellationToken());
            childThread.Abort();
        }
    }
}
