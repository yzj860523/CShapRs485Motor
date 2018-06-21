using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotoRs485.BaseSpace
{
    public class ControlMotorClass : JMC_MODBUS_MOTOR
    {
        public MOTORCONTROLINI mINI;

        /// <summary>
        /// 初始化马达
        /// </summary>
        /// <param name="COM_INIAddress">Com口INI地址</param>
        /// <param name="Motor_INIAddress">马达参数INI地址</param>
        /// <param name="isDebug">是否Debug</param>
        public ControlMotorClass( string COM_INIAddress, string Motor_INIAddress, bool isDebug)
        {
            base.mSERIALPORT = new SERIALPORT(COM_INIAddress, isDebug);
            LOADING(Motor_INIAddress);
        }
        /// <summary>
        /// 初始化马达
        /// </summary>
        /// <param name="iPro">站号</param>
        /// <param name="motor">Com口 类</param>
        public ControlMotorClass( SERIALPORT motor,string Motor_INIAddress)
        {
            base.mSERIALPORT = motor;
            LOADING(Motor_INIAddress);
        }
        void LOADING(string INIAddress)
        {
            mINI = new MOTORCONTROLINI(INIAddress);
            base.PRO = (byte)mINI.iPRO;
            base.Initial();
            base.MotorAcceleration = mINI.iMotorAcceleration;
            base.MotorDeceleration = mINI.iMotorDeceleration;
            base.MotorHomeAddSubtract = mINI.iMotorHomeAddSubtract;
            base.MotorHomeMechanicsSpeed = mINI.iMotorHomeMechanicsSpeed;
            base.MotorHomeSpeed = mINI.iMotorHomeSpeed;
            base.MotorQuickStopDeceleration = mINI.iMotorQuickStopDeceleration;
            base.MotorZeroOffset = mINI.iMotorZeroOffset;
            base.MotorObjectSpeed = mINI.iSpeedHight;
        }
        /// <summary>
        /// 马达模式切换(重写)
        /// </summary>
        public override MotorOperationModeEnum MotorOperationMODE
        {
            get
            {
                return base.MotorOperationMODE;
            }
            set
            {
                if (base.MotorOperationMODE == value)
                    return;
                switch (value)
                {
                    case MotorOperationModeEnum.PositionMODE:
                        int iPosition = base.MotorObjectrPosition;
                        int iSpeed = base.MotorObjectSpeed;
                        base.SETMOTORINITIAL();
                        base.MotorObjectrPosition = iPosition;
                        base.MotorObjectSpeed = iSpeed;
                        base.MotorOperationMODE = MotorOperationModeEnum.PositionMODE;
                        break;
                    case MotorOperationModeEnum.SpeedMODE:
                        Stop();
                        base.MotorOperationMODE = MotorOperationModeEnum.SpeedMODE;
                        break;
                    case MotorOperationModeEnum.HOMEMODE:
                        base.MotorOperationMODE = MotorOperationModeEnum.HOMEMODE;
                        base.MotorHomeMode = MotorHomeModeCodeEnum.Mode8;
                        base.MotorHomeSpeed = 100;
                        base.MotorHomeAddSubtract = 100;
                        base.MotorHomeMechanicsSpeed = 10;
                        base.MotorZeroOffset = 0;
                        break;
                }
            }
        }
        /// <summary>
        /// 回HOME
        /// </summary>
        public void Home()
        {
            MotorOperationMODE = MotorOperationModeEnum.HOMEMODE;
            base.Run();
        }
        /// <summary>
        /// 到定位
        /// </summary>
        /// <param name="iPosition"></param>
        public void Position(int iPosition)
        {
            MotorOperationMODE = MotorOperationModeEnum.PositionMODE;
            base.MotorObjectrPosition = iPosition;
            base.Run();
        }
        /// <summary>
        /// 正转
        /// </summary>
        public void Corotation()
        {
            if (base.MotorObjectSpeed < 0)
                base.MotorObjectSpeed = -base.MotorObjectSpeed;
            MotorOperationMODE = MotorOperationModeEnum.SpeedMODE;
            base.Run();
        }
        /// <summary>
        /// 反转
        /// </summary>
        public void Reversal()
        {
            if (base.MotorObjectSpeed > 0)
                base.MotorObjectSpeed = -base.MotorObjectSpeed;
            MotorOperationMODE = MotorOperationModeEnum.SpeedMODE;
            base.Run();
        }
        /// <summary>
        /// 到定位点
        /// </summary>
        public void GoPosition()
        {
            MotorOperationMODE = MotorOperationModeEnum.PositionMODE;
            base.MotorObjectrPosition = mINI.iGoPosition;
            base.Run();
        }
        /// <summary>
        /// 到待命位置
        /// </summary>
        public void ComeBack()
        {
            MotorOperationMODE = MotorOperationModeEnum.PositionMODE;
            base.MotorObjectrPosition = mINI.iComeBack;
            base.Run();
        }

    }
    [Serializable]
    public class MOTORCONTROLINI
    {
        #region 引入dll
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //参数说明:(ini文件中的段落,INI中的关键字,INI中关键字的数值,INI文件的完整路径)
        static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //参数说明:(INI文件中的段落,INI中的关键字,INI无法读取时的缺省值,读取数值,数值大小,INI文件的完整路径)
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        /// <summary>
        ///  INI地址
        /// </summary>
        string st_AD = "";
        /// <summary>
        /// 站号
        /// </summary>
        public int iPRO = 1;
        /// <summary>
        /// 标题名称
        /// </summary>
        public string Name = "X轴";
        /// <summary>
        /// 正转按扭标题
        /// </summary>
        public string CorotationText = "→";
        /// <summary>
        /// 反转按扭标题
        /// </summary>
        public string ReversalText = "←";
        /// <summary>
        /// 跑线位置
        /// </summary>
        public int iGoPosition = 0;
        /// <summary>
        /// 待命位置
        /// </summary>
        public int iComeBack = 0;
        /// <summary>
        /// 加速度
        /// </summary>
        public int iMotorAcceleration = 0;
        /// <summary>
        /// 减速度
        /// </summary>
        public int iMotorDeceleration = 0;
        /// <summary>
        /// 快停减速度
        /// </summary>
        public int iMotorQuickStopDeceleration = 0;
        /// <summary>
        /// 机械回零速度
        /// </summary>
        public int iMotorHomeMechanicsSpeed = 0;
        /// <summary>
        /// 回零速度 
        /// </summary>
        public int iMotorHomeSpeed = 0;
        /// <summary>
        /// 回零加减速
        /// </summary>
        public int iMotorHomeAddSubtract = 0;
        /// <summary>
        /// 零点偏移
        /// </summary>
        public int iMotorZeroOffset = 0;
        /// <summary>
        /// 高速
        /// </summary>
        public int iSpeedHight = 0;
        /// <summary>
        /// 低速
        /// </summary>
        public int iSpeedLow = 0;
        /// <summary>
        /// 初始化INI
        /// </summary>
        /// <param name="INIAddress"></param>
        public MOTORCONTROLINI(string INIAddress)
        {
            st_AD = INIAddress;
            INIRead();
        }
        /// <summary>
        /// INI读取
        /// </summary>
        void INIRead()
        {
            iPRO = int.Parse(INIReadIO("Recording", "PRO", "1"));
            Name = INIReadIO("Recording", "Name", "X");
            CorotationText = INIReadIO("Recording", "CorotationText", ">");
            ReversalText = INIReadIO("Recording", "ReversalText", "<");

            iGoPosition = int.Parse(INIReadIO("Recording", "GoPosition", "-1"));
            iComeBack = int.Parse(INIReadIO("Recording", "ComeBack", "-1"));
            iMotorAcceleration = int.Parse(INIReadIO("Recording", "MotorAcceleration", "-1"));
            iMotorDeceleration = int.Parse(INIReadIO("Recording", "MotorDeceleration", "-1"));
            iMotorQuickStopDeceleration = int.Parse(INIReadIO("Recording", "MotorQuickStopDeceleration", "-1"));
            iMotorHomeMechanicsSpeed = int.Parse(INIReadIO("Recording", "MotorHomeMechanicsSpeed", "-1"));
            iMotorHomeSpeed = int.Parse(INIReadIO("Recording", "MotorHomeSpeed", "-1"));
            iMotorHomeAddSubtract = int.Parse(INIReadIO("Recording", "MotorHomeAddSubtract", "-1"));
            iMotorZeroOffset = int.Parse(INIReadIO("Recording", "MotorZeroOffset", "-1"));
            iSpeedHight = int.Parse(INIReadIO("Recording", "SpeedHight", "-1"));
            iSpeedLow = int.Parse(INIReadIO("Recording", "SpeedLow", "-1"));
        }
        /// <summary>
        /// 从INI里读出内容
        /// </summary>
        /// <param name="st_Class">INI文件中的段落</param>
        /// <param name="st_">INI中的关键字</param>
        /// <param name="st_ad">INI无法读取时的缺省值</param>
        string INIReadIO(string st_Class, string st_, string st_ad)
        {
            string st_retStr = "";
            StringBuilder stb_ini = new StringBuilder(255);

            GetPrivateProfileString(st_Class, st_, st_ad, stb_ini, 255, st_AD);

            st_retStr = stb_ini.ToString();
            if (st_retStr == "")
            {
                st_retStr = st_ad;
            }

            return st_retStr;

        }
        /// <summary>
        /// 保存INI参数
        /// </summary>
        public void SaveValue()
        {
            try
            {
                if(!System.IO.File.Exists(st_AD))
                {
                    System.IO.File.Create(st_AD);
                }
               System.IO. File.SetAttributes(st_AD,System.IO. FileAttributes.Normal);
                WritePrivateProfileString("Recording", "PRO", iPRO.ToString(), st_AD);
                WritePrivateProfileString("Recording", "Name", Name, st_AD);
                WritePrivateProfileString("Recording", "CorotationText", CorotationText.ToString(), st_AD);
                WritePrivateProfileString("Recording", "ReversalText", ReversalText, st_AD);
                WritePrivateProfileString("Recording", "GoPosition", iGoPosition.ToString(), st_AD);
                WritePrivateProfileString("Recording", "ComeBack", iComeBack.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorAcceleration", iMotorAcceleration.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorDeceleration", iMotorDeceleration.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorQuickStopDeceleration", iMotorQuickStopDeceleration.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorHomeMechanicsSpeed", iMotorHomeMechanicsSpeed.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorHomeSpeed", iMotorHomeSpeed.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorHomeAddSubtract", iMotorHomeAddSubtract.ToString(), st_AD);
                WritePrivateProfileString("Recording", "MotorZeroOffset", iMotorZeroOffset.ToString(), st_AD);
                WritePrivateProfileString("Recording", "SpeedHight", iSpeedHight.ToString(), st_AD);
                WritePrivateProfileString("Recording", "SpeedLow", iSpeedLow.ToString(), st_AD);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
