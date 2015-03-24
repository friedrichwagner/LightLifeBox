using System;
using Lumitech.Helpers;
using System.Threading;
using System.Collections.Concurrent;
using LightLife.Data;
using System.Diagnostics;

namespace Lumitech.Interfaces
{
    class LightLifeLogger: IObserver<LightLifeData>
    {        
        private string name;
        private LTSQLCommand cmd;
        private IDisposable cancellation;
        private const string sqlInsert = "insert into LLData(roomID, userID, VLID, SceneID, SequenceID, StepID, Brightness, CCT, duv, x, y, pimode, sender, receiver, MsgTypeID, Remark, IP, groupiD)" +
                                         " values (:1, :2,:3,:4,:5,:6,:7,:8,:9,:10,:11,:12,:13,:14,:15,:16,:17,:18)";
 
        private Logger log;
        private ConcurrentQueue<LightLifeData> dataQueue;
        private Thread logThread; 
        private bool done ;
        public EventWaitHandle _waitHandle = new AutoResetEvent(false);

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

        public virtual void Subscribe(IObservable<LightLifeData> provider)
        {
            cancellation = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            done=true;
            _waitHandle.Close();
            if (logThread.IsAlive) logThread.Join();
            cancellation.Dispose();
        }

        //Called from UDP Server when closing application
        public virtual void OnCompleted()
        {
            done = true;
            _waitHandle.Close();
            if (logThread.IsAlive) logThread.Join();
        }

        public virtual void OnError(Exception e)
        {
            Debug.Print(e.Message);
        }

        //Called from UDP Server when new data arrive
        public virtual void OnNext(LightLifeData info)
        {
            //push to queue
            dataQueue.Enqueue(info);
            _waitHandle.Set();
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
                    _waitHandle.WaitOne();

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
                        cmd.Params[i++] = info.stepid;
                        cmd.Params[i++] = info.piled.brightness;
                        cmd.Params[i++] = info.piled.cct;
                        cmd.Params[i++] = info.piled.duv;
                        cmd.Params[i++] = info.piled.xy[0];
                        cmd.Params[i++] = info.piled.xy[1];
                        cmd.Params[i++] = info.piled.mode.ToString();
                        cmd.Params[i++] = info.piled.sender;
                        cmd.Params[i++] = info.piled.receiver;
                        cmd.Params[i++] = (int)info.piled.msgtype;                        
                        cmd.Params[i++] = info.remark;
                        cmd.Params[i++] = info.ip;
                        cmd.Params[i++] = info.piled.groupid;

                        //FW 16.2.2015: auskommentiert, damit nicht soviel in die DB geschreiben wird beim Testen
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
                _waitHandle.Close();
                if (cmd.Connection.State == System.Data.ConnectionState.Open) 
                        cmd.Connection.Close();
            }
        }
    }
}
