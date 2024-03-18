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

        // Register read
        //private const string REG_PLC_Read_STATUS = "D20";
        //private const string REG_PLC_Read_ALARM_MESSAGE = "D2400";
        //private const int ALARM_MESSAGE_LEGHT = 40;
        private const string REG_PLC_RESET = "M2002";

        private const string REG_TRIGGER_CAM_1 = "D2600";
        private const string REG_TRIGGER_CAM_2 = "D2601";
        private const string REG_TRIGGER_CAM_3 = "D2602";
        private const string REG_TRIGGER_CAM_4 = "D2603";

        // Register Write
        private const string REG_PLC_Write = "D";
        private const int REG_PLC_Start = 900;
        private const string REG_Vision_Bussy = "M420";

        //machine status
        //private const int bit_Start = 0;
        //private const int bit_Stop = 1;
        //private const int bit_Alarm = 2;

        //private bool isStart { get; set; } = false;
        //private bool isStop { get; set; } = false;
        //private bool isAlarm { get; set; } = false;
        //private bool isEMG { get; set; } = false;
        //private bool isDisconnected { get; set; } = false;

        //public string AlarmMessage
        //{
        //    get
        //    {
        //        int[] data = new int[ALARM_MESSAGE_LEGHT];
        //        _plc.ReadDeviceBlock(REG_PLC_Read_ALARM_MESSAGE, ALARM_MESSAGE_LEGHT, out data[0]);
        //        string str = "";
        //        for (int i = 0; i < data.Length; i++)
        //        {
        //            Byte[] buf = BitConverter.GetBytes(data[i]);
        //            str += System.Text.Encoding.ASCII.GetString(buf);
        //        }
        //        str = Regex.Replace(str, "[^A-Za-z0-9 ]", ""); ;
        //        return str;
        //    }
        //}

        private bool isStart = false;
        private bool isEMG = false;
        private bool isStop = false;
        private bool isAlarm = false;

        public EventHandler PLCPushStart;
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

                //Thread thread2 = new Thread(ReadEventResetFromPLC);
                //thread2.IsBackground = true;
                //thread2.Name = "ReadEventResetFromPLC";
                //thread2.Start();



                //_logger.Info("Connect successfully PLC!");
                //Console.WriteLine("Connect successfully PLC!");

                //Thread read data status PLC from register
                //Thread thread = new Thread(ReadDataFromRegister);
                //thread.IsBackground = true;
                //thread.Name = "REG_PLC_STATUS";
                //thread.Start();
            }
            else
            {
                _logger.Error("PLC has been connected!");
                Console.WriteLine("PLC has been connected!");
            }
        }

        bool isStartHistory = false;

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

            //while (!isExist)
            //{
            //    int valueReaded = 0;
            //    int resetPLC = 0;

            //    int triggerCam1 = 0;
            //    int triggerCam2 = 0;
            //    int triggerCam3 = 0;
            //    int triggerCam4 = 0;

            //    _plc.ReadDeviceBlock(REG_PLC_Read_STATUS, 1, out valueReaded);
            //    _plc.ReadDeviceBlock(REG_PLC_RESET, 1, out resetPLC);

            //    //read trigger cam 1
            //    _plc.ReadDeviceBlock(REG_TRIGGER_CAM_1, 1, out triggerCam1);

            //    //read trigger cam 2
            //    _plc.ReadDeviceBlock(REG_TRIGGER_CAM_2, 1, out triggerCam2);

            //    //read trigger cam 3
            //    _plc.ReadDeviceBlock(REG_TRIGGER_CAM_3, 1, out triggerCam3);

            //    //read trigger cam 4
            //    _plc.ReadDeviceBlock(REG_TRIGGER_CAM_4, 1, out triggerCam4);

            //    SetStatusOfMachine(valueReaded);

            //    Global.valuePLC = isDisconnected ? 4 : valueReaded;
            //    Global.resetPLC = resetPLC;

            //    Thread.Sleep(timeSleep);
            //}
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
                    Console.WriteLine("start restart here");
                    Global.resetPLC1 = 1;
                    Global.resetPLC2 = 1;
                    Global.resetPLC3 = 1;
                    Global.resetPLC4 = 1;
                    Global.resetClient = 1;
                    isStartHistory = true;
                    if (PLCPushStart != null) PLCPushStart(this, EventArgs.Empty);
                } else
                {
                    Global.resetClient = 0;
                    Console.WriteLine("Ko start restart here");
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

            //isAlarm = isStart = isStop = isEMG = isDisconnected =  false;

            //switch (binary)
            //{
            //    case 0:
            //        isEMG = true;
            //        break;
            //    case 1:
            //        isStart = true;
            //        break;
            //    case 2:
            //        isStop = true;
            //        break;
            //    case 3:
            //        isAlarm = true;
            //        break;
            //    default:
            //        isDisconnected = true;
            //        break;
            //}

            //Console.WriteLine(string.Format("EMG: {0} - Start: {1} - Stop: {2} - Alarm: {3} - Default: {4}", isEMG, isStart, isStop, isAlarm, isDisconnected));
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
