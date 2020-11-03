using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace TimeLineRecorder
{
    public partial class Ribbon1
    {
        PowerPoint.Application app;
        bool start = false;
        public static Recorder recorder;
        public static Login loginStatus = null;  // 登录状态
        public static string[] Courses;
        public static string startLabel = "开始上课";
        public static string playLabel = "继续录制";
        public static string pauseLabel = "暂停";

        public static string uploadLabel = "上传本堂课件";
        public static string stopLabel = "结束本次课程";
        public static string classId;
        public static string courseId;
        

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            app = Globals.ThisAddIn.Application;
        }
        
        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            // 登录窗口
            if (loginStatus == null)
            {
                LoginForm loginForm = new LoginForm(button1);
                loginForm.Show();
                
            }
            else
            {
                DialogResult dr = MessageBox.Show("是否退出登录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    loginStatus = null;
                    MessageBox.Show("成功退出登录");
                    button1.Label = "登录";
                }
            }

        }

        private void button2_Click_1(object sender, RibbonControlEventArgs e)
        {
            if (button2.Label.Equals(startLabel))
            {
                if (loginStatus == null)
                {
                    MessageBox.Show("请先登录");
                    return;
                }
                Course[] courses = Course.download();

                ChooseCourseForm chooseCourseForm = new ChooseCourseForm(courses, button2, button3);
                chooseCourseForm.Show();
            }
            else if (button2.Label.Equals(pauseLabel))  // 当前是暂停按钮
            {
                Ribbon1.recorder.Stop();
                button2.Label = playLabel;
            }
            else if (button2.Label.Equals(playLabel))   // 当前是继续按钮
            {

                Ribbon1.recorder.Record();
                button2.Label = pauseLabel;
            }
        }


        private void button3_Click(object sender, RibbonControlEventArgs e)
        {
            //var wssv = new WebSocketServer(8082);
            //wssv.AddWebSocketService<Laputa>("/Laputa");
            //MessageBox.Show("true");
            //wssv.Start();


            //MessageBox.Show("1");
            //Csharpsocket websocket = new Csharpsocket();
            //websocket.SendHeart();

            //wssv.Stop();
            //MessageBox.Show("结束");
            recorder.Stop();
            Heartbeat.start(Ribbon1.classId, courseId, Ribbon1.loginStatus.userId);
            SendFile sder = new SendFile();
            sder.sendPPT(recorder);
            sder.sendRecord(recorder);
            sder.sendTimeSpan(recorder);
            Confirm confirm = new Confirm(sder);
            confirm.Show();
        }
        //public static void main(string[] args)
        //{
        //    var outputFolder = AppDomain.CurrentDomain.BaseDirectory;
        //    Console.WriteLine(outputFolder);
        //    Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //}
    }

    
}
