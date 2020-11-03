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
    
    public class Login
    {
        Login() { }
        
        public string userName = "";
        public string userSex = "";
        public string userId = "";

        /// <summary>
        /// 尝试登陆
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="passWord">用户密码</param>
        /// <returns></returns>
        public static Login TryLogin(string id, string passWord) 
        {
            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
           // MessageBox.Show("id = " + id);
            //MessageBox.Show("password = " + passWord);
            Login ans = null;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(serverIP);

            req.Method = "POST";    //确定传值的方式，此处为post方式传值
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder builder = new StringBuilder();
            builder.Append("id=" + id +"&password=" + passWord);
            MessageBox.Show(builder.ToString());
            byte[] bs = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = bs.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }

            //在这里对接收到的页面内容进行处理 
            using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream(), Encoding.UTF8))
            {
                string msg = reader.ReadToEnd();
                
                JObject msgJS = (JObject)JsonConvert.DeserializeObject(msg);

                if (msgJS["flag"].ToString().Equals("True"))
                {
                    ans = new Login();
                    JObject resJS = (JObject)msgJS["result"];
                    ans.userName = resJS["name"].ToString();
                    ans.userSex = resJS["sex"].ToString();
                }
            }
            ans.userId = id;
            return ans;
        }
        
     }
}
