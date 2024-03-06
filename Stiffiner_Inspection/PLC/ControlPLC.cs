using ActUtlTypeLib;
using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stiffiner_Inspection
{
    public class ControlPLC
    {
        public EventHandler PLCEvent;
        private ActUtlType _plc = new ActUtlType();
        private const int _plcStation = 1;
        private bool isExist = false;
        private const int timeSleep = 1000;
        // 

        // Register read
        private const string REG_PLC_Read_STATUS = "D1999";
        private const string REG_PLC_Read_ALARM_MESSAGE = "D2400";
        private const int ALARM_MESSAGE_LEGHT = 40;
        // Register Write
        private const string REG_PLC_Write = "D";
        private const int REG_PLC_Start = 2000;
        private const string REG_Vision_Bussy = "D2009";


        //machine status
        private const int bit_Start = 0;
        private const int bit_Stop = 1;
        private const int bit_Alarm = 2;
        private bool isStart = false;
        private bool isStop = false;
        private bool isAlarm = false;

        public bool IsStart { get => isStart; set => isStart = value; }
        public bool IsStop { get => isStop; set => isStop = value; }
        public bool IsAlarm { get => isAlarm; set => isAlarm = value; }
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
                Console.WriteLine("Connected to PLC at station: " + _plcStation);
                Thread thread = new Thread(ReadDataFromRegister);
                thread.IsBackground = true;
                thread.Name = "REG_PLC_STATUS";
                thread.Start();
            }
            else
            {
                Console.WriteLine("Can't connected to PLC at station: " + _plcStation);
            }
        }

        private void ReadDataFromRegister()
        {
            while (!isExist)
            {
                int valueReaded = 0;
                _plc.ReadDeviceBlock(REG_PLC_Read_STATUS, 1, out valueReaded);
                string binary = Convert.ToString(valueReaded, 2).PadLeft(16, '0');
                SetStatusOfMachine(ReverseString(binary));
                Thread.Sleep(timeSleep);
            }
        }
        private string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        private void SetStatusOfMachine(string binary)
        {
            isStart = IsCheckBit(binary.ToCharArray()[bit_Start]);
            isStop = IsCheckBit(binary.ToCharArray()[bit_Stop]);
            isAlarm = IsCheckBit(binary.ToCharArray()[bit_Alarm]);
            Console.WriteLine(string.Format("Start: {0} - Stop: {1} - Alarm: {2}", isStart, isStop, isAlarm));
        }

        public void WriteDataToRegister(int data, int index)
        {
            _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, data);
        }
        public void WriteSampleStatusByIndex(Global.eSampleStatus sampleStatus, int index)
        {
            _plc.WriteDeviceBlock(GetWriteRegisterByIndex(index), 1, (int)sampleStatus);
        }

        private string GetWriteRegisterByIndex(int index)
        {
            return string.Format("{0}{1}", REG_PLC_Write, REG_PLC_Start + index);
        }
        private bool IsCheckBit(char value)
        {
            return value == '1' ? true : false;
        }
        public void VisionBusy(bool status)
        {
            int data = status ? 1 : 0;
            _plc.WriteDeviceBlock(REG_Vision_Bussy, 1, data);
        }
    }
}
