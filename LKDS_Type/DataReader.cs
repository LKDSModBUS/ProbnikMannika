﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LKDS_Type;

namespace DeviceManagerLKDS.Classes
{
    public class DataReader
    {

        public Stream st = null;
        public string inputBytes;
        public string outputBytes;

        public static string log_conStatus;
        public static string log_input;
        public static string log_output;

        public byte[] setOfBytes;
        public List<byte> rawData = new List<byte>();
        public List<byte[]> bytePackets = new List<byte[]>();
        public delegate void OnRsvDataDelegate(byte[] data);
        public AutoResetEvent OnDataEvent = new AutoResetEvent(false);
        public OnRsvDataDelegate OnRsvData = null;

        object locker = new object();

        public SerialPort port = null;
        public DataReader(string portName)//, byte[] data)
        {
            try
            {
                port = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
                port.Handshake = Handshake.None;


                port.DataReceived += new SerialDataReceivedEventHandler(PackageSeeker);
                port.Open();

                //m_Port.Open();
                st = port.BaseStream;

                log_conStatus = $"\nОткрыто соединение с портом {portName}";
            }
            catch
            {
                log_conStatus = $"\nСоединение с портом {portName} уже открыто";
            }
        }
        public void Send(byte[] data)
        {
            if (!port.IsOpen)
                return;
            byte[] CRC = new byte[2];

            CRC = Parent.ModbusCRC16Calc(data, data.Length - 2);
            data[data.Length - 2] = CRC[CRC.Length - 2];
            data[data.Length - 1] = CRC[CRC.Length - 1];
            port.Write(data, 0, data.Length);

            inputBytes = ByteToString(data).ToUpper();
        }
        
        public string ByteToString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2}", b).Append(" ");
            }

            return hex.ToString();
        }
        
        public void PackageSeeker(object s, EventArgs e)
        {
            lock (locker)
            {
                int len = port.BytesToRead;
                byte[] bytes = new byte[len];

                if (port.IsOpen)
                {
                    port.Read(bytes, 0, len);
                    rawData.AddRange(bytes);
                    outputBytes += ByteToString(bytes).ToUpper();
                }
                try
                {
                    if ((rawData.Count > 2) && (rawData[2] + 2 <= rawData.Count - 3))
                    {
                        for (int i = 0; i < rawData.Count; i++)
                        {
                            if (rawData[i] == Parent.mask && rawData[i + 1] == 0x04)//0x01 и 0x04 костыли
                            {
                                setOfBytes = new byte[rawData[2] + 2];

                                rawData.CopyTo(3, setOfBytes, 0, rawData[2] + 2);
                                bytePackets.Add(setOfBytes);
                                OnRsvData?.Invoke(setOfBytes);
                                OnDataEvent.Set();
                            }
                        }

                        log_input = "\n\n[" + DateTime.Now + "-" + DateTime.Now.Millisecond + "] >: " + inputBytes;
                        log_output = "\n[" + DateTime.Now + "-" + DateTime.Now.Millisecond + "] <: " + outputBytes;
                    }
                }
                catch { }
            }
        }
        public void Disconnect()
        {

            port.Close();

            if (st != null)
            {
                GC.SuppressFinalize(st);
                st = null;
            }

            port.Dispose();

            //log_conStatus = $"\nЗакрыто соединение с портом {portName}";
        }

    }
}
