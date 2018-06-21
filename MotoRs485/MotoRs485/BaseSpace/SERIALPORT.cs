using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MotoRs485.BaseSpace
{
    /// <summary>
    /// 串口
    /// </summary>
    public class SERIALPORT
    {
        #region 变量申明
        SerialPort mSerial = new SerialPort();
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        
        /// <summary>
        /// COM口是否离线
        /// </summary>
        public bool isRUNing = false;
        /// <summary>
        /// COM口是否在跑线状态（Debug时用）
        /// </summary>
        public bool isCOMRun = false;
        /// <summary>
        /// 检查PLC掉线用到的临时变量
        /// </summary>
        bool isResetTemp = true;
        /// <summary>
        /// 每秒通信的数量
        /// </summary>
        public int iCount = 0;
        /// <summary>
        /// 离线扫描次数
        /// </summary>
        public int Off_line_Number = 100;
        /// <summary>
        /// COM口无响应次数
        /// </summary>
        long iStopContTemp = 0;
        /// <summary>
        /// 记录返回多少条信息临时记录
        /// </summary>
        int iCountTemp = 0;
        /// <summary>
        /// 发送出去的数据暂存（以防发出去了没有回来，方便之后再次发送）
        /// </summary>
        byte[] SendBytes;
        /// <summary>
        /// 固定发送读取值频率(1为除优先发送的指令外,一直在读取. 2为两条指令中有一条是读取,3表示三条指令中有一条为读取固定值)
        /// </summary>
        public int iSixedCount = 3;
        /// <summary>
        /// 固定值发送临时变量
        /// </summary>
        int iSixedTemp = 2;
        /// <summary>
        /// 固定值发送的序号
        /// </summary>
        int index = 0;
        /// <summary>
        /// 优先发送的指令
        /// </summary>
        List<byte[]> list_bytes_First = new List<byte[]>();
        /// <summary>
        /// 次优先发送的指令集
        /// </summary>
        List<byte[]> list_bytes = new List<byte[]>();
        /// <summary>
        /// 发送的指令集(这个指令集为不变指令集，增加上来的指令会循环发送）
        /// </summary>
        List<byte[]> list_bytes_Sixed = new List<byte[]>();
        /// <summary>
        /// 存储返回来的数据
        /// </summary>
        List<byte> MyOutDataTempList = new List<byte>();
        #endregion
        #region 初始化
        public SERIALPORT(string INIAddress, bool isDebug)
        {
            Initial(INIAddress, isDebug);
        }
        /// <summary>
        /// 初始化COM口
        /// </summary>
        /// <param name="isDebug">COM是否跑线</param>
        /// <param name="PLCiniAddress">COM口的INI地址</param>
        public void Initial(string INIAddress, bool isDebug)
        {
            isCOMRun = !isDebug;
            isRUNing = isDebug;
            try
            {
                if (isCOMRun)
                {
                    SERIALPOETINI mINI = new SERIALPOETINI(INIAddress);

                    if (!mSerial.IsOpen)
                    {
                        mSerial.PortName = mINI.stPort;
                        mSerial.BaudRate = mINI.iRates;
                        mSerial.DataBits = mINI.iDataBits;

                        //奇偶校验位
                        if (mINI.Parity == "Even")
                            mSerial.Parity = Parity.Even;
                        if (mINI.Parity == "Mark")
                            mSerial.Parity = Parity.Mark;
                        if (mINI.Parity == "None")
                            mSerial.Parity = Parity.None;
                        if (mINI.Parity == "Odd")
                            mSerial.Parity = Parity.Odd;
                        if (mINI.Parity == "Space")
                            mSerial.Parity = Parity.Space;
                        //停止位
                        if (mINI.StopBits == 1)
                            mSerial.StopBits = StopBits.One;
                        if (mINI.StopBits == 0)
                            mSerial.StopBits = StopBits.None;
                        if (mINI.StopBits == 1.5)
                            mSerial.StopBits = StopBits.OnePointFive;
                        if (mINI.StopBits == 2)
                            mSerial.StopBits = StopBits.Two;

                        mSerial.ReadTimeout = 60 * 60 * 1000;//读超时
                        mSerial.WriteTimeout = 60 * 1000;//写超时
                        mSerial.ReadBufferSize = 1024; //设定读出的缓存区大小
                        mSerial.WriteBufferSize = 512; //设定写入的缓存区大小

                        //bAddress = (byte)mINI.iPRO;
                    }
                    ConnnectToCOM();
                }

            }
            catch (Exception eee)
            {
                MESSIGEEventArgs eE = new MESSIGEEventArgs();
                eE.MyType = MESSIGEEventArgs.MessageType.Error;
                eE.MyMessage = eee.ToString();
                OnTRIGGERMESSIGE(eE);
            }

        }
        #endregion
        #region  读取COM端返回区的资料
        void m_Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            int n = mSerial.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
            byte[] buf = new byte[n];//
            mSerial.Read(buf, 0, n);//读取缓冲数据
            mSerial.DiscardInBuffer();//清除输入缓冲区

            foreach (byte b in buf)
                MyOutDataTempList.Add(b);

            if (MyOutDataTempList.Count > 2)
            {
                if (CRC16_CHECK(MyOutDataTempList))//CRC校验比对是否成功
                {

                    byte[] byteArray = new byte[MyOutDataTempList.Count];
                    for (int i = 0; i < MyOutDataTempList.Count; i++)
                        byteArray[i] = MyOutDataTempList[i];
                    MyOutDataTempList.Clear();

                    String str = BitConverter.ToString(byteArray, 0).ToUpper();
                    MESSIGEEventArgs eE = new MESSIGEEventArgs();
                    eE.MyMessage = str;
                    eE.SendData = SendBytes.Clone() as byte[];
                    eE.MyData = byteArray;
                    eE.MyType = MESSIGEEventArgs.MessageType.Data;
                    OnTRIGGERMESSIGE(eE);

                    iCountTemp++;
                    if (!watch.IsRunning)
                        watch.Start();
                    if (watch.ElapsedMilliseconds > 1000)
                    {
                        watch.Stop();
                        watch.Reset();
                        watch.Start();
                        iCount = iCountTemp;
                        iCountTemp = 0;
                    }
                    isRUNing = true;
                    iStopContTemp = 0;
                    SendBytes = null;
                    SendData();
                }
            }

        }
        #endregion
        #region 检查Com端口是否死掉 及重置
        public void Tick()
        {
            if (isCOMRun)
            {
                if (SendBytes == null && 
                    list_bytes.Count == 0 && 
                    list_bytes_First.Count == 0 &&
                   list_bytes_Sixed.Count==0  )
                    return;

                if (iStopContTemp > Off_line_Number)
                {
                    if (isResetTemp)
                    {
                        iCount = 0;
                        DisConnectToCOM();
                        ConnnectToCOM();
                        if (iStopContTemp > Off_line_Number - 2)
                            iStopContTemp = Off_line_Number - 5;
                        isRUNing = false;//PLC掉线
                        isResetTemp = false;
                    }
                    else
                    {
                        if (iStopContTemp == Off_line_Number + 1)
                        {
                            iStopContTemp++;

                            MESSIGEEventArgs eE = new MESSIGEEventArgs();
                            eE.MyData = SendBytes;
                            eE.MyMessage = mSerial.PortName +" 站号:" +SendBytes[0]+"离线!";
                            eE.MyType = MESSIGEEventArgs.MessageType.Warning;
                            OnTRIGGERMESSIGE(eE);
                        }
                    }


                }
                else if (iStopContTemp > 0 && iStopContTemp % 10 == 0)
                {
                    if (SendBytes != null)
                    {
                        WriteCOM(SendBytes);
                        iCount = 1;
                    }
                    else
                        SendData();
                }

                iStopContTemp++;
            }
        }
        /// <summary>
        /// 重置COM口数据
        /// </summary>
        public void ResetCOM()
        {
            iStopContTemp = 0;
            isResetTemp = true;
            isRUNing = true;
        }
        #endregion
        #region 连接/断开 COM口设备
        void ConnnectToCOM(SerialPort sp)
        {
            if (isCOMRun)
            {
                mSerial = new SerialPort();
                mSerial = sp;

                bool isportname = true;
                string[] strr = System.IO.Ports.SerialPort.GetPortNames();
                foreach (string strCom in strr)
                {
                    if (strCom == mSerial.PortName)
                    {
                        if (!mSerial.IsOpen)
                            mSerial.Open();

                        iStopContTemp = 0;
                        isportname = false;
                        mSerial.DataReceived += new SerialDataReceivedEventHandler(m_Serial_DataReceived);
                    }
                }
                if (isportname)
                {
                    MESSIGEEventArgs eE = new MESSIGEEventArgs();
                    eE.MyMessage = "无" + mSerial.PortName + "端口!";
                    eE.MyType = MESSIGEEventArgs.MessageType.Error;
                    OnTRIGGERMESSIGE(eE);
                }
            }
        }
        /// <summary>
        /// 连接COM口设备
        /// </summary>
        /// <returns></returns>
        public bool ConnnectToCOM()
        {
            if (isCOMRun)
            {
                bool isportname = true;
                string[] strr = System.IO.Ports.SerialPort.GetPortNames();
                foreach (string strCom in strr)
                {
                    if (strCom == mSerial.PortName)
                    {
                        if (!mSerial.IsOpen)
                        {
                            try
                            {
                                mSerial.Open();
                                mSerial.DiscardInBuffer();//清除输入缓冲区
                                mSerial.DiscardOutBuffer();//清除输出缓冲区

                                mSerial.DataReceived += new SerialDataReceivedEventHandler(m_Serial_DataReceived);
                                isportname = false;

                            }
                            catch
                            {
                                MESSIGEEventArgs eE = new MESSIGEEventArgs();
                                eE.MyMessage = mSerial.PortName + "被占用！";
                                eE.MyType = MESSIGEEventArgs.MessageType.Error;
                                OnTRIGGERMESSIGE(eE);

                            }
                        }
                        return mSerial.IsOpen;
                    }

                    if (isportname)
                    {
                        MESSIGEEventArgs eEE = new MESSIGEEventArgs();
                        eEE.MyMessage = "无" + mSerial.PortName + "端口!";
                        eEE.MyType = MESSIGEEventArgs.MessageType.Error;
                        OnTRIGGERMESSIGE(eEE);
                    }
                }
            }
            return mSerial.IsOpen;
        }
        /// <summary>
        /// 关闭COM口
        /// </summary>
        /// <returns></returns>
        public bool DisConnectToCOM()
        {
            if (isCOMRun)
            {
                bool isportname = true;
                string[] strr = System.IO.Ports.SerialPort.GetPortNames();
                foreach (string strCom in strr)
                {
                    if (strCom == mSerial.PortName)
                    {
                        if (mSerial.IsOpen)
                            mSerial.Close();


                        isportname = false;
                        return !mSerial.IsOpen;
                    }
                }
                if (isportname)
                {
                    MESSIGEEventArgs eE = new MESSIGEEventArgs();
                    eE.MyMessage = "无" + mSerial.PortName + "端口!";
                    eE.MyType = MESSIGEEventArgs.MessageType.Error;
                    OnTRIGGERMESSIGE(eE);
                }
            }
            return !mSerial.IsOpen;
        }

        #endregion
        #region 指令发送及排队
        /// <summary>
        /// 检查是否有发送的指令
        /// </summary>
        public void SendData()
        {
           //优先发送
            if (list_bytes_First.Count > 0)
            {
                lock (list_bytes_First)
                {
                    SendBytes = list_bytes_First[0].Clone() as byte[];
                    list_bytes_First.RemoveAt(0);
                }
                //  isSendFirst = true;
                WriteCOM(SendBytes);
            }
            else
            {
                //记录已发送出去的指令数
                if (list_bytes_Sixed.Count > 0)
                    iSixedTemp++;
                //固定发送的指令列表
                if (iSixedTemp > iSixedCount && list_bytes_Sixed.Count > 0)
                {
                    lock (list_bytes_Sixed)
                    {
                        SendBytes = list_bytes_Sixed[index].Clone() as byte[];

                        index++;
                        if (index >= list_bytes_Sixed.Count)
                            index = 0;
                    }
                    WriteCOM(SendBytes);
                    iSixedTemp = 0;

                }
                else if (list_bytes.Count > 0)//排队发送的指令
                {
                    lock (list_bytes)
                    {

                        SendBytes = list_bytes[0].Clone() as byte[];
                        list_bytes.RemoveAt(0);
                    }
                    WriteCOM(SendBytes);
                }
                else
                {
                    if (list_bytes_Sixed.Count > 0)
                        SendData();
                }

            }
        }
        /// <summary>
        /// 向COM口发送指令
        /// </summary>
        /// <param name="cmd">需要发送的指令</param>
        public void WriteCOM(byte[] cmd)
        {
            MyOutDataTempList.Clear();
            string[] strr = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string strCom in strr)
            {
                if (strCom == mSerial.PortName)
                {
                    try
                    {
                        if (mSerial.IsOpen)
                        {
                            mSerial.DiscardOutBuffer();//清除输出缓冲区
                            mSerial.DiscardInBuffer();//丢弃来自串行驱动程序的接收缓冲区的数据
                            mSerial.Write(cmd, 0, cmd.Length);

                        }
                    }
                    catch (Exception eee)
                    {
                        MESSIGEEventArgs eE = new MESSIGEEventArgs();
                        eE.MyMessage = eee.ToString();
                        eE.MyType = MESSIGEEventArgs.MessageType.Error;
                        OnTRIGGERMESSIGE(eE);
                    }
                }
            }
        }
        
        /// <summary>
        /// 增加第一优先发送的指令
        /// </summary>
        public byte[] ADDInstructionsFirst
        {
            set
            {
                //try
                //{
                //    lock (list_bytes_First)
                //    {
                //        //如果已经有这条指令了，就不要重复再下了
                //        for (int i = list_bytes_First.Count - 1; i > 0; i--)
                //        {
                //            bool equals = list_bytes_First[i].OrderBy(a => a).SequenceEqual(value.OrderBy(a => a));

                //            if (equals)
                //            {
                //                return;
                //            }
                //        }
                //    }
                //}
                //catch { }

                lock (list_bytes_First)
                {
                    if (value != null)
                        list_bytes_First.Add(value);
                }
            }
        }
        /// <summary>
        /// 增加次优先发送的指令
        /// </summary>
        public byte[] ADDInstructions
        {
            set
            {
                //try
                //{
                //    lock (list_bytes)
                //    {
                //        //如果已经有这条指令了，就不要重复再下了
                //        for (int i = list_bytes.Count - 1; i > 0; i--)
                //        {
                //            bool equals = list_bytes[i].OrderBy(a => a).SequenceEqual(value.OrderBy(a => a));

                //            if (equals)
                //            {
                //                return;
                //            }
                //        }
                //    }
                //}
                //catch { }

                lock (list_bytes)
                {
                    if (value != null)
                        list_bytes.Add(value);
                }
            }
        }
        /// <summary>
        /// 增加循环发送的指令(固定指令)
        /// </summary>
        public byte[] ADDInstructionsSixed
        {
            set
            {
                //try
                //{
                //    lock (list_bytes_Sixed)
                //    {
                //        //如果已经有这条指令了，就不要重复再下了
                //        for (int i = list_bytes_Sixed.Count - 1; i > 0; i--)
                //        {
                //            bool equals = list_bytes_Sixed[i].OrderBy(a => a).SequenceEqual(value.OrderBy(a => a));

                //            if (equals)
                //            {
                //                return;
                //            }
                //        }
                //    }
                //}
                //catch { }

                lock (list_bytes_Sixed)
                {
                    if (value != null)
                        list_bytes_Sixed.Add(value);
                }
            }
        }

        #endregion
        #region 方法组
        /// <summary>
        /// 字符转byte[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] hexStringToByte(String hex)
        {
            hex = hex.ToUpper();
            int len = (hex.Length / 2);
            byte[] result = new byte[len];
            char[] achar = hex.ToCharArray();
            for (int i = 0; i < len; i++)
            {
                int pos = i * 2;
                result[i] = (byte)(toByte(achar[pos]) << 4 | toByte(achar[pos + 1]));
            }
            return result;
        }
        private static int toByte(char c)
        {

            byte b = (byte)"0123456789ABCDEF".IndexOf(c);
            return b;
        }

        /// <summary>
        /// CRC校验(返回完整数据)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static  byte[] CRC16_TO(byte[] data)
        {
            byte[] byt = CRC16_C(data);

            byte[] Sendbyte = new byte[data.Length + 2];
            for (int i = 0; i < data.Length; i++)
            {
                Sendbyte[i] = data[i];
            }
            Sendbyte[Sendbyte.Length - 2] = byt[1];
            Sendbyte[Sendbyte.Length - 1] = byt[0];

            return Sendbyte;
        }
        /// <summary>
        /// CRC校验(返回校验值)
        /// </summary>
        /// <param name="data">要校验的byte数组</param>
        /// <returns>校验值</returns>
        public static byte[] CRC16_C(byte[] data)
        {
            byte CRC16Lo;
            byte CRC16Hi;   //CRC寄存器 
            byte CL; byte CH;       //多项式码&HA001 
            byte SaveHi; byte SaveLo;
            byte[] tmpData;
            int I;
            int Flag;
            CRC16Lo = 0xFF;
            CRC16Hi = 0xFF;
            CL = 0x01;
            CH = 0xA0;
            tmpData = data;
            for (int i = 0; i < tmpData.Length; i++)
            {
                CRC16Lo = (byte)(CRC16Lo ^ tmpData[i]); //每一个数据与CRC寄存器进行异或 
                for (Flag = 0; Flag <= 7; Flag++)
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);      //高位右移一位 
                    CRC16Lo = (byte)(CRC16Lo >> 1);      //低位右移一位 
                    if ((SaveHi & 0x01) == 0x01) //如果高位字节最后一位为1 
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);   //则低位字节右移后前面补1 
                    }             //否则自动补0 
                    if ((SaveLo & 0x01) == 0x01) //如果LSB为1，则与多项式码进行异或 
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            byte[] ReturnData = new byte[2];
            ReturnData[0] = CRC16Hi;       //CRC高位 
            ReturnData[1] = CRC16Lo;       //CRC低位 
            return ReturnData;
        }
        /// <summary>
        /// CRC校验比对
        /// </summary>
        /// <param name="data">要比对的byte数组</param>
        /// <returns>是否比对成功</returns>
        bool CRC16_CHECK(List<byte> data)
        {
            byte CRC16Lo;
            byte CRC16Hi;   //CRC寄存器 
            byte CL; byte CH;       //多项式码&HA001 
            byte SaveHi; byte SaveLo;
            List<byte> tmpData;
            int I;
            int Flag;
            CRC16Lo = 0xFF;
            CRC16Hi = 0xFF;
            CL = 0x01;
            CH = 0xA0;
            tmpData = data;
            for (int i = 0; i < tmpData.Count - 2; i++)
            {
                CRC16Lo = (byte)(CRC16Lo ^ tmpData[i]); //每一个数据与CRC寄存器进行异或 
                for (Flag = 0; Flag <= 7; Flag++)
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);      //高位右移一位 
                    CRC16Lo = (byte)(CRC16Lo >> 1);      //低位右移一位 
                    if ((SaveHi & 0x01) == 0x01) //如果高位字节最后一位为1 
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);   //则低位字节右移后前面补1 
                    }             //否则自动补0 
                    if ((SaveLo & 0x01) == 0x01) //如果LSB为1，则与多项式码进行异或 
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            bool isCheck = false;
            if (tmpData[tmpData.Count - 1] == CRC16Hi &&  //CRC高位 
                tmpData[tmpData.Count - 2] == CRC16Lo)  //CRC低位 
                isCheck = true;

            return isCheck;
        }

        #endregion

        #region Trigger 事件
        /// <summary>
        /// 事件参数类
        /// </summary>
        public class MESSIGEEventArgs : EventArgs
        {
            /// <summary>
            /// 数据
            /// </summary>
            public byte[] SendData { get; set; }
            /// <summary>
            /// 数据
            /// </summary>
            public byte[] MyData { get; set; }
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
        /// COM口发生的所有事件
        /// </summary>
        public event MESSIGEEventHandler TRIGGERMESSIGE;
        // 触发事件的方法
        protected virtual void OnTRIGGERMESSIGE(MESSIGEEventArgs e)
        {
            TRIGGERMESSIGE?.Invoke(this, e);
        }
        #endregion
    }

    /// <summary>
    /// 串口INI档案
    /// </summary>
    [Serializable]
    class SERIALPOETINI
    {
        #region 引入dll
        [DllImport("kernel32.dll")]
        //参数说明:(ini文件中的段落,INI中的关键字,INI中关键字的数值,INI文件的完整路径)
        static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        //参数说明:(INI文件中的段落,INI中的关键字,INI无法读取时的缺省值,读取数值,数值大小,INI文件的完整路径)
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
        
        /// <summary>
        ///  INI地址
        /// </summary>
        string st_AD = "";// @"D:\AUTOMATION\Eazy Tools Class\ComPro\ComPortTest\FATEKPLC.INI";
        /// <summary>
        /// COM口
        /// </summary>
        public string stPort = "COM3";
        /// <summary>
        /// 鲍率
        /// </summary>
        public int iRates = 57600;
        /// <summary>
        /// 数据位
        /// </summary>
        public int iDataBits = 7;
        /// <summary>
        /// 奇偶校验位
        /// </summary> 
        public string Parity = "None";
        /// <summary>
        /// 停止位
        /// </summary>
        public int StopBits = 1;
        /// <summary>
        /// 站号
        /// </summary>
        public int iPRO = 1;
        /// <summary>
        /// 初始化INI
        /// </summary>
        /// <param name="PLCiniAddress"></param>
        public SERIALPOETINI(string PLCiniAddress)
        {
            st_AD = PLCiniAddress;
            INIRead();
        }
        /// <summary>
        /// INI读取
        /// </summary>
        public void INIRead()
        {
            //读INI文件
            stPort = INIReadIO("Recording", "PLCPort", "COM1");
            Parity = INIReadIO("Recording", "Parity", "None");

            iRates = int.Parse(INIReadIO("Recording", "PLCRates", "9600"));
            iDataBits = int.Parse(INIReadIO("Recording", "DataBits", "8"));
            StopBits = int.Parse(INIReadIO("Recording", "StopBits", "1"));
            iPRO = int.Parse(INIReadIO("Recording", "PRO", "1"));

        }
        /// <summary>
        /// 从INI里读出内容
        /// </summary>
        /// <param name="st_Class">INI文件中的段落</param>
        /// <param name="st_">INI中的关键字</param>
        /// <param name="st_ad">INI无法读取时的缺省值</param>
        public string INIReadIO(string st_Class, string st_, string st_ad)
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

    }
}