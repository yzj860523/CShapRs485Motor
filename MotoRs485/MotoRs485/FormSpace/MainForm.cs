using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace MotoRs485
{
    public partial class MainForm : Form
    {
        BaseSpace.ControlMotorClass mMotor;
        BaseSpace.SERIALPORT mCom;
        UISpace.MotorControl mMotorX;

        RichTextBox rtbResult;
        Button btnClear;
        Timer timer = new Timer();
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            rtbResult = richTextBox1;
            btnClear = button1;
            btnClear.Click += BtnClear_Click;
            
            this.CenterToScreen();

            string strADD = System.AppDomain.CurrentDomain.BaseDirectory;
            mCom = new BaseSpace.SERIALPORT(strADD + "COM.INI", false);
            mCom.TRIGGERMESSIGE += MCom_TRIGGERMESSIGE;
            mMotor = new BaseSpace.ControlMotorClass( mCom, strADD + "MOTOR_X.INI");
            mMotor.TRIGGERMESSIGE += Motor_TRIGGERMESSIGE;
            mMotorX = motorControl1;
            mMotorX.Initial(mMotor);

            timer.Interval = 20;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            rtbResult.Text = "";
        }

        private void Motor_TRIGGERMESSIGE(object sender, BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs e)
        {
            switch (e.MyType)
            {
                case BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs.MessageType.Warning:
                    LogWarning(e.MyMessage);
                    break;
                case BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs.MessageType.Message:
                    LogMessage(e.MyMessage);
                    break;
                case BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs.MessageType.Error:
                    LogError(e.MyMessage);
                    break;
            }
        }

        private void MCom_TRIGGERMESSIGE(object sender, BaseSpace.SERIALPORT.MESSIGEEventArgs e)
        {

            switch (e.MyType)
            {
                case BaseSpace.SERIALPORT.MESSIGEEventArgs.MessageType.Warning:
                    LogWarning(e.MyMessage);
                    if (System.Windows.Forms.MessageBox.Show(e.MyMessage + "是否重新连接",
                       "Error", System.Windows.Forms.MessageBoxButtons.OKCancel)
                       == System.Windows.Forms.DialogResult.OK)
                    {
                        mCom.ResetCOM();
                    }
                    else
                        this.Close();
                    break;
                case BaseSpace.SERIALPORT.MESSIGEEventArgs.MessageType.Message:
                    LogMessage(e.MyMessage);
                    break;
                case BaseSpace.SERIALPORT.MESSIGEEventArgs.MessageType.Error:
                    LogError(e.MyMessage);
                    break;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            watch.Start();
            mCom.Tick();
            mMotor.Tick();
            mMotorX.Tick();
            
            watch.Stop();
            this.Text ="串口每秒通信:" +mCom.iCount.ToString()+" 刷新用时:"+ (watch.ElapsedMilliseconds / 1000).ToString("0.000");
            watch.Reset();
        }
        

        #region 日志记录、支持其他线程访问 
        public delegate void LogAppendDelegate(Color color, string text);
        /// <summary> 
        /// 追加显示文本 
        /// </summary> 
        /// <param name="color">文本颜色</param> 
        /// <param name="text">显示文本</param> 
        private void LogAppend(Color color, string text)
        {

            rtbResult.SelectionColor = color;
            rtbResult.AppendText(text);
            rtbResult.AppendText("\n");
            rtbResult.HideSelection = false;
            // rtbMess.Select(rtbMess.Text.Length, 0);
        }
        /// <summary> 
        /// 显示错误日志 
        /// </summary> 
        /// <param name="text"></param> 
        public void LogError(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            rtbResult.Invoke(la, Color.Red,  text);
        }
        /// <summary> 
        /// 显示警告信息 
        /// </summary> 
        /// <param name="text"></param> 
        public void LogWarning(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            rtbResult.Invoke(la, Color.Violet, text);
        }
        /// <summary> 
        /// 显示信息 
        /// </summary> 
        /// <param name="text"></param> 
        public void LogMessage(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            rtbResult.Invoke(la, Color.Black, text);
        }

        #endregion

    }
}
