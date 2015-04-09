#ifdef WIN32
	#include <WinSock2.h>
#endif

#include <iostream>
#include <algorithm>
#include <string>
#include "ControlBox.h"
#include "DebugServer.h"
#include "helpers.h"
#include "PIButton.h"
#include "Photometric.h"

//FW 5.2.2015 - Only LightLifeLogger 
/*#include "DMXClient.h"
#include "ZLLClient.h"
#include "DaliClient.h"
#include "NeoLinkClient.h"*/
#include "LightLifeLogger.h"
#include "RemoteCommands.h"

using namespace std;

#define VERSION "Light Life Control Box\r\nVersion 0.1"

//Forward declarations
char* getCmdOption(char ** begin, char ** end, const std::string & option);
bool cmdOptionExists(char** begin, char** end, const std::string& option);
void printHelp();

#ifdef WIN32
	DWORD WINAPI startLLBox(void* param1);
#else
	int startLLBox(char* param);
#endif

//Testing
void testSendData();
void testPrintData();
string test();

int main(int argc, char * argv[])
{
	if (lumitech::PlatformInit(argv[0]) < 0 ) return -1;

	/*fCieCoords_t xy = { 0.0f, 0.0f };
	int ret = CCTDuv2xy(2500, 0.02, &xy);	// Soll = 0,5232 / 0,4810, Ist=0.52318 / 0.48100 siehe Excel "00 Verifizierung Umrechnungen.xls" im LT.XLL git Source
	ret = CCTDuv2xy(7000, 0.02, &xy);		// Soll= 0,3020 / 0,3536, ist= 0.30132 /  0.35299
	*/

	Logger* log=Logger::getInstance();
	DebugServer *dbgSrv = NULL; // DebugServer::getInstance();

	try
	{
		if (cmdOptionExists(argv, argv + argc, "-help") || cmdOptionExists(argv, argv + argc, "-h") || !cmdOptionExists(argv, argv + argc, "-b"))
		{
			printHelp();
		}
		else //if ((argc==3) && (cmdOptionExists(argv, argv+argc, "-box") || cmdOptionExists(argv, argv+argc, "-b")) )
		{
			InitWiringPi();
			
			string boxName="";		

	#ifdef USE_FTDI
			ftdi::CreateDeviceInfoList();
	#endif


			if (cmdOptionExists(argv, argv+argc, "-box")) boxName = getCmdOption(argv, argv+argc,"-box");
			if (cmdOptionExists(argv, argv+argc, "-b")) boxName = getCmdOption(argv, argv+argc,"-b");

			dbgSrv = DebugServer::getInstance();
			log->addClient((IObserver*)dbgSrv);
				
			//Run in a Thread			
			lumitech::startSequenceThread(&startLLBox, (char*)boxName.c_str());

			//wait for Thread to be finished
			lumitech::waitSequenceThread();	
		}		
	}
	catch(exception& ex)
	{
		log->cout("something happend on the way to heaven");
		log->cout(ex.what());
		//if (dbgSrv) delete dbgSrv;
	}

	if (dbgSrv != NULL)
	{		
		log->removeClients();
		delete dbgSrv;		
	}

#ifdef _DEBUG
	lumitech::waitOnKeyPress();
#endif
	
	log->cout("llbox: stopped!");
 	delete log;

	lumitech::PlatformClose();
	return 0;
}

char* getCmdOption(char ** begin, char ** end, const std::string & option)
{
    char ** itr = std::find(begin, end, option);
    if (itr != end && ++itr != end)
    {
        return *itr;
    }
    return 0;
}

bool cmdOptionExists(char** begin, char** end, const std::string& option)
{
    return std::find(begin, end, option) != end;
}

void printHelp()
{
	cout << VERSION << endl << endl;
	cout << " -help \t\t this help screen" << endl;
	cout << " -box/-b \t Name of box from settings.ini file" << endl;
	//cout << " -light/-l \t Name of PILight from settings.ini file" << endl;
	cout << endl;
}

void runLightLifeBox(string boxName)
{
	ControlBox* box = ControlBox::getInstance(boxName);

	if (box->isDone) 
	{
		//delete box;
		return;
	}		
	lumitech::setSequencePointer((void*) box);

	LightLifeLogger* ll = new LightLifeLogger(box->getName());
	
	if (ll->getCntClients() > 0) box->Lights[0]->addComClient(ll);

	//Prgram loops here endlessly
	box->EventLoop();


	box->Lights[0]->removeComClients();

	delete ll;
	delete box;
}

#ifdef WIN32
DWORD WINAPI startLLBox(void* param1)
{		 
	string boxName((char*) param1);

	runLightLifeBox(boxName);

	return 0;
}
#else
int startLLBox(char* param)
{		 
	string s(param);

	runLightLifeBox(s);

	return 0;
}
#endif


