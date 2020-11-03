using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Tools.Ribbon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeLineRecorder
{
    public partial class ChooseCourseForm : Form
    {
        RibbonButton bt1;
        RibbonButton bt2;
        public ChooseCourseForm(Course[] courses, RibbonButton bt1, RibbonButton bt2)
        {
            InitializeComponent();
            this.bt1 = bt1;
            this.bt2 = bt2;
            for (int i = 0; i < courses.Length; i++)
                comboBox1.Items.Add(new ListComponentItem(courses[i].id + ", " + courses[i].name, courses[i]));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Course chosenCourse = (Course)(((ListComponentItem)comboBox1.SelectedItem).Value);
            //MessageBox.Show("id = " + chosenCourse.id);
            //MessageBox.Show("destination = " + chosenCourse.destination);
            bt1.Label = Ribbon1.pauseLabel;
            bt2.Label = Ribbon1.stopLabel;
             string serverIP = ConfigurationManager.AppSettings["AddClassIP"];
            
            if (textBox1.Text == null || textBox1.Text.Equals(""))
            {
                MessageBox.Show("请输入className!");
                return;
            }

            // 请求classID
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(serverIP);
            req.Method = "POST";    //确定传值的方式，此处为post方式传值
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder builder = new StringBuilder();
            builder.Append("className=" + textBox1.Text +"&courseId=" + chosenCourse.id);
            Ribbon1.courseId = chosenCourse.id;
            MessageBox.Show(builder.ToString());
            byte[] bs = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = bs.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            string classID = null;
            using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream(), Encoding.UTF8))
            {
                string msg = reader.ReadToEnd();
                JObject msgJS = (JObject)JsonConvert.DeserializeObject(msg);
                if (msgJS["flag"].ToString().Equals("True"))
                {
                    JObject resJS = (JObject)msgJS["result"];
                    classID = resJS["id"].ToString();
                }
            }
            if (classID == null)
            {
                MessageBox.Show("服务器失效!");
                return;
            }
            Ribbon1.recorder = new Recorder(Ribbon1.loginStatus.userId, classID);
            Ribbon1.recorder.Record();
            Ribbon1.classId = classID;
            Heartbeat.start(classID, chosenCourse.id, Ribbon1.loginStatus.userId);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
