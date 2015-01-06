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

	this->listenPort = ini->Read<int>("ControlBox", "CommandPort", 1001);
	box = pBox; 
	done = false;


	cmdQ = new queue<RemoteCommand>();
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

unsigned long RemoteCommands::Push(void)
{
	log->cout("RemoteCommand PUSH Thread started ...");
	while (!done)
	{
		//Wait for Remote commands on UDP

		RemoteCommand cmd1;
		if (cmd1.cmdId > 0)
		{
			cmdQ->push(cmd1);
		}

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
	string s = box->Name;
}

void RemoteCommands::EnableButtonsCommand(RemoteCommand cmd)
{
	//Wie kommen die Daten cmd.cmdParams
	for (unsigned int i = 0; i < box->Buttons.size(); i++)
	{
		box->Buttons[i]->Active = false;
	}

	for (unsigned int i = 0; i < box->Potis.size(); i++)
	{
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


