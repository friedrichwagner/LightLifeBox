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
	cmd.cmdParams = "CmdId=" + lumitech::itos(cmd.cmdId) + ";BoxNr=" + lumitech::itos(box->ID) + ";BoxName=" + box->Name;
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

	cmd.cmdParams = "CmdId=" + lumitech::itos(cmd.cmdId) + ";BoxNr=" + lumitech::itos(box->ID);
	send(cmd);
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
		case CCT_MODE:
			box->Lights[0]->setCCT(cct);
			box->Lights[0]->setBrightness(br);
			break;

		case XY_MODE:
			box->Lights[0]->setXY(xy);
			box->Lights[0]->setBrightness(br);

			break;

		case RGB_MODE:
			box->Lights[0]->setRGB(rgb);
			break;
	}

	cmd.cmdParams = "CmdId=" + lumitech::itos(cmd.cmdId) + ";BoxNr=" + lumitech::itos(box->ID);
	send(cmd);
}

void RemoteCommands::GetPILEDCommand(RemoteCommand cmd)
{
	cmd.cmdParams = "CmdId=" + lumitech::itos(cmd.cmdId) + ";" + box->Lights[0]->getFullState();
	send(cmd);
}


