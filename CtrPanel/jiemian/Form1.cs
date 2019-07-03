using System;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using myfunctiondemo;
using speed_rms;
using MathWorks.MATLAB.NET.Arrays;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATLAB_WAVEANA;
using mainpro;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using iTextSharp.text;
using iTextSharp.text.pdf;




namespace TorsionalTest
{
    public partial class CtrPanel : Form
    {
        public CtrPanel()
        {
            InitializeComponent();
        }

        //数据初始化

        //电机参数初始化
        public DllImport.ProcessDelegate m_delRtcnProcess = null;
        int m_dwGWCount;
        int m_dwNodeCount;
        IntPtr m_gatewayInfoObj;
        IntPtr m_drvInfoObj; 
        public DllImport.GATEWAY_INFO_OBJ[] gatewayInfoObj;
        public DllImport.DRV_INFO_OBJ[] drvInfoObj;
        bool m_bDevCnectFlg = false;
        uint m_uiDevIndex = 0;
        uint iNodeID = 0;
        //bool m_bRtcnNotify = false;

        //数据通信初始化
        private delegate void setTextDelegate(string msg);

        //通信协议
        IPAddress ip;
        IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
        string protocol;
        string strRec;
        Socket aimSocket;
        Socket mySocket;

        //计时器、线程
        System.Windows.Forms.Timer tmr = new System.Windows.Forms.Timer();
        bool IsSending = false;
        Thread thReceive;
        int count = 0;
        double indexTime = 0;
        double offsetNumber = 0;//偏移量
        double signal_y;
        bool bRecvRun;
        int speed = 100;
        int offset_x = 1;
        static int j = 0;

        //标志位
        bool con2 = false;
        static int con1 = 0;

        //图表参数
        const int chartInitiMaxSizeX = 300;
        const int chartInitiMinSizeX = 0;
        const double chartMaxSizeY = 0.5;
        const double chartMinSizeY = -0.5;
        const int chartVerRange = 20;
        const int chartInitiVerScrollMin = 0;
        const int chartInitiHorMin = 0;
        const int chartIntervalX = 10;
        const double chartIntervalY = 0.05;
        const int chartInitializeVerMax = 1;
        const int chartInitializeVerMin = 0;

        //定义数据存储变量
        static int Fs = 2530;   //传感器采样频率
        double[,] a = new double[120, Fs];
        float[,] b = new float[120, Fs];
        bool flagisok = true;
        string signal;
        int rms;


        //winform加载与关闭
        private void CtrPanel_Load(object sender, EventArgs e)
        {
            m_gatewayInfoObj = Marshal.AllocHGlobal(DllImport.MAX_GATEWAY_COUNT * Marshal.SizeOf(typeof(DllImport.GATEWAY_INFO_OBJ)));
            m_drvInfoObj = Marshal.AllocHGlobal(DllImport.MAX_NODE_COUNT * Marshal.SizeOf(typeof(DllImport.DRV_INFO_OBJ)));
            gatewayInfoObj = new DllImport.GATEWAY_INFO_OBJ[DllImport.MAX_GATEWAY_COUNT];
            drvInfoObj = new DllImport.DRV_INFO_OBJ[DllImport.MAX_NODE_COUNT];
            SearchGW.Enabled = true;
            OpenGW.Enabled = false;
            CloseGW.Enabled = false;
            //m_delRtcnProcess = RevRtcnNotify;
            //delegateShowRTCN = ShowRTCN;
            //GC.KeepAlive(m_delRtcnProcess);

            cobProtocol.SelectedIndex = 0;
            txtIP.Text = GetAddressIP();

            timer1.Enabled = false;//禁止计时器运行
            timer1.Interval = 1;
            timer2.Enabled = false;
            timer2.Interval = 20000;
            timer3.Enabled = false;
            timer3.Interval = 10000;
            InitChart();

            tmr.Interval = 1500;  //设置中断时间
            tmr.Enabled = false;
            tmr.Tick += new EventHandler(GetSpd_Click);

            CheckForIllegalCrossThreadCalls = false;//初始化为TCP Server模式
            thReceive = new Thread(TSReceive);
            thReceive.IsBackground = true;
            bRecvRun = true;


            /*
            //实例化线程，用来初次调用matlab，并把图像窗体放到winform
            startload = new Thread(new ThreadStart(startload_run));
            //运行线程方法
            startload.Start();
            */

            for (int i = 0; i < 19; i++)
            {
                int j = 200 + i * 100;
                string Spd = j.ToString();
                string[] Row = new string[] { Spd };

                dataGridView1.Rows.Add(Row);
                dataGridView1.AllowUserToAddRows = false;
            }
        }

