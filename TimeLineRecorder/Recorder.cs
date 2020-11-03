using System;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TimeLineRecorder
{
    public partial class Recorder
    {
        readonly String outputFolder;
        WaveInEvent waveSource = null;
        WaveFileWriter writer = null;
        int counting = 0;
        TimeSpan startTime = new TimeSpan(0, 0, 0);
        TimeSpan timeSpan = new TimeSpan(0, 0, 5);
        DateTime currentStartTime;
        int slidesNum = 0;
        bool flag = false;
        List<KeyValuePair<int, TimeSpan>> slideChange = new List<KeyValuePair<int, TimeSpan>>();

        public long GetRecordTime() {
            TimeSpan recordTime;
            if (flag)
            {
                recordTime = getTimeStamp();
            }
            else 
            {
                recordTime = startTime;
            }
            return (long)recordTime.TotalMilliseconds;
        }

        public bool GetState() 
        {
            return flag;
        }

        public int GetRecordCounting()
        {
            return counting;
        }

        public int GetSlidesNum()
        {
            return slidesNum;
        }

        public String GetPath() {
            return outputFolder;
        }

        public Recorder(String userId, String classId)
        {
            Globals.ThisAddIn.Application.SlideSelectionChanged += (s) =>
            {
                try
                {
                    var slides = Globals.ThisAddIn.Application.ActiveWindow.View.Slide;
                    int index = slides.SlideIndex;
                    if (slideChange.Count == 0 || slideChange[slideChange.Count - 1].Key != index)
                    {
                        slideChange.Add(new KeyValuePair<int, TimeSpan>(index, getTimeStamp()));
                    }
                }
                catch (System.Runtime.InteropServices.COMException e) { 
                    
                }
            };
            Globals.ThisAddIn.Application.SlideShowNextSlide += (s) =>
            {
                int index = Globals.ThisAddIn.Application.SlideShowWindows[1].View.Slide.SlideIndex;
                if(slideChange.Count == 0 || slideChange[slideChange.Count - 1].Key != index)
                {
                    slideChange.Add(new KeyValuePair<int, TimeSpan>(index, getTimeStamp()));
                }
            };
            outputFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, userId, classId);
            Directory.CreateDirectory(outputFolder);
            waveSource = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1)
            };
            waveSource.DataAvailable += (s, a) =>
            {
                writer?.Write(a.Buffer, 0, a.BytesRecorded);
                writer?.Flush();
                writer?.Flush();
            };

            waveSource.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
            };
        }

        public void Record()
        {
            flag = true;
            currentStartTime = DateTime.Now;
            writer = new WaveFileWriter(Path.Combine(outputFolder, counting.ToString() + ".wav"), waveSource.WaveFormat);
            MessageBox.Show(Path.Combine(outputFolder, counting.ToString()));
            waveSource.StartRecording();
            counting+=1;
        }

        private TimeSpan getTimeStamp() {
            return startTime.Add(DateTime.Now.Subtract(currentStartTime));
        }

        public void Stop() 
        {
            flag = false;
            try
            {
                var slides = Globals.ThisAddIn.Application.ActiveWindow.View.Slide;
                int index = slides.SlideIndex;
                if (slideChange.Count == 0 || slideChange[slideChange.Count - 1].Key != index)
                {
                    slideChange.Add(new KeyValuePair<int, TimeSpan>(index, getTimeStamp()));
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {

            }
            startTime = startTime.Add(DateTime.Now.Subtract(currentStartTime));
            waveSource.StopRecording();
        }

        public List<KeyValuePair<int, TimeSpan>> GetSlideChange() {
            Boolean[] flag = new Boolean[slideChange.Count];
            for (int i = 0; i < slideChange.Count - 1; i++) 
            {
                if (slideChange[i + 1].Value.Subtract(slideChange[i].Value).CompareTo(timeSpan) < 0)
                {
                    flag[i] = true;
                }
            }
            List<KeyValuePair<int, TimeSpan>> ans = new List<KeyValuePair<int, TimeSpan>>();
            string val = "";
            for (int i = 0; i < slideChange.Count; i++)
            {
                if (!flag[i])
                {
                    ans.Add(new KeyValuePair<int, TimeSpan>(slideChange[i].Key, TimeSpan.Parse(Regex.Replace(slideChange[i].Value.ToString(), @"\.\d+$", string.Empty))));
                    val = val + "\n" + slideChange[i].Value;
                }
            }
            System.Windows.Forms.MessageBox.Show(val);
            return ans;
        }
    }
}
