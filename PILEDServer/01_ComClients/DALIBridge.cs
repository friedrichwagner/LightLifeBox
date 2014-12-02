using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace PiledToolbox.Interfaces
{
    class DALIBridge : SerialPort
    {
        static DALIBridge daliBridgeInstance = null;
        public bool DaliBridgeOpened = false;
        public bool DaliBackwardFrameNeeded = false;
        public bool CmdSeriesStarting = false;
        public byte DaliRecData = 0;
        public string DaliReceptionState = "";
        private List<byte> daliWrongData = new List<byte>();
        private byte daliDataReceived = 0;

        System.Windows.Forms.Timer daliWaitForQueryAnswerTimer = new System.Windows.Forms.Timer();


       
        private DALIBridge() 
        {
            daliWaitForQueryAnswerTimer.Interval = 75;
            daliWaitForQueryAnswerTimer.Tick += new EventHandler(daliWaitForQueryAnswerTimer_Tick);
            this.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            this.DtrEnable = true;
        }


        private DALIBridge(string comPort, int baudRate, int parityValue, int dataBits, int stopBits) : this()
        {
            this.PortName = comPort;
            this.BaudRate = baudRate;
            this.DataBits = dataBits;
            switch (parityValue)
            {
                case 0:
                    this.Parity = Parity.None;
                    break;

                case 1:
                    this.Parity = Parity.Odd;
                    break;

                case 2:
                    this.Parity = Parity.Even;
                    break;
            }
            switch (stopBits)
            {
                case 1:
                    this.StopBits = StopBits.One;
                    break;

                case 2:
                    this.StopBits = StopBits.Two;
                    break;
            }
        }



        //check if DALI bridge instance already existing
        public static DALIBridge Instance(string comPort, int baudRate, int parityValue, int dataBits, int stopBits)
        {
            if (daliBridgeInstance == null)
            {
                daliBridgeInstance = new DALIBridge(comPort, baudRate, parityValue, dataBits, stopBits);
            }
            return daliBridgeInstance;
        }




        public void DeleteInstance()
        {
            daliBridgeInstance = null;
        }



        //Open COM port
        public void OpenPort()
        {
            try
            {
                if (this.IsOpen == false)
                    this.Open();
                DaliBridgeOpened = true;
            }
            catch
            {
                DaliBridgeOpened = false;
                //Error 1: Opening COM port
            }
        }



        //Close COM port
        public void ClosePort()
        {
            try
            {
                if (this.IsOpen == true)
                    this.Close();
                DaliBridgeOpened = false;
            }
            catch
            {
                //Error 2: Closing COM port
            }
        }




        //send 2-Byte-Array to DALI bridge
        public void SendData(byte[] bytesToSend)
        {
            try
            {
                if (DaliBridgeOpened == true)
                {
                    if (DaliBackwardFrameNeeded == true)
                    {
                        //start 50ms-timer - time for sending and waiting for backward frame
                        daliWaitForQueryAnswerTimer.Start();
                    }
                    this.Write(bytesToSend, 0, bytesToSend.Length);

                }
                else
                {
                    //Error 3: DALI bridge not ready
                }
            }
            catch
            {
                //Error 4: Error when sending data over DALI bridge
            }
        }




        //Receive backward frame from DALI bridge --> 1 Byte
        private void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            daliWaitForQueryAnswerTimer.Stop();
            if (DaliBackwardFrameNeeded == true)
            {
                //--> read byte only if reception happens in time
                try
                {
                    if (this.BytesToRead > 1)
                    {
                        daliWrongData.Clear();
                        for (int i = 0; i < this.BytesToRead; i++)
                        {
                            daliWrongData.Add((byte)this.ReadByte());
                        }
                        DaliReceptionState = "ERROR - More than 1 byte received!";
                        //Error 5: Reception of wrong data size
                    }
                    else if (this.BytesToRead == 0)
                    {
                        DaliReceptionState = "ERROR - No byte received!";
                    }
                    else
                    {
                        daliDataReceived = (byte)this.ReadByte();
                        DaliRecData = daliDataReceived;
                        DaliReceptionState = "Received answer: " + DaliRecData.ToString();
                    }
                }
                catch
                {
                    //Error 6: Error when receiving data over DALI bridge
                }
            }
            else
            {
                if (this.BytesToRead > 1)
                {
                    daliWrongData.Clear();
                    for (int i = 0; i < this.BytesToRead; i++)
                    {
                        daliWrongData.Add((byte)this.ReadByte());
                    }
                }
                //Error 7: timeout for data reception
            }
        }



        //timer for backward frame reception
        private void daliWaitForQueryAnswerTimer_Tick(object sender, EventArgs e)
        {
            daliWaitForQueryAnswerTimer.Stop();
            DaliReceptionState = "ERROR - Timeout for data reception!";
        }
    }
}
