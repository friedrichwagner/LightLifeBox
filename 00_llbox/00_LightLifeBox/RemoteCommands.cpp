#include "RemoteCommands.h"
#include <sstream>
#include <mutex>
#include <condition_variable>
#include "LightLifeLogger.h"

using namespace std;

mutex mtx;
condition_variable cv;
bool ready = false;

RemoteCommands::RemoteCommands(ControlBox* pBox)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();
	box = pBox;
	done = false;
	cmdQ = new queue<RemoteCommand>();

	_sendSock = new UDPSendSocket(ini->Read<string>("ControlBox_General", "Admin-Console-IP", "127.0.0.1"), ini->Read<int>(box->getName(), "Send-Command-Port", 1748));
	int p = ini->Read<int>(box->getName(), "Receive-Command-Port", 1749);
	_recvSock = new UDPRecvSocket(p);

	lllogger = NULL;
}


void RemoteCommands::NowAddLLLogger()
{
	lllogger = (LightLifeLogger*)box->Lights[0]->getComClient(0); // Assume LightLifeLogger is on pos=0

	if (lllogger == NULL)
	{
		log->cout("RemoteCommands: LightLifeLogger is null");

	}
	else if (lllogger->clientType != CLIENT_LIGHTLIFE)
	{
		log->cout("RemoteCommands: Client is not LightLifeLogger");
		lllogger = NULL;
	}
}

RemoteCommands::~RemoteCommands()
{	
	stop();
	delete cmdQ;
}

void RemoteCommands::stop()
{
	done = true;
	go();	

	delete _recvSock;
	delete _sendSock;	

	if (threadPull.joinable())  threadPull.join();
	if (threadPush.joinable())  threadPush.join();
}

void RemoteCommands::start()
{
	spawnPushThread();
	spawnPullThread();
}

int RemoteCommands::send(RemoteCommand cmd)
{
	int ret = -1;

	if (_sendSock->isValid)
	{
		//log->cout("send:" + cmd.cmdParams);
		memset(&sendBuf[0], 0, recvBufSize);
		int len = cmd.ToString(&sendBuf[0]);
		int ret =_sendSock->send(&sendBuf[0], len); 

		if (ret != len)
		{
			int errnr = errno;	
			log->cout("RemoteCommand: send erronr:" + lumitech::itos(errnr));
			return ret;
		}
	}

	return ret;
}


unsigned long RemoteCommands::Push(void)
{
	log->cout("RemoteCommand PUSH Thread started ...");
	while (!done)
	{
		if (_recvSock->isValid)
		{
			//blocking
			memset(&recvBuf[0], 0, recvBufSize);
			int ret=_recvSock->receive(&recvBuf[0], recvBufSize);
			if (ret <= 0)
			{

				done = true;
				break;
			}

			try
			{
				RemoteCommand cmd1;
				//cmd1.cmdId = recvBuf[0] - char('0'); //Command kommt als ASCII "1" = 49 dezimal --> 49-48=1
				cmd1.cmdParams = string((const char*)&recvBuf[0]);
				cmd1.flds = cmd1.cmdParams.split2map(';', '=');
				cmd1.cmdId = lumitech::stoi(cmd1.flds["CmdId"]);

				log->cout("RCmd:" + lumitech::itos(cmd1.cmdId) + " Params:" + cmd1.cmdParams);

				if (cmd1.cmdId > 0)
					cmdQ->push(cmd1);

				go();
			}
			catch (exception ex)
			{
				log->cout(ex.what());
			}
		}
		else
			done = true;
	}

	log->cout("RemoteCommand PUSH Thread stopped!");

	return 0;
}

unsigned long RemoteCommands::Pull(void)
{
	unique_lock<std::mutex> lck(mtx);

	log->cout("RemoteCommand PULL Thread started ...");
	while (!done)
	{
		//while (!ready) 
		cv.wait(lck);

		if (!cmdQ->empty())
		{
			ExecuteReceiveCommands(cmdQ->front());
			cmdQ->pop();
		}
	}

	log->cout("RemoteCommand PULL Thread stopped!");

	return 0;
}

void RemoteCommands::go()
{
	unique_lock<std::mutex> lck(mtx);
	ready = true;
	cv.notify_all();
}


//----------------------------------
//    Remote Commands Handling
//----------------------------------
void RemoteCommands::ExecuteReceiveCommands(RemoteCommand cmd)
{
	ostringstream s;

	switch (cmd.cmdId)
	{
	case LL_DISCOVER:
		DiscoverCommand(cmd);
		
		//FW 9.4.2015 Von hier jetzt die remote Adresse und das Port nehmen
		//to be tested
		_sendSock->setServerAddress(_recvSock->remoteAddress);
		break;

	case LL_ENABLE_BUTTONS:
		EnableButtonsCommand(cmd);
		break;
	case LL_SET_PILED:
		SetPILEDCommand(cmd);
		break;
	case LL_GET_PILED:
		GetPILEDCommand(cmd);
		break;
	case LL_SET_SEQUENCEDATA:
		SequenceHandlingCommand(cmd);
		break;


	default:
		s << "ExecuteCommand: unknown command:" << cmd.cmdId << " Data:" << cmd.cmdParams;
		log->error(s.str());
		break;
	}
}

