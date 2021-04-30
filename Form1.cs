using System;
using System.Management;
using System.Windows.Forms;
using static USBDRIVERPACK.DisDev;

namespace USBDRIVERPACK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            start();
            loadUsbDevices();
        }
        private void loadUsbDevices()
        {
           using (var searcher = new ManagementObjectSearcher(
                @"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB%'"))
            {
                ManagementObjectCollection collection = searcher.Get();

                foreach (var device in collection)
                {
                    listBox2.Items.Add((string)device.GetPropertyValue("Name"));
                    listBox2.Items.Add((string)device.GetPropertyValue("ClassGuid"));
                    listBox2.Items.Add((string)device.GetPropertyValue("DeviceID"));
                 
                }
            }
        }
        void start()
        {
            checkedListBox1.Items.Clear();
            listBox1.Items.Clear();
            ManagementObjectCollection collection;
            //@"Select *From Win32_USBHub"
            using (var finddevice = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB%'"))
                collection = finddevice.Get();
                foreach (var device in collection) {
                checkedListBox1.Items.Add(device.GetPropertyValue("DeviceID"));
                checkedListBox1.Items.Add(device.GetPropertyValue("Description"));
                checkedListBox1.Items.Add("");
                
                //   listBox1.Items.Add( device.GetPropertyValue("PNPDeviceID").ToString().Trim());
            }
            string diskName = string.Empty;

            //предварительно очищаем список
          //  listBox1.Items.Clear();

            //Получение списка накопителей подключенных через интерфейс USB
            foreach (System.Management.ManagementObject drive in
            new System.Management.ManagementObjectSearcher(
            "select * from Win32_DiskDrive where InterfaceType='USB'").Get())
            {
                //Получаем букву накопителя
                foreach (System.Management.ManagementObject partition in
                new System.Management.ManagementObjectSearcher(
                "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"]
                + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (System.Management.ManagementObject disk in
                    new System.Management.ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='"
                    + partition["DeviceID"]
                    + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                    {
                        //Получение буквы устройства
                        diskName = disk["Name"].ToString().Trim();
                        listBox1.Items.Add("Буква накопителя=" + diskName);
                    }
                }

                //Получение модели устройства
                listBox1.Items.Add("Модель=" + drive["Model"]);

                //Получение Ven устройства
                listBox1.Items.Add("Ven=" +
                parseVenFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));

                //Получение Prod устройства
                listBox1.Items.Add("Prod=" +
                parseProdFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));

                //Получение Rev устройства
                listBox1.Items.Add("Rev=" +
                parseRevFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));

                //Получение серийного номера устройства
                string serial = drive["SerialNumber"].ToString().Trim();
                //WMI не всегда может вернуть серийный номер накопителя через данный класс
                if (serial.Length > 1)
                    listBox1.Items.Add("Серийный номер=" + serial);
                else
                    //Если серийный не получен стандартным путем,
                    //Парсим информацию Plug and Play Device ID
                    listBox1.Items.Add("Серийный номер=" +
                    parseSerialFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));

                //Получение объема устройства в гигабайтах
                decimal dSize = Math.Round((Convert.ToDecimal(
                new System.Management.ManagementObject("Win32_LogicalDisk.DeviceID='"
                + diskName + "'")["Size"]) / 1073741824), 2);
                listBox1.Items.Add("Полный объем=" + dSize + " gb");

                //Получение свободного места на устройстве в гигабайтах
                decimal dFree = Math.Round((Convert.ToDecimal(
                new System.Management.ManagementObject("Win32_LogicalDisk.DeviceID='"
                + diskName + "'")["FreeSpace"]) / 1073741824), 2);
                listBox1.Items.Add("Свободный объем=" + dFree + " gb");

                //Получение использованного места на устройстве
                decimal dUsed = dSize - dFree;
                listBox1.Items.Add("Используемый объем=" + dUsed + " gb");

                listBox1.Items.Add("");
            }
        }
        private string parseSerialFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string[] serialArray;
            string serial;
            int arrayLen = splitDeviceId.Length - 1;

            serialArray = splitDeviceId[arrayLen].Split('&');
            serial = serialArray[0];

            return serial;
        }

        private string parseVenFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string Ven;
            //Разбиваем строку на несколько частей.
            //Каждая чаcть отделяется по символу &
            string[] splitVen = splitDeviceId[1].Split('&');

            Ven = splitVen[1].Replace("VEN_", "");
            Ven = Ven.Replace("_", " ");
            return Ven;
        }

        private string parseProdFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string Prod;
            //Разбиваем строку на несколько частей.
            //Каждая чаcть отделяется по символу &
            string[] splitProd = splitDeviceId[1].Split('&');

            Prod = splitProd[2].Replace("PROD_", ""); ;
            Prod = Prod.Replace("_", " ");
            return Prod;
        }

        private string parseRevFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string Rev;
            //Разбиваем строку на несколько частей.
            //Каждая чаcть отделяется по символу &
            string[] splitRev = splitDeviceId[1].Split('&');

            Rev = splitRev[3].Replace("REV_", ""); ;
            Rev = Rev.Replace("_", " ");
            return Rev;
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            start();
            loadUsbDevices();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\USBSTOR", "Start", 3, Microsoft.Win32.RegistryValueKind.DWord);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged_2(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\USBSTOR", "Start", 4, Microsoft.Win32.RegistryValueKind.DWord);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Guid deviceGuid = new Guid("{"+textBox1.Text+"}");//думаю это можно захардкодить
            string instancePath = @textBox2.Text;//это захардкодить вряд ли выйдет, нужно будет поискать в девайс менеджере через шарп нужный путь

            DeviceHelper.SetDeviceEnabled(deviceGuid, instancePath, false); //фалс отключает девайс / тру - включает
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Guid deviceGuid = new Guid("{" + textBox1.Text + "}");//думаю это можно захардкодить
            string instancePath = @textBox2.Text;//это захардкодить вряд ли выйдет, нужно будет поискать в девайс менеджере через шарп нужный путь

            DeviceHelper.SetDeviceEnabled(deviceGuid, instancePath, true); //фалс отключает девайс / тру - включает

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
