#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include "ControlBox.h"
#include <queue>
#include "Logger.h"

//Forward Declaration
class ControlBox;

enum enumRemoteCommand { REMOTECMD_DISCOVER = 1, REMOTECMD_ENABLE_BUTTONS = 2, REMOTECMD_SET_PILED = 3, REMOTECMD_GET_PILED = 4 };

using namespace std;

struct RemoteCommand
{
	short cmdId;
	string			  cmdParams;
};

class RemoteCommands
{
	protected:
		ControlBox* box;
		bool done;
		Logger* log;
		Settings* ini;
		int listenPort;

		virtual unsigned long Pull(void);
		virtual unsigned long Push(void);

		std::thread threadPush;
		virtual void spawnPushThread()
		{
			threadPush = std::thread(&RemoteCommands::Push, this);
		};

		std::thread threadPull;
		virtual void spawnPullThread()
		{
			threadPull = std::thread(&RemoteCommands::Pull, this);
		};

		queue<RemoteCommand>* cmdQ;
		void ExecuteCommands(RemoteCommand);

		//Individual Commands
		void DiscoverCommand(RemoteCommand);
		void EnableButtonsCommand(RemoteCommand);
		void SetPILEDCommand(RemoteCommand);
		void GetPILEDCommand(RemoteCommand);
	public:
		RemoteCommands(ControlBox* pBox);
		~RemoteCommands();

		void stop();
		void start();
};
