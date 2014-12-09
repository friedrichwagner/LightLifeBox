using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Lumitech.Helpers;
using System.Threading;
using System.Collections.Concurrent;
using PILEDServer;

namespace Lumitech.Interfaces
{
    class LightLifeLogger: IObserver<LightLifeData>
    {        
        private string name;
        private LTSQLCommand cmd;
        private IDisposable cancellation;
        private const string sqlInsert = "insert into LLData(roomID, userID, VLID, SceneID, SequenceID, Brightness, CCT, x, y, pimode, sender, receiver, MsgTypeID, Remark)" +
                                          " values (:1, :2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14)";

        private Logger log;
        private ConcurrentQueue<LightLifeData> dataQueue;
        private Thread logThread; 
        private bool done ;

        public LightLifeLogger()
        {
            log = Logger.GetInstance();
            Settings ini = Settings.GetInstance();

            dataQueue = new ConcurrentQueue<LightLifeData>();
           
            name = ini.ReadAttrib<string>("LightLifeLogger", "name", "");

            done = false;
            logThread = new Thread(new ThreadStart(StartLogger));
            logThread.Start();
        }

        public virtual void Subscribe(UDPServer provider)
        {
            cancellation = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            done=true;
            if (logThread.IsAlive) logThread.Join();
            cancellation.Dispose();
        }

        //Called from UDP Server when closing application
        public virtual void OnCompleted()
        {
            done = true;
            if (logThread.IsAlive) logThread.Join();
        }

        public virtual void OnError(Exception e)
        {

        }

        //Called from UDP Server when new data arrive
        public virtual void OnNext(LightLifeData info)
        {
            //push to queue
            dataQueue.Enqueue(info);
        }

        private void StartLogger()
        {
            try
            {
                LightLifeData info;
                cmd = new LTSQLCommand();   //connect to <Database> implizit
                cmd.Connection.Open();

                while (!done)
                {
                    while (dataQueue.TryDequeue(out info))
                    {
                        string stmt = sqlInsert;
                        cmd.prep(stmt);
                        int i = 0;
                        cmd.Params[i++] = info.roomid;
                        cmd.Params[i++] = info.userid;
                        cmd.Params[i++] = info.vlid;
                        cmd.Params[i++] = info.sceneid;
                        cmd.Params[i++] = info.sequenceid;
                        cmd.Params[i++] = info.piled.brightness;
                        cmd.Params[i++] = info.piled.cct;
                        cmd.Params[i++] = info.piled.xy[0];
                        cmd.Params[i++] = info.piled.xy[1];
                        cmd.Params[i++] = (int)info.piled.mode;
                        cmd.Params[i++] = info.piled.sender;
                        cmd.Params[i++] = info.piled.receiver;
                        cmd.Params[i++] = (int)info.piled.msgtype;                        
                        cmd.Params[i++] = info.remark;
                        cmd.Exec();

                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex.Message);

            }
            finally
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Open) 
                        cmd.Connection.Close();
            }
        }
    }
}
