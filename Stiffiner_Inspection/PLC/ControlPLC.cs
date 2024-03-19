using ActUtlType64Lib;
using log4net;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Stiffiner_Inspection
{
    public class ControlPLC
    {
        public EventHandler? PLCEvent;

        private ActUtlType64 _plc = new ActUtlType64();
        private const int _plcStation = 1;
        private bool isExist = false;
        private const int timeSleep = 100;
        private readonly ILog _logger = LogManager.GetLogger(typeof(ControlPLC));

        // Register read
        private const string REG_PLC_Read_STATUS = "D20";
        private const string REG_PLC_RefeshData = "M2010";
        private const string REG_PLC_EndInspection = "M2008";

        private const string REG_TRIGGER_CAM_1 = "D2600";
        private const string REG_TRIGGER_CAM_2 = "D2601";
        private const string REG_TRIGGER_CAM_3 = "D2602";
        private const string REG_TRIGGER_CAM_4 = "D2603";

        // Register Write
        private const string REG_PLC_Write = "D";
        private const int REG_PLC_Start = 900;
        private const string REG_Vision_Bussy = "M420";

        private bool isStart = false;
        private bool isEMG = false;
        private bool isStop = false;
        private bool isAlarm = false;

        public EventHandler PLCPushStart;
        public EventHandler PLCEndIns;

        public bool IsStart { get => isStart; set => isStart = value; }
        public bool IsStop { get => isStop; set => isStop = value; }
        public bool IsAlarm { get => isAlarm; set => isAlarm = value; }
        public bool IsEMG { get => isEMG; set => isEMG = value; }

        public ControlPLC()
        {
            _plc.ActLogicalStationNumber = _plcStation;
        }

        public void Connect()
        {
            if (_plc.Open() == 0)
            {
                Thread thread = new Thread(ReadDataFromRegister);
                thread.IsBackground = true;
                thread.Name = "REG_PLC_STATUS";
                thread.Start();

                Thread thread1 = new Thread(ReadEventStartFromPLC);
                thread1.IsBackground = true;
                thread1.Name = "ReadEventStartFromPLC";
                thread1.Start();
            }
            else
            {
                _logger.Error("Can not connect to PLC");
                Console.WriteLine("Can not connect to PLC");
            }
        }

        bool isStartHistory = false;
        bool isEndHistory = false;

        private void ReadDataFromRegister()
        {
            while (!isExist)
            {
                int valueReaded = 0;
                _plc.ReadDeviceBlock(REG_PLC_Read_STATUS, 1, out valueReaded);
                // string binary = Convert.ToString(valueReaded, 2).PadLeft(16, '0');
                SetStatusOfMachine(valueReaded);
                Global.valuePLC = valueReaded;
                Thread.Sleep(timeSleep);
            }
        }

        private void ReadEventStartFromPLC()
        {
            while (!isExist)
            {
                int valueReaded = 0;
                _plc.GetDevice(REG_PLC_RefeshData, out valueReaded);

                if (valueReaded == 0) isStartHistory = false;
                //kiem tra neu start nhan thi gui cho clent tin hieu star de clear tray
                if (!isStartHistory && valueReaded == 1)
                {
                    Global.resetPLC1 = 1;
                    Global.resetPLC2 = 1;
                    Global.resetPLC3 = 1;
                    Global.resetPLC4 = 1;
                    Global.resetClient = 1;
                    Global.toggleLight = true;

                    isStartHistory = true;                    
                    if (PLCPushStart != null) PLCPushStart(this, EventArgs.Empty);
                } else
                {
                    Global.resetClient = 0;
                }

                // Check End Insection signal
                valueReaded = 0;
                _plc.GetDevice(REG_PLC_EndInspection, out valueReaded);
                if (valueReaded == 0) isEndHistory = false;
                //kiem tra neu start nhan thi gui cho clent tin hieu star de clear tray
                if (!isEndHistory && valueReaded == 1)
                {
                    Console.WriteLine("wf-test-Stop-plc");
                    isEndHistory = true;
                    Global.toggleLight = false;
                    if (PLCEndIns != null) PLCEndIns(this, EventArgs.Empty);
                }

                Thread.Sleep(timeSleep);
            }
        }

        private void SetStatusOfMachine(int binary)
        {
            isAlarm = isStart = isStop = isEMG = false;

            switch (binary)
            {
                case 0:
                    isEMG = true;
                    break;
                case 1:
                    isStart = true;
                    break;
                case 2:
                    isStop = true;
                    break;
                case 3:
                    isAlarm = true;
                    break;
                default:
                    break;
            }
        }

        public void WriteDataToRegister(int data, int index)
        {
            _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, data);
        }

        private string GetWriteRegisterByIndex(int index)
        {
            return string.Format("{0}{1}", REG_PLC_Write, REG_PLC_Start + index);
        }

        //public void WriteDataToRegister(int data, int index)
        //{
        //    _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, data);
        //}
        //public void WriteSampleStatusByIndex(eRunStatus sampleStatus, int index)
        //{
        //    ClassCommon.Common.SaveLogString(eSAVING_LOG_TYPE.PLC, string.Format("Write data to Register: {0} is {1}", GetWriteRegisterByIndex(index), (int)sampleStatus));
        //    _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, (int)sampleStatus);
        //}

        //private string GetWriteRegisterByIndex(int index)
        //{
        //    return string.Format("{0}{1}", REG_PLC_Write, REG_PLC_Start + index);
        //}

        //private bool IsCheckBit(char value)
        //{
        //    return value == '1' ? true : false;
        //}

        //public void VisionBusy(bool status)
        //{
        //    //busy = 1, ready 0
        //    int data = status ? 0 : 1;
        //    _plc.SetDevice(REG_Vision_Bussy, data);
        //}
    }
}
