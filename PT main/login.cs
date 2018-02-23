using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using Subsembly.SmartCard;
using Subsembly.SmartCard.PcSc;

namespace PT_main
{
    public partial class login : Form
    {
        string user1 = "admin";
        string user2 = "100357";
        string user3 = "122022";
        string user4 = "pw";
        string pswd1 = "admin";
        string pswd2 = "100357";
        string pswd3 = "122022";
        string pswd4 = "pw";


        private string _currentTerminalName;
        private int _cardMode;
        public CardType CardType { get; private set; }
        //private CardDetails cardDetails;
        internal List<IPlugin> _plugins;
        private IEncoder _encoder;
        private CardTerminalManager _terminalManager;
        //private PluginList pluginList;
        private string _currentSN;
        //private About about;
        private Array terminaleTab;

        public login()
        {
            InitializeComponent();
        }

        private void login_Load(object sender, EventArgs e)
        {

        }

        private void button_login_Click(object sender, EventArgs e)
        {
            int caseSwitch = 0;
            string username = textBox_User.Text;
            string password = textBox_Pass.Text;
            if ((username == user1) && (password == pswd1)) //admin
                caseSwitch = 4;
            if ((username == user2) && (password == pswd2)) //100357
                caseSwitch = 3;
            if ((username == user3) && (password == pswd3)) //122022
                caseSwitch = 3;
            if ((username == user4) && (password == pswd4)) //pw
                caseSwitch = 1;


            switch (caseSwitch) // 1 - prowadzacy; 3 - student, 4 - admin
            {
                case 1:
                    Form1 frm1 = new Form1(username, this);
                    frm1.updateEvent += new EventHandler(handleUpdateEvent);
                    frm1.FormClosed += new FormClosedEventHandler(form_FormClosed);
                    Visible = false;
                    frm1.Show();
                    break;

                case 3:
                    Form3 frm3 = new Form3(username, this);
                    frm3.updateEvent += new EventHandler(handleUpdateEvent);
                    frm3.FormClosed += new FormClosedEventHandler(form_FormClosed);
                    Visible = false;
                    frm3.Show();
                    break;

                case 4:
                    Form4 frm4 = new Form4(username);
                    frm4.updateEvent += new EventHandler(handleUpdateEvent);
                    frm4.FormClosed += new FormClosedEventHandler(form_FormClosed);
                    Visible = false;
                    frm4.Show();
                    break;

                default:
                    System.Windows.Forms.MessageBox.Show("Nieprawidłowy login lub hasło");
                    break;
            }

        }

        void form_FormClosed(object sender, FormClosedEventArgs e)
        {
            //użyc jednej z ponizszych linii

            this.Visible = true;  //dla powracania do logowania po zamknieciu form1
            //this.Close();           //dla zamykania okna logowania po zamknieciu form1
        }


        void handleUpdateEvent(object sender, EventArgs e)
        {
            this.BackColor = Color.Red;
        }

        public IEncoder Encoder
        {
            get
            {
                try
                {
                    if (_encoder == null)
                        InitEncoder();
                }
                catch (Exception e)
                {
                    ResetData();
                }
                return _encoder;
            }
        }
        private void terminalManager_CardRemovedEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            if (aEventArgs.TerminalName == _currentTerminalName)
            {
                ResetData();
                //SetText(labelStatus, "Wyjęto kartę");
                _terminalManager.StopPolling();
            }
        }

        private void InitEncoder()
        {
            //SetEnabled(terminalComboBox, false);
            _currentTerminalName = (string)terminaleTab.GetValue(0);
            //SetText(labelTerminal, _currentTerminalName);
            _encoder = new ContactEncoder();
            //_encoder = new GemCardEncoder();
            _encoder.Initialize(_currentTerminalName);
            _terminalManager.MonitoredTerminalNames.Clear();
            _terminalManager.MonitoredTerminalNames.Add(_currentTerminalName);
            _terminalManager.StartPolling();
        }
        private void ResetData()
        {
            SetATR("");
            SetSN("");
            //SetText(labelTerminal, "");
            //SetText(labelCardType, "");
            _currentTerminalName = "";
            _encoder = null;
            //SetEnabled(terminalComboBox);
            //SetText(labelStatus, "GOTOWY");
            //SetText(connectButton, "Połącz");
            //SetVisible(disconnectButton, false);
        }
        private void SetATR(string atr)
        {
            atr = atr.TrimEnd(' ');
            //if (cardDetails != null) cardDetails.SetATR(atr);
            //SetText(labelATR2, atr);
        }

