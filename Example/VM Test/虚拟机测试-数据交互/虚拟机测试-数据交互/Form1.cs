using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace 虚拟机测试_数据交互
{
    public partial class Form1 : Form, FormShow
    {
        private readonly VisualMachine Machine;
        private readonly Device Device;
        private readonly System.Timers.Timer Timer;

        delegate void ShowCallBack(string str);
        delegate void TimeEventCallBack(object sender, System.Timers.ElapsedEventArgs e);

        public Form1()
        {
            InitializeComponent();
            Machine = new VisualMachine(this);
            Device = new Device(Machine);
            Machine.AddDevice(0, Device);
            Timer = new System.Timers.Timer(100)
            {
                AutoReset = true
            };
            Timer.Elapsed += new System.Timers.ElapsedEventHandler(Time_Event);
            Timer.Start();
        }

        ~Form1()
        {
            Timer.Stop();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if ("" != textBox2.Text)
            {
                Device.Send(textBox2.Text);
                textBox1.AppendText("交付设备处理\r\n");
                textBox1.AppendText("\t" + textBox2.Text + "\r\n");
                textBox2.Text = "";
            }
        }

        public void Show(string str)
        {
            if (null != str)
            {
                if (textBox1.InvokeRequired)
                {
                    while (!textBox1.IsHandleCreated)
                        if (textBox1.Disposing || textBox1.IsDisposed)
                            return;
                    ShowCallBack back = new ShowCallBack(Show);
                    textBox1.Invoke(back, new object[] { str });
                }
                else
                {
                    textBox1.AppendText("设备消息\r\n");
                    textBox1.AppendText("\t" + str + "\r\n");
                }
            }
        }

        private void Time_Event(object sender, System.Timers.ElapsedEventArgs e)
        {
            string str = Device.Receive();
            if (null != str)
                Show("设备处理完成\r\n\t" + str + "\r\n");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if ("" != textBox2.Text)
            {
                Machine.Boot(textBox2.Text);
                textBox2.Text = "";
            }
            else
                MessageBox.Show("请输入文件路径");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Machine.Shutdown();
        }
    }
}
