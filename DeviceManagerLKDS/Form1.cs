using DeviceManagerLKDS.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Timers;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static DeviceManagerLKDS.Classes.Enums;
using LKDS_Type;

namespace DeviceManagerLKDS
{
    public partial class Form1 : Form
    {
        public Stream st = null;
        DataReader dr;

        #region Конструктор формы и её методы
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            dr?.Disconnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getCOMports();
        }

        #endregion

        #region Клик-события

        private void button1_Click(object sender, EventArgs e)
        {
            string title = "Page " + (mainUserControl.mainTabControl.TabCount + 1).ToString();
            TabPage myTabPage = new TabPage(title);
            mainUserControl.mainTabControl.TabPages.Add(myTabPage);
        }
        bool iscnt = false;
        private void bConnectPort_Click(object sender, EventArgs e)
        {
            if (cbConnectedPorts.Items.Count != 0)
            {
                if (iscnt)
                {
                    try
                    {
                        dr.Disconnect();
                        rtbLog.Text += $"\nСоединение с портом {cbConnectedPorts.SelectedItem} разорвано";
                        timer1.Stop();
                        btnConnect.Text = "Открыть соединение";
                        while (mainUserControl.mainTabControl.TabPages.Count > 1)
                            mainUserControl.mainTabControl.TabPages.RemoveAt(1);
                        clone = new byte[34];
                        dr = null;
                    }
                    catch
                    {
                    }
                    iscnt = false;
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < cbConnectedPorts.Items.Count; i++)
                        {
                            if (i == cbConnectedPorts.SelectedIndex)
                            {
                                dr = new DataReader(cbConnectedPorts.SelectedItem.ToString());
                                btnConnect.Text = "Закрыть соединение";
                                timer1.Start();
                                mainUserControl.mainTabControl.SelectedIndex = 0;

                            }
                        }
                        rtbLog.Text += DataReader.log_conStatus;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    iscnt = true;
                }
                rtbLog.Text += $"\n\n[{DateTime.Now}-{DateTime.Now.Millisecond}] Выбранна вкладка: {mainUserControl.mainTabControl.TabPages[mainUserControl.mainTabControl.SelectedIndex].Text}";
            }

        }

