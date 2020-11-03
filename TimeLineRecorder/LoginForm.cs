using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Tools.Ribbon;

namespace TimeLineRecorder
{

    public partial class LoginForm : Form
    {

        RibbonButton bt; // 待修改的外部button
        public LoginForm(RibbonButton bt)
        {
            InitializeComponent();
            this.bt = bt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string id = idBox.Text;
            string passWord = passwordBox.Text;
            this.Close();
            try
            {
                Ribbon1.loginStatus = Login.TryLogin(id, passWord);
                if (Ribbon1.loginStatus != null)
                {
                    MessageBox.Show("登录成功！");
                    MessageBox.Show("name = " + Ribbon1.loginStatus.userName);
                    bt.Label = Ribbon1.loginStatus.userName;
                }
                else
                {
                    MessageBox.Show("登录失败！");
                }
            }
            catch
            {
                MessageBox.Show("登录失败，未连接上服务器！");
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        
    }
}
