using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MotoRs485.UISpace
{
    public partial class MotorControl : UserControl
    {
        
        #region 申明变量
        BaseSpace.ControlMotorClass mMotor;
        BaseSpace.MotorState mState;
        Button btn_UP;
        Button btn_Down;
        Button btn_Home;
        Button btnSetGoPosition;
        Button btnSetComeBack;
        Button btn_Location;
        Button btnGoPosition;
        Button btnGoComeBack;
        Button btnStop;
        
        Label lblEnable;
        Label lblDriverErr;
        Label lblAlert;
        Label lblUPSenser;
        Label lblDOWNSenser;
        Label lblHOMESenser;
        Label lblSuspend;
        Label lblRuning;
        Label lblLocation;
        Label lblNOW;
        Label lblSpeedNow;
        Label lblSpeedObject;
        Label lblMotorModel;
        Label lblMotorVersionHardware;
        Label lblMotorVersionSoftware;
        
        TextBox tbGoPosition;
        TextBox tbComeBack;
        RichTextBox rtbResult;

        NumericUpDown nudMotorAcceleration;
        NumericUpDown nudMotorDeceleration;
        NumericUpDown nudMotorQuickStopDeceleration;
        NumericUpDown nudMotorHomeMechanicsSpeed;
        NumericUpDown nudMotorHomeSpeed;
        NumericUpDown nudMotorHomeAddSubtract;
        NumericUpDown nudMotorZeroOffset;
        NumericUpDown nudSpeedHight;
        NumericUpDown nudSpeedLow;

        GroupBox gbSet;
        GroupBox gbControl;
        CheckBox cbSpeedSet;
        #endregion
        enum SetStation
        {
            Acceleration,
            Deceleration,
            QuickStopDeceleration,
            HomeMechanicsSpeed,
            HomeSpeed,
            HomeAddSubtract,
            ZeroOffset,
            SpeedHight,
            SpeedLow,
        }
        public string MyText
        {
            set
            {
                gbControl.Text = value;
                gbSet.Text = value + "_" + gbSet.Text;
            }
        }
        public string UpText
        {
            set
            {
                btn_UP.Text = value;
            }
        }
        public string DownText
        {
            set
            {
                btn_Down.Text = value;
            }
        }

        public MotorControl()
        {
            InitializeComponent();
        }
        public void Initial(BaseSpace.ControlMotorClass PAR)
        {
            mMotor = PAR;
            Loading();
           
            mState = mMotor.MotorStateNow;
            MyText = PAR.mINI.Name;
            UpText = PAR.mINI.CorotationText;
            DownText = PAR.mINI.ReversalText;
        }
        private void Loading()
        {
            lblEnable = label39;
            lblDriverErr = label10;
            lblAlert = label9;
            lblUPSenser = label8;
            lblDOWNSenser = label3;
            lblHOMESenser = label2;
            gbControl = groupBox2;
            gbSet = groupBox1;
            gbSet.Location = gbControl.Location;
            gbSet.Visible = false;

            lblSpeedNow = label33;
            lblSpeedObject = label35;
            lblLocation = label6;
            lblNOW = label28;
            rtbResult = richTextBox1;
            lblSuspend = label17;
            lblRuning = label19;
            lblMotorModel = label23;
            lblMotorVersionHardware = label24;
            lblMotorVersionSoftware = label26;
            tbComeBack = textBox2;
            tbGoPosition = textBox1;

            nudMotorAcceleration = numericUpDown2;
            nudMotorAcceleration.Tag = SetStation.Acceleration;
            nudMotorDeceleration = numericUpDown3;
            nudMotorDeceleration.Tag = SetStation.Deceleration;
            nudMotorZeroOffset = numericUpDown4;
            nudMotorZeroOffset.Tag = SetStation.ZeroOffset;
            nudMotorHomeAddSubtract = numericUpDown5;
            nudMotorHomeAddSubtract.Tag = SetStation.HomeAddSubtract;
            nudMotorHomeSpeed = numericUpDown6;
            nudMotorHomeSpeed.Tag = SetStation.HomeSpeed;
            nudMotorHomeMechanicsSpeed = numericUpDown7;
            nudMotorHomeMechanicsSpeed.Tag = SetStation.HomeMechanicsSpeed;
            nudMotorQuickStopDeceleration = numericUpDown8;
            nudMotorQuickStopDeceleration.Tag = SetStation.QuickStopDeceleration;
            nudSpeedHight = numericUpDown10;
            nudSpeedHight.Tag = SetStation.SpeedHight;
            nudSpeedLow = numericUpDown9;
            nudSpeedLow.Tag = SetStation.SpeedLow;

            FillData();

            nudMotorAcceleration.ValueChanged += NudMotor_ValueChanged;
            nudMotorAcceleration.Tag = SetStation.Acceleration;
            nudMotorDeceleration.ValueChanged += NudMotor_ValueChanged;
            nudMotorZeroOffset.ValueChanged += NudMotor_ValueChanged;
            nudMotorHomeAddSubtract.ValueChanged += NudMotor_ValueChanged;
            nudMotorHomeSpeed.ValueChanged += NudMotor_ValueChanged;
            nudMotorHomeMechanicsSpeed.ValueChanged += NudMotor_ValueChanged;
            nudMotorQuickStopDeceleration.ValueChanged += NudMotor_ValueChanged;
            nudSpeedHight.ValueChanged += NudMotor_ValueChanged;
            nudSpeedLow.ValueChanged += NudMotor_ValueChanged;

            btn_UP = button15;
            btn_Down = button5;
            btn_Home = button18;
            btnSetGoPosition = button1;
            btnSetComeBack = button2;
            btn_Location = button3;
            btnGoComeBack = button6;
            btnGoPosition = button4;
            btnStop = button7;

            btnStop.Click += BtnStop_Click;
            btnGoPosition.Click += BtnGoPosition_Click;
            btnGoComeBack.Click += BtnGoComeBack_Click;
            btn_UP.MouseDown += new MouseEventHandler(btn_UP_MouseDown);
            btn_UP.MouseUp += new MouseEventHandler(btn_UP_MouseUp);
            btn_Down.MouseDown += new MouseEventHandler(btn_Down_MouseDown);
            btn_Down.MouseUp += new MouseEventHandler(btn_Down_MouseUp);
            btn_Location.Click += new EventHandler(btn_Location_Click);
            btn_Home.Click += new EventHandler(btn_Home_Click);
            btnSetComeBack.Click += BtnSetComeBack_Click;
            btnSetGoPosition.Click += BtnSetGoPosition_Click;
            gbSet.MouseDoubleClick += GbSet_MouseDoubleClick;
            gbControl.MouseDoubleClick += GbControl_MouseDoubleClick;

            cbSpeedSet = checkBox1;
            cbSpeedSet.CheckedChanged += CbSpeedSet_CheckedChanged;
            mMotor.TRIGGERMESSIGE += MyMoto_TRIGGERMESSIGE;
        }
        private void FillData()
        {
            tbComeBack.Text = mMotor.mINI.iComeBack.ToString();
            tbGoPosition.Text = mMotor.mINI.iGoPosition.ToString();
            nudMotorAcceleration.Value = mMotor.mINI.iMotorAcceleration;
            nudMotorDeceleration.Value = mMotor.mINI.iMotorDeceleration;
            nudMotorHomeAddSubtract.Value = mMotor.mINI.iMotorHomeAddSubtract;
            nudMotorHomeMechanicsSpeed.Value = mMotor.mINI.iMotorHomeMechanicsSpeed;
            nudMotorHomeSpeed.Value = mMotor.mINI.iMotorHomeSpeed;
            nudMotorQuickStopDeceleration.Value = mMotor.mINI.iMotorQuickStopDeceleration;
            nudMotorZeroOffset.Value = mMotor.mINI.iMotorZeroOffset;
            nudSpeedHight.Value = mMotor.mINI.iSpeedHight;
            nudSpeedLow.Value = mMotor.mINI.iSpeedLow;
        }
        public void Tick()
        {
            islblText(mState.ISDriverErr, lblDriverErr);
            islblText(mState.ISEnable, lblEnable);
            islblText(mState.ISNotice, lblAlert);
            islblText(mState.ISCCW, lblUPSenser);
            islblText(mState.ISCW, lblDOWNSenser);
            islblText(mState.ISSW, lblHOMESenser);
            islblText(mState.ISSuspend, lblSuspend);
            islblText(mState.ISRuning, lblRuning);


            lblLocation.Text = mMotor.MotorObjectrPosition.ToString();
            lblNOW.Text = mMotor.MotorPositionNow.ToString();

            lblMotorModel.Text = mMotor.MotorModel;
            lblMotorVersionHardware.Text = mMotor.MotorVersionHardware;
            lblMotorVersionSoftware.Text = mMotor.MotorVersionSoftware;
            lblSpeedNow.Text = mMotor.MotorSpeedNow.ToString();
            lblSpeedObject.Text = mMotor.MotorObjectSpeed.ToString();
        }
        private void MyMoto_TRIGGERMESSIGE(object sender, BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs e)
        {
            switch(e.MyType)
            {
                case BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs.MessageType.Error:
                    LogError(e.MyMessage);
                    break;
                case BaseSpace.JMC_MODBUS_MOTOR.MESSIGEEventArgs.MessageType.Warning:
                    LogWarning(e.MyMessage);
                    break;
            }
        }
        /// <summary>
        /// 改变label样式
        /// </summary>
        /// <param name="isno"></param>
        /// <param name="lbl"></param>
        void islblText(bool isno, Label lbl)
        {
            if (isno)
            {
                lbl.Text = "●";
                lbl.ForeColor = Color.Red;
            }
            else
            {
                lbl.Text = "○";
                lbl.ForeColor = Color.Black;
            }
        }

        private void CbSpeedSet_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSpeedSet.Checked)
                mMotor.MotorObjectSpeed = mMotor.mINI.iSpeedHight;
            else
                mMotor.MotorObjectSpeed = mMotor.mINI.iSpeedLow;
        }
        private void NudMotor_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;
            int iValue = (int)nud.Value;
            SetStation station = (SetStation)nud.Tag;
            switch (station)
            {
                case SetStation.Acceleration:
                    mMotor.MotorAcceleration = iValue;
                    mMotor.mINI.iMotorAcceleration = iValue;
                    break;
                case SetStation.Deceleration:
                    mMotor.MotorDeceleration = iValue;
                    mMotor.mINI.iMotorDeceleration = iValue;
                    break;
                case SetStation.HomeAddSubtract:
                    mMotor.MotorHomeAddSubtract = iValue;
                    mMotor.mINI.iMotorHomeAddSubtract = iValue;
                    break;
                case SetStation.HomeMechanicsSpeed:
                    mMotor.MotorHomeMechanicsSpeed = iValue;
                    mMotor.mINI.iMotorHomeMechanicsSpeed = iValue;
                    break;
                case SetStation.HomeSpeed:
                    mMotor.MotorHomeSpeed = iValue;
                    mMotor.mINI.iMotorHomeSpeed = iValue;
                    break;
                case SetStation.QuickStopDeceleration:
                    mMotor.MotorQuickStopDeceleration = iValue;
                    mMotor.mINI.iMotorQuickStopDeceleration = iValue;
                    break;
                case SetStation.ZeroOffset:
                    mMotor.MotorZeroOffset = iValue;
                    mMotor.mINI.iMotorZeroOffset = iValue;
                    break;
                case SetStation.SpeedHight:
                    mMotor.mINI.iSpeedHight = iValue;
                    if (cbSpeedSet.Checked)
                        mMotor.MotorObjectSpeed = mMotor.mINI.iSpeedHight;
                    else
                        mMotor.MotorObjectSpeed = mMotor.mINI.iSpeedLow;
                    break;
                case SetStation.SpeedLow:
                    mMotor.mINI.iSpeedLow = iValue;
                    if (cbSpeedSet.Checked)
                        mMotor.MotorObjectSpeed = mMotor.mINI.iSpeedHight;
                    else
                        mMotor.MotorObjectSpeed = mMotor.mINI.iSpeedLow;
                    break;

            }
            mMotor.mINI.SaveValue();
        }
        private void btn_Down_MouseUp(object sender, MouseEventArgs e)
        {
            btn_Down.BackColor = SystemColors.Control;
            mMotor.Stop();
        
        }
        private void btn_Down_MouseDown(object sender, MouseEventArgs e)
        {
            btn_Down.BackColor = Color.Red;
            mMotor.Reversal();
        }
        private void btn_UP_MouseUp(object sender, MouseEventArgs e)
        {
            btn_UP.BackColor = SystemColors.Control;
            mMotor.Stop();
        }
        private void btn_UP_MouseDown(object sender, MouseEventArgs e)
        {
            btn_UP.BackColor = Color.Red;
            mMotor.Corotation();
        }
        private void btn_Location_Click(object sender, EventArgs e)
        {
            int i_ShuDu = (int)numericUpDown1.Value;
            mMotor.Position(i_ShuDu);
            numericUpDown1.Value = 0;

        }
        private void btn_Home_Click(object sender, EventArgs e)
        {
            mMotor.Home();
        }
        private void BtnSetGoPosition_Click(object sender, EventArgs e)
        {
            mMotor.mINI.iGoPosition = mMotor.MotorPositionNow;
            tbGoPosition.Text = mMotor.mINI.iGoPosition.ToString();
            mMotor.mINI.SaveValue();
        }
        private void BtnSetComeBack_Click(object sender, EventArgs e)
        {
            mMotor.mINI.iComeBack = mMotor.MotorPositionNow;
            tbComeBack.Text = mMotor.mINI.iComeBack.ToString();
            mMotor.mINI.SaveValue();
        }
        private void GbControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                gbControl.Visible = false;
                gbSet.Visible = true;
            }
        }
        private void GbSet_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                gbControl.Visible = true;
                gbSet.Visible = false;
            }
        }
        private void BtnGoComeBack_Click(object sender, EventArgs e)
        {
            mMotor.Position(mMotor.mINI.iComeBack);
        }
        private void BtnGoPosition_Click(object sender, EventArgs e)
        {
            mMotor.Position(mMotor.mINI.iGoPosition);
        }
        private void BtnStop_Click(object sender, EventArgs e)
        {
            mMotor.Stop();
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
            rtbResult.Invoke(la, Color.Red, text);
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
