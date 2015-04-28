using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using PILEDServer;

namespace PILEDServerSvc
{
    static class Program
    {        
        static void Main(string[] args)
        {
#if (!DEBUG)
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new PILEDService()  };
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#else
            // Debug code: this allows the process to run as a non-service.
            // It will kick off the service start point, but never kill it.
            // Shut down the debugger to exit
            PILEDService service = new PILEDService();
            service.doStart();

            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#endif 
        }
    }

    public class PILEDService : ServiceBase
    {
        private UDPServer UDPSrv;

        public PILEDService()
        {
            this.ServiceName = "LightLifeService";
            UDPSrv = new UDPServer();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            doStart();
        }

        protected override void OnStop()
        {
            base.OnStop();

            doStop();

        }

        public void doStart()
        {
            if (!UDPSrv.isStarted)
                UDPSrv.Start();
        }

        public void doStop()
        {
            if (UDPSrv.isStarted)
                UDPSrv.Stop();
        }
    }
}
