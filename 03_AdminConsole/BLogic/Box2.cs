using LightLifeAdminConsole.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using LightLife.Data;
using Lumitech.Helpers;
using System.Linq;

namespace LightLifeAdminConsole
{    
    //public enum BoxPotis : byte { BRIGHTNESS, CCT, JUDD };

    class Box2 //: IObserver<RemoteCommandData>
    {
        public static int VLID = 0;   //Damit man es zur Box schicken kann
        public static IDictionary<int, Box2> boxes = new Dictionary<int, Box2>();
        public static IDictionary<int, BoxWindow> boxWindows = new Dictionary<int, BoxWindow>();
        public static IDictionary<int, PracticeBoxWindow> practiceboxWindows = new Dictionary<int, PracticeBoxWindow>();

        private const byte NUM_POTIS = 3;
        private const byte NUM_BUTTONS = 2;
        private const int DEFAULT_CCT = 4000; //K
        private const int DEFAULT_BRIGHTNESS = 25; //100= ca. 2000lx --> 500lx= ca. 25
        private const int DEFAULT_NEOLINK_FADETIME = 300; //ms
        private const int WAIT_FADETIME = 30000; //30 sek
        private const string ALL_BUTTONS_DISABLED = "00000";


        public int BoxNr { get; private set; }
        public string Name { get; private set; }     
        public IPAddress BoxIP { get; private set; }
        public int GroupID { get; private set; }
        public bool IsActive { get; private set; }
        private AdminBase boxdata;
        public LLRemoteCommand rCmd { get; private set; }

        public bool IsPracticeBox { get; private set; }

        //private byte[] PotisActive = new byte[NUM_POTIS];
        //private byte[] ButtonsActive = new byte[NUM_BUTTONS];

        private LLMsgType lastmsgtype;
        private int lastBrightness;
        private static Logger log;

        public LLTestSequence testsequence;
        public DeltaTest dTest;

        public static void getBoxes()
        {
            AdminBase boxes = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLBox"]);
            DataTable dt = boxes.select("where active=1");
            
            //"Echte" Testboxen in Testraum 1 und Testraum 2
            for (int i=0; i < dt.Rows.Count; i++)
            {
                Box2.boxes.Add(dt.Rows[i].Field<int>("BoxID"), new Box2(dt.Rows[i].Field<int>("BoxID")));

                if (dt.Rows[i].Field<int>("isPracticeBox")==0)
                    Box2.boxWindows.Add(dt.Rows[i].Field<int>("BoxID"), new BoxWindow(dt.Rows[i].Field<int>("BoxID")));
                else
                    Box2.practiceboxWindows.Add(dt.Rows[i].Field<int>("BoxID"), new PracticeBoxWindow(dt.Rows[i].Field<int>("BoxID")));
            }
        }

        public Box2(int boxnr)
        {
            log = Logger.GetInstance();

            boxdata = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLBox"]);

            InitBox(boxnr);

            DataTable dt = boxdata.select("where boxID=" + boxnr.ToString());

            if (dt.Rows.Count > 0)
            {
                //IsActive = (dt.Rows[0].Field<int>("active") == 1) ? true : false;
                BoxIP = IPAddress.Parse(dt.Rows[0].Field<string>("BoxIP"));
                GroupID = dt.Rows[0].Field<int>("GroupID");
                int sendport = dt.Rows[0].Field<int>("sendPort");
                int recvport = dt.Rows[0].Field<int>("recvPort");
                Name = dt.Rows[0].Field<string>("Name");

                IsPracticeBox = (dt.Rows[0].Field<int>("IsPracticeBox") ==1) ? true : false;

                rCmd = new LLRemoteCommand(BoxIP, sendport, recvport, true);
                rCmd.ReceiveData += ReceiveDatafromControlBox;
            }

            //Hier noch nicht "pingen", erstge bei DoppelKlick auf der Form
            //IsActive = rCmd.Ping(GroupID, IsPracticeBox);
            IsActive = false;

            testsequence = new LLTestSequence(boxnr, -1);

            if (IsActive)
            {
                if (IsPracticeBox)
                    EnableBoxButtons("11111", ALL_BUTTONS_DISABLED); //Alle Buttons aktivieren
                else
                    EnableBoxButtons(testsequence.EnabledButtons, ALL_BUTTONS_DISABLED);
            }
        }

        /*public Box2(DataTable dt)
        {
            log = Logger.GetInstance();
            InitBox(dt.Rows[0].Field<int>("BoxID"));
            InitSequence(dt);
        }*/

        private void InitBox(int boxnr)
        {
            BoxNr = boxnr;
            GroupID = 0;
            IsActive = false;
            IsPracticeBox = false;
            BoxIP = IPAddress.Loopback;
            lastmsgtype = LLMsgType.LL_NONE;
            lastBrightness = 0;
            dTest = null;
        }

        public void ReloadSequence(int pSeqid)
        {
            LLTestSequence Newtestsequence = new LLTestSequence(BoxNr, pSeqid);

                if (Newtestsequence.SequenceID != pSeqid)            
                throw new ArgumentException("Sequence ID does not exist (on this Box)!");

            testsequence = Newtestsequence;
        }

        public bool Ping()
        {
            IsActive = rCmd.Ping(GroupID, IsPracticeBox);
            return IsActive;
        }

        public void Close()
        {
            rCmd.Close();
        }

        public void Refresh()
        {
            //UpdateVarious(lastmsgtype, lastBrightness);
            ReloadSequence(testsequence.SequenceID);
        }
     

        private void EnableBoxButtons(string enabledButtons, string blinkleds)
        {
            string Params = ";buttons=" + enabledButtons + ";blinkleds=" + blinkleds;

           rCmd.EnableButtons(Params);           
        }

        private void ReceiveDatafromControlBox(RemoteCommandData rCmd)
        {
            try
            {
                switch (rCmd.cmdId)
                {
                    case (int)LLMsgType.LL_DISCOVER:
                        IsActive = true;
                        break;
                    case (int)LLMsgType.LL_SET_LOCKED:
                        testsequence.UpdatePos(rCmd.cmdParams);
                        break;

                    case (int)LLMsgType.LL_SET_DEFAULT:                        
                        break;

                    case (int)LLMsgType.LL_SET_LOCKED_DELTATEST:
                        if (dTest != null) 
                            dTest.Save(rCmd.cmdParams);
                        break;

                    case (int)LLMsgType.LL_AFTER_WAIT_TIME:
                        CBoxSetPILed(PILEDMode.SET_CCT, DEFAULT_BRIGHTNESS, DEFAULT_CCT, WAIT_FADETIME);

                        //Nächsten Schritt schon hier holen, damit man die richtige LED blinken lassen kann
                        if (testsequence.Next())
                        {
                            System.Threading.Thread.Sleep(100);

                            //Buttons deaktivieren, Nächste LED blinkt                            
                            string blink = LLSQL.llstep.Select("Stepid=" + (int)testsequence.StepID)[0].Field<string>("EnabledButtons");
                            EnableBoxButtons(ALL_BUTTONS_DISABLED, blink);
                        }
                        else
                        {
                            testsequence.Stop();
                        }
                        break;


                    case (int)LLMsgType.LL_AFTER_FADE_TIME:
                            CBoxSetPILed(PILEDMode.SET_CCT, DEFAULT_BRIGHTNESS, DEFAULT_CCT, DEFAULT_NEOLINK_FADETIME);
                            System.Threading.Thread.Sleep(100);
                            CBoxSetSequenceStep();
                            System.Threading.Thread.Sleep(100);
                            EnableBoxButtons(testsequence.EnabledButtons, ALL_BUTTONS_DISABLED);
                        break;

                    default:
                        
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void CBoxSetPILed(PILEDMode mode, int brightnessLevel, int cct, int fadetime)
        {
            //SetCCT + brightness
            rCmd.SetPILED(mode, brightnessLevel, cct, new int[] { 0, 0, 0 }, new float[] { 0f, 0f }, fadetime, 0.0f);
        }

        private void CBoxSetSequenceStep()
        {
            string Params = ";userid=" + testsequence.ProbandID + ";vlid=" + Box2.VLID + ";cycleid=" + testsequence.CycleID+ ";sequenceid=" + testsequence.SequenceID
                            + ";posid=" + testsequence.PosID + ";stepid=" + testsequence.StepID + ";activationstate=" + testsequence.ActivationID + ";msgtype=" + ((int)LLMsgType.LL_SET_SEQUENCEDATA).ToString();
            rCmd.SetSequence(Params);
        }

        /*private void UpdateVarious(LLMsgType msgtype, int brightness)
        {
            //if (State > BoxStatus.NONE)
            {
                lastmsgtype = msgtype;
                lastBrightness = brightness;

                SetBoxTestSequenceState(msgtype);
                SetBoxToDefault(brightness);    //Set Box to Default Settings
                if (IsPracticeBox)
                    EnableBoxButtons("11111", "00000"); //Alle Buttons aktivieren
                else
                    EnableBoxButtons(testsequence.EnabledButtons, "00000");           // Send Controlbox Buttons
            }
        }*/

    }
}

