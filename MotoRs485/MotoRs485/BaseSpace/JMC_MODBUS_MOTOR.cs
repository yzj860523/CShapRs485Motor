using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotoRs485.BaseSpace
{
    public class JMC_MODBUS_MOTOR
    {
        /// <summary>
        /// COM口 Class
        /// </summary>
        public SERIALPORT mSERIALPORT;
        
        int iObjectrPosition = -1;//目标位置
        int iObjectSpeed = -1;//目标速度
        int iAcceleration = -1;//加速度
        int iDeceleration = -1;//减速度
        int iQuickStopDeceleration = -1;//快停减速度
        int iHomeAddSubtract = -1;//回零加减速
        int iHomeMechanicsSpeed = -1;//回机械原点速度
        int iHomeSpeed = -1;// 回零速度
        int iZeroOffset = -1;//零位偏移
        MotorOperationModeEnum mMODE = MotorOperationModeEnum.PositionMODE;
        MotorStopCodeEnum mSTOP = MotorStopCodeEnum.Stop1;
        MotorHomeModeCodeEnum mHomeMode = MotorHomeModeCodeEnum.Mode7;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// 马达 站号
        /// </summary>
        public byte PRO = 1;

        /// <summary>
        /// 功能码
        /// </summary>
        enum FunctionCodeEnum : int
        {
            /// <summary>
            /// 单个读取
            /// </summary>
          //  SingleRead = 01,
            /// <summary>
            ///多个读取
            /// </summary>
            MultipleReads = 03,
            /// <summary>
            ///  单个写入
            /// </summary>
            SingleWrite = 06,
            /// <summary>
            /// 多个写入
            /// </summary>
            MultipleWrites = 10,

        }
        /// <summary>
        /// 寄存器地址
        /// </summary>
        enum RegisterEnum : int
        {
            /// <summary>
            /// 马达错误信息
            /// </summary>
            MotorErrorCode = 0x1001,
            /// <summary>
            /// 马达型号
            /// </summary>
            MotorMode = 0x1008,
            /// <summary>
            /// 硬件版本
            /// </summary>
            MotorVersionHardware = 0x1009,
            /// <summary>
            /// 软件版本
            /// </summary>
            MotorVersionSoftware = 0x100A,
            /// <summary>
            /// 电机状态
            /// </summary>
            MotorState = 0x6041,
            /// <summary>
            /// 电机控制字
            /// </summary>
            MotorControl = 0x6040,
            /// <summary>
            /// 停止
            /// </summary>
            MotorStop = 0x605A,
            /// <summary>
            /// 操作模式
            /// </summary>
            MotorOperationMode = 0x6060,
            /// <summary>
            /// 马达现在的位置地址
            /// </summary>
            MotorPositionNow = 0x6064,
            /// <summary>
            /// 目标位置
            /// </summary>
            MotorObjectPosition = 0x607A,
            /// <summary>
            /// 现在的速度
            /// </summary>
            MotorSpeedNow = 0x606C,
            /// <summary>
            /// 目标速度
            /// </summary>
            MotorObjectSpeed = 0x6081,
            /// <summary>
            /// 加速度
            /// </summary>
            MotorAcceleration = 0x6083,
            /// <summary>
            /// 减速度
            /// </summary>
            MotorDeceleration = 0x6084,
            /// <summary>
            /// 快停减速度
            /// </summary>
            MotorQuickStopDeceleration = 0x6085,
            /// <summary>
            /// 回零模式
            /// </summary>
            MotorHomeMode = 0x6098,
            /// <summary>
            /// 回机械原点速度
            /// </summary>
            MotorHomeMechanicsSpeed = 0x6099,
            /// <summary>
            /// 回零速度
            /// </summary>
            MotorHomeSpeed = 0x609B,
            /// <summary>
            /// 回零加减速
            /// </summary>
            MotorHomeAddSubtract = 0x609A,
            /// <summary>
            /// 零点偏移
            /// </summary>
            MotorZeroOffset = 0x607C,

        }
        /// <summary>
        /// 马达运动模式
        /// </summary>
        public enum MotorOperationModeEnum
        {
            /// <summary>
            /// 位置模式
            /// </summary>
            PositionMODE = 1,
            /// <summary>
            /// 速度模式
            /// </summary>
            SpeedMODE = 3,
            /// <summary>
            /// 转矩模式（伺服）暂不支持
            /// </summary>
            TorqueMODE = 4,
            /// <summary>
            /// 回零模式
            /// </summary>
            HOMEMODE = 6,

        }
        /// <summary>
        /// 马达停止代码
        /// </summary>
        public enum MotorStopCodeEnum
        {
            /// <summary>
            /// 以当前减速度停止
            /// </summary>
            Stop1 = 1,
            /// <summary>
            /// 以快速停止速度停止
            /// </summary>
            Stop2 = 2,
            /// <summary>
            /// 直接立即停止
            /// </summary>
            Stop3 = 3,
        }
        /// <summary>
        /// 回零模式
        /// </summary>
        public enum MotorHomeModeCodeEnum
        {
            /// <summary>
            /// 向左运动，回到CW限位时停止运动
            /// （回零最高速度为300r/min）
            /// </summary>
            Mode1 = 1,
            /// <summary>
            /// 向右运动，回到CCW限位时停止运动。
            /// （回零最高速度为300r/min）
            /// </summary>
            Mode2 = 2,
            /// <summary>
            /// 向左运动，若到达CW限位，
            /// 则向右运行（未触碰左限位则继续向左运行），
            /// 到达机械原点限位时停止运动。
            /// （回零最高速度为300r/min）
            /// </summary>
            Mode3 = 3,
            /// <summary>
            /// 向右运动，若到达CCW限位，
            /// 则向左运行（未触碰右限位则继续向右运行），
            /// 到达机械原点限位时停止运动。
            /// （回零最高速度为300r/min）
            /// </summary>
            Mode4 = 4,
            /// <summary>
            /// 高速向左遇到限位开关后快速降低，
            /// 然后向右低速移动，
            /// 过限位开关后低速向左回零。
            /// （回零最高速度为1200r/min）
            /// </summary>
            Mode5 = 5,
            /// <summary>
            /// 高速向右遇到限位开关后快速降低，
            /// 然后向左低速移动，
            /// 过限位开关后低速向右回零。
            /// （回零最高速度为1200r/min）
            /// </summary>
            Mode6 = 6,
            /// <summary>
            /// 高速向左运行，如遇CW限位开关，
            /// 则转换速度方向向右高速运行
            /// （若未遇到机械原点则继续向左运行），
            /// 遇到机械原点限位开关后快速降低，
            /// 然后向右低速移动，过限位开关后低速向左回零。
            /// （回零最高速度为1200r/min）
            /// </summary>
            Mode7 = 7,
            /// <summary>
            /// 高速向右运行，如遇CW限位开关，
            /// 则转换速度方向向右高速运行
            /// （若未遇到机械原点则继续向左运行），
            /// 遇到机械原点限位开关后快速降低，
            /// 然后向右低速移动，过限位开关后低速向左回零。
            /// （回零最高速度为1200r/min）
            /// </summary>
            Mode8 = 8,
        }

        /// <summary>
        /// 厂商及型号
        /// </summary>
        public string MotorModel { get; set; }
        /// <summary>
        /// 硬件版本
        /// </summary>
        public string MotorVersionHardware { get; set; }
        /// <summary>
        /// 软件版本
        /// </summary>
        public string MotorVersionSoftware { get; set; }
        /// <summary>
        /// 马达错误信息
        /// </summary>
        string MotorError { get; set; }
        /// <summary>
        /// 目标速度
        /// </summary>
        public int MotorObjectSpeed
        {
            get
            {

                //GetObjectSpeed();
                //isSend = true;
                //while (true)
                //{
                //    System.Windows.Forms.Application.DoEvents();
                //    if (isSend)
                //        break;
                //}
                return iObjectSpeed;

            }
            set
            {
                SetObjectSpeed(value);
                iObjectSpeed = value;
            }

        }
        /// <summary>
        /// 目标位置
        /// </summary>
        public int MotorObjectrPosition
        {
            get
            {
                //GetObjectrPosition();
                //isSend = true;
                //while (true)
                //{
                //    System.Windows.Forms.Application.DoEvents();
                //    if (isSend)
                //        break;
                //}
                return iObjectrPosition;

            }
            set
            {
                SetObjectPosition(value);
                iObjectrPosition = value;
            }

        }
        /// <summary>
        /// 现在的速度
        /// </summary>
        public int MotorSpeedNow { get; set; } = -1;
        /// <summary>
        /// 马达现在的位置
        /// </summary>
        public int MotorPositionNow { get; set; } = -1;
        /// <summary>
        /// 电机状态
        /// </summary>
        public MotorState MotorStateNow { get; set; } = new MotorState();
        /// <summary>
        /// 马达加速度
        /// </summary>
        public int MotorAcceleration
        {
            get
            {
                return iAcceleration;
            }

            set
            {
                SetAcceleration(value);
                iAcceleration = value;
            }
        }
        /// <summary>
        /// 马达减速度
        /// </summary>
        public int MotorDeceleration
        {
            get
            {
                return iDeceleration;
            }
            set
            {
                SetDeceleration(value);
                iDeceleration = value;
            }
        }
        /// <summary>
        /// 马达快停减速度
        /// </summary>
        public int MotorQuickStopDeceleration
        {
            get
            {
                return iQuickStopDeceleration;
            }
            set
            {
                SetQuickStopDeceleration(value);
                iQuickStopDeceleration = value;
            }
        }
        /// <summary>
        /// 回机械原点速度
        /// </summary>
        public int MotorHomeMechanicsSpeed
        {
            get
            {
                return iHomeMechanicsSpeed;
            }
            set
            {
                SetHomeMechanicsSpeed(value);
                iHomeMechanicsSpeed = value;
            }
        }
        /// <summary>
        /// 回零速度
        /// </summary>
        public int MotorHomeSpeed
        {
            get
            {
                return iHomeSpeed;
            }
            set
            {
                SetHomeSpeed(value);
                iHomeSpeed = value;
            }
        }
        /// <summary>
        /// 回零加减速
        /// </summary>
        public int MotorHomeAddSubtract
        {
            get
            {
                return iHomeAddSubtract;
            }
            set
            {
                SetHomeAddSubtract(value);
                iHomeAddSubtract = value;
            }
        }
        /// <summary>
        /// 零点偏移
        /// </summary>
        public int MotorZeroOffset
        {
            get
            {
                return iZeroOffset;
            }
            set
            {
                SetZeloOffset(value);
                iZeroOffset = value;
            }
        }
        /// <summary>
        /// 运动模式
        /// </summary>
        public virtual MotorOperationModeEnum MotorOperationMODE
        {
            get
            {
                return mMODE;
            }
            set
            {
                SetMotorOperationMode(value);
                mMODE = value;
            }
        }
        /// <summary>
        /// 马达停止模式
        /// </summary>
        public MotorStopCodeEnum MotorSTOP
        {
            get
            {
                return mSTOP;
            }
            set
            {
                SetMOTORSTOP(value);
                mSTOP = value;
            }
        }

        public MotorHomeModeCodeEnum MotorHomeMode
        {
            get
            {
                return mHomeMode;
            }
            set
            {
                SetHomeMode(value);
                mHomeMode = value;
            }
        }
        /// <summary>
        /// 发送的数据列表
        /// </summary>
        ModbusData SENDDATA
        {
            set
            {
                mSERIALPORT.ADDInstructions = value.GetSenddata;
            }
        }
        /// <summary>
        /// 优先发送的数据列表
        /// </summary>
        ModbusData SENDDATAFIRST
        {
            set
            {
                mSERIALPORT.ADDInstructionsFirst = value.GetSenddata;
            }
        }
        /// <summary>
        /// 数据列表(数据会固定,一直发送)
        /// </summary>
        ModbusData SENDDATASixed
        {
            set
            {
                mSERIALPORT.ADDInstructionsSixed = value.GetSenddata;
            }
        }

        public JMC_MODBUS_MOTOR()
        { }
        /// <summary>
        /// 初始化马达
        /// </summary>
        /// <param name="iPro">站号</param>
        /// <param name="INIAddress">Com口INI地址</param>
        /// <param name="isDebug">是否Debug</param>
        public JMC_MODBUS_MOTOR(byte iPro, string INIAddress, bool isDebug)
        {
            PRO = iPro;
            mSERIALPORT = new SERIALPORT(INIAddress, isDebug);
            Initial();
        }
        /// <summary>
        /// 初始化马达
        /// </summary>
        /// <param name="iPro">站号</param>
        /// <param name="motor">Com口 类</param>
        public JMC_MODBUS_MOTOR(byte iPro, SERIALPORT motor)
        {
            PRO = iPro;
            mSERIALPORT = motor;
            Initial();
        }
        public bool Initial()
        {
           
            MotorInitial();
            mSERIALPORT.TRIGGERMESSIGE += MSERIALPORT_TRIGGERMESSIGE;

            watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            System.Diagnostics.Stopwatch watchTemp = new System.Diagnostics.Stopwatch();
            watchTemp.Start();
            while (true)
            {
                if (!mSERIALPORT.isCOMRun)
                    break;
                if (watchTemp.ElapsedMilliseconds > 20)
                {
                    mSERIALPORT.Tick();
                    watchTemp.Stop();
                    watchTemp.Reset();
                    watchTemp.Start();
                }
                if (watch.ElapsedMilliseconds > 5000)
                    return false;

                System.Windows.Forms.Application.DoEvents();
                if (MotorQuickStopDeceleration != -1)
                    break;

            }
            return true;
           
        }
        public void Tick()
        {
            mSERIALPORT.Tick();

            if (watch.ElapsedMilliseconds > 2000)
            {
                watch.Stop();
                watch.Reset();
                watch.Start();
                MotorUpdata();
            }

        }
        private void MSERIALPORT_TRIGGERMESSIGE(object sender, SERIALPORT.MESSIGEEventArgs e)
        { 
            switch (e.MyType)
            {
                case SERIALPORT.MESSIGEEventArgs.MessageType.Warning:
                    break;
                case SERIALPORT.MESSIGEEventArgs.MessageType.Error:
                    break;
                case SERIALPORT.MESSIGEEventArgs.MessageType.Message:
                    break;
                case SERIALPORT.MESSIGEEventArgs.MessageType.Data:
                    if (e.MyData[0] == PRO)
                    {
                        //发出去的寄存器地址
                        string strTemp = "";
                        strTemp = e.SendData[2].ToString("X2") + e.SendData[3].ToString("X2");
                        int iRegister = Convert.ToInt32(strTemp, 16);
                        RegisterEnum temp = (RegisterEnum)iRegister;
                        //发出去的功能码
                        string strFunction = e.SendData[1].ToString("X2");
                        FunctionCodeEnum Function = (FunctionCodeEnum)Convert.ToInt32(strFunction);

                        int inumber = 0;
                        byte[] byteArray = null;
                        if (Function == FunctionCodeEnum.MultipleReads)
                        {
                            try
                            {
                                //抓取数据回来
                                inumber = e.MyData[2];
                                byteArray = new byte[inumber];
                                for (int i = 0; i < inumber; i++)
                                    byteArray[i] = e.MyData[i + 3];
                            }
                            catch(Exception ex)
                            {
                                MESSIGEEventArgs ee = new MESSIGEEventArgs();
                                ee.MyType = MESSIGEEventArgs.MessageType.Error;
                                ee.MyMessage = ex.Message;
                                OnTRIGGERMESSIGE(ee);
                            }

                        }
                        else
                            return;

                        switch (temp)
                        {
                            case RegisterEnum.MotorMode:
                                MotorModel = System.Text.Encoding.Default.GetString(byteArray).Replace("\0", "");
                                break;
                            case RegisterEnum.MotorVersionHardware:
                                MotorVersionHardware = System.Text.Encoding.Default.GetString(byteArray).Replace("\0", "");
                                break;
                            case RegisterEnum.MotorVersionSoftware:
                                MotorVersionSoftware = System.Text.Encoding.Default.GetString(byteArray).Replace("\0", "");
                                break;
                            case RegisterEnum.MotorErrorCode:
                                string error = "";
                                foreach (byte by in byteArray)
                                    error += Convert.ToString(by, 2).PadLeft(8, '0');
                                error = ToMotorError(error);
                                if (error != "")
                                {
                                    MESSIGEEventArgs ee = new MESSIGEEventArgs();
                                    ee.MyType = MESSIGEEventArgs.MessageType.Error;
                                    ee.MyMessage = error;
                                    OnTRIGGERMESSIGE(ee);
                                }
                                break;
                            case RegisterEnum.MotorPositionNow:
                                strTemp = "";
                                if (byteArray.Length == 4)
                                {
                                    for (int i = 0; i < byteArray.Length; i++)
                                        strTemp += byteArray[i].ToString("X2");
                                }
                                MotorPositionNow = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorSpeedNow:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");

                                MotorSpeedNow = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorObjectPosition:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");

                                iObjectrPosition = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorObjectSpeed:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");
                                iObjectSpeed = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorAcceleration:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");

                                iAcceleration = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorDeceleration:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");

                                iDeceleration = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorQuickStopDeceleration:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");

                                iQuickStopDeceleration = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorHomeAddSubtract:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");
                                iHomeAddSubtract = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorHomeMechanicsSpeed:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");
                                iHomeMechanicsSpeed = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorHomeSpeed:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");
                                iHomeSpeed = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorZeroOffset:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");
                                iZeroOffset = Convert.ToInt32(strTemp, 16);
                                break;
                            case RegisterEnum.MotorState:
                                strTemp = "";
                                for (int i = 0; i < byteArray.Length; i++)
                                    strTemp += byteArray[i].ToString("X2");
                                strTemp = Convert.ToString(Convert.ToInt32(strTemp, 16), 2).PadLeft(16, '0');
                                ToMotorState(strTemp);
                                break;
                        }
                    }
                    break;

            }
        }
        /// <summary>
        /// 读取马达初始化信息
        /// </summary>
        void MotorInitial()
        {
            GetModel();
            GetVersionHardware();
            GetVersionSoftware();
            GetObjectSpeed();
            GetObjectrPosition();
            GetAcceleration();
            GetDeceleration();
            GetQuickStopDeceleration();
            GetHomeAddSubtract();
            GetHomeMechanicsSpeed();
            GetHomeSpeed();
            GetZeloOffset();
           
            GetPositionNow();
            GetStateNow();

            SETMOTORINITIAL();
            MotorOperationMODE = MotorOperationModeEnum.PositionMODE;
            MotorSTOP = MotorStopCodeEnum.Stop1;
            MotorOperationMODE = MotorOperationModeEnum.SpeedMODE;
            mHomeMode = MotorHomeModeCodeEnum.Mode7;
            MotorOperationMODE = MotorOperationModeEnum.PositionMODE;


        }
        /// <summary>
        /// 更新马参数
        /// </summary>
        void MotorUpdata()
        {
            GetSpeedNow();
            GetErrorCode();
        }
        /// <summary>
        /// 二进制转成MotorState
        /// </summary>
        /// <param name="strData"></param>
        void ToMotorState(string strData)
        {
            char[] chars = strData.ToCharArray();
            Array.Reverse(chars);
            // string newstr = new string(chars);

            MotorState state = MotorStateNow;
            state.ISReadyInitial = chars[0] == '1' ? true : false;
            state.ISInitialOK = chars[1] == '1' ? true : false;
            state.ISEnable = chars[2] == '1' ? true : false;
            state.ISDriverErr = chars[3] == '1' ? true : false;
            state.ISDriverRuning = chars[4] == '1' ? true : false;
            state.ISQuickStop = chars[5] == '1' ? true : false;
            state.ISInitialState = chars[6] == '1' ? true : false;
            state.ISNotice = chars[7] == '1' ? true : false;
            state.ISSuspend = chars[8] == '1' ? true : false;
            state.ISRuning = chars[9] == '1' ? true : false;
            state.ISInPlace = chars[10] == '1' ? true : false;
            state.ISSW = chars[11] == '1' ? true : false;
            state.ISSET = chars[12] == '1' ? true : false;
            state.ISOverproof = chars[13] == '1' ? true : false;
            state.ISCW = chars[15] == '1' ? true : false;
            state.ISCCW = chars[14] == '1' ? true : false;

            MotorStateNow = state;
        }
        /// <summary>
        /// 二进制转成马达错误信息
        /// </summary>
        /// <param name="strData"></param>
        string ToMotorError(string strData)
        {
            char[] chars = strData.ToCharArray();
            Array.Reverse(chars);
            // string newstr = new string(chars);

            string strerr = "";
            strerr = chars[0] == '1' ? "常规错误" : "";
            strerr = chars[1] == '1' ? "电流错误" : "";
            strerr = chars[2] == '1' ? "电压错误" : "";
            strerr = chars[3] == '1' ? "温度报警" : "";
            strerr = chars[4] == '1' ? "通信错误" : "";
            strerr = chars[5] == '1' ? "位置超差" : "";
            strerr = chars[6] == '1' ? "" : "";
            strerr = chars[7] == '1' ? "电机缺相" : "";

            MotorError = strerr;
            return strerr;
        }

        /// <summary>
        /// 设备厂商及型号
        /// </summary>
        void GetModel()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorMode;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATA = sendtemp;

        }
        /// <summary>
        /// 硬件版本
        /// </summary>
        void GetVersionHardware()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorVersionHardware;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATA = sendtemp;

        }
        /// <summary>
        /// 软件版本
        /// </summary>
        void GetVersionSoftware()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorVersionSoftware;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATA = sendtemp;

        }
        /// <summary>
        /// 马达停止
        /// </summary>
        /// <param name="stopcode">停止代码</param>
        void SetMOTORSTOP(MotorStopCodeEnum stopcode)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorStop;
            sendtemp.mySendDataNumber = 0;
            sendtemp.myDataList.Add((int)stopcode);

            SENDDATAFIRST = sendtemp;
        }
        /// <summary>
        /// 运动模式
        /// </summary>
        /// <param name="Mode">位置模式;转距模式;速度模式;回零模式;可选</param>
        void SetMotorOperationMode(MotorOperationModeEnum Mode)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorOperationMode;
            sendtemp.mySendDataNumber = 0x0000;
            sendtemp.myDataList.Add((int)Mode);

            SENDDATA = sendtemp;

        }
        /// <summary>
        /// 错误码
        /// </summary>
        void GetErrorCode()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorErrorCode;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取现在的位置
        /// </summary>
        void GetPositionNow()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorPositionNow;
            sendtemp.mySendDataNumber = 0x0002;

            SENDDATASixed = sendtemp;
        }
        /// <summary>
        /// 读取现在电机的状态
        /// </summary>
        void GetStateNow()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorState;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATASixed = sendtemp;
        }
        /// <summary>
        /// 读取目标位置
        /// </summary>
        void GetObjectrPosition()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorObjectPosition;
            sendtemp.mySendDataNumber = 0x0002;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定目标位置
        /// </summary>
        void SetObjectPosition(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleWrites;
            sendtemp.myRegister = RegisterEnum.MotorObjectPosition;
            sendtemp.mySendDataNumber = 0x0002;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
            GetObjectrPosition();
        }
        /// <summary>
        /// 读取现在的速度
        /// </summary>
        void GetSpeedNow()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorSpeedNow;
            sendtemp.mySendDataNumber = 0x0002;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取目标速度
        /// </summary>
        void GetObjectSpeed()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorObjectSpeed;
            sendtemp.mySendDataNumber = 0x0002;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定目标速度
        /// </summary>
        /// <param name="iValue"></param>
        void SetObjectSpeed(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleWrites;
            sendtemp.myRegister = RegisterEnum.MotorObjectSpeed;
            sendtemp.mySendDataNumber = 0x0002;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;

        }
        /// <summary>
        /// 读取加速度
        /// </summary>
        void GetAcceleration()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorAcceleration;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取减速度
        /// </summary>
        void GetDeceleration()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorDeceleration;
            sendtemp.mySendDataNumber = 0x0001;
            //  sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取快停减速度
        /// </summary>
        void GetQuickStopDeceleration()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorQuickStopDeceleration;
            sendtemp.mySendDataNumber = 0x0001;
            //sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取回机械原点的速度
        /// </summary>
        void GetHomeMechanicsSpeed()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorHomeMechanicsSpeed;
            sendtemp.mySendDataNumber = 0x0002;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取回零速度
        /// </summary>
        void GetHomeSpeed()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorHomeSpeed;
            sendtemp.mySendDataNumber = 0x0002;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 读取回零加减速
        /// </summary>
        void GetHomeAddSubtract()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorHomeAddSubtract;
            sendtemp.mySendDataNumber = 0x0001;

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定加速度
        /// </summary>
        /// <param name="iValue"></param>
        void SetAcceleration(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorAcceleration;
            sendtemp.mySendDataNumber = 0x0000;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定减速度
        /// </summary>
        /// <param name="iValue"></param>
        void SetDeceleration(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorDeceleration;
            sendtemp.mySendDataNumber = 0x0000;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定快停减速度
        /// </summary>
        /// <param name="iValue"></param>
        void SetQuickStopDeceleration(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorQuickStopDeceleration;
            sendtemp.mySendDataNumber = 0x0000;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 回机械原点的速度
        /// </summary>
        /// <param name="iValue"></param>
        void SetHomeMechanicsSpeed(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleWrites;
            sendtemp.myRegister = RegisterEnum.MotorHomeMechanicsSpeed;
            sendtemp.mySendDataNumber = 0x0002;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 回零速度
        /// </summary>
        /// <param name="iValue"></param>
        void SetHomeSpeed(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleWrites;
            sendtemp.myRegister = RegisterEnum.MotorHomeSpeed;
            sendtemp.mySendDataNumber = 0x0002;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定回零加减速
        /// </summary>
        /// <param name="iValue"></param>
        void SetHomeAddSubtract(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorHomeAddSubtract;
            sendtemp.mySendDataNumber = 0x0000;
            //sendtemp.myDataList.Add(0);
            sendtemp.myDataList.Add(iValue);

            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定回零模式
        /// </summary>
        /// <param name="Mode">模式代码</param>
        void SetHomeMode(MotorHomeModeCodeEnum Mode)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtemp.myRegister = RegisterEnum.MotorHomeMode;
            sendtemp.mySendDataNumber = 0x0000;
            sendtemp.myDataList.Add((int)Mode);

            SENDDATAFIRST = sendtemp;
        }
        /// <summary>
        /// 设定零点偏移
        /// </summary>
        void GetZeloOffset()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleReads;
            sendtemp.myRegister = RegisterEnum.MotorZeroOffset;
            sendtemp.mySendDataNumber = 0x0002;
            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 设定零点偏移
        /// </summary>
        /// <param name="iValue"></param>
        void SetZeloOffset(int iValue)
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleWrites;
            sendtemp.myRegister = RegisterEnum.MotorZeroOffset;
            sendtemp.mySendDataNumber = 0x0002;
            sendtemp.myDataList.Add(iValue);
            SENDDATA = sendtemp;
        }

        /// <summary>
        /// 设定电机初始化
        /// </summary>
        /// <param name="iPosition"></param>
        public void SETMOTORINITIAL()
        {
            ModbusData sendtemp = new ModbusData();
            sendtemp.myPRO = PRO;
            sendtemp.myFunctionCode = FunctionCodeEnum.MultipleWrites;
            sendtemp.myRegister = RegisterEnum.MotorControl;
            sendtemp.mySendDataNumber = 0x0000;
            sendtemp.myDataList.Add(0x0001);
            SENDDATA = sendtemp;

            sendtemp.myDataList.Clear();
            sendtemp.myDataList.Add(0x0003);
            SENDDATA = sendtemp;

            sendtemp.myDataList.Clear();
            sendtemp.myDataList.Add(0x000F);
            SENDDATA = sendtemp;
        }
        /// <summary>
        /// 运行
        /// </summary>
        public void Run()
        {
            // Reset();
            ModbusData sendtempControl = new ModbusData();
            sendtempControl.myPRO = PRO;
            sendtempControl.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtempControl.myRegister = RegisterEnum.MotorControl;
            sendtempControl.mySendDataNumber = 0x0000;
            sendtempControl.myDataList.Add(0x000F);
            SENDDATA = sendtempControl;

            sendtempControl.myDataList.Clear();
            sendtempControl.myDataList.Add(0x001F);
            SENDDATA = sendtempControl;
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            ModbusData sendtempControl = new ModbusData();
            sendtempControl.myPRO = PRO;
            sendtempControl.myFunctionCode = FunctionCodeEnum.SingleWrite;
            sendtempControl.myRegister = RegisterEnum.MotorControl;
            sendtempControl.mySendDataNumber = 0x0000;
            sendtempControl.myDataList.Add(0x010F);
            SENDDATA = sendtempControl;
        }


        /// <summary>
        /// 事件参数类
        /// </summary>
        public class MESSIGEEventArgs : EventArgs
        {
            /// <summary>
            /// 消息
            /// </summary>
            public string MyMessage { get; set; } = "";
            /// <summary>
            /// 消息类型
            /// </summary>
            public MessageType MyType { get; set; } = MessageType.Message;
            public enum MessageType
            {
                /// <summary>
                /// 警告信息
                /// </summary>
                Warning,
                /// <summary>
                /// 错误信息
                /// </summary>
                Error,
                /// <summary>
                /// 消息
                /// </summary>
                Message,
                /// <summary>
                /// 数据
                /// </summary>
                Data,
            }
        }
        // 声明委托
        public delegate void MESSIGEEventHandler(object sender, MESSIGEEventArgs e);
        /// <summary>
        /// 马达发生的所有事件
        /// </summary>
        public event MESSIGEEventHandler TRIGGERMESSIGE;
        // 触发事件的方法
        protected virtual void OnTRIGGERMESSIGE(MESSIGEEventArgs e)
        {
            TRIGGERMESSIGE?.Invoke(this, e);
        }


        /// <summary>
        /// 组合Modbus指令
        /// </summary>
        class ModbusData
        {
            /// <summary>
            /// 站号
            /// </summary>
            public byte myPRO = 1;
            /// <summary>
            /// 功能码
            /// </summary>
            public FunctionCodeEnum myFunctionCode = 0;
            /// <summary>
            /// 寄存器
            /// </summary>
            public RegisterEnum myRegister = RegisterEnum.MotorMode;
            /// <summary>
            /// 数量(如果[写]16位的资料,需设定为0x0000,这时发送出去的资料不会发数量)
            /// </summary>
            public int mySendDataNumber = 0x0001;
            /// <summary>
            /// 需要发出去的资料
            /// </summary>
            public List<int> myDataList = new List<int>();
            /// <summary>
            /// 发送出去的指令
            /// </summary>
            public byte[] GetSenddata
            {
                get
                {
                    string strdata = "";
                    strdata += Convert.ToString(myPRO, 16).PadLeft(2, '0');
                    strdata += ((int)myFunctionCode).ToString().PadLeft(2, '0');//Convert.ToString((int)myFunctionCode, 16).PadLeft(2, '0');
                    strdata += Convert.ToString((int)myRegister, 16).PadLeft(4, '0');
                    if (mySendDataNumber != 0)
                        strdata += Convert.ToString(mySendDataNumber, 16).PadLeft(4, '0');
                    if (myFunctionCode != FunctionCodeEnum.MultipleWrites)
                    {
                        if (myDataList.Count > 0)
                        {
                            for (int i = 0; i < myDataList.Count; i++)
                                strdata += Convert.ToString(myDataList[i], 16).PadLeft(4, '0');
                        }
                    }
                    else
                    {
                        strdata += (myDataList.Count * 4).ToString().PadLeft(2, '0');
                        if (myDataList.Count > 0)
                        {
                            for (int i = 0; i < myDataList.Count; i++)
                            {
                                string strtemp = Convert.ToString(myDataList[i], 16);
                                strdata += strtemp.PadLeft(8, '0');
                            }
                        }
                    }

                    strdata = strdata.ToUpper();

                    byte[] senddata = SERIALPORT.hexStringToByte(strdata);
                    senddata = SERIALPORT.CRC16_TO(senddata);

                    string strTemp = "";
                    for (int i = 0; i < senddata.Length; i++)
                        strTemp += senddata[i].ToString("X2") + " ";

                    //string strsenddata = System.Text.Encoding.Default.GetString(senddata);
                    return senddata;
                }
            }

        }

    }
    public class MotorState
    {
        /// <summary>
        /// 初始化是否准备好
        /// </summary>
        public bool ISReadyInitial { get; set; } = false;
        /// <summary>
        /// 初始化是否完成
        /// </summary>
        public bool ISInitialOK { get; set; } = false;
        /// <summary>
        /// 电机是否使能
        /// </summary>
        public bool ISEnable { get; set; } = false;
        /// <summary>
        /// 驱动器是否有错误
        /// </summary>
        public bool ISDriverErr { get; set; } = false;
        /// <summary>
        /// 驱动器是否正在工作
        /// </summary>
        public bool ISDriverRuning { get; set; } = false;
        /// <summary>
        /// 是否快停
        /// </summary>
        public bool ISQuickStop { get; set; } = false;
        /// <summary>
        /// 是否在初始化状态
        /// </summary>
        public bool ISInitialState { get; set; } = false;
        /// <summary>
        /// 是否有警告
        /// </summary>
        public bool ISNotice { get; set; } = false;
        /// <summary>
        /// 是否暂停运行
        /// </summary>
        public bool ISSuspend { get; set; } = false;
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool ISRuning { get; set; } = false;
        /// <summary>
        /// 是否到位
        /// </summary>
        public bool ISInPlace { get; set; } = false;
        /// <summary>
        /// 是否在机械源点原点
        /// </summary>
        public bool ISSW { get; set; } = false;
        /// <summary>
        /// 是否在负极限
        /// </summary>
        public bool ISCW { get; set; } = false;
        /// <summary>
        /// 是否在正极限
        /// </summary>
        public bool ISCCW { get; set; } = false;
        /// <summary>
        /// 是否可设定新位置
        /// </summary>
        public bool ISSET { get; set; } = false;
        /// <summary>
        /// 是否位置超差
        /// </summary>
        public bool ISOverproof { get; set; } = false;
    }
}