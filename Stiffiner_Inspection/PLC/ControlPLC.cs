using ActUtlTypeLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stiffiner_Inspection.Hubs;
using System;
using System.Collections.Generic;
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
        public readonly IHubContext<HomeHub> _hubContext;

        // Register read
        private const string REG_PLC_Read_STATUS = "D20";
        private const string REG_PLC_Read_ALARM_MESSAGE = "D2400";
        private const int ALARM_MESSAGE_LEGHT = 40;

        // Register Write
        private const string REG_PLC_Write = "D";
        private const int REG_PLC_Start = 900;
        private const string REG_Vision_Bussy = "M420";


        //machine status
        private const int bit_Start = 0;
        private const int bit_Stop = 1;
        private const int bit_Alarm = 2;
        private bool isStart = false;
        private bool isStop = false;
        private bool isAlarm = false;
        private bool isEMG = false;

        public bool IsStart { get => isStart; set => isStart = value; }
        public bool IsStop { get => isStop; set => isStop = value; }
        public bool IsAlarm { get => isAlarm; set => isAlarm = value; }
        public bool IsEMG { get => isEMG; set => isEMG = value; }

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
                SetStatusOfMachine(valueReaded);
                Global.tempValuePLC = valueReaded;
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

            Console.WriteLine(string.Format("EMG: {0} - Start: {1} - Stop: {2} - Alarm: {3}", isEMG, isStart, isStop, isAlarm));

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