        private void SetSN(string sn)
        {
            //if (cardDetails != null) cardDetails.SetSN(sn);
            _currentSN = sn;
        }
        /*
        private bool IsCard(CardTypes type)
        {
            if ((_cardMode & (int)type) == (int)type)
            {
                return true;
            }
            return false;
        }
         */
        /*
        private string ReadSN()
        {
            ByteArray response;
            ByteArray SN = new ByteArray();
            if (!_currentTerminalName.Contains("CL") && CardType.CMaid.Length != 0)
            {
                try
                {
                    ByteArray selectCardManager = new ByteArray("00 A4 04 00"); //TODO: Odczyt numeru seryjnego karty przez interfejs stykowy, działające z wieloma rodzajami kart
                    selectCardManager += (byte)CardType.CMaid.Length;
                    selectCardManager += CardType.CMaid;
                    Encoder.SendCommand(selectCardManager);
                    response = Encoder.SendCommand(new ByteArray("80 CA 9F 7F 00"));
                    SN = response.Extract(13, 8);

                    //response = Encoder.SendCommand(new ByteArray("80 c0 02 a1 08"));
                    //SN = response.Extract(0, 8);
                }
                catch
                {
                    SN = new ByteArray();
                }

            }
            else if (_currentTerminalName.Contains("CL"))
            {
                try
                {
                    response = Encoder.SendCommand(new ByteArray("FF CA 00 00 00"));
                    SN = response.Extract(0, 4);
                    _cardMode |= (int)CardTypes.MIFARE;
                }
                catch
                {
                    SN = new ByteArray();
                }
            }
            SetSN(SN.ToString());
            return SN.ToString();
        }
          */
        /*
                private void DetectCard(ByteArray atr)
                {
            
                    string cardType = "";
            
            
                    CardType = CardType.GetCardType(atr);
            
                    //Logger.Log("[MAIN] Łączenie z kartą, ATR: {0}", atr);
            
                    foreach (CardTypes cT in Enum.GetValues(typeof(CardTypes)))
                    {
                        if (IsCard(cT) && cT != CardTypes.Unknown)
                        {
                            cardType += Enum.GetName(typeof(CardTypes), cT) + " ";
                        }
                    }
            
            
                    cardType = cardType.TrimEnd(' ') + " " + CardType.Name;

            
                    //Logger.Log("[MAIN] Typ karty: {0}", cardType);
                    Console.WriteLine(cardType);
                    //SetText(labelCardType, cardType);
            
                    ReadSN();
            
                }
        */
        private string connectCard()
        {
            try
            {
                //SetText(labelStatus, "Odczytuję dane z karty");
                //if (connectButton.Text == "Połącz")
                //{

                InitEncoder();

                //}
                //Application.DoEvents();
                ByteArray ATR = Encoder.ConnectCard(true);
                SetATR(ATR.ToString());
                string output = ATR.ToString();
                return output;
                //Console.WriteLine("ATR: " + ATR);
                //SetText(connectButton, "Reset karty");
                //Console.WriteLine("przed detect card");
                //DetectCard(ATR);
                //Console.WriteLine("po detect card");
                //SetText(labelStatus, "GOTOWY");
                //Console.WriteLine("GOTOWY");
                //SetVisible(disconnectButton);
                //Console.WriteLine("Numer seryjny:" + ReadSN());
            }
            catch (Exception exception)
            {
                //Console.WriteLine("Nie udało się nawiązać połączenia. " + exception.Message, "Błąd");
                ResetData();
                return ("");
            }

        }

        private void disconnectCard()
        {
            try
            {
                CardHandle ch = _terminalManager.Cards[_currentTerminalName];
                ch.CardTerminal.Disconnect(SCardDisposition.EjectCard);
                ResetData();
                //Console.WriteLine("Odłączono kartę");
                _terminalManager.StopPolling();
            }
            catch (Exception exception)
            {
                //Console.WriteLine("Nie udało się odłączyć karty. Wyjmij kartę z czytnika.");
            }
        }


        private void connectReader()
        {
            _terminalManager = CardTerminalManager.Singleton;
            if (_terminalManager.AllTerminalNames.Count < 1)
            {
                //Console.WriteLine("[MAIN] Nie wykryto czytników");
                //if (MessageBox.Show("Do działania tej aplikacji wymagany jest terminal kart elektronicznych. Nie wykryto terminala. Podłącz terminal do komputera a następnie uruchom aplikację ponownie. Czy pomimo tego chcesz włączyć program?", "Nie wykryto terminala", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                //{
                //    Logger.Log("[MAIN] Zamykanie aplikacji");
                //    Application.Exit();
                //}
                //Console.WriteLine("[MAIN] Uruchamianie programu bez czytników");
            }
            else
            {
                //Console.WriteLine("[MAIN] Znalezione czytniki w systemie:");
                foreach (var terminalName in _terminalManager.AllTerminalNames)
                {
                    //Console.WriteLine("[MAIN] {0}", terminalName);
                }
            }
            //_terminalManager.CardInsertedEvent += new CardTerminalEventHandler(terminalManager_CardInsertedEvent);
            _terminalManager.CardRemovedEvent += new CardTerminalEventHandler(terminalManager_CardRemovedEvent);
            _terminalManager.MonitoredTerminalNames.Clear();
            terminaleTab = _terminalManager.AllTerminalNames.ToArray();
            //terminalComboBox.Items.AddRange(_terminalManager.AllTerminalNames.ToArray());
            //try
            //{
            //    terminalComboBox.SelectedIndex = 0;
            //}
            //catch { }

        }

        private void disconnectReader()
        {
            if (_terminalManager.StartedUp) _terminalManager.StopPolling();
            //_terminalManager.CardInsertedEvent -= terminalManager_CardInsertedEvent;
            _terminalManager.CardRemovedEvent -= terminalManager_CardRemovedEvent;
        }
        public string czytajAtr()
        {
            string atr;
            connectReader();
            atr = connectCard();
            disconnectCard();
            disconnectReader();
            return atr;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //label1.Text = czytajAtr();
        }
    }
}