        private void CtrPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_bDevCnectFlg)
            {
                //设备已经连接，断开
                if (false == DllImport.CloseGateWay(m_uiDevIndex))
                {
                    MessageBox.Show("关闭设备失败!");
                    SearchGW.Enabled = true;
                    OpenGW.Enabled = false;
                    CloseGW.Enabled = false;
                    return;
                }
                m_bDevCnectFlg = false;
                m_uiDevIndex = 0;
            }
            Marshal.FreeHGlobal(m_gatewayInfoObj);
            Marshal.FreeHGlobal(m_drvInfoObj);
        }

        //winform控件初始化
        private void InitChart()
        {
            chart2.Series[0].ChartType = SeriesChartType.Line;
            //chart2.ChartAreas[0].AxisY.Minimum = chartMinSizeY;
            //chart2.ChartAreas[0].AxisY.Maximum = chartMaxSizeY;
            //chart2.ChartAreas[0].AxisY.Interval = chartIntervalY;
            chart2.ChartAreas[0].AxisX.Minimum = chartInitiMinSizeX;
            chart2.ChartAreas[0].AxisX.Maximum = chartInitiMaxSizeX;
            chart2.ChartAreas[0].AxisX.Interval = chartIntervalX;
            //设置chart网格
            chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Transparent;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Transparent;
            //初始化x，y坐标轴
            //points数据为：(0,0),(500,0),(1000,0)...(3500,0)
            for (double i = 0; i <= 20; i += 1)
            {
                chart2.Series[0].Points.AddXY(i, 0);
                chart2.Series[0].Points[chart2.Series[0].Points.Count - 1].IsEmpty = true;
            }

            //chart1的属性定义
            //chart1.ChartAreas[0].AxisY.Minimum = 0;
            //chart1.ChartAreas[0].AxisY.Maximum = 0.2;
            //chart1.ChartAreas[0].AxisY.Interval = 0.02;
            chart1.ChartAreas[0].AxisX.Minimum = 200;
            chart1.ChartAreas[0].AxisX.Maximum = 2000;
            chart1.ChartAreas[0].AxisX.Interval = 100;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Transparent;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Transparent;
            for (double i = 0; i <= 20; i += 1)
            {
                chart1.Series[0].Points.AddXY(i, 0);
                chart1.Series[0].Points[chart1.Series[0].Points.Count - 1].IsEmpty = true;
            }
        }

        private bool IsSiteValidated()
        {
            if (!m_bDevCnectFlg)
            {
                MessageBox.Show("尚未连接设备，请连接后再执行本操作");
                return false;
            }

            if (m_uiDevIndex == 0)
            {
                MessageBox.Show("尚未打开设备，请成功打开后再执行本指令");
                return false;
            }

            if (iNodeID == 0)
            {
                MessageBox.Show("尚未选择站点，请选中后再执行本指令");
                return false;
            }

            return true;
        }

        private void cobProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            protocol = cobProtocol.SelectedItem.ToString();

        }


        //方法区：

        //UIfunction.dll导入
        public static class DllImport
        {
            //1. 网关类型宏定义
            const int UIGW_RS232CAN = 0x0100;
            const int UIGW_USBCAN = 0x1000;
            public const int UIGW_ALL = (UIGW_RS232CAN | UIGW_USBCAN);
            public const int MAX_GATEWAY_COUNT = 5;
            public const int MAX_NODE_COUNT = 128;
            //	变化通知
            const int RTCN_SYS_EMR = 0;
            const int RTCN_DIO_P1L = 1;
            const int RTCN_DIO_P1H = 2;
            const int RTCN_DIO_P2L = 3;
            const int RTCN_DIO_P2H = 4;
            const int RTCN_DIO_P3L = 5;
            const int RTCN_DIO_P3H = 6;
            const int RTCN_DIO_P4L = 7;
            const int RTCN_DIO_P4H = 8;
            const int RTCN_DIO_P5L = 9;
            const int RTCN_DIO_P5H = 10;
            const int RTCN_DIO_P6L = 11;
            const int RTCN_DIO_P6H = 12;
            const int RTCN_DIO_P7L = 13;
            const int RTCN_DIO_P7H = 14;
            const int RTCN_DIO_P8L = 15;
            const int RTCN_DIO_P8H = 16;
            const int RTCN_DIO_P9L = 17;
            const int RTCN_DIO_P9H = 18;
            const int RTCN_DIO_P10L = 19;
            const int RTCN_DIO_P10H = 20;
            const int RTCN_DIO_P11L = 21;
            const int RTCN_DIO_P11H = 22;
            const int RTCN_DIO_P12L = 23;
            const int RTCN_DIO_P12H = 24;
            const int RTCN_DIO_P13L = 25;
            const int RTCN_DIO_P13H = 26;
            const int RTCN_DIO_P14L = 27;
            const int RTCN_DIO_P14H = 28;
            const int RTCN_DIO_P15L = 29;
            const int RTCN_DIO_P15H = 30;
            const int RTCN_DIO_P16L = 31;
            const int RTCN_DIO_P16H = 32;
            const int RTCN_MXN_STP = 41;
            const int RTCN_MXN_ORG = 42;
            const int RTCN_MXN_STL = 43;
            const int RTCN_MXN_PVW = 44;
            const int RTCN_MXN_PVS = 45;
            const int RTCN_UPG_PRT = 60;
            //定义网关信息结构
            public struct GATEWAY_INFO_OBJ
            {
                public uint dwGWIndex;            //网关的索引号，中间包含网关的类型
                public uint dwCanBtr;             //网关的can比特率
                public int dwBaudRate;                // 串口波特率
                public char gwID;                     /*网关站点地址*/
                public uint dwDrvNum;                     /*设备站点数量*/
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
                public string GWName;  //网关的相关信息	
            };

            //定义驱动器信息结构
            public struct DRV_INFO_OBJ
            {
                public uint uiDrvID;                        //驱动器can站点地址
                public int uiDrvSN;                                // 驱动器SN号
                public uint uiGWID;                         //网关地址 
                public uint dwCanBtr;                     //网关的can比特率
                public uint dwBaudRate;                    // 串口波特率
                public uint dwGWIndex;                       //网关的索引号，中间包含网关的类型
                public uint uiDrvGroupID;                   //驱动器can站点组地址
                public uint uiFirewareVersion;              //驱动器固件版本
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string szModelName;                      //驱动器型号
            };
            public struct CAN_MASTER_OBJ
            {
                public uint iNodeID; // 
                public uint iBitRate; // 波特率
            };
            public struct CAN_SLAVE_OBJ
            {
                public uint iNodeID; // 
                public uint iBitRate; // 波特率
                public uint iGroupID; // 组号
                public uint iTargetID; // 目标号
                public uint uiDrvSN;
            };
            public struct CAN_ADR_INFO
            {
                public uint iOldNodeID; //老ID
                public uint iNewNodeID; //新ID
                public uint iSN;//SN
            };
            public struct POWERUP_CONFIG_OBJ
            {
                /*
                struct
                {
                    unsigned bAuto_Mxn_On : 1; // 自动上电使能
                    unsigned bRotation_Adjust : 1; // 正向旋转方向
                    unsigned bEnable_UPG : 1;
                    unsigned bLock_System : 1;
                    unsigned bAccAndDcc_mode : 1; // 加减速模式
                    unsigned bABS : 1; // 编码器类型
                    unsigned bclose_loop_control : 1; // 开环闭环选择
                };
                 */
                public uint uiPowerUpCfgVal;
            };
            public struct MOTOR_PARAMETER_OBJ
            {
                public uint iMicrostep; // 细分
                public uint iCurrent; // 电流*10
                public uint iPowerSaveRate; // 待机电流
                public uint iAutoMotorOntime; //  上电使能时间
            };

            public struct PVTMOTION_PARAMETER_OBJ
            {
                public uint iStartPos; // 点位序列的起点位置标号
                public uint iEndPos; // 点位序列的终点位置标号
                public uint iMode; //  点位序列执行模式 
                public uint iRtcn; //  点位序列的水位通知
                public uint iNextPos; // 下一个点位序列的写入点  ,该程序620新版本，值
            };
            public struct PVT_POINT_OBJ
            {
                public int iQP; //  
                public int iQV; // 
                public uint iQT; //	
                public uint iQA;
            };
            public struct ENCODE_CONFIG_OBJ
            {
                public int iEncoderLines; // 编码器线数
                public int iStallDetectionTolerance; // 
            };
            public struct UI_MSG_OBJ
            {
                public byte canNodeId;              //驱动器站点地址
                public byte cmd;                    //消息指令码
                public byte DataLen;                //数据长度，长度不能超过8;
                public int subCmd;                 //消息子指令码
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] Data;             //数据

            };
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ProcessDelegate(uint dwGWIndex, UI_MSG_OBJ uiMsgObj);

            /*==============================================================================
             NAME:  uint SearchGateWay(uint dwDevType, P_GATEWAY_INFO_OBJ pGateWayInfoObj, uint uiLen)
             ----------------------------------------------------------------------------
             Function:查找网关
             ----------------------------------------------------------------------------
             Parameter:
                  uint dwDevType[in]:需要查询的网关类型
                  P_GATEWAY_INFO_OBJ pGateWayInfoObj[out]:获取的网关信息结构数组
                  uint uiLen[in]:查询的网关最大个数
             ----------------------------------------------------------------------------
             Return Value:  
                            成功则返回查询到的网关个数；
                            失败则返回0
             ----------------------------------------------------------------------------
             Note:		
         ==============================================================================*/
            [DllImport("UIRobotFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SearchGateWay", CharSet = CharSet.Ansi)]

            public static extern int SearchGateWay(uint dwDevType, IntPtr pGateWayInfoObj, uint uiLen);
            /*==============================================================================
                NAME:   int OpenGateWay(uint dwGWIndex, P_DRV_INFO_OBJ pDRVInfoObj, uint uiLen, uint * pdCanBtr)
                ----------------------------------------------------------------------------
                Function:打开网关
                ----------------------------------------------------------------------------
                Parameter:
                      uint dwGWIndex[in]:网关号
                      P_DRV_INFO_OBJ pDRVInfoObj[out]：获取的站点信息数组
                      uint uiLen[in]:站点最大个数
                      uint * pdCanBtr[out]：获取can比特率
                ----------------------------------------------------------------------------
                Return Value:  
                               成功则回返查询到的站点个数；
                               失败则返回0
                ----------------------------------------------------------------------------
                Note:		
            ==============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "OpenGateWay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int OpenGateWay(uint dwDevIndex, IntPtr pDRVInfoObj, uint uiLen, ref uint pdCanBtr);
            /*==============================================================================
                NAME:  bool CloseGateWay(uint dwGWIndex)
                ----------------------------------------------------------------------------
                Function:关闭网关
                ----------------------------------------------------------------------------
                Parameter:
                      uint dwGWIndex[in]:网关号
                ----------------------------------------------------------------------------
                Return Value:  
                               If the function fails, the return value is false. 
                               If the function succeeds, the return value is true. 
                ----------------------------------------------------------------------------
                Note:		
            ==============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "CloseGateWay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool CloseGateWay(uint dwDevIndex);


            /*=============================================================================
                NAME: uint SetComm_RS232(uint dwDevIndex, uint dwCanMasterId, uint dwBaudRate,
                                           bool bAckEna, uint* pBaudRateOut)
                ------------------------------------------------------------------------------------------------
                Function:设置RS232串口通讯参数
                -----------------------------------------------------------------------------------------------
                Parameter:
                    dwDevIndex[in]:网关设备编号
                    dwCanMasterId [in]网关设备站点号
                    dwBaudRate[in]:RS232串口通讯波特率索引值
                                                         0：4800
                                                            1：9600
                                                            2：19200
                                                            3：38400
                                                            4：57600
                                                            5：115200
                    bAckEna[in]:是否需要返回ACK信息
                    pBaudRateOut[out]:当前网关通讯波特率索引值
                ------------------------------------------------------------------------------------------
                Return Value:  
                               If the function fails, the return value is -1 
                               If the function succeeds, the return value is 1.	
            ------------------------------------------------------------------------------------------
                Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetComm_RS232", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetComm_RS232(uint dwDevIndex, uint dwCanMasterId, uint dwBaudRate,
                                          bool bAckEna, ref uint pBaudRateOut);
            /*=============================================================================
               NAME: uint GetComm_RS232(uint dwDevIndex, uint dwCanMasterId, uint* pBaudRate)
               ------------------------------------------------------------------------------------------------
               Function:获取RS232串口通讯参数
               -----------------------------------------------------------------------------------------------
               Parameter:
                   dwDevIndex[in]:网关设备编号  
                   dwCanMasterId [in]网关设备站点号
                   pBaudRate[out]:当前网关通讯参数
               ------------------------------------------------------------------------------------------
               Return Value:  
                              If the function fails, the return value is -1 
                              If the function succeeds, the return value is 1.	
           ------------------------------------------------------------------------------------------
               Note:
           =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetComm_RS232", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetComm_RS232(uint dwDevIndex, uint dwCanMasterId, ref uint pBaudRate);
            /*=============================================================================
                 NAME: uint SetComm_CAN_Master(uint dwDevIndex, uint dwCanMasterId,
                                 uint dwBitRate, bool bAckEna, uint *pBitRateOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置网关CAN通讯参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  、
                     dwCanMasterId  [in]网关设备站点号
                     dwBitRate      [in]网关CAN通讯波特率索引值
                     bAckEna        [in]是否需要返回ACK信息
                     pBitRateOut    [out]网关CAN通讯波特率索引值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetComm_CAN_Master", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetComm_CAN_Master(uint dwDevIndex, uint dwCanMasterId, uint dwBitRate, bool bAckEna, ref uint pBitRateOut);
            /*=============================================================================
                 NAME: uint GetComm_CAN_Master(uint dwDevIndex, uint dwCanMasterId, P_CAN_MASTER_OBJ pCANMasterObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取网关通讯参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                         dwDevIndex     [in]网关设备编号 
                     dwCanMasterId  [in]网关设备站点号 
                     pCANMasterObj  [out]网关CAN通讯波特率索引值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetComm_CAN_Master", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetComm_CAN_Master(uint dwDevIndex, uint dwCanMasterId,
                                    ref CAN_MASTER_OBJ pCANMasterObj);
            /*=============================================================================
                 NAME: uint SetComm_CAN_Slave(uint dwDevIndex, uint dwCanMasterId, uint dwSlaveId, 
                                P_CAN_SLAVE_OBJ pCANSlaveObjIn, bool bAckEna, P_CAN_SLAVE_OBJ pCANSlaveObjOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置站点CAN通讯参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId    [in]站点号 
                     pCANSlaveObjIn[in]:站点通讯参数
                     bAckEna[in]:是否需要返回ACK信息
                     pCANSlaveObjOut[out]:当前站点通讯参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetComm_CAN_Slave", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetComm_CAN_Slave(uint dwDevIndex, uint dwCanSlaveId,
                                  ref CAN_SLAVE_OBJ pCANSlaveObjIn, bool bAckEna, ref CAN_SLAVE_OBJ pCANSlaveObjOut);
            /*=============================================================================
                 NAME: uint GetComm_CAN_Slave(uint dwDevIndex, uint dwSlaveId, P_CAN_SLAVE_OBJ pCANSlaveObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取站点CAN通讯参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                         dwDevIndex      [in]网关设备编号  
                     dwCanSlaveId    [in]站点号 
                 pCANSlaveObj     [out]当前站点通讯参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetComm_CAN_Slave", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetComm_CAN_Slave(uint dwDevIndex, uint dwCanSlaveId, ref CAN_SLAVE_OBJ pCANSlaveObj);
            /*=============================================================================
                 NAME: int GetPowerOnTime(uint dwDevIndex, uint dwCanSlaveId, uint *pPoweOnTime);
                 ------------------------------------------------------------------------------------------------
                 Function:获取驱动器上电时间
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:]站点号 
                     pPoweOnTime[out]:驱动器上电时间
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPowerOnTime", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPowerOnTime(uint dwDevIndex, uint dwCanSlaveId, ref uint pPoweOnTime);
            /*=============================================================================
                 NAME: int ResetPowerOnTime(uint dwDevIndex, uint dwCanSlaveId);
                 ------------------------------------------------------------------------------------------------
                 Function清零驱动器上电时间
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:]站点号 

                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "ResetPowerOnTime", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int ResetPowerOnTime(uint dwDevIndex, uint dwCanSlaveId);
            /*=============================================================================
                 NAME: int Set_ADR(uint dwDevIndex, uint dwCanSlaveId, P_CAN_ADR_INFO pCanADRInfoIn);
                 ------------------------------------------------------------------------------------------------
                 Function: 设置驱动器can站点地址信息 
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:]站点号 
                         pCanADRInfoIn[in]:驱动器can站点地址信息 
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note: 
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "Set_ADR", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int Set_ADR(uint dwDevIndex, uint dwCanSlaveId, ref CAN_ADR_INFO pCanADRInfoIn);


            /*=============================================================================
                 NAME: int Comm_Update(uint dwDevIndex, uint dwNodeId, uint dwUpdateType)
                 ------------------------------------------------------------------------------------------------
                 Function:更新通讯参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwNodeId[in]:更新网关/站点通讯参数
                     dwUpdateType[in]:当前更新类型
                 ------------------------------------------------------------------------------------------
                 Return Value:
                                If the function fails, the return value is -1
                                If the function succeeds, the return value is 1.
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "Comm_Update", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int Comm_Update(uint dwDevIndex, uint dwNodeId, uint dwUpdateType, bool bAckEna, ref uint pRetValue);
            /*=============================================================================
                 NAME: uint SetPowerUpCfg(uint dwDevIndex, uint dwCanSlaveId, P_POWERUP_CONFIG_OBJ pPowerUpCfgObjIn,
                                                                      bool bAckEna, P_POWERUP_CONFIG_OBJ pPowerUpCfgObjOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置驱动器上电配置参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pPowerUpCfgObjIn[in]:上电配置参数
                     bAckEna[in]:是否需要返回ACK信息
                     pPowerUpCfgObjOut[Out]:当前上电配置参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPowerUpCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPowerUpCfg(uint dwDevIndex, uint dwCanSlaveId,
                          ref POWERUP_CONFIG_OBJ pPowerUpCfgObjIn, bool bAckEna, ref POWERUP_CONFIG_OBJ pPowerUpCfgObjOut);
            /*=============================================================================
                 NAME: uint GetPowerUpCfg(uint dwDevIndex, uint dwCanSlaveId, P_POWERUP_CONFIG_OBJ pPowerUpCfgObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取驱动器上电配置参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pPowerUpCfgObj[Out]:当前上电配置参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPowerUpCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPowerUpCfg(uint dwDevIndex, uint dwCanSlaveId, ref uint pPowerUpCfgObj);
            /*=============================================================================
                 NAME: uint SetInterruptCfg(uint dwDevIndex, uint dwCanSlaveId, P__INTERRUPT_CONFIG_OBJ pInterruptCfgObjIn, 
                                            bool bAckEna, P__INTERRUPT_CONFIG_OBJ pInterruptCfgObjOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置驱动器信息反馈开关
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pInterruptCfgObjIn[in]:信息反馈开关
                     bAckEna[in]:是否需要返回ACK信息
                     pInterruptCfgObjOut[Out]:当前信息反馈开关
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetInterruptCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetInterruptCfg(uint dwDevIndex, uint dwCanSlaveId, ref uint pInterruptCfgObjIn,
                                                 bool bAckEna, ref uint pInterruptCfgObjOut);
            /*=============================================================================
                 NAME: uint GetInterruptCfg(uint dwDevIndex, uint dwCanSlaveId, P_INTERRUPT_CONFIG_OBJ pInterruptCfgObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取驱动器信息反馈开关
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pInterruptCfgObj[Out]:当前上电配置参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetInterruptCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetInterruptCfg(uint dwDevIndex, uint dwCanSlaveId, ref uint pInterruptCfgObj);
            /*=============================================================================
                 NAME: uint GetLastErr(uint dwDevIndex, uint dwCanNodeId, uint dwErrorTypeId)
                 ------------------------------------------------------------------------------------------------
                 Function:获取驱动器当前错误信息
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     dwErrorTypeId[in]:错误类型
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetLastErr", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetLastErr(uint dwDevIndex, uint dwCanSlaveId, uint dwErrorTypeId);
            /*=============================================================================
                 NAME: uint SetMotorPara(uint dwDevIndex, uint dwCanSlaveId, P_MOTOR_PARAMETER_OBJ pMotorParaObjIn, 
                                          bool bAckEna, P_MOTOR_PARAMETER_OBJ pMotorParaOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置步进电机参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pMotorParaObjIn[in]:步进电机参数
                     bAckEna[in]:是否需要返回ACK信息
                     pMotorParaOut[Out]:当前步进电机参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetMotorPara", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetMotorPara(uint dwDevIndex, uint dwCanSlaveId, ref MOTOR_PARAMETER_OBJ pMotorParaObjIn,
                                                   bool bAckEna, ref MOTOR_PARAMETER_OBJ pMotorParaOut);
            /*=============================================================================
                 NAME: uint GetMotorPara(uint dwDevIndex, uint dwCanNodeId, P_MOTOR_PARAMETER_OBJ pMotorParaObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取步进电机参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pMotorParaObj[Out]:当前步进电机参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetMotorPara", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetMotorPara(uint dwDevIndex, uint dwCanSlaveId, ref MOTOR_PARAMETER_OBJ pMotorParaObj);
            /*=============================================================================
                 NAME: uint BeginMotion(uint dwDevIndex, uint dwCanSlaveId)
                 ------------------------------------------------------------------------------------------------
                 Function:设置命令开始起作用
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "BeginMotion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int BeginMotion(uint dwDevIndex, uint dwCanSlaveId, bool bAckEna, ref uint pRetValue);
            //       private static extern int BeginMotion(uint dwDevIndex, uint dwCanSlaveId, bool bAckEna, ref uint pRetValue);
            /*=============================================================================
                 NAME: SetMotorOn(uint dwDevIndex, uint dwCanSlaveId, bool bEnable, bool bAckEna, bool &bEnableOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置站点使能状态
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     bEnable[in]:TRUE表示站点使能；FALSE表示站点脱机
                     bAckEna[in]:是否需要返回ACK信息
                     bEnableOut[Out]:当前站点使能状态
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetMotorOn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetMotorOn(uint dwDevIndex, uint dwCanSlaveId, bool bEnable,
                                                 bool bAckEna, ref bool pbEnableOut);
            /*=============================================================================
                 NAME: uint GetMotorOn(uint dwDevIndex, uint dwCanSlaveId, bool &pbEnable)
                 ------------------------------------------------------------------------------------------------
                 Function:获取站点使能状态
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pbEnable[Out]:当前站点使能状态
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetMotorOn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetMotorOn(uint dwDevIndex, uint dwCanSlaveId, ref bool pbEnable);
            /*=============================================================================
                 NAME: uint StopMotion(uint dwDevIndex, uint dwCanSlaveId)
                 ------------------------------------------------------------------------------------------------
                 Function:设置驱动器站点急停
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                        dwDevIndex      [in]网关设备编号  
                       dwCanSlaveId    [in]驱动器站点的节点号
                       bAckEna         [in]是否需要返回ACK信息
                       pRetValue  [Out]:指向存放返回驱动器站点急停设置值的指针
                                    此空间由调用此函数的外部程序分配。
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "StopMotion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int StopMotion(uint dwDevIndex, uint dwCanSlaveId, bool bAckEna, ref uint pRetValue);
            /*=============================================================================
                 NAME: uint SetPVTMotionPara(uint dwDevIndex, uint dwCanSlaveId, P_PVTMOTION_PARAMETER_OBJ pPVTMotionParamObjIn, 
                                  bool bAckEna, P_PVTMOTION_PARAMETER_OBJ pPVTMotionParamObjOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置PVT运动参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pPVTMotionParamObjIn[in]:PVT运动参数
                     bAckEna[in]:是否需要返回ACK信息
                     pPVTMotionParamObjOut[Out]:当前PVT运动参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPVTMotionPara", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPVTMotionPara(uint dwDevIndex, uint dwCanSlaveId, ref PVTMOTION_PARAMETER_OBJ pPVTMotionParamObjIn,
                                                        bool bAckEna, ref PVTMOTION_PARAMETER_OBJ pPVTMotionParamObjOut);
            /*=============================================================================
                 NAME: uint GetPVTMotionPara(uint dwDevIndex, uint dwCanSlaveId, P_PVTMOTION_PARAMETER_OBJ pPVTMotionParamObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取PVT运动参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pPVTMotionParamObj[Out]:当前PVT运动参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPVTMotionPara", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPVTMotionPara(uint dwDevIndex, uint dwCanSlaveId, ref PVTMOTION_PARAMETER_OBJ pPVTMotionParamObj);
            /*=============================================================================
                 NAME: uint SetPVTPoint(uint dwDevIndex, uint dwCanSlaveId, int iIndex, P_PVT_POINT_OBJ pPVTPointObjIn, 
                                                         bool bAckEna, P_PVT_POINT_OBJ pPVTPointObjOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置PVT一个运动点数据
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     INT iIndex[in]:PVT运动点索引号
                     pPVTPointObjIn[in]:PVT运动点数据
                     bAckEna[in]:是否需要返回ACK信息
                     pPVTPointObjOut[Out]:指定PVT运动点当前数据
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
                      iIndex:取值范围为0~2047
                      iQP:取值范围为-  2^31 ~ +2^31
                      iQV:取值范围为-  2^31 ~ +2^31
                      iQT:取值范围为50 ~ 250ms
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPVTPoint", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPVTPoint(uint dwDevIndex, uint dwCanSlaveId, int iIndex, ref PVT_POINT_OBJ pPVTPointObjIn,
                                 bool bAckEna, ref PVT_POINT_OBJ pPVTPointObjOut);
            /*=============================================================================
                 NAME: uint GetPVTPoint(uint dwDevIndex, uint dwCanSlaveId, int iIndex, P_PVT_POINT_OBJ pPVTPointObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取PVT一个运动点数据
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     int iIndex[in]:PVT运动点索引号
                     pPVTPointObj[Out]:指定PVT运动点当前数据
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPVTPoint", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPVTPoint(uint dwDevIndex, uint dwCanSlaveId, int iIndex, ref PVT_POINT_OBJ pPVTPointObj);
            /*=============================================================================
                 NAME: uint SetPVTStartPoint(uint dwDevIndex, uint dwCanSlaveId, uint dwStartPointIndex, 
                                                       bool bAckEna, uint * pdwStartPointIndex)
                 ------------------------------------------------------------------------------------------------
                 Function:设置PVT运动起始点号
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     dwStartPointIndex[in]:PVT运动起始点号
                     bAckEna[in]:是否需要返回ACK信息
                     pdwStartPointIndex[Out]:当前PVT运动PVT运动起始点号
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPVTStartPoint", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPVTStartPoint(uint dwDevIndex, uint dwCanSlaveId, uint dwStartPointIndex,
                                                        bool bAckEna, ref uint pdwStartPointIndex);
            /*=============================================================================
                 NAME: uint GetPVTCurrPoint(uint dwDevIndex, uint dwCanSlaveId, uint * pdwCurrPointIndex)
                 ------------------------------------------------------------------------------------------------
                 Function:获取PVT运动起始点号
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pdwCurrPointIndex[Out]:当前PVT运动起始点号
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPVTCurrPoint", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPVTCurrPoint(uint dwDevIndex, uint dwCanSlaveId, ref uint pdwCurrPointIndex);
            /*=============================================================================
                 NAME: uint SetJogVelosity(uint dwDevIndex, uint dwCanSlaveId, int iJVSpeedIn, 
                                                       bool bAckEna, int * piJVSpeedOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置JogVelosity模式速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iJVSpeedIn[in]:JogVelosity运动速度
                     bAckEna[in]:是否需要返回ACK信息
                     piJVSpeedOut[Out]:当前JogVelosity运动速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetJogVelocity", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetJogVelocity(uint dwDevIndex, uint dwCanSlaveId, int iJVSpeedIn,
                                                         bool bAckEna, ref int piJVSpeedOut);
            /*=============================================================================
                 NAME: uint GetJogVelosity(uint dwDevIndex, uint dwCanSlaveId, int * piJVSpeed)
                 ------------------------------------------------------------------------------------------------
                 Function:获取JogVelosity模式速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piJVSpeed[Out]:当前JogVelosity运动速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetJogVelocity", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetJogVelocity(uint dwDevIndex, uint dwCanSlaveId, ref int piJVSpeed);
            /*=============================================================================
                 NAME: uint SetPTPSpeed(uint dwDevIndex, uint dwCanSlaveId, int iPTPSpeedIn, 
                                                       bool bAckEna, int * piPTPSpeedOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置Point to Point运动速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iPTPSpeedIn[in]:Point to Point运动速度
                     bAckEna[in]:是否需要返回ACK信息
                     piPTPSpeedOut[Out]:当前Point to Point运动速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPTPSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPTPSpeed(uint dwDevIndex, uint dwCanSlaveId, int iPTPSpeedIn,
                                                       bool bAckEna, ref int piPTPSpeedOut);
            /*=============================================================================
                 NAME: uint GetPTPSpeed(uint dwDevIndex, uint dwCanSlaveId, int * piPTPSpeed)
                 ------------------------------------------------------------------------------------------------
                 Function:获取Point to Point运动速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piPTPSpeed[Out]:当前Point to Point运动速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPTPSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPTPSpeed(uint dwDevIndex, uint dwCanSlaveId, ref int piPTPSpeed);
            /*=============================================================================
                 NAME: uint SetPTPRelativePosition(uint dwDevIndex, uint dwCanSlaveId, int iPTPRelPostionIn, 
                                                       bool bAckEna, int * piPTPRelPostionOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置Point to Point运动相对位置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iPTPRelPostionIn[in]:Point to Point运动相对位置
                     bAckEna[in]:是否需要返回ACK信息
                     piPTPRelPostionOut[Out]:当前Point to Point运动相对位置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPTPRelativePosition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPTPRelativePosition(uint dwDevIndex, uint dwCanSlaveId, int iPTPRelPostionIn,
                                                       bool bAckEna, ref int piPTPRelPostionOut);
            /*=============================================================================
                 NAME: uint GetPTPRelativePosition(uint dwDevIndex, uint dwCanSlaveId, int * piPTPRelPostion)
                 ------------------------------------------------------------------------------------------------
                 Function:获取Point to Point运动相对位置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piPTPRelPostion[Out]:当前Point to Point运动相对位置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPTPRelativePosition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPTPRelativePosition(uint dwDevIndex, uint dwCanSlaveId, ref int piPTPRelPostion);
            /*=============================================================================
                 NAME: uint SetPTPAbsolutePosition(uint dwDevIndex, uint dwCanSlaveId, int iPTPAbsPositonIn, 
                                                       bool bAckEna, int * piAbsPositonOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置Point to Point运动绝对位置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iPTPRelPostionIn[in]:Point to Point运动绝对位置
                     bAckEna[in]:是否需要返回ACK信息
                     piPTPRelPostionOut[Out]:当前Point to Point运动绝对位置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetPTPAbsolutePosition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetPTPAbsolutePosition(uint dwDevIndex, uint dwCanSlaveId, int iPTPAbsPositonIn,
                                                      bool bAckEna, ref int piAbsPositonOut);
            /*=============================================================================
                 NAME: uint GetPTPAbsolutePosition(uint dwDevIndex, uint dwCanSlaveId, int * piPTPAbsPositon)
                 ------------------------------------------------------------------------------------------------
                 Function:获取Point to Point运动绝对位置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piPTPAbsPositon[Out]:当前Point to Point运动绝对位置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetPTPAbsolutePosition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetPTPAbsolutePosition(uint dwDevIndex, uint dwCanSlaveId, ref int piPTPAbsPositon);
            /*=============================================================================
                 NAME: uint SetOriginPosition(uint dwDevIndex, uint dwCanSlaveId, int iOriginPositionIn, 
                                                       bool bAckEna, int * piOriginPositionOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置当前位置相对原点的位置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iOriginPositionIn[in]:相对原点的位置
                     bAckEna[in]:是否需要返回ACK信息
                     piOriginPositionOut[Out]:当前相对原点的位置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetOriginPosition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetOriginPosition(uint dwDevIndex, uint dwCanSlaveId, int iOriginPositionIn,
                                                       bool bAckEna, ref int piOriginPositionOut);
            /*=============================================================================
                 NAME: uint SetAcceleration(uint dwDevIndex, uint dwCanSlaveId, int iAccelerationIn, 
                                                       bool bAckEna, int * piAccelerationOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置启动加速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iAccelerationIn[in]:加速度 
                     bAckEna[in]:是否需要返回ACK信息
                     piAccelerationOut[Out]:当前加速度值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetAcceleration", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetAcceleration(uint dwDevIndex, uint dwCanSlaveId, int iAccelerationIn,
                                                        bool bAckEna, ref int piAccelerationOut);
            /*=============================================================================
                 NAME: uint GetAcceleration(uint dwDevIndex, uint dwCanSlaveId, int * piAcceleration)
                 ------------------------------------------------------------------------------------------------
                 Function:获取启动加速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piAcceleration[Out]:当前加速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetAcceleration", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetAcceleration(uint dwDevIndex, uint dwCanSlaveId, ref int piAcceleration);
            /*=============================================================================
                 NAME: uint SetDeceleration(uint dwDevIndex, uint dwCanSlaveId, int iDecelerationIn, 
                                                       bool bAckEna, int * piDecelerationOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置减速加速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iDecelerationIn[in]:加速度 
                     bAckEna[in]:是否需要返回ACK信息
                     piDecelerationOut[Out]:当前加速度值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetDeceleration", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetDeceleration(uint dwDevIndex, uint dwCanSlaveId, int iDecelerationIn,
                                                        bool bAckEna, ref int piDecelerationOut);
            /*=============================================================================
                 NAME: uint GetDeceleration(uint dwDevIndex, uint dwCanSlaveId, int * piDeceleration)
                 ------------------------------------------------------------------------------------------------
                 Function:获取减速加速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piDeceleration[Out]:当前加速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetDeceleration", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetDeceleration(uint dwDevIndex, uint dwCanSlaveId, ref int piDeceleration);
            /*=============================================================================
                 NAME: uint SetStartingSpeed(uint dwDevIndex, uint dwCanSlaveId, int iStartingSpeedIn, 
                                                       bool bAckEna, int * piStartingSpeedOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置启动起始速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iStartingSpeedIn[in]:加速度 
                     bAckEna[in]:是否需要返回ACK信息
                     piStartingSpeedOut[Out]:当前加速度值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetStartingSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetStartingSpeed(uint dwDevIndex, uint dwCanSlaveId, int iStartingSpeedIn,
                                                        bool bAckEna, ref int piStartingSpeedOut);
            /*=============================================================================
                 NAME: uint GetStartingSpeed(uint dwDevIndex, uint dwCanSlaveId, int * piStartingSpeed)
                 ------------------------------------------------------------------------------------------------
                 Function:获取启动起始速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piStartingSpeed[Out]:当前启动起始速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetStartingSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetStartingSpeed(uint dwDevIndex, uint dwCanSlaveId, ref int piStartingSpeed);
            /*=============================================================================
                 NAME: uint SetStopDeceleration(uint dwDevIndex, uint dwCanSlaveId, int iStopDecelerationIn, 
                                                       bool bAckEna, int * piStopDecelerationOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置急停减速加速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iStopDecelerationIn[in]:急停减速加速度
                     bAckEna[in]:是否需要返回ACK信息
                     piStopDecelerationOut[Out]:当前急停减速加速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetStopDeceleration", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetStopDeceleration(uint dwDevIndex, uint dwCanSlaveId, int iStopDecelerationIn,
                                                    bool bAckEna, ref int piStopDecelerationOut);
            /*=============================================================================
                 NAME: uint GetStopDeceleration(uint dwDevIndex, uint dwCanSlaveId, int * piStopDeceleration)
                 ------------------------------------------------------------------------------------------------
                 Function:获取急停减速加速度
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piStopDeceleration[Out]:当前急停减速加速度
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetStopDeceleration", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetStopDeceleration(uint dwDevIndex, uint dwCanSlaveId, ref int piStopDeceleration);
            /*=============================================================================
                 NAME: uint SetSensorActionCfg(uint dwDevIndex, uint dwCanSlaveId, P_SCFG_INFO_OBJ pSensorActionCfgIn, 
                                                           bool bAckEna, P_SCFG_INFO_OBJ pSensorActionCfgOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置传感器触发配置参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     uiScfgValIn[in]:传感器触发动作
                     bAckEna[in]:是否需要返回ACK信息
                     pScfgValOut[Out]:当前传感器触发动作
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetSensorActionCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetSensorActionCfg(uint dwDevIndex, uint dwCanSlaveId, int iSCIndex, uint uiScfgValIn,
                                                          bool bAckEna, ref uint pScfgValOut);
            /*=============================================================================
                 NAME: uint GetSensorActionCfg(uint dwDevIndex, uint dwCanSlaveId, int iSCIndex,  uint* pScfgVal)
                 ------------------------------------------------------------------------------------------------
                 Function:获取传感器触发配置参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pScfgVal[Out]:当前传感器触发动作
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetSensorActionCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetSensorActionCfg(uint dwDevIndex, uint dwCanSlaveId, int iSCIndex, ref uint pScfgVal);

            [DllImport("UIRobotFunc.dll", EntryPoint = "SetSensorTriggerCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetSensorTriggerCfg(uint dwDevIndex, uint dwCanSlaveId, int iSCIndex, uint iStgTimeIn, bool bAckEna, ref int piStgTimeOut);

            [DllImport("UIRobotFunc.dll", EntryPoint = "GetSensorTriggerCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetSensorTriggerCfg(uint dwDevIndex, uint dwCanSlaveId, int iSCIndex, ref int piStgTimeIn);
            /*=============================================================================
                 NAME: uint SetMotionFormatSwitch(uint dwDevIndex, uint dwCanSlaveId, int iMotionFormatSwitchIn, 
                                                       bool bAckEna, int * piMotionFormatSwitchOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置运动情景索引值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iMotionFormatSwitchIn[in]:运动情景索引值
                     bAckEna[in]:是否需要返回ACK信息
                     piMotionFormatSwitchOut[Out]:当前运动情景索引值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
                    在各种情景下，都有相应的运动参数和加减速，
                    通过该函数指定对应的情景		
                    // = 0 : Set Motion Formats under Instructions
                    // = 1 : Set Motion Formats under Stall
                    // = 2 : Set Motion Formats under S1R Action
                    // = 3 : Set Motion Formats under S1F Action
                    // = 4 : Set Motion Formats under S2R Action
                    // = 5 : Set Motion Formats under S2F Action
                    // = 6 : Set Motion Formats under S3R Action
                    // = 7 : Set Motion Formats under S3F Action
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetMotionFormatSwitch", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetMotionFormatSwitch(uint dwDevIndex, uint dwCanSlaveId, int iMotionFormatSwitchIn,
                                                        bool bAckEna, ref int piMotionFormatSwitchOut);
            /*=============================================================================
                 NAME: uint GetMotionFormatSwitch(uint dwDevIndex, uint dwCanSlaveId, int * piMotionFormatSwitch)
                 ------------------------------------------------------------------------------------------------
                 Function:获取传感器触发方式
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piMotionFormatSwitch[Out]:当前运动情景索引值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
                    通过该函数获取的情景索引值
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetMotionFormatSwitch", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetMotionFormatSwitch(uint dwDevIndex, uint dwCanSlaveId, ref int piMotionFormatSwitch);
            /*=============================================================================
                 NAME: uint SetBacklashComp(uint dwDevIndex, uint dwCanSlaveId, int iBacklashCompIn, 
                                                       bool bAckEna, int * piBacklashCompOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置返程间隙补偿值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     iBacklashCompIn[in]:返程间隙补偿值
                     bAckEna[in]:是否需要返回ACK信息
                     piBacklashCompOut[Out]:当前返程间隙补偿值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetBacklashComp", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetBacklashComp(uint dwDevIndex, uint dwCanSlaveId, int iBacklashCompIn,
                                                       bool bAckEna, ref int piBacklashCompOut);
            /*=============================================================================
                 NAME: uint GetBacklashComp(uint dwDevIndex, uint dwCanSlaveId, int * piBacklashComp)
                 ------------------------------------------------------------------------------------------------
                 Function:获取返程间隙补偿值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piBacklashComp[Out]:当前返程间隙补偿值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
                    通过该函数获取的情景索引值
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetBacklashComp", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetBacklashComp(uint dwDevIndex, uint dwCanSlaveId, ref int piBacklashComp);
            /*=============================================================================
                 NAME: uint GetDesiredValues(uint dwDevIndex, uint dwCanSlaveId, P_DESIRED_VALUES_OBJ pDesiredValuesObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取运动期望数值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pDesiredValuesObj[Out]:当前运动期望数值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:		
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetDesiredValues", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetDesiredValues(uint dwDevIndex, uint dwCanSlaveId, int nDesireIndex, ref int pValue);
            /*=============================================================================
                 NAME: uint SetIOconfig(uint dwDevIndex, uint dwCanSlaveId, P_IOCONFIG_OBJ pIOconfigIn, 
                                  bool bAckEna, P_IOCONFIG_OBJ pIOconfigOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置IO端口功能配置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pIOconfigIn[in]:IO端口功能配置
                     bAckEna[in]:是否需要返回ACK信息
                     pIOconfigOut[Out]:当前IO端口功能配置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetIOconfig", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetIOconfig(uint dwDevIndex, uint dwCanSlaveId, int iIOIndex, ref int pIOconfigIn,
                               bool bAckEna, ref int pIOconfigOut);
            /*=============================================================================
                 NAME: uint GetIOconfig(uint dwDevIndex, uint dwCanSlaveId, P_IOCONFIG_OBJ pIOconfig)
                 ------------------------------------------------------------------------------------------------
                 Function:获取IO端口功能配置
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pIOconfig[Out]:当前IO端口功能配置
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetIOconfig", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetIOconfig(uint dwDevIndex, uint dwCanSlaveId, int iIOIndex, ref int pIOconfig);
            /*=============================================================================
                 NAME: uint SetTemperatureLimit(uint dwDevIndex, uint dwCanSlaveId, int * piTempLimitIn, 
                                  bool bAckEna, int * piTempLimitOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置温度报警值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pTempLimitIn[in]:温度报警值
                     bAckEna[in]:是否需要返回ACK信息
                     pTempLimitOut[Out]:当前温度报警值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetTemperatureLimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetTemperatureLimit(uint dwDevIndex, uint dwCanSlaveId, ref int piTempLimitIn,
                                 bool bAckEna, ref int piTempLimitOut);
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetTemperatureLimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetTemperatureLimit(uint dwDevIndex, uint dwCanSlaveId, ref int piTempLimitIn);
            /*=============================================================================
                 NAME: uint GetCurrTemperature(uint dwDevIndex, uint dwCanSlaveId, int * piCurrTemperature)
                 ------------------------------------------------------------------------------------------------
                 Function:获取温度值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     piCurrTemperature[Out]:当前温度值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetCurrTemperature", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetCurrTemperature(uint dwDevIndex, uint dwCanSlaveId, ref int piCurrTemperature);
            /*=============================================================================
                 NAME: uint SetEncoderCfg(uint dwDevIndex, uint dwCanSlaveId, P_ENCODE_CONFIG_OBJ pEncoderCfgObjIn, 
                                                     bool bAckEna, P_ENCODE_CONFIG_OBJ pEncoderCfgObjOut)
                 ------------------------------------------------------------------------------------------------
                 Function:设置编码器配置参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pEncoderCfgObjIn[in]:编码器配置参数
                     bAckEna[in]:是否需要返回ACK信息
                     pEncoderCfgObjOut[Out]:当前编码器配置参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetEncoderCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetEncoderCfg(uint dwDevIndex, uint dwCanSlaveId, ref ENCODE_CONFIG_OBJ pEncoderCfgObjIn,
                                                    bool bAckEna, ref ENCODE_CONFIG_OBJ pEncoderCfgObjOut);
            /*=============================================================================
                 NAME: uint GetEncoderCfg(uint dwDevIndex, uint dwCanSlaveId, P_ENCODE_CONFIG_OBJ pEncoderCfgObj)
                 ------------------------------------------------------------------------------------------------
                 Function:获取编码器配置参数
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pEncoderCfgObj[Out]:当前编码器配置参数
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetEncoderCfg", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetEncoderCfg(uint dwDevIndex, uint dwCanSlaveId, ref ENCODE_CONFIG_OBJ pEncoderCfgObj);
            /*=============================================================================
                 NAME: uint GetModelInfo(uint dwDevIndex, uint dwCanSlaveId, PCHAR pModelInfo)
                 ------------------------------------------------------------------------------------------------
                 Function:获取驱动器型号信息
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pModelInfo[Out]:当前驱动器型号信息
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetModelInfo", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetModelInfo(uint dwDevIndex, uint dwCanSlaveId, ref char pModelInfo);
            /*=============================================================================
                 NAME: uint  GetSerialNumber(uint dwDevIndex, uint dwCanSlaveId, int * pSerialNumber)
                 ------------------------------------------------------------------------------------------------
                 Function:获取驱动器序列号
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pSerialNumber[Out]:当前驱动器序列号
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetSerialNumber", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetSerialNumber(uint dwDevIndex, uint dwCanSlaveId, ref int pSerialNumber);
            /*=============================================================================
                 ------------------------------------------------------------------------------------------------
                 Function:设置IO端口数字输出值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pDigitalValueIn[in]:IO端口数字输出值
                     bAckEna[in]:是否需要返回ACK信息
                     pDigitalValueOut[Out]:当前IO端口数字输出值UIROB
                 ------------------------------------------------------------------------------------------
                 Return Value:	UIROBOTFUNC_API int
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "SetDigitalValue", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int SetDigitalValue(uint dwDevIndex, uint dwCanSlaveId, ref uint pDigitalValueIn,
                                                  bool bAckEna, ref uint pDigitalValueOut);
            /*=============================================================================
                 ------------------------------------------------------------------------------------------------
                 Function:获取IO端口数字输出值
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pDigitalValue[Out]:当前IO端口数字输出值
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetDigitalValue", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetDigitalValue(uint dwDevIndex, uint dwCanSlaveId, ref uint pDigitalValue);
            /*=============================================================================
                 ------------------------------------------------------------------------------------------------
                 Function:获取IO端口输入模拟量
                 -----------------------------------------------------------------------------------------------
                 Parameter:
                     dwDevIndex[in]:网关设备编号  
                     dwCanSlaveId[in]:驱动器站点的节点号
                     pAnalogValue[Out]:当前IO端口输入模拟量
                 ------------------------------------------------------------------------------------------
                 Return Value:	
                                If the function fails, the return value is -1 
                                If the function succeeds, the return value is 1. 
             ------------------------------------------------------------------------------------------
                 Note:
            =============================================================================*/
            [DllImport("UIRobotFunc.dll", EntryPoint = "GetAnalogValue", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetAnalogValue(uint dwDevIndex, uint dwCanSlaveId, int iIOIndex, ref int pAnalogValue);



            /*==============================================================================
                NAME: bool UIMRegRtcnCallBack(PF_SENSOR_NOTIFY_CALLBACK pCallBackFunc)
                ----------------------------------------------------------------------------
                Function:回调函数注册函数
                ----------------------------------------------------------------------------
                Parameter:
                      PF_SENSOR_NOTIFY_CALLBACK pCallBackFunc:回调函数指针
                ----------------------------------------------------------------------------
                Return Value:  
                               If the function fails, the return value is false. 
                               If the function succeeds, the return value is true. 
                ----------------------------------------------------------------------------
                Note:	 
                   用于网关回调函数的注册
            ==============================================================================*/

            [DllImport("UIRobotFunc.dll", EntryPoint = "UIMReg620RtcnCallBack", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool UIMReg620RtcnCallBack(ProcessDelegate pCallBackFunc);
        }

        //读取txt文件中总行数的方法
        public static int readFileLines(String _fileName)
        {
            Stopwatch sw = new Stopwatch();
            var path = _fileName;
            int lines = 0;

            //按行读取
            sw.Restart();
            using (var sr = new StreamReader(path))
            {
                var ls = "";
                while ((ls = sr.ReadLine()) != null)
                {
                    lines++;
                }
            }
            sw.Stop();
            return lines;
        }

        //chart2重置x轴属性方法
        private void SetChartX(double offset)
        {

            chart2.ChartAreas[0].AxisX.Minimum = chartInitiMinSizeX + offsetNumber * offset;
            chart2.ChartAreas[0].AxisX.Maximum = chartInitiMaxSizeX + offsetNumber * offset;
            chart2.ChartAreas[0].AxisX.Interval = chartIntervalX;

            int cnt = chart2.Series[0].Points.Count;
            double tmp = 0;

            for (int i = 0; i < 8; i++)
            {
                tmp = chart2.Series[0].Points[i].XValue + offset;
                chart2.Series[0].Points[i].XValue = tmp;

            }
        }

        //获取脉冲频率，得到实时转速
        private void GetSpd_Click(object sender, EventArgs e)
        {
            if (!IsSiteValidated()) return;
            if (iNodeID != 0)
            {
                int RtnValue = 0;
                int SpdValue = 0;
                int dwExecResult = 0;
                dwExecResult = DllImport.GetJogVelocity(m_uiDevIndex, iNodeID, ref RtnValue);
                if (dwExecResult < 0) //函数执行失败
                {
                    MessageBox.Show("PTP模式查询相对位置失败!");
                    return;
                }
                else
                {
                    SpdValue = RtnValue * 60 / 2000;
                    TextBox_JOGSpd.Text = SpdValue.ToString();
                }
            }
        }

        //时域图绘图
        int p = 0;
        static bool flagisupdate = true;
        Bitmap imgt = new Bitmap(Fs * 24, 2000);
        Pen pen1 = new Pen(Color.Black, 15);
        System.Drawing.Drawing2D.AdjustableArrowCap linecap
                            = new System.Drawing.Drawing2D.AdjustableArrowCap(6, 6, false);

        //报文反馈
        /*private void checkBox_RTCN_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_RTCN.Checked && !IsSiteValidated())
            {
                this.checkBox_RTCN.Checked = false;
                return;
            }
            if (this.checkBox_RTCN.Checked)
            {
                UIMReg620RtcnCallBack(m_delRtcnProcess);
            }
            else
            {
                UIMReg620RtcnCallBack(null);
            }
        }
       
        private void textBox_RTCN_TextChanged(object sender, EventArgs e)
        {

        }
        */
        /*
    void RevRtcnNotify(uint dwGWIndex, UI_MSG_OBJ uiMsgObj)
    {
     string strRTCNType = string.Empty;
     switch (uiMsgObj.subCmd)
     {
         case RTCN_SYS_EMR:
             strRTCNType = string.Format("温度过高");// 系统温度过热
             break;
         case RTCN_DIO_P1L:
             strRTCNType = string.Format("P1L"); // 端口P1检测到下降沿
             break;
         case RTCN_DIO_P1H:
             strRTCNType = string.Format("P1H");  // 端口P1检测到上升沿
             break;
         case RTCN_DIO_P2L:
             strRTCNType = string.Format("P2L"); // 端口P2检测到下降沿
             break;
         case RTCN_DIO_P2H:
             strRTCNType = string.Format("P2H"); // 端口P2检测到上升沿
             break;
         case RTCN_DIO_P3L:
             strRTCNType = string.Format("P3L"); //端口P3检测到下降沿
             break;
         case RTCN_DIO_P3H:
             strRTCNType = string.Format("P3H"); // 端口P3检测到上升沿
             break;
         case RTCN_DIO_P4L:
             strRTCNType = string.Format("P4L"); // 端口P4检测到下降沿
             break;
         case RTCN_DIO_P4H:
             strRTCNType = string.Format("P4H"); // 端口P4检测到上升沿
             break;
         case RTCN_MXN_STP:
             strRTCNType = string.Format("STP"); // PTP 定位完成
             break;
         case RTCN_MXN_ORG:
             strRTCNType = string.Format("ORG"); // 运动到原点通知
             break;
         case RTCN_MXN_STL:
             strRTCNType = string.Format("STL");  // 检测到堵转情况
             break;
         case RTCN_UPG_PRT:
             strRTCNType = string.Format("PRT"); // 用户程序通知
             break;
         case RTCN_MXN_PVW:
             strRTCNType = string.Format("PVT LL OverFlow"); // PVT水位低于报警值
             break;
         case RTCN_MXN_PVS:
             strRTCNType = string.Format("PVT Run Over");    // PVT执行完毕，并已停止  
             break;

         default: break;
     }

     this.BeginInvoke(delegateShowRTCN, strRTCNType);
     return;
    }
    */
        /*
        public delegate void DelegateRTCN(string strRTCN);
        DelegateRTCN delegateShowRTCN;
        public void ShowRTCN(string strRTCN)
         {
             if (textBox_RTCN.Text.Length > 10240)
                 textBox_RTCN.Text = "";
             else
                 textBox_RTCN.Text = strRTCN + "\r\n" + textBox_RTCN.Text;
         }
         */

        /*
        private void button_draw_Click(object sender, EventArgs e)
        {
            
            if (flagisupdate)
            {               
                //Update();
                Invalidate();             
                int t = 0;
                switch (p)
                {
                    case 0:
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < Fs - 1; j++)
                            {
                                Graphics t_acc = Graphics.FromImage(imgt);//图像画布添加绘图
                                //Console.WriteLine(b[i, j ]);
                                t++;
                                t_acc.DrawLine(pen1, t, b[i, j], t + 1, b[i, j + 1]);
                            }
                               //k = b[i, j + 1];
                               //MessageBox.Show(k.ToString); 
                        }
                        
                        pictureBox1.Image = imgt;
                        break;
                    case 1:
                        
                        for (int i = 48; i < 96; i++)
                        {
                            for (int j = 0; j < Fs - 1; j++)
                            {
                                Bitmap imgt = new Bitmap(Fs * 24, 2000);//?
                                Graphics t_acc = Graphics.FromImage(imgt);//图像画布添加绘图
                                Pen pen1 = new Pen(Color.Black, 15);
                                //Console.WriteLine(b[i, j ]);
                                t++;
                                t_acc.DrawLine(pen1, t, b[i, j], t + 1, b[i, j + 1]);
                            }
                            //k = b[i, j + 1];
                            //MessageBox.Show(k.ToString); 
                        }

                        pictureBox1.Image = imgt;
                        break;
                }
            }
            flagisupdate = false;
        }

         private void button_clear_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Refresh();
            imgt.Dispose();
            //Bitmap imgt = new Bitmap(Fs * 24, 2000);
            flagisupdate = true;
            p = 1;
        }


         private void button_update_Click(object sender, EventArgs e)
        {
            Invalidate();
            flagisupdate = true;

        }
        */
        /*
        //matlab作图函数加载
        private void startload_run()
        {
            int count50ms = 0;//实例化matlab对象                    
            int L = readFileLines("G:\\F\\1000.txt");
            System.IO.StreamReader sr = System.IO.File.OpenText(@"G:\F\1000.txt");
            string[] yy = new string[L];
            string[] xx = sr.ReadToEnd().Split(new string[] { "," }, StringSplitOptions.None);

            for (int j = 0; j < L; j++)
            {
                if (xx[j].StartsWith("\r\n"))
                {
                    xx[j] = xx[j].Remove(0, 2);
                }

            }
            
            for (int j = 0; j < L; j++)
            {
                yy[j] = xx[1 + 3 * j];
            }

            float [] aa= new float[L];
            aa = Array.ConvertAll<string, float>(yy, c => float.Parse(c));
            //for (i = 0; i < L; i++)
            //{ aa[i] = int.Parse(zz[i]); }
            //int[] aa = Array.ConvertAll<string, int>(zz, delegate (string s) { return int.Parse(s); });

            //MWArray mwArr = aa;
            MWNumericArray arr = (MWNumericArray)aa;
            //MWNumericArray js = new MWNumericArray(MWArrayComplexity.Real, 1, 2);


            speed_rms.rms te = new speed_rms.rms();
            te.rms1(arr);


            //循环查找figure1窗体
            while (figure1 == IntPtr.Zero)
            {
                //查找matlab的Figure 1窗体
                figure1 = FindWindow("SunAwtFrame", "Figure 1");
                //延时50ms
                Thread.Sleep(50);
                count50ms++;
                //20s超时设置
                if (count50ms >= 400)
                {
                    label1.Text = "matlab资源加载时间过长！";
                    return;
                }
            }
            //跨线程，用委托方式执行
            UpdateUI update = delegate
            {
                //隐藏标签
                label1.Visible = false;
                //设置matlab图像窗体的父窗体为panel
                SetParent(figure1, panel1.Handle);
                //获取窗体原来的风格
                var style = GetWindowLong(figure1, GWL_STYLE);
                //设置新风格，去掉标题,不能通过边框改变尺寸
                SetWindowLong(figure1, GWL_STYLE, style & ~WS_CAPTION & ~WS_THICKFRAME);
                //移动到panel里合适的位置并重绘
                MoveWindow(figure1, 0, 0, panel1.Width + 20, panel1.Height + 40, true);
                //调用显示窗体函数，隐藏再显示相当于刷新一下窗体
                //radiobutton按钮使能               
                //radioButton1.Enabled = true;
            };
            panel1.Invoke(update);
            //再移动一次，防止显示错误
            Thread.Sleep(100);
            MoveWindow(figure1, 0, 0, panel1.Width + 20, panel1.Height + 40, true);

        }
       */
        /*
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            MoveWindow(figure1, 0, 0, panel1.Width + 20, panel1.Height + 40, true);
        }
        */
        /*
#region //Windows API
[DllImport("user32.dll")]
public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);//
[DllImport("user32.dll")]
public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
[DllImport("user32.dll", CharSet = CharSet.Auto)]
public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

const int GWL_STYLE = -16;
const int WS_CAPTION = 0x00C00000;
const int WS_THICKFRAME = 0x00040000;
const int WS_SYSMENU = 0X00080000;
[DllImport("user32")]
private static extern int GetWindowLong(System.IntPtr hwnd, int nIndex);
[DllImport("user32")]
private static extern int SetWindowLong(System.IntPtr hwnd, int index, int newLong);

/// <summary>最大化窗口，最小化窗口，正常大小窗口
/// nCmdShow:0隐藏,3最大化,6最小化，5正常显示
/// </summary>
//[DllImport("user32.dll", EntryPoint = "ShowWindow")]
//public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
#endregion
*/
        /*
        //matlab图像初始化
        public delegate void UpdateUI();//委托用于更新UI 
        Thread startload;//线程用于matlab窗体处理
        //MatlabFunction matlabFunction;//matlab编译的类
        function s;
        IntPtr figure1;//图像句柄
        */

        //显示控件
        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            e.Item.Selected = true;

            if (listView1.CheckedItems.Count == 2)//2个以上才需要判断，事实上最多有2个
            {
                ListViewItem newItem;
                if (e.Item == listView1.CheckedItems[0])//当前项为选中集中第一个，即[0]，则去[1]
                {
                    newItem = listView1.CheckedItems[1];
                    newItem.Checked = false;
                    newItem.Selected = false;
                }
                else
                {
                    newItem = listView1.CheckedItems[0];
                    newItem.Checked = false;
                    newItem.Selected = false;
                }
                e.Item.Selected = true;
                TextBox_SiteID.Text = listView1.Items[e.Item.Index].SubItems[1].Text;
                iNodeID = uint.Parse(listView1.Items[e.Item.Index].SubItems[1].Text);
            }
            else if (listView1.CheckedItems.Count == 1)
            {
                e.Item.Selected = e.Item.Checked;
                TextBox_SiteID.Text = listView1.Items[e.Item.Index].SubItems[1].Text;
                iNodeID = uint.Parse(listView1.Items[e.Item.Index].SubItems[1].Text);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                TextBox_SiteID.Text = listView1.Items[0].SubItems[1].Text;
                iNodeID = uint.Parse(listView1.Items[0].SubItems[1].Text);

            }
        }

        private void TextBox_SiteID_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void TextBox_JOGSpd_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtRec_TextChanged(object sender, EventArgs e)
        {

        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {

            if (listView1.SelectedItems.Count > 0)

            {

                TextBox_SiteID.Text = listView1.Items[0].SubItems[1].Text;

                iNodeID = uint.Parse(listView1.Items[0].SubItems[1].Text);


            }

            MessageBox.Show("ok");
        }

        private void listView1_Click(object sender, EventArgs e)
        {

            if (listView1.SelectedItems.Count > 0)

            {

                TextBox_SiteID.Text = listView1.Items[0].SubItems[1].Text;

                iNodeID = uint.Parse(listView1.Items[0].SubItems[1].Text);

            }
        }


        //网关协议
        string GetAddressIP()
        {
            string AddressIP = "";
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                    ip = _IPAddress;//设定全局的IP
                }
            }
            return AddressIP;
        }

        void Interrupt()
        {
            // textBox1.AppendText("a");
        }

        //TCP Server接收数据线程
        public void TSReceive()
        {
            Thread.Sleep(1000);
            aimSocket = mySocket.Accept();//服务端监听到的Socket为服务端发送数据的目标socket
            byte[] buffer = new byte[3000];//缓冲区设置
            int i = 0;
            int j = 0;

            //  Thread newThread = new Thread(new ParameterizedThreadStart(test));
            while (flagisok)
            {
                try
                {
                    int r = aimSocket.Receive(buffer);//接收到监听的Socket的字节数
                    strRec = Encoding.Default.GetString(buffer, 0, r);//要解码的第一个字节索引、字节数                                                    
                    richTextBox1.AppendText(strRec);

                    int nIndex = strRec.IndexOf("\n");
                    string strData = strRec.Substring(nIndex + 1, 1);
                    signal = (strData[0] == '-') ? (strRec.Substring(nIndex + 1, 6)) : (signal = strRec.Substring(nIndex + 2, 5));

                    if (signal.StartsWith("-"))
                    {
                        String[] tempa = signal.Split('-');
                        if (tempa[1].Length < 4 || !Regex.Match(tempa[1], "^\\d+$").Success)
                        {
                            signal = "00000";
                        }
                    }
                    else
                    {
                        if (!Regex.Match(signal, "^\\d+$").Success)
                        {
                            signal = "00000";
                        }
                    }

                    //处理丢数据
                    //String[] ss = strRec.Split('\r');                   
                    //foreach (string s in ss)
                    //{
                    //    if (s.Trim() != "")
                    //    {
                    //        if (j < Fs)
                    //        {
                    //        a[i, j] = Convert.ToDouble(s);
                    //            if (a[i, j] < 200)
                    //            {
                    //                b[i, j] = b[i, j-1];
                    //            }
                    //            else
                    //            { b[i, j] = (float)a[i, j]; }
                    //        j++;
                    //        }
                    //        else
                    //        {
                    //        j = 0;
                    //        i++;
                    //        a[i, j] = Convert.ToDouble(s);
                    //            if (a[i, j] < 200)
                    //            {
                    //                b[i, j] = b[i, j -1];
                    //            }
                    //            else
                    //            { b[i, j] = (float)a[i, j]; }
                    //        j++;
                    //        }
                    //    }
                    //}
                }
                catch { }
            }
        }

        //搜索、打开、关闭网关
        private void SearchGW_Click(object sender, EventArgs e)
        {
            if (comboBox_Device.Items.Count > 0)
            {
                comboBox_Device.Items.Clear();
            }

            m_dwGWCount = DllImport.SearchGateWay(DllImport.UIGW_ALL, m_gatewayInfoObj, DllImport.MAX_GATEWAY_COUNT);

            if (m_dwGWCount <= 0) // 函数失败或是网关数量为0
            {
                MessageBox.Show("没有找到网关!");
            }
            else
            {
                for (int i = 0; i < m_dwGWCount; i++)
                {
                    gatewayInfoObj[i] = (DllImport.GATEWAY_INFO_OBJ)Marshal.PtrToStructure((IntPtr)(m_gatewayInfoObj.ToInt32() + i * Marshal.SizeOf(typeof(DllImport.GATEWAY_INFO_OBJ))), typeof(DllImport.GATEWAY_INFO_OBJ));
                    comboBox_Device.Items.Add(gatewayInfoObj[i].GWName); // gatewayInfoObj[i].GWName
                }
                comboBox_Device.SelectedIndex = 0;
                SearchGW.Enabled = false;
                OpenGW.Enabled = true;
                CloseGW.Enabled = false;

            }
        }

        private void OpenGW_Click(object sender, EventArgs e)
        {
            if (m_dwGWCount > 0) // 存在网关数量
            {
                if (comboBox_Device.SelectedIndex != -1)
                {
                    if (m_bDevCnectFlg)
                    {
                        //设备已经连接，断开
                        if (false == DllImport.CloseGateWay(m_uiDevIndex))
                        {
                            MessageBox.Show("关闭设备失败!");
                            SearchGW.Enabled = true;
                            OpenGW.Enabled = false;
                            CloseGW.Enabled = false;
                            return;
                        }
                        m_bDevCnectFlg = false;
                        m_uiDevIndex = 0;
                        comboBox_Device.Text = "";
                        comboBox_Device.Items.Clear();
                        listView1.Items.Clear();
                    }
                    m_uiDevIndex = gatewayInfoObj[comboBox_Device.SelectedIndex].dwGWIndex;
                    uint m_dwCanBtr = 0;
                    m_dwNodeCount = DllImport.OpenGateWay(m_uiDevIndex, m_drvInfoObj, DllImport.MAX_NODE_COUNT, ref m_dwCanBtr);
                    if (m_dwNodeCount <= 0) // 函数失败或是站点个数为0
                    {
                        MessageBox.Show("没有找到站点!");
                        SearchGW.Enabled = true;
                        OpenGW.Enabled = false;
                        CloseGW.Enabled = false;
                        return;
                    }
                    else
                    {
                        SearchGW.Enabled = false;
                        OpenGW.Enabled = false;
                        CloseGW.Enabled = true;
                        m_bDevCnectFlg = true;
                        listView1.Items.Clear();
                        DllImport.DRV_INFO_OBJ[] infos = new DllImport.DRV_INFO_OBJ[m_dwNodeCount];
                        // 把站点信息放到列表中
                        for (int i = 0; i < m_dwNodeCount; i++)
                        {
                            ListViewItem item = new ListViewItem();
                            infos[i] = (DllImport.DRV_INFO_OBJ)Marshal.PtrToStructure((IntPtr)(m_drvInfoObj.ToInt32() + i * Marshal.SizeOf(typeof(DllImport.DRV_INFO_OBJ))), typeof(DllImport.DRV_INFO_OBJ));
                            item.SubItems[0].Text = infos[i].uiDrvGroupID.ToString();
                            item.SubItems.Add(infos[i].uiDrvID.ToString());
                            item.SubItems.Add(infos[i].szModelName);
                            item.SubItems.Add(string.Format("{0:d}", (int)infos[i].uiFirewareVersion));
                            listView1.Items.Add(item);
                        }

                    }

                }
            }
        }

        private void CloseGW_Click(object sender, EventArgs e)
        {
            if (m_bDevCnectFlg)
            {
                //设备已经连接，断开
                if (false == DllImport.CloseGateWay(m_uiDevIndex))
                {
                    MessageBox.Show("关闭设备失败!");
                    SearchGW.Enabled = true;
                    OpenGW.Enabled = false;
                    CloseGW.Enabled = false;
                    return;
                }
                m_bDevCnectFlg = false;
                SearchGW.Enabled = true;
                OpenGW.Enabled = false;
                CloseGW.Enabled = false;

                m_uiDevIndex = 0;
                comboBox_Device.Text = "";
                comboBox_Device.Items.Clear();
                listView1.Items.Clear();
            }
        }

        //使能、脱机
        private void setEnable_Click(object sender, EventArgs e)
        {
            if (!IsSiteValidated()) return;
            if (iNodeID != 0)
            {
                bool bEnaOut = false;
                bool bEna = true;
                int dwExecResult = 0;
                dwExecResult = DllImport.SetMotorOn(m_uiDevIndex, iNodeID, bEna, true, ref bEnaOut);
                if (dwExecResult < 0) //函数执行失败
                {
                    MessageBox.Show("电机使能失败!");
                    return;
                }
            }
        }

        private void setOff_Click(object sender, EventArgs e)
        {
            if (!IsSiteValidated()) return;
            if (iNodeID != 0)
            {
                bool bEnaOut = false;
                bool bEna = false;
                int dwExecResult = 0;
                dwExecResult = DllImport.SetMotorOn(m_uiDevIndex, iNodeID, bEna, true, ref bEnaOut);
                if (dwExecResult < 0) //函数执行失败
                {
                    MessageBox.Show("电机脱机失败!");
                    return;
                }
            }
        }

        //开始、停止
        private void setBegin_Click(object sender, EventArgs e)
        {
            con1 = 1;
            con2 = true;

            if (!IsSiteValidated()) return;
            if (iNodeID != 0)
            {
                uint bEnaOut = 0;
                int dwExecResult = 0;
                dwExecResult = DllImport.BeginMotion(m_uiDevIndex, iNodeID, true, ref bEnaOut);
                if (dwExecResult < 0) //函数执行失败
                {
                    MessageBox.Show("启动出错!");
                    return;
                }
            }
            if (!IsSending)
            {
                tmr.Start();
                IsSending = true;
            }

            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));//绑定端口号
            mySocket.Bind(point);//绑定监听端口
            // MessageBox.Show("TCP Server绑定成功");
            mySocket.Listen(1);//等待连接是一个阻塞过程，创建线程来监听
            Thread thReceive = new Thread(TSReceive);
            thReceive.IsBackground = true;
            thReceive.Start();

            chart2.Series[0].Points.Clear();
            InitChart();
            timer1.Start();

            indexTime = 0;
            offsetNumber = 0;
            count = 0;
        }

        private void StopMotor_Click(object sender, EventArgs e)
        {
            con1 = 0;
            con2 = false;

            if (!IsSiteValidated()) return;
            if (iNodeID != 0)
            {

                uint RtnValue = 0;
                int dwExecResult = 0;
                dwExecResult = DllImport.StopMotion(m_uiDevIndex, iNodeID, true, ref RtnValue);
                if (dwExecResult < 0) //函数执行失败
                {
                    MessageBox.Show("停止运行失败!");
                    return;
                }
            }

            tmr.Stop();
            IsSending = false;

            try
            {
                aimSocket.Close();
            }
            catch
            {
            }


        }

        //自动测试
        private void AutoSpd_Click_1(object sender, EventArgs e)
        {
            Thread t1 = new Thread(new ThreadStart(AutoTest));  //无参数的委托
            timer2.Start();
            if (con2 == true)
            {
                t1.Start();
            }
            else
            {
                //MessageBox.Show("yes");
                t1.Abort();
                t1.Join();
            }
        }

        
        public void AutoTest()
        {
            int RtnValue = 0;
            uint RtnValue1 = 0;
            uint bEnaout = 0;

            uint dwDevIndex = gatewayInfoObj[comboBox_Device.SelectedIndex].dwGWIndex;           
            int iAccelerationIn=700;
            bool bAckEna;
            int piAccelerationOut=0;
            //int nStepSPD = 100;    //转速步长
            int nCurrSPD = 2000 * 100 / 60;  //100rpm对应的脉冲频率
            int TestTime = 5000;     //测试时间
            int n = 100;            //转速
            int ts = 20000;
            bool flag = true;
            try
            {
                while (con1 == 1)
                {
                    if (n < 2000 || n == 2000)
                    {
                        DllImport.SetJogVelocity(m_uiDevIndex, iNodeID, nCurrSPD, false, ref RtnValue);                       
                        DllImport.SetAcceleration(dwDevIndex, iNodeID, iAccelerationIn, false, ref piAccelerationOut);
                        DllImport.BeginMotion(m_uiDevIndex, iNodeID, false, ref bEnaout);
                        //ts = 120000 / n * 30  + TestTime;//转2圈时间+测试时间
                        n += 100;
                        System.Threading.Thread.Sleep(ts);
                    }
                    if (n > 2000)
                    {
                        //nCurrSPD = 3200 * 200 / 60;
                        //DllImport.SetJogVelocity(m_uiDevIndex, iNodeID, nCurrSPD, true, ref RtnValue);
                        //DllImport.BeginMotion(m_uiDevIndex, iNodeID, true, ref bEnaout);
                        //Thread.Sleep(5000);
                        for (int i = 0; i < 9; i++)
                        {
                            nCurrSPD -= 2000 * 200 / 60;
                            DllImport.SetDeceleration(dwDevIndex, iNodeID, iAccelerationIn, false, ref piAccelerationOut);
                            DllImport.SetJogVelocity(m_uiDevIndex, iNodeID, nCurrSPD, true, ref RtnValue);
                            DllImport.BeginMotion(m_uiDevIndex, iNodeID, true, ref bEnaout);
                            Thread.Sleep(2000);
                        }
                        DllImport.StopMotion(m_uiDevIndex, iNodeID, true, ref RtnValue1);
                        Thread.Sleep(1000);
                        flag = false;
                        break;
                    }
                    else
                    {
                        nCurrSPD = n * 2000 / 60;//不同转速对应的脉冲频率                     
                    }

                }

                ExportChart("AutoTestPic", chart1);

                // create and save pdf document
                Document document = new Document();
                document.Open();
                string chinese = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "KAIU.TTF");
                BaseFont baseFont = BaseFont.CreateFont(chinese, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font cn = new iTextSharp.text.Font(baseFont, 12, 0);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(@"D:\temp.pdf", FileMode.Create));
                document.Add(new Paragraph("AutoTestPic", cn));
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("G:/研究生/实验数据/Data_autotest/AutoTestPic.jpeg");
                //string fileName = "AutoTestChartImage.pdf";           
                writer.DirectContent.AddImage(img);
                document.Close();




                //if (!flag)
                //{
                //    //MessageBox.Show("反转");
                //    int m = -200;
                //    ts = 0;
                //    nCurrSPD = 20000 / 3*(-1);
                //    while (con1 == 1 )
                //    {
                //        DllImport.SetJogVelocity(m_uiDevIndex, iNodeID, nCurrSPD, false, ref RtnValue);
                //        DllImport.BeginMotion(m_uiDevIndex, iNodeID, true, ref bEnaout);
                //        //ts = 120000 / (-m) * 30 + TestTime;//转2圈时间+测试时间
                //        ts = 15000;
                //        System.Threading.Thread.Sleep(ts);
                //        m -= 100;
                //        //MessageBox.Show(m.ToString());
                //        if (Math.Abs(m) > 2000)
                //        {
                //            DllImport.StopMotion(m_uiDevIndex, iNodeID, true, ref RtnValue1);
                //            break;
                //        }
                //        else
                //        {
                //            nCurrSPD = m * 2000 / 60;
                //        }                      

                //    }
                //}
            }
            catch (Exception e)
            { }
        }

        // datagridview显示转速,加速度有效值  
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, dataGridView1.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
            dataGridView1.RowHeadersDefaultCellStyle.Font, rectangle,
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        //计时器
        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            indexTime = count * 2 / 50;
            signal_y = Convert.ToDouble(signal) / 10000;

            if (indexTime + 100 > (chartInitiMaxSizeX + offsetNumber * 100))//
            {
                SetChartX(100);
                offsetNumber++;
            }

            chart2.Series[0].Points.AddXY(indexTime, signal_y);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            richTextBox1.Rtf = "";
            //this.richTextBox1.Document.Blocks.Clear();               
            timer3.Start();

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            //if (this.richTextBox1.Text == "")
            //{
            //MessageBox.Show("ok");
            //    return;
            //}
            //StreamWriter datafile = new StreamWriter(foldPath+Convert.ToString(textBox1)+".txt");
            //Stream outStream = new FileStream(foldPath + textBox1.Text + ".txt", FileMode.Append);
            //StreamWriter datafile = new StreamWriter(outStream, Encoding.UTF8);
            //datafile.Write(richTextBox1.ToString());
            // foreach (string line in richTextBox1.Lines)
            //{
            //     datafile.WriteLine(line);
            // }
            //datafile.Flush();
            //datafile.Close();

            //把text的内容转换成数组。
            // System.IO.MemoryStream mstream = new System.IO.MemoryStream();
            //this.richTextBox1.SaveFile(mstream, RichTextBoxStreamType.RichText);   //将流转换成数组

            if (speed >= 2000)
            {
                return;
            }
            Thread.Sleep(2000);
            String stra = richTextBox1.Text;
            String[] strb = stra.Split('\n');
            speed += 100;
            String strtext = Convert.ToString(speed);
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"G:\研究生\实验数据\Data_autotest\" + strtext + ".txt", true);
            foreach (string line in strb)
            {
                file.WriteLine(line);// 直接追加文件末尾，换行 
            }

            string str = richTextBox1.Text.ToString();
            char[] szSplit = { '\r', '\n' };
            string[] strData1 = str.Split(szSplit);
            double sum = 0.0;
            int k = 0;
            for (int i = 0; i < strData1.Length; i++)
            {
                if (strData1[i] == " " || strData1[i] == "" || strData1[i].Length > 7 || strData1[i] == "-")
                    continue;
                sum += Math.Pow(Convert.ToDouble(strData1[i]) / 10000.0, 2);
                k++;
            }
            offset_x++;

            double index = offset_x * 100;
            double[] rms = new double[19];
            rms[j] = System.Math.Sqrt(sum / k);
            chart1.Series[0].Points.AddXY(index, System.Math.Sqrt(sum / k));

            string[] Rms = new string[19];
            Rms[j] = rms[j].ToString("0.0000");
            dataGridView1.Rows[j].Cells[1].Value = Rms[j];
            j++;
            timer3.Stop();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            uint dwDevIndex = gatewayInfoObj[comboBox_Device.SelectedIndex].dwGWIndex;
            int iDecelerationIn = 700;
            bool bAckEna;
            int piDecelerationOut = 0;

            int RtnValue = 0;
            uint RtnValue1 = 0;
            uint bEnaout = 0;
            int n = 0;
            n = int.Parse(TextBox_JOGSpd.Text);
            double k = Math.Truncate((double)n / 100);
            int nCurrSPD = 2000 * n / 60;  //n rpm对应的脉冲频率
            for (int i = 0; i < k; i++)
            {
                nCurrSPD -= 2000 * 100 / 60;
                DllImport.SetJogVelocity(m_uiDevIndex, iNodeID, nCurrSPD, true, ref RtnValue);
                DllImport.SetDeceleration(dwDevIndex, iNodeID, iDecelerationIn, false, ref piDecelerationOut);
                DllImport.BeginMotion(m_uiDevIndex, iNodeID, true, ref bEnaout);
                Thread.Sleep(1000);
            }
            DllImport.StopMotion(m_uiDevIndex, iNodeID, true, ref RtnValue1);
            con1 = 0;
            con2 = false;
        }

        public void ExportChart(string fileName, Chart chart1)
        {
            string GR_Path = @"G:\研究生\实验数据\Data_autotest";
            string fullFileName = GR_Path + "\\" + fileName + ".png";
            chart1.SaveImage(fullFileName, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
        }

       
       
    }
}
