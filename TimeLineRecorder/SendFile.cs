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
using System.Collections.Specialized;

namespace TimeLineRecorder
{

    public partial  class SendFile
    {
        String outputFolder;
        String classID;

        /// <summary>  
        /// 将本地PPT上传到指定的服务器(HttpWebRequest方法)  
        /// </summary>  
        /// <param name="fileNamePath">要上传的本地文件（全路径）</param>  
        /// <returns>成功返回1，失败返回0</returns>  

        public int SlidesFetcher(String outputFolder)
        {
            float width = Globals.ThisAddIn.Application.ActivePresentation.PageSetup.SlideWidth;
            float height = Globals.ThisAddIn.Application.ActivePresentation.PageSetup.SlideHeight;
            var slidesNum = Globals.ThisAddIn.Application.ActivePresentation.Slides.Count;
            Globals.ThisAddIn.Application.ActivePresentation.Export(Path.Combine(outputFolder, "slides"), "png", (int)width, (int)height);
            return slidesNum;
        }

        public int sendPPT(Recorder r)
        {
            String path = r.GetPath();
            int returnValue = 0;
            int slidesNum = SlidesFetcher(path);
            for (int i = 0; i < slidesNum; i++)
            {
                MessageBox.Show(path);
                sendFile(Path.Combine(path, "slides", "幻灯片" + (i+1) + ".png"), "pptimage", (i+1) + ".png", Ribbon1.classId);
                sendFileEnd();
            }
            return returnValue;
        }

        // <summary>  
        /// 将本地录音上传到指定的服务器(HttpWebRequest方法)  
        /// </summary>  
        /// <param name="fileNamePath">要上传的本地文件（全路径）</param>  
        /// <returns>成功返回1，失败返回0</returns>  
        public int sendRecord(Recorder r)
        {
            String path = r.GetPath();
            int returnValue = 0;
            int RecordCounting = r.GetRecordCounting();
            for (int i = 0; i < RecordCounting; i++)
            {
                sendFile(Path.Combine(path , i + ".wav"), "audio", i + ".wav", Ribbon1.classId);
                sendFileEnd();
            }
            return returnValue;
        }

        // <summary>  
        /// 将本地时间戳上传到指定的服务器(HttpWebRequest方法)  
        /// </summary>  
        /// <param name="fileNamePath">要上传的本地文件（全路径）</param>  
        /// <returns>成功返回1，失败返回0</returns>  
        public int sendTimeSpan(Recorder r)
        {
           
            String path = r.GetPath();
            int returnValue = 0;
            List<KeyValuePair<int, TimeSpan>> RecordCounting = r.GetSlideChange();
            String sendString = "[\n";
            for (int i = 0; i < RecordCounting.Count; i++)
            {
                if (i != 0)
                {
                    sendString = sendString + ",\n";
                }
                sendString = sendString + "{\"order\":"+i + ",\"page\":" + RecordCounting[i].Key + ",\"time\":\"" + RecordCounting[i].Value + "\"}";
            }
            sendString = sendString + "\n]";

            try
            {
                outputFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory);
                FileStream fs = new FileStream(Path.Combine(outputFolder, "timeofppt.txt"), FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(sendString);
                sw.Close();
                fs.Close();
                fs.Dispose();
                MessageBox.Show("文件保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            sendFile(Path.Combine(outputFolder, "timeofppt.txt"), "normal", "timeofppt.txt", Ribbon1.classId);
            sendFileEnd();
            return returnValue;
        }


        public void sendFileEnd()
        {
            string serverIP = ConfigurationManager.AppSettings["FileEndIP"];
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(serverIP);

            req.Method = "POST";    //确定传值的方式，此处为post方式传值
            req.ContentType = "application/x-www-form-urlencoded";
            StringBuilder builder = new StringBuilder();
            builder.Append("classId=" + Ribbon1.classId);
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
                    MessageBox.Show("4545");
                }
            }
        }



            // <summary>  
            /// 将本地文件上传到指定的服务器(HttpWebRequest方法)  
            /// </summary>  
            /// <param name="fileNamePath">要上传的本地文件（全路径）</param>  
            /// <returns>成功返回1，失败返回0</returns>  
            public void sendFile(string fileNamePath, string type, string fileName, string classId)
        {
            string contentType = "*";
            string paramName = "file";
            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("classId",classId);
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            string serverIP = ConfigurationManager.AppSettings["FileUploadIP"] + "/" + type;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverIP);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            Stream requestStream = request.GetRequestStream();
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nameValueCollection.Keys)
            {
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nameValueCollection[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                requestStream.Write(formitembytes, 0, formitembytes.Length);
            }
            requestStream.Write(boundarybytes, 0, boundarybytes.Length);
            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, fileName, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            requestStream.Write(headerbytes, 0, headerbytes.Length);
            FileStream fileStream = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            requestStream.Write(trailer, 0, trailer.Length);
            requestStream.Close();
            WebResponse webResponse = null;
            try
            {
                webResponse = request.GetResponse();
                Stream responseStream = webResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string result = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse = null;
                }
            }
            finally
            {
                request = null;
            }



            //string formdataTemplate = "Content-Disposition: form-data; filename=\"{0}\";\r\nContent-Type: image/jpeg\r\n\r\n";
            //string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            //byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            //request.ServicePoint.Expect100Continue = false;
            //request.Method = "POST";
            //request.ContentType = "multipart/form-data; boundary=" + boundary;

