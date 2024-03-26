using ActUtlType64Lib;
using log4net;
using System.IO.Ports;

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

        // Register Write
        private const string REG_PLC_Write = "D";
        private const int REG_PLC_Start = 900;
        private const string REG_Vision_Bussy = "M240";
        private const string REG_ENOUGH_QUANTITY= "M250"; //để tạm thời m250

        private bool isStart = false;
        private bool isEMG = false;
        private bool isStop = false;
        private bool isAlarm = false;

        public bool IsStart { get => isStart; set => isStart = value; }
        public bool IsStop { get => isStop; set => isStop = value; }
        public bool IsAlarm { get => isAlarm; set => isAlarm = value; }
        public bool IsEMG { get => isEMG; set => isEMG = value; }

        bool isStartHistory = false;
        bool isEndHistory = false;

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

        private void ReadDataFromRegister()
        {
            while (!isExist)
            {
                int valueReaded = 0;
                _plc.ReadDeviceBlock(REG_PLC_Read_STATUS, 1, out valueReaded);
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

                    Global.currentTrayLeft.Clear();
                    Global.currentTrayRight.Clear();
                    Global.currentTray++;

                    Global.fileNameCSV = "MAY_1_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_Stiffiner.csv";

                    TurnOnLightControl();
                    isStartHistory = true;
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
                    isEndHistory = true;
                    TurnOffLightControl();
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

        public void TurnOnLightControl()
        {
            SerialPort lightControl1 = new SerialPort("COM4", 115200);
            SerialPort lightControl2 = new SerialPort("COM5", 115200);

            lightControl1.Open();
            lightControl2.Open();

            lightControl1.WriteLine("@SI00/255/255/255/255");
            lightControl2.WriteLine("@SI00/255/255/255/255");

            lightControl1.Close();
            lightControl2.Close();
        }

        public void TurnOffLightControl()
        {
            SerialPort lightControl1 = new SerialPort("COM4", 115200);
            SerialPort lightControl2 = new SerialPort("COM5", 115200);

            lightControl1.Open();
            lightControl2.Open();

            lightControl1.WriteLine("@SI00/0/0/0/0");
            lightControl2.WriteLine("@SI00/0/0/0/0");

            lightControl1.Close();
            lightControl2.Close();
        }

        private bool IsCheckBit(char value)
        {
            return value == '1' ? true : false;
        }
        public void VisionBusy(bool status)
        {
            //busy = 1, ready 0
            int data = status ? 0 : 1;
            _plc.SetDevice(REG_Vision_Bussy, data);
        }

        public void AlertEnoughQuantity(bool status)
        {
            //enough = 1, ready 0
            _plc.SetDevice(REG_ENOUGH_QUANTITY, status ? 0 : 1);
        }

        //public void WriteSampleStatusByIndex(eRunStatus sampleStatus, int index)
        //{
        //    ClassCommon.Common.SaveLogString(eSAVING_LOG_TYPE.PLC, string.Format("Write data to Register: {0} is {1}", GetWriteRegisterByIndex(index), (int)sampleStatus));
        //    _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, (int)sampleStatus);
        //}
    }
}