/*        public  void OnRsvDataOnForm(byte[] data)
            {

            }*/

        private void clearbutton_Click(object sender, EventArgs e)
        {
            rtbLog.Text = "";
        }

        #endregion

        #region Функции и процедуры

        int i = 0;

        public void ClearArrList()
        {
            dr.setOfBytes = null;
            dr.rawData = new List<byte>();
            dr.bytePackets = new List<byte[]>();
        }

        bool SendQuery(byte[] query)
        {
            dr.Send(query);

            bool isdata = dr.OnDataEvent.WaitOne(20);
            //while (dr.setOfBytes == null)
            //{

            //}

            rtbLog.Text += DataReader.log_input + DataReader.log_output;
            dr.outputBytes = "";
            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.ScrollToCaret();
            logWriter.AutoFlush = true;
            logWriter.Write(rtbLog.Text);
            return isdata;

        }

        #endregion

        #region Таймер
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            i++;

            ClearArrList();

            if (!SendQuery(query))
            {
                timer1.Start();
                return
                    ;
            }
            try
            {
                if (clone[clone.Length - 1] != dr.setOfBytes[dr.setOfBytes.Length - 1] || clone[clone.Length - 2] != dr.setOfBytes[dr.setOfBytes.Length - 2])
                {

                    Array.Copy(dr.setOfBytes, clone, 34);
                    List<int> bits = new List<int>();
                    for (int i = 0; i < 256; i++)
                    {
                        int b = dr.setOfBytes[(int)(i / 8)];
                        if (((b & (1 << (i % 8))) != 0) && i >= 32)
                        {
                            bits.Add(i);

                        }
                    }
                    //обнуление списка подключённых устройств и вкладок
                    for (int p = 0; connectedDevices[p] != 0; p++)
                    {
                        connectedDevices[p] = 0; 
                    }
                    mainUserControl.mainTabControl.TabPages.Clear();


                    TabPage lbPage = new TabPage("ЛБ Концентратор");
                    mainUserControl.mainTabControl.TabPages.Add(lbPage);

                    for (int i = 0, j = 0; i < bits.Count; i++)
                    {
                        ClearArrList();

                        int adress = 0x1200 + 0x0010 * Math.Abs((bits[i] - 32));
                        byte[] array = new byte[]
                        {
                           (byte)adress,
                           Convert.ToByte(adress>>8)
                        };
                        Union16 var = new Union16();
                        var.Byte0 = array[0];
                        var.Byte1 = array[1];
                        query3[2] = var.Byte1;
                        query3[3] = var.Byte0;
                        query3[query.Length - 3] = 1;


                        SendQuery(query3);



                        if (dr.setOfBytes[1] != 255)
                        {
                            connectedDevices[j] = dr.setOfBytes[1];
                            connectedDevices[j + 1] = bits[i];
                            j = j + 2;
                        }
                    }
                    rtbLog.Text += "\n\n----------------\nПодключенные устройства:\n";
                    for (int o = 0; o < connectedDevices.Length; o++)
                    {
                        if (connectedDevices[o] != 0)
                        {

                            foreach (CAN_Devices value in Enum.GetValues(typeof(CAN_Devices)))
                            {
                                if (connectedDevices[o] == (byte)value)
                                {
                                    TabPage newPage = new TabPage(value.GetNameOfEnum());
                                    newPage.Name = $"{connectedDevices[o + 1]}";
                                    mainUserControl.mainTabControl.TabPages.Add(newPage);
                                    rtbLog.Text += $"\n{connectedDevices[o]} - {value.GetNameOfEnum()}. Адрес CAN: {newPage.Name}";
                                }
                            }
                        }
                    }

                    rtbLog.Text += "\n----------------\n";
                }

            }
            catch { }


            if (i % 3 == 0)
            {
                if (mainUserControl.mainTabControl.SelectedIndex != 0)
                {
                    int adress = 0x1200 + 0x0010 * Math.Abs((Convert.ToInt32(mainUserControl.mainTabControl.SelectedTab.Name) - 32));
                    byte[] array = new byte[]
                    {
                           (byte)adress,
                           Convert.ToByte(adress>>8)
                    };
                    Union16 var = new Union16();
                    var.Byte0 = array[0];
                    var.Byte1 = array[1];
                    query3[2] = var.Byte1;
                    query3[3] = var.Byte0;
                    query3[query.Length - 3] = 16;
                    SendQuery(query3);
                }
            }
            if (i % 15 == 0)
            {
                if (mainUserControl.mainTabControl.SelectedIndex == 0)
                {
                    ClearArrList();

                    SendQuery(query2);
                }
            }

            Update();

            timer1.Start();

        }
        #endregion

        #region Проверка подключенных COM-портов

        private struct DEV_BROADCAST_HDR
        {
            //отключаем предупреждения компилятора для ошибки 0649
        #pragma warning disable 0649
            internal UInt32 dbch_size;
            internal UInt32 dbch_devicetype;
            internal UInt32 dbch_reserved;
            //включаем предупреждения компилятора для ошибки 0649
        #pragma warning restore 0649
        };
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0219)
            {
                DEV_BROADCAST_HDR dbh;
                switch ((int)m.WParam)
                {
                    case 0x8000:
                        dbh = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_HDR));
                        if (dbh.dbch_devicetype == 0x00000003)
                        {
                            getCOMports();
                        }
                        break;
                    case 0x8004:
                        dbh = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_HDR));
                        if (dbh.dbch_devicetype == 0x00000003)
                        {

                            cbConnectedPorts.Text = "";
                            getCOMports();
                        }
                        break;
                }
            }
        }
        public void getCOMports()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                cbConnectedPorts.Items.Clear();
                cbConnectedPorts.Items.AddRange(ports);
                cbConnectedPorts.SelectedIndex = 0;
            }
            catch (Exception)
            {
                // MessageBox.Show("Нет доступных соединений");
            }
        }

        #endregion

        #region Прочее
        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mainUserControl.mainTabControl.SelectedIndex != -1)
                rtbLog.Text += $"\n\n[{DateTime.Now}-{DateTime.Now.Millisecond}] Выбранна вкладка: {mainUserControl.mainTabControl.TabPages[mainUserControl.mainTabControl.SelectedIndex].Text}";

        }

        #endregion

        private void button3_Click(object sender, EventArgs e)
        {

        }

        public void button4_Click(object sender, EventArgs e)
        {
            Process.Start("C:\\Users\\nazar\\Desktop\\DeviceManagerLKDS - df211a1bece6d3d77418f95e2cf132bcd44257fa\\DeviceManagerLKDS\\Logs");
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
