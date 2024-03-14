using ActUtlType64Lib;
using log4net;
using System.Text.RegularExpressions;

namespace Stiffiner_Inspection
{
    public class ControlPLC
    {
        public EventHandler? PLCEvent;

        private ActUtlType64 _plc = new ActUtlType64();
        private const int _plcStation = 1;
        private bool isExist = false;
        private const int timeSleep = 1000;
        private readonly ILog _logger = LogManager.GetLogger(typeof(ControlPLC));

        // Register read
        private const string REG_PLC_Read_STATUS = "D20";
        private const string REG_PLC_Read_ALARM_MESSAGE = "D2400";
        private const int ALARM_MESSAGE_LEGHT = 40;
        private const string REG_PLC_READ_NEW_TRAY = "M2002";

        // Register Write
        private const string REG_PLC_Write = "D";
        private const int REG_PLC_Start = 900;
        private const string REG_Vision_Bussy = "M420";

        //machine status
        private const int bit_Start = 0;
        private const int bit_Stop = 1;
        private const int bit_Alarm = 2;

        private bool isStart { get; set; } = false;
        private bool isStop { get; set; } = false;
        private bool isAlarm { get; set; } = false;
        private bool isEMG { get; set; } = false;
        private bool isDisconnected { get; set; } = false;

        public string AlarmMessage
        {
            get
            {
                int[] data = new int[ALARM_MESSAGE_LEGHT];
                _plc.ReadDeviceBlock(REG_PLC_Read_ALARM_MESSAGE, ALARM_MESSAGE_LEGHT, out data[0]);
                string str = "";
                for (int i = 0; i < data.Length; i++)
                {
                    Byte[] buf = BitConverter.GetBytes(data[i]);
                    str += System.Text.Encoding.ASCII.GetString(buf);
                }
                str = Regex.Replace(str, "[^A-Za-z0-9 ]", ""); ;
                return str;
            }
        }

        public ControlPLC()
        {
            _plc.ActLogicalStationNumber = _plcStation;
        }

        public void Connect()
        {
            if (_plc.Open() == 0)
            {
                _logger.Info("Connect successfully PLC!");
                Console.WriteLine("Connect successfully PLC!");

                //Thread read data status PLC from register
                Thread thread = new Thread(ReadDataFromRegister);
                thread.IsBackground = true;
                thread.Name = "REG_PLC_STATUS";
                thread.Start();
            }
            else
            {
                _logger.Error("PLC has been connected!");
                Console.WriteLine("PLC has been connected!");
            }
        }

        private void ReadDataFromRegister()
        {
            while (!isExist)
            {
                int valueReaded = 0;
                int valueReset = 0;
                _plc.ReadDeviceBlock(REG_PLC_Read_STATUS, 1, out valueReaded);
                _plc.ReadDeviceBlock(REG_PLC_READ_NEW_TRAY, 1, out valueReset);
                SetStatusOfMachine(valueReaded);

                Global.valuePLC = isDisconnected ? 4 : valueReaded;
                Global.plcReset = valueReset;

                Thread.Sleep(timeSleep);
            }
        }

        private void SetStatusOfMachine(int binary)
        {
            isAlarm = isStart = isStop = isEMG = isDisconnected =  false;

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
                    isDisconnected = true;
                    break;
            }

            Console.WriteLine(string.Format("EMG: {0} - Start: {1} - Stop: {2} - Alarm: {3} - Default: {4}", isEMG, isStart, isStop, isAlarm, isDisconnected));
        }

        public void WriteDataToRegister(int data, int index)
        {
            _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, data);
        }

        private string GetWriteRegisterByIndex(int index)
        {
            return string.Format("{0}{1}", REG_PLC_Write, REG_PLC_Start + index);
        }
    }
}