void RemoteCommands::SendRemoteCommand(LLMsgType cmdId, string params)
{
	//Hier in Queue schreiben oder gleich schicken ??
	//cmdQ->push(cmd);
	//go();

	ostringstream s;

	switch (cmdId)
	{
	case LL_SET_LOCKED:
		SendLock(params);
		break;

	default:
		s << "ExecuteCommand: unknown command:" << cmdId << " Data:" << params;
		log->error(s.str());
		break;
	}
}

void RemoteCommands::DiscoverCommand(RemoteCommand cmd)
{
	splitstring s = cmd.cmdParams;
	map<string, string> flds = s.split2map(';', '=');

	if (flds.size() < 1) return;

	//Gruppe setzen die von dieser Box gesteuert wird
	box->Lights[0]->setGroup((char)lumitech::stoi(flds["groupid"]));

	//Send Back Name of Controlbox
	StandardAnswer(cmd);
}
void RemoteCommands::SequenceHandlingCommand(RemoteCommand cmd)
{
	splitstring s = cmd.cmdParams;
	map<string, string> flds = s.split2map(';', '=');
	
	if (lllogger != NULL)
	{
		lllogger->lldata->roomid = box->ID;
		lllogger->lldata->userid = lumitech::stoi(flds["userid"]);
		lllogger->lldata->vlid = lumitech::stoi(flds["vlid"]);
		lllogger->lldata->sequenceid = lumitech::stoi(flds["sequenceid"]);
		lllogger->lldata->stepid = lumitech::stoi(flds["stepid"]);
		lllogger->lldata->activationState = (ActivationState)lumitech::stoi(flds["activationstate"]);
		lllogger->lldata->msgtype = (LLMsgType)lumitech::stoi(flds["msgtype"]);

		lllogger->logLLEvent();
	}
	else
	{
		log->error("RemoteCommands: LightLifeLogger is null");
	}

	StandardAnswer(cmd);
}

void RemoteCommands::EnableButtonsCommand(RemoteCommand cmd)
{
	//buttons=00;potis=1111;
	splitstring s = cmd.cmdParams;
	map<string, string> flds = s.split2map(';','=');


	//Need to change admin Console 
 	for (unsigned int i = 0; i < box->Buttons.size(); i++)
	{
		if (flds["buttons"].at(i) == '1')
			box->Buttons[i]->setActive(true);
		else
			box->Buttons[i]->setActive(false);
	}

	if (lllogger != NULL)
	{
		lllogger->lldata->msgtype = LL_ENABLE_BUTTONS;
		lllogger->logLLEvent();
	}
	else
	{
		log->error("RemoteCommands: LightLifeLogger is null");
	}


	StandardAnswer(cmd);

	//Reset to Default when new Buttons were sent
	//box->Lights[0]->resetDefault();
}

void RemoteCommands::SetPILEDCommand(RemoteCommand cmd)
{
	splitstring s = cmd.cmdParams;
	map<string, string> flds = s.split2map(';', '=');

	int pimode = lumitech::stoi(flds["mode"]);
	unsigned int br = lumitech::stoi(flds["brightness"]);
	unsigned int cct = lumitech::stoi(flds["cct"]);
	unsigned int rgb[3];
	rgb[0] = lumitech::stoi(flds["r"]); rgb[1] = lumitech::stoi(flds["g"]); rgb[2] = lumitech::stoi(flds["b"]);

	float xy[2];
	xy[0] = lumitech::stof(flds["x"]); xy[1] = lumitech::stof(flds["y"]);

	switch (pimode)
	{
	case PILED_SET_CCT:
			box->Lights[0]->setCCT(cct);
			box->Lights[0]->setBrightness(br);
			break;

	case PILED_SET_XY:
			box->Lights[0]->setXY(xy);
			box->Lights[0]->setBrightness(br);

			break;

	case PILED_SET_RGB:
			box->Lights[0]->setRGB(rgb);
			break;
	}

	StandardAnswer(cmd);
}

void RemoteCommands::GetPILEDCommand(RemoteCommand cmd)
{
	cmd.cmdParams =";" + box->Lights[0]->getFullState();
	send(cmd);
}

void RemoteCommands::StandardAnswer(RemoteCommand cmd)
{
	//cmd.cmdParams = "CmdId=" + lumitech::itos(cmd.cmdId) + ";BoxNr=" + lumitech::itos(box->ID);
	cmd.cmdParams = ";BoxNr=" + lumitech::itos(box->ID) + ";BoxName=" + box->Name;
	send(cmd);
}

//----------------------------------
//    Send Remote Commands
//----------------------------------
void RemoteCommands::SendLock(string params)
{														// do not expect any additional parameters here
	RemoteCommand cmd;
	cmd.cmdId = LL_SET_LOCKED;
	ostringstream ss;
	ss << ";BoxNr=" << box->ID << ";" << box->Lights[0]->getFullState();

	cmd.cmdParams = ss.str();
	send(cmd);
}
