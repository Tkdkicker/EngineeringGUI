using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Drivers;

namespace EngineeringGUI
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Version number
        /// </summary>
        public const string _version = "1.3.4";

        private const float _currentLimit = 0.8f;
        private const float _supplyVoltage = 12.0f;

        /// <summary>
        /// Variable to hold the name of the Erymanthos board
        /// </summary>
        private string _serialNumber = string.Empty;

        /// <summary>
        /// I2C comms to the module
        /// </summary>
        private KeterexInterface _sfpketerexInterface;

        /// <summary>
        /// UART comms to the module
        /// </summary>
        private SfpUART _sfpUartInterface;

        /// <summary>
        /// UART comms to the Erymanthos board
        /// </summary>
        private Erymanthos _erymanthos;

        /// <summary>
        /// UART comms to the Erymanthos board,
        /// used when scanning the serial bus
        /// </summary>
        private Erymanthos _tempErymanthos;

        /// <summary>
        /// Power supply to the module
        /// </summary>
        private Tenma2000 _psu;

        /// <summary>
        /// List of instruments , driverName, resource properties string
        /// used to create the driver objects
        /// </summary>
        private Dictionary<string, string> _instruments;

        /// <summary>
        /// Declare our worker thread
        /// </summary>
        private Thread _workerThread = null;

        /// <summary>
        /// Boolean flag used to stop the NVM read in the worker thread
        /// </summary>
        private bool _stopProcess = false;

        /// <summary>
        /// List of last temperatures set for the bays, driverName, resource properties string.
        /// Key value = bay number and temp set point
        /// </summary>
        private Dictionary<int, string> _baySetTemperatures;

        //Delegates which hold references to functions, used for callbacks to main GUI thread.
        //Delegates are set up in form load event

        /// <summary>
        /// Delegate for updating the module status during a power cycle
        /// </summary>
        /// <param name="s"></param>
        private delegate void UpdateModuleStatusDelegate(string s);

        private UpdateModuleStatusDelegate _updateModuleStatusDelegate = null;

        /// <summary>
        /// Delegate for updating the grid box during NVM read
        /// </summary>
        /// <param name="s">row of data for the grid Box</param>
        private delegate void UpdateGridViewDelegate(string[] row);

        private int _gridViewRowToShowIndexModuleTemps = 0;

        /// <summary>
        /// List of Erymanthos boards found
        /// </summary>
        private List<string> _erymanthosBoardsPresent;

        /// <summary>
        /// Indicates setting the channel combo at start up without completing a write
        /// </summary>
        private bool _startUpChanneRead = true;

        #region Constructor

        public Form1()
        {
            InitializeComponent();

            Text = $"{Text} VERSION {_version}";
            PsuControl();
            InstrumentSettings();
            ErymanthosSearchBtn.Select();
        }

        #endregion Constructor

        #region Form1_Load

        private void Form1_Load(object sender, EventArgs e)
        {
            _updateModuleStatusDelegate = new UpdateModuleStatusDelegate(UpdateModuleStatus);

            SetupDataGridView();

            //For Erymanthos tab set up
            _baySetTemperatures = new Dictionary<int, string>();

            //Needed for Erymanthos Tab
            for (int bay = 0; bay < 6; bay++)
            {
                _baySetTemperatures.Add(bay, "None");
            }

            //Want the Erymanthos tab
            ErymanthosSearchBtn.Enabled = true;
            Cursor = Cursors.Default;

            ErymanthosSearchBtn.Select();
            ErymanthosSearchBtn.Focus();
        }

        #endregion Form1_Load

        #region SetupDataGridView

        /// <summary>
        /// Display what the DataGrid will look like
        /// </summary>
        private void SetupDataGridView()
        {
            ModuleTempsDataGridView.ColumnCount = 5;

            ModuleTempsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            //ModuleTempsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            ModuleTempsDataGridView.Columns[0].Name = "Bay";
            ModuleTempsDataGridView.Columns[1].Name = "TECLoop";

            ModuleTempsDataGridView.Columns[2].Name = "Set";
            ModuleTempsDataGridView.Columns[2].DefaultCellStyle.ForeColor = Color.Red;

            ModuleTempsDataGridView.Columns[3].Name = "Case";
            ModuleTempsDataGridView.Columns[3].DefaultCellStyle.ForeColor = Color.DarkGray;

            ModuleTempsDataGridView.Columns[4].Name = "Water";
            ModuleTempsDataGridView.Columns[4].DefaultCellStyle.ForeColor = Color.Blue;

            ModuleTempsDataGridView.ReadOnly = true;

            //foreach (DataGridViewColumn col in ModuleTempsDataGridView.Columns)
            //{
            //    col.Width = ModuleTempsDataGridView.Width / 5;
            //}

            ModuleTempsDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ModuleTempsDataGridView.RowTemplate.Resizable = DataGridViewTriState.True;
            ModuleTempsDataGridView.RowTemplate.Height = 8;
        }

        #endregion SetupDataGridView

        #region InstrumentSettings

        /// <summary>
        /// Load settings from App.config file.
        /// Called when the form is loaded to set up attached instruments
        /// </summary>
        private void InstrumentSettings()
        {
            //Get a dictionary of instrumentNames, & comma separated properties string which holds the com properties
            //like baud rate, buffer size, parity etc.
            _instruments = new Dictionary<string, string>();
            if (ConfigurationManager.GetSection("instrumentSettings") is NameValueCollection instrumentSettings)
            {
                foreach (var instrumentName in instrumentSettings.AllKeys)
                {
                    string resourceValue = instrumentSettings.GetValues(instrumentName).FirstOrDefault();
                    if (resourceValue.ToLower().Contains("connected=false"))
                        continue;
                    _instruments.Add(instrumentName, resourceValue);
                }
            }

            //Find sfpUart settings
            if (_instruments.ContainsKey("sfpUART"))
            {
                _sfpUartInterface = new SfpUART("sfpUART", _instruments["sfpUART"]);
            }

            if (_instruments.ContainsKey("Tenma2540"))
            {
                _psu = new Tenma2000("Tenma", _instruments["Tenma2540"]);
            }

            //if (instruments.ContainsKey("Erymanthos"))
            //{
            // this.erymanthos = new Erymanthos("Erymanthos", instruments["Erymanthos"]);
            //}

            //if (instruments.ContainsKey("OSA"))
            //{
            //    /// this.osa = new OSA("JDS_osa", instruments["OSA"]);

            //    //string doWeHaveComms = osa.Identity();

            //    // string test = osa.Query("SYST:AUD?");

            //    // osa.WriteLine("MOD:FUNC:SEL BOTH,SLIC1,\"OSA\",ON");

            //    // string port = osa.Query("MOD:FUNC:PORT? BOTH,SLIC1,\"OSA\"");

            //    //string test = osa.Query("CUR:BUF?");

            //    // osa.SingleMode();

            //    // run a mesurement
            //    //osa.DoSweep();
            //}

            if (_instruments.ContainsKey("Wavemeter"))
            {
                //this.wavemeter = new BristolWavemeter("Bristol", instruments["Wavemeter"]);

                //string doWeHaveComms = wavemeter.Identity();

                //wavemeter.PowerUnits(PowerUnits.dBm);
                //wavemeter.WavelengthUnits(WavelengthUnits.THz);

                //double freq = wavemeter.GetFrequency();

                //double wavelength = wavemeter.GetWavelength();

                //double power = wavemeter.GetPower();

                //  double avgTime = wavelengthMeter.GetAveraging();

                //string powerOffset = wavemeter.GetPowerOffset();

                // string test = osa.Query("SYST:AUD?");

                // osa.WriteLine("MOD:FUNC:SEL BOTH,SLIC1,\"OSA\",ON");

                // string port = osa.Query("MOD:FUNC:PORT? BOTH,SLIC1,\"OSA\"");

                //string test = osa.Query("CUR:BUF?");

                // osa.SingleMode();

                // run a mesurement
                //osa.DoSweep();
            }

            try
            {
                if (_psu != null)
                {
                    if (!_psu.PortOpen)
                        _psu.PortOpen = true;

                    //If output on, then turn off, display psu off at the end
                    //If off , turn on, wait until module is in a stable state after turning on
                    _psu.SetCurrent(_currentLimit);
                    _psu.SetVoltage(_supplyVoltage);

                    _psu.Ouput(outputOn: true);

                    Thread.Sleep(3000);
                }
                //If not connected this will throw an error, dont want one at this stage,
                //want at least to see the form
                if (_instruments.ContainsKey("KeterexInterface"))
                {
                    _sfpketerexInterface = new KeterexInterface();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in InstrumentSettings :" + ex.Message);
            }
        }

        #endregion InstrumentSettings

        #region UpdateModuleStatus

        /// <summary>
        /// This is called by delegate from the worker thread to update the controls
        /// </summary>
        /// <param name="moduleStatus"></param>
        private void UpdateModuleStatus(string moduleStatus)
        {
            if (moduleStatus.ToLower() == "abort")
            {
                Update();
                _stopProcess = false;
            }
            else
            {
                UpdateSummaryInfo(fullUpdate: false);
            }
        }

        #endregion UpdateModuleStatus

        #region UpdateSummaryInfo

        /// <summary>
        /// Updates the serail number/product code/etc info.
        /// Needed for OOBA or just the module state.
        /// For stat transitions
        /// </summary>
        private void UpdateSummaryInfo(bool fullUpdate = true)
        {
            if (fullUpdate)
            {
                ushort channelRead = 0;
                double channelFreqGHz = 0;

                _sfpketerexInterface.ChannelInfo(ref channelRead, ref channelFreqGHz);

                //This bit reenbkes the controls, so we want to disable again
                _startUpChanneRead = false;

                _sfpketerexInterface.WritePasswordWP3();

                _sfpketerexInterface.WritePasswordWP3();
                string firmwareRevision = _sfpketerexInterface.Read<string>(SlaveRegister.effectFirmwareRevision).ToString();  //4 bytes in ASCII

                int aaaRevisionTag = Convert.ToInt32(_sfpketerexInterface.Read<string>(SlaveRegister.aaaRevisionTag).ToString());
                int bbbRevisionTag = Convert.ToInt32(_sfpketerexInterface.Read<string>(SlaveRegister.bbbRevisionTag).ToString());
                int cccRevisionTag = Convert.ToInt32(_sfpketerexInterface.Read<string>(SlaveRegister.cccRevisionTag).ToString());

                //NEED TO ADD MORE

                _sfpketerexInterface.WritePasswordWP3();
                _serialNumber = _sfpketerexInterface.Read<string>(SlaveRegister.effectSerialNumber).ToString();
            }
            else
            {
                _sfpketerexInterface.WritePasswordWP3();
            }
        }

        #endregion UpdateSummaryInfo

        #region ReadModuleStaus

        /// <summary>
        /// Read the module status and
        /// call delegate to update main GUI controls
        /// </summary>
        private void ReadModuleStatus(ModuleState expectedModuleState = ModuleState.MS_HPWR)
        {
            _sfpketerexInterface.WritePasswordWP3();
            ModuleState tempModuleState = ((ModuleState)_sfpketerexInterface.Read<byte>(SlaveRegister.effectModuleState));
            //Read a maximum of 30 times
            for (int i = 1; i < 30; i++)
            {
                //Each time call the delegate...

                tempModuleState = ((ModuleState)_sfpketerexInterface.Read<byte>(SlaveRegister.effectModuleState));

                //Wait for stable state for DEBUG or Customer mode
                Invoke(_updateModuleStatusDelegate, tempModuleState.ToString());
                if (tempModuleState == expectedModuleState)//Soft Tx assert has been actioned
                    break;

                if (tempModuleState == ModuleState.MS_FAULT)//We dont get out of this do we
                    break;

                //We are in UART mode,
                if (tempModuleState == ModuleState.MS_HPWR & expectedModuleState == ModuleState.MS_TXOFF)
                    break;

                Thread.Sleep(500);
            }

            Invoke(_updateModuleStatusDelegate, "abort");
        }

        #endregion ReadModuleStaus

        #region PsuControl

        /// <summary>
        /// Toggles the power to module:
        /// if already on turns it off, else turns it on
        /// reads the firmware/serialnumber also
        /// </summary>
        public void PsuControl()
        {
            try
            {
                if (_psu == null)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("Power supply not connected, please control manually");
                    return;
                }

                if (!_psu.PortOpen)
                    _psu.PortOpen = true;

                // If output on , then turn off, display psu off at the end.
                // If off , turn on, wait until module is in a stable state after turning on
                _psu.SetCurrent(_currentLimit);
                _psu.SetVoltage(_supplyVoltage);

                if (_psu.OutputOn())
                {
                    //See if we can read stuff first
                    //this.moduleSate.Text = ((ModuleState)sfpketerexInterface.Read<byte>(SlaveRegister.effectModuleState)).ToString();
                    //this.firmware.Text = sfpketerexInterface.Read<string>(SlaveRegister.effectFirmwareRevision).ToString();
                    //this.serialNumber.Text = sfpketerexInterface.Read<string>(SlaveRegister.effectSerialNumber).ToString();
                    //this.partCode.Text = sfpketerexInterface.Read<string>(SlaveRegister.effectPartNumber).ToString();
                    // power down
                    _psu.Ouput(outputOn: false);
                }
                else
                {
                    _psu.Ouput(outputOn: true);

                    _workerThread = new Thread(() => ReadModuleStatus());
                    _workerThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show($"{ex.Message} please check com ports and .config set up file");
            }
        }

        #endregion PsuControl

        #region ScanForErymanthos

        /// <summary>
        /// Look for the Erymanthods board
        /// </summary>
        private void ScanForErymanthos()
        {
            List<string> comPorts = SerialInterface.GetPorts();

            _erymanthosBoardsPresent = new List<string>();

            for (int index = 0; index < comPorts.Count; index++)
            {
                //Create instance; open port; see if read serial number with ok; if yes, add to list of Erymanthos
                string port = comPorts[index];

                string portResources = "PORT=" + port + ",BAUDRATE=1000000,READBUFFER=8192,WRITEBUFFER=8192";
                _tempErymanthos = new Erymanthos("Erymanthos", portResources);

                try
                {
                    if (!_tempErymanthos.PortOpen)
                        _tempErymanthos.PortOpen = true;
                }
                catch
                {
                    //Try next port
                    continue;
                }

                List<string> response = _tempErymanthos.WriteReadLines("GET SNR");

                response = _tempErymanthos.WriteReadLines("GET SNR");

                foreach (string line in response)
                {
                    if (line.Contains("OK"))
                    {
                        //Extract serial number
                        if (line.Count() < 2)
                            _serialNumber = string.Empty;
                        if (line.Contains("NOK"))
                            continue;//Could be unrecognised command, so gives

                        _serialNumber = line.Split(' ')[1];

                        int ermaPort = Convert.ToInt32(port.Replace("COM", string.Empty));

                        //Add base port
                        int basePort = ermaPort - 1;

                        string basePortString = "COM" + basePort.ToString();

                        _erymanthosBoardsPresent.Add(basePortString + ", " + _serialNumber);

                        //Now increment
                    }
                }

                //Now close port
                _tempErymanthos.PortOpen = false;
                _tempErymanthos = null;
            }
        }

        #endregion ScanForErymanthos

        #region DisableIRQs_Click

        /// <summary>
        /// Need this in a delegate as it takes a while.
        /// Disable all the controls and and display a message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisableIRQs_Click(object sender, EventArgs e)
        {
            Update();

            if (_psu != null)
            {
                if (!_psu.PortOpen)
                    _psu.PortOpen = true;

                //If output on, then turn off, display psu off at the end.
                //If off, turn on, wait until module is in a stable state after turning on
                _psu.SetCurrent(current: _currentLimit);
                _psu.SetVoltage(voltage: _supplyVoltage);
                _psu.Ouput(outputOn: true);
            }

            if (!_erymanthos.PortOpen)
                _erymanthos.PortOpen = true;

            if (!_sfpUartInterface.PortOpen)
                _sfpUartInterface.PortOpen = true;

            _erymanthos.DisableIRQs();

            //Always do a SET CRC 0 in case some one has power cycled
            _erymanthos.WriteCommand("SET CRC 0", out string reply);

            _serialNumber = _erymanthos.GetSerialNumber();

            Update();
        }

        #endregion DisableIRQs_Click

        #region AvailErymanBoards_SelectedIndexChanged

        private void AvailErymanBoards_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Now get the base port stuff you selected
            if (_erymanthos != null)
            {
                //Destroy every thing do far , for reassignment
                _erymanthos.PortOpen = false;
                _erymanthos = null;

                _sfpUartInterface.PortOpen = false;
                _sfpUartInterface = null;
            }

            string portBoard = ErymanBoardsCmbBx.SelectedItem.ToString().ToUpper().Trim();

            string[] portInfo = portBoard.Split(',');

            int basePort = Convert.ToInt32(portInfo[0].Replace("COM", string.Empty));
            string selectedErymanPort = "COM" + (basePort + 1).ToString();
            string portResources = "PORT=" + selectedErymanPort + ",BAUDRATE=1000000,READBUFFER=8192,WRITEBUFFER=8192";
            _erymanthos = new Erymanthos("Erymanthos", portResources);

            try
            {
                if (!_erymanthos.PortOpen)
                    _erymanthos.PortOpen = true;
            }
            catch
            {
                //Error message
            }
            string selectedUARTPort = "COM" + (basePort + 3).ToString();
            portResources = "PORT=" + selectedUARTPort + ",BAUDRATE=62500,READBUFFER=8192,WRITEBUFFER=8192";
            _sfpUartInterface = new SfpUART("sfpUART", portResources);

            if (!_sfpUartInterface.PortOpen)
                _sfpUartInterface.PortOpen = true;

            Thread.Sleep(500);
        }

        #endregion AvailErymanBoards_SelectedIndexChanged

        #region GetISO8601Week

        /// <summary>
        /// Ian's function to get the week number
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public int GetISO8601Week(DateTime date)
        {
            DateTime thursday = date.AddDays(3 - ((int)date.DayOfWeek + 6) % 7);
            return (thursday.DayOfYear - 1) / 7 + 1;
        }

        #endregion GetISO8601Week

        #region ErmanthosSearchBtn

        /// <summary>
        /// Search for Erymanthos boards and display those available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErmanthosSearchBtn(object sender, EventArgs e)
        {
            if (_erymanthos != null)
            {
                //Destroy every thing do far, for reassignment
                _erymanthos.PortOpen = false;
                _erymanthos = null;

                _sfpUartInterface.PortOpen = false;
                _sfpUartInterface = null;
            }

            StatusLbl.Text = "Searching...";
            StatusLbl.Enabled = true;
            StatusLbl.Visible = true;
            StatusLbl.BackColor = Color.GreenYellow;
            Cursor = Cursors.WaitCursor;
            Update();

            ScanForErymanthos();

            Thread.Sleep(1000);

            Cursor = Cursors.Default;

            if (_erymanthosBoardsPresent.Count == 0)
            {
                StatusLbl.Text = "Not connected";
                StatusLbl.BackColor = Color.Red;
                ErymanthosSearchBtn.Enabled = true;
                Cursor = Cursors.Default;
            }
            else
            {
                StatusLbl.Text = "Connected";
                StatusLbl.BackColor = Color.GreenYellow;
                ReadTemperaturesBtn.Enabled = true;
                ErymanthosSearchBtn.Enabled = false;

                ErymanBoardsCmbBx.DataSource = _erymanthosBoardsPresent;
                SelectBayNumberCmbBx.DataSource = Enum.GetValues(typeof(ModulePosition));
            }
            Update();
        }

        #endregion ErmanthosSearchBtn

        #region ReadTemperaturesBtn_Click

        /// <summary>
        /// Read all module temperatures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadTemperaturesBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            int maxBayPosn = 5;
            string waterTemp = "1";
            string moduleTemp = "0";
            string erymanthCmdTECLoopOn;

            ModuleTempsDataGridView.Rows.Clear();

            for (int bay = 0; bay <= maxBayPosn; bay++)
            {
                //First parameter => bay position.
                //Second parameter => module temp = 0 , water temp = 1

                //Get module temperture
                string erymanthosCmd = "GET TEMP " + bay + " " + moduleTemp;

                string response = _erymanthos.QueryCommand(erymanthosCmd.ToUpper().Trim());
                double moduleTempRead = Convert.ToDouble(response) / 1000;

                //Get water temperature
                erymanthosCmd = "GET TEMP " + bay + " " + waterTemp;
                response = _erymanthos.QueryCommand(erymanthosCmd.ToUpper().Trim());
                double waterTempRead = Convert.ToDouble(response) / 1000;

                erymanthCmdTECLoopOn = $"{"GET TEC"} {bay}";
                response = _erymanthos.QueryCommand(erymanthCmdTECLoopOn.ToUpper().Trim());

                //Add to gridView
                List<string> dataItems = new List<string>
                {
                    bay.ToString(),
                    response,
                    _baySetTemperatures[bay],
                    moduleTempRead.ToString(),
                    waterTempRead.ToString()
                };

                ModuleTempsDataGridView.Rows.Add(dataItems.ToArray());
                _gridViewRowToShowIndexModuleTemps++;
                ModuleTempsDataGridView.FirstDisplayedScrollingRowIndex = 0;
                Thread.Sleep(50);

                AssignDataToLiveChart(bay, response, moduleTempRead, waterTempRead);
            }

            //All displayed, game over
            _gridViewRowToShowIndexModuleTemps = 0;
            Cursor = Cursors.Default;
            SetAllBaysBtn.Enabled = true;
            SetSelectedBayBtn.Enabled = true;
        }

        #endregion ReadTemperaturesBtn_Click

        #region SetSelectedBayBtn_Click

        /// <summary>
        /// Set the case temperature for the specified bay in the drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetSelectedBayBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            ModuleTempsDataGridView.Rows.Clear();

            Enum.TryParse(SelectBayNumberCmbBx.SelectedValue.ToString(), out ModulePosition bayNumber);
            int bay = (int)bayNumber;

            SetCaseTemperatureForABay(bay);

            Thread.Sleep(50);
            //All displayed, game over
            _gridViewRowToShowIndexModuleTemps = 0;
            Cursor = Cursors.Default;
        }

        #endregion SetSelectedBayBtn_Click

        #region SetCaseTemperatureForABay

        /// <summary>
        /// Set the case temperature and turn on the TEC loops for the specified bay
        /// </summary>
        /// <param name="bay"></param>
        private void SetCaseTemperatureForABay(int bay)
        {
            string waterTemp = "1";
            string moduleTemp = "0";
            string setCaseTempCmd = "SET TEMP";//Then need bay number and temperature
            double caseTemp;
            try
            {
                caseTemp = Convert.ToDouble(SetTempTxtBx.Text);//int degC - Need a try catch
            }
            catch
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("Case temperature outside vaild allowed range of -40 to 85 degC");

                return;
            }

            if (caseTemp < -40 || caseTemp > 85)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("Case temperature outside vaild allowed range of -40 to 85 degC");

                return;
            }

            _baySetTemperatures[bay] = caseTemp.ToString();

            //Convert for the command
            int cmdTemperature = (int)(caseTemp * 1000);

            string erymanthCmd = $"{setCaseTempCmd} {bay} {moduleTemp} {cmdTemperature}";//SET TEMP 0 #bay Number # temperature

            List<string> response = _erymanthos.WriteReadLines(erymanthCmd.ToUpper().Trim());
            //this.SetTempResponseLbl.Text = string.Empty + erymanthCmd + '\n';
            //UpdateSetTempResponse(response);

            //Now turn on the TEC loop
            string erymanthCmdTECLoopOn = $"{$"{"SET TEC"} {bay}"} 1";
            response = _erymanthos.WriteReadLines(erymanthCmdTECLoopOn.ToUpper().Trim());
            //this.SetTempResponseLbl.Text += '\n';
            //this.SetTempResponseLbl.Text += erymanthCmdTECLoopOn + '\n';
            //UpdateSetTempResponse(response);

            //Now read the stuff
            Update();

            erymanthCmd = $"GET TEMP {bay} {moduleTemp}";

            //Read the bay temperature
            string readResponse = _erymanthos.QueryCommand(erymanthCmd.ToUpper().Trim());
            double moduleTempRead = Convert.ToDouble(readResponse) / 1000;

            erymanthCmd = "GET TEMP " + bay + " " + waterTemp;
            readResponse = _erymanthos.QueryCommand(erymanthCmd.ToUpper().Trim());
            double waterTempRead = Convert.ToDouble(readResponse) / 1000;

            //TEC loop status
            erymanthCmdTECLoopOn = $"{"GET TEC"} {bay}";
            readResponse = _erymanthos.QueryCommand(erymanthCmdTECLoopOn.ToUpper().Trim());
            int TECloopStatus = Convert.ToInt16(readResponse);

            //Add to gridView
            List<string> dataItems = new List<string>
            {
                bay.ToString(),
                readResponse,
                caseTemp.ToString(),
                moduleTempRead.ToString(),
                waterTempRead.ToString()
            };

            ModuleTempsDataGridView.Rows.Add(dataItems.ToArray());
            _gridViewRowToShowIndexModuleTemps++;

            AssignDataToLiveChart(bay, readResponse, moduleTempRead, waterTempRead);
        }

        #endregion SetCaseTemperatureForABay

        #region SetAllBaysBtn_Click

        /// <summary>
        /// Set the bay case temperature for all the bays
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAllBaysBtn_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            int maxBayPosn = 5;
            double caseTemp;

            //this.SetTempResponseLbl.Text = string.Empty;
            ModuleTempsDataGridView.Rows.Clear();

            for (int bay = 0; bay <= maxBayPosn; bay++)
            {
                //this.SetTempResponseLbl.Text = string.Empty;

                try
                {
                    caseTemp = Convert.ToDouble(SetTempTxtBx.Text);//int degC - need a try catch
                }
                catch
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("Case temperature outside vaild allowed range of -40 to 85 degC");

                    return;
                }

                if (caseTemp < -40 || caseTemp > 85)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("Case temperature outside vaild allowed range of -40 to 85 degC");

                    return;
                }

                SetCaseTemperatureForABay(bay);

                //this.SetTempResponseLbl.Text += '\n';
            }

            //All displayed, game over
            _gridViewRowToShowIndexModuleTemps = 0;
            ModuleTempsDataGridView.FirstDisplayedScrollingRowIndex = 0;
            Cursor = Cursors.Default;
        }

        #endregion SetAllBaysBtn_Click

        #region AssignDataToLiveChart

        /// <summary>
        /// Assign data to chart
        /// </summary>
        /// <param name="bay">What position the module is on</param>
        /// <param name="response">The TECLoop temperature</param>
        /// <param name="moduleTempRead">The case temperature</param>
        /// <param name="waterTempRead">The water temperature</param>
        public void AssignDataToLiveChart(int bay, string response, double moduleTempRead, double waterTempRead)
        {
            RefreshAtLbl.Visible = true;
            TimeLbl.Visible = true;
            //Loop through to the last bay position which is 5
            for (int start = 0; start <= bay; start++)
            {
                LivePIDChrt.Series["TECLoop"].Points.AddXY(xValue: bay, yValue: response);
                LivePIDChrt.Series["Set"].Points.AddXY(xValue: bay, yValue: _baySetTemperatures[bay]);
                LivePIDChrt.Series["Case"].Points.AddXY(xValue: bay, yValue: moduleTempRead);
                LivePIDChrt.Series["Water"].Points.AddXY(xValue: bay, yValue: waterTempRead);
            }
            TimeLbl.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        #endregion AssignDataToLiveChart
    }
}