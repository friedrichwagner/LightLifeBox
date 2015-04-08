﻿using LightLifeAdminConsole.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using LightLife.Data;
using Lumitech.Helpers;

namespace LightLifeAdminConsole
{    
    public enum BoxPotis : byte { BRIGHTNESS, CCT, JUDD };

    class Box2 //: IObserver<RemoteCommandData>
    {
        public static int VLID = 0;   //Damit man es zur Box schicken kann
        public static IDictionary<int, Box2> boxes = new Dictionary<int, Box2>();
        public static IDictionary<int, BoxWindow> boxWindows = new Dictionary<int, BoxWindow>();

        private const byte NUM_POTIS = 3;
        private const byte NUM_BUTTONS = 2;
        private const int DEFAULT_CCT = 4000; //K
        private const int DEFAULT_BRIGHTNESS = 125; //255:2

        public int BoxNr { get; private set; }       
        public IPAddress BoxIP { get; private set; }
        public int GroupID { get; private set; }
        public bool IsActive { get; private set; }
        private AdminBase boxdata;
        private LLRemoteCommand rCmd;

        //private byte[] PotisActive = new byte[NUM_POTIS];
        //private byte[] ButtonsActive = new byte[NUM_BUTTONS];

        private LLMsgType lastmsgtype;
        private int lastBrightness;
        private static Logger log;

        public LLTestSequence testsequence;

        public static void getBoxes()
        {
            AdminBase boxes = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLBox"]);
            DataTable dt = boxes.select("where active=1");
            
            for (int i=0; i < dt.Rows.Count; i++)
            {
                Box2.boxes.Add(dt.Rows[i].Field<int>("BoxID"), new Box2(dt.Rows[i].Field<int>("BoxID")));
                Box2.boxWindows.Add(dt.Rows[i].Field<int>("BoxID"), new BoxWindow(dt.Rows[i].Field<int>("BoxID")));
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

                rCmd = new LLRemoteCommand(BoxIP, sendport, recvport, true);
                rCmd.ReceiveData += ReceiveDatafromControlBox;
            }

            IsActive = rCmd.Ping(GroupID);

            testsequence = new LLTestSequence(boxnr, -1);

            if (IsActive)
                EnableBoxButtons();
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
            BoxIP = IPAddress.Loopback;
            lastmsgtype = LLMsgType.LL_NONE;
            lastBrightness = 0;              
        }

        public void ReloadSequence(int pSeqid)
        {
            testsequence = new LLTestSequence(BoxNr, pSeqid);

            if (testsequence.SequenceID != pSeqid)
                throw new ArgumentException("Sequence ID does not exist (on this Box)!");
        }

        public bool Ping()
        {
            IsActive = rCmd.Ping(GroupID);
            return IsActive;
        }

        public void Close()
        {
            rCmd.Close();
        }


        public void Refresh()
        {
            //EnableBoxButtons();
            UpdateVarious(lastmsgtype, lastBrightness);
        }
     

        private void EnableBoxButtons()
        {
            string Params = ";buttons=" + testsequence.EnabledButtons;

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
                        //NextStep();
                        break;

                    case (int)LLMsgType.LL_SET_DEFAULT:                        
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

        private void SetBoxToDefault(int brightnessLevel)
        {
            //SetCCT + brightness
            rCmd.SetPILED(PILEDMode.SET_CCT, brightnessLevel, DEFAULT_CCT, new int[] { 0, 0, 0 }, new float[] { 0f, 0f });
        }

        private void SetBoxTestSequenceState(LLMsgType msgtype)
        {
            string Params = ";userid=" + testsequence.ProbandID + ";vlid=" + Box2.VLID + ";sequenceid=" + testsequence.SequenceID + ";stepid=" + testsequence.StepID + ";msgtype=" + ((int)msgtype).ToString();
            rCmd.SetSequence(Params);
        }

        private void UpdateVarious(LLMsgType msgtype, int brightness)
        {
            //if (State > BoxStatus.NONE)
            {
                lastmsgtype = msgtype;
                lastBrightness = brightness;

                SetBoxTestSequenceState(msgtype);
                SetBoxToDefault(brightness); //Set Box to Default Settings
                EnableBoxButtons(); // Send Controlbox Buttons
                //testsequence.UpdateHeadState(); // Update Database Table TestSequenceHeader
            }
        }
    }
}