            //using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    using (Stream requestStream = request.GetRequestStream())
            //    {
            //        requestStream.Write(boundarybytes, 0, boundarybytes.Length);
            //        string formitem = string.Format(formdataTemplate, Path.GetFileName(filePath));
            //        byte[] formbytes = Encoding.UTF8.GetBytes(formitem);
            //        requestStream.Write(formbytes, 0, formbytes.Length);
            //        byte[] buffer = new byte[1024 * 4];
            //        int bytesLeft = 0;

            //        while ((bytesLeft = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            //        {
            //            requestStream.Write(buffer, 0, bytesLeft);
            //        }

            //    }
            //}

            //try
            //{
            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) { }

            //    Console.WriteLine("Success");
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}





            //int returnValue = 0;

            //// 要上传的文件  
            //FileStream fs = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            //BinaryReader r = new BinaryReader(fs);

            //string serverIP = ConfigurationManager.AppSettings["FileUploadIP"] + "/" + type;
            //HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(serverIP);
            //httpReq.Method = "POST";    //确定传值的方式，此处为post方式传值

            ////对发送的数据不使用缓存  
            //httpReq.AllowWriteStreamBuffering = false;

            //string boundary = string.Format("---------------------------{0}", DateTime.Now.Ticks.ToString("x"));

            //byte[] header = Encoding.UTF8.GetBytes(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: application/octet-stream\r\n\r\n",
            //    boundary, fileNamePath, Path.GetFileName(fileNamePath)));
            //byte[] footer = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));

            //httpReq.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            //httpReq.ContentLength = header.Length + fs.Length + footer.Length;

            //long fileLength = fs.Length;
            ////    httpReq.ContentLength = length;


            //try
            //{
            //    //每次上传4k  
            //    int bufferLength = 4096;
            //    byte[] buffer = new byte[bufferLength];

            //    //已上传的字节数  
            //    long offset = 0;

            //    //开始上传时间  
            //    DateTime startTime = DateTime.Now;
            //    int size = r.Read(buffer, 0, bufferLength);
            //    Stream postStream = httpReq.GetRequestStream();

            //    // 写入分割线及数据信息
            //    postStream.Write(header, 0, header.Length);

            //    while (size > 0)
            //    {
            //        postStream.Write(buffer, 0, size);
            //        offset += size;
            //        size = r.Read(buffer, 0, bufferLength);
            //    }
            //    //添加尾部的时间戳  
            //    //postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            //    //postStream.Close();
            //    // 写入尾部
            //    postStream.Write(footer, 0, footer.Length);

            //    //获取服务器端的响应  
            //    WebResponse webRespon = httpReq.GetResponse();
            //    Stream s = webRespon.GetResponseStream();
            //    StreamReader sr = new StreamReader(s);

            //    MessageBox.Show("响应");

            //    //读取服务器端返回的消息  
            //    string msg = sr.ReadToEnd();
            //    MessageBox.Show("消息" + msg);
            //    JObject msgJS = (JObject)JsonConvert.DeserializeObject(msg);


            //    if (msgJS["flag"].ToString().Equals("true"))
            //    {
            //        returnValue = 1;
            //    }
            //    else if (msgJS["flag"].ToString().Equals("false"))
            //    {
            //        returnValue = 0;
            //    }
            //    s.Close();
            //    sr.Close();
            //}
            //catch
            //{
            //    returnValue = 0;
            //}
            //finally
            //{
            //    fs.Close();
            //    r.Close();
            //}

            //MessageBox.Show("" + returnValue);
            //return returnValue;


            //HttpWebRequest request = null;
            ////FileStream fileStream = FileHelper.GetFileStream(fileNamePath);
            //FileStream fileStream = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            //String strUrl = ConfigurationManager.AppSettings["FileUploadIP"] + "/" + type;
            //try
            //{
            //    if (fileStream == null)
            //    {
            //        throw new FileNotFoundException();
            //    }

            //    request = (HttpWebRequest)WebRequest.Create(strUrl);
            //    request.Method = "POST";
            //    request.KeepAlive = false;
            //    request.Timeout = 30000;


            //    string boundary = string.Format("---------------------------{0}", DateTime.Now.Ticks.ToString("x"));

            //    byte[] header = Encoding.UTF8.GetBytes(string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: application/octet-stream\r\n\r\n",
            //        boundary, fileNamePath, Path.GetFileName(fileNamePath)));
            //    byte[] footer = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));

            //    request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            //    request.ContentLength = header.Length + fileStream.Length + footer.Length;

            //    using (Stream reqStream = request.GetRequestStream())
            //    {
            //        // 写入分割线及数据信息
            //        reqStream.Write(header, 0, header.Length);

            //        // 写入文件
            //        FileHelper.WriteFile(reqStream, fileStream);

            //        // 写入尾部
            //        reqStream.Write(footer, 0, footer.Length);
            //    }

            //    strResult = GetResponseResult(request, cookieContainer);
            //}
            //catch (Exception ex)
            //{
            //    strResult = ex.Message;
            //    return false;
            //}
            //finally
            //{
            //    if (request != null)
            //    {
            //        request.Abort();
            //    }
            //    if (fileStream != null)
            //    {
            //        fileStream.Close();
            //    }
            //}

            //return true;
        }

    }

   
}
