#pragma once

#include "RemoteCommands.h"
#include <sstream>
#include <mutex>

using namespace std;

mutex mtx;

RemoteCommands::RemoteCommands(ControlBox* pBox)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();
	box = pBox;
	done = false;
	cmdQ = new queue<RemoteCommand>();

	_sendSock = new UDPSendSocket(ini->Read<string>("LIGHTLIFESERVER", "IP-Address", "127.0.0.1"), ini->Read<int>("LIGHTLIFESERVER", "CommandPort", 9998));
	_recvSock = new UDPRecvSocket(ini->Read<int>("ControlBox", "CommandPort", 9998));
}

RemoteCommands::~RemoteCommands()
{
	stop();
	delete cmdQ;
}

void RemoteCommands::stop()
{
	done = true;
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
	if (_sendSock->isValid)
	{
		memset(&sendBuf[0], 0, recvBufSize);
		int len = cmd.ToString(&sendBuf[0]);
		return _sendSock->send(&sendBuf[0], len); 
	}

	return -1;
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
			int ret = _recvSock->receive(&recvBuf[0], recvBufSize);

			RemoteCommand cmd1;
			cmd1.cmdId = recvBuf[0];
			cmd1.cmdParams = string((const char*)&recvBuf[1]);

			if (cmd1.cmdId > 0)
				cmdQ->push(cmd1);
		}
		else
			done = true;
	}

	log->cout("RemoteCommand PUSH Thread stopped!");

	return 0;
}

unsigned long RemoteCommands::Pull(void)
{
	log->cout("RemoteCommand PULL Thread started ...");
	while (!done)
	{
		if (!cmdQ->empty())
		{
			ExecuteCommands(cmdQ->front());
			cmdQ->pop();
		}
	}

	log->cout("RemoteCommand PULL Thread stopped!");

	return 0;
}

void RemoteCommands::ExecuteCommands(RemoteCommand cmd)
{
	ostringstream s;

	switch (cmd.cmdId)
	{
	case REMOTECMD_DISCOVER:
		DiscoverCommand(cmd);
		break;
	case REMOTECMD_ENABLE_BUTTONS:
		EnableButtonsCommand(cmd);
		break;
	case REMOTECMD_SET_PILED:
		SetPILEDCommand(cmd);
		break;
	case REMOTECMD_GET_PILED:
		GetPILEDCommand(cmd);
		break;

	default:
		s << "ExecuteCommand: unknown command:" << cmd.cmdId << " Data:" << cmd.cmdParams;
		log->error(s.str());
		break;
	}
}

//------------------------
//Remote Commands
//------------------------
void RemoteCommands::DiscoverCommand(RemoteCommand cmd)
{
	//Send Back Name of Controlbox
	cmd.cmdParams = "BoxNr=" + lumitech::itos(box->ID) + ";BoxName=" + box->Name;
	send(cmd);
}

void RemoteCommands::EnableButtonsCommand(RemoteCommand cmd)
{
	//buttons=00;potis=1111;
	splitstring s = cmd.cmdParams;
	map<string, string> flds = s.split2map(';','=');

	if (flds.size() < 2) return;

 	for (unsigned int i = 0; i < box->Buttons.size(); i++)
	{
		if (flds["buttons"].at(i) == '1')
			box->Buttons[i]->Active = true;
		else
			box->Buttons[i]->Active = false;
	}

	for (unsigned int i = 0; i < box->Potis.size(); i++)
	{
		if (flds["potis"].at(i) == '1')
			box->Potis[i]->Active = true;
		else
			box->Potis[i]->Active = false;
	}
}

void RemoteCommands::SetPILEDCommand(RemoteCommand cmd)
{
	//Wie kommen die Daten cmd.cmdParams
	box->Lights[0]->setBrightness(123);
}

void RemoteCommands::GetPILEDCommand(RemoteCommand cmd)
{
	//Wie kommen die Daten cmd.cmdParams
	box->Lights[0]->setBrightness(123);
}


