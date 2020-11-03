using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeLineRecorder
{
    public class Course
    {
        public string id;
        public string name;
        public string teacherId;
        public int maxVol;
        public string destination;
        public int realVol;

        //public override string ToString()
        //{
        //    return this.id + ", " + this.name;
        //}
        public static Course[] download()
        {
            Course[] ans = null;
            string serverIP = ConfigurationManager.AppSettings["CourseIP"];
            //MessageBox.Show("id = " + id);
            //MessageBox.Show("password = " + passWord);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(serverIP);

            req.Method = "POST";    //确定传值的方式，此处为post方式传值
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder builder = new StringBuilder();
            builder.Append("id=" + Ribbon1.loginStatus.userId);
            byte[] bs = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = bs.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            //在这里对接收到的页面内容进行处理 
            using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                string msg = reader.ReadToEnd();
                //MessageBox.Show(msg);
                JObject msgJS = (JObject)JsonConvert.DeserializeObject(msg);
                
                if (msgJS["flag"].ToString().Equals("True"))
                {

                    //JObject resJS = (JObject)msgJS["result"];
                    JArray resJS = (JArray)msgJS["result"];
                    MessageBox.Show(resJS.ToString());
                    ans = new Course[resJS.Count];
                    for (int i = 0; i < resJS.Count; i++)
                    {
                        JObject courseJS = (JObject)resJS[i];
                        Course c = new Course();
                        c.id = courseJS["id"].ToString();
                        c.name = courseJS["name"].ToString();
                        c.teacherId = courseJS["teacherId"].ToString();
                        c.maxVol = (int)(courseJS["maxVol"]);
                        c.destination = courseJS["destination"].ToString();
                        c.realVol = (int)(courseJS["realVol"]);
                        ans[i] = c;
                    }

                }
            }

            return ans;
        }
    }
}
