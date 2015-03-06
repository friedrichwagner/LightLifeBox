#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include "ControlBox.h"
#include <queue>
#include "Logger.h"
#include "BaseClient.h"

#define recvBufSize	255

//Forward Declaration
class ControlBox;

enum enumRemoteCommand { REMOTECMD_DISCOVER = 1, REMOTECMD_ENABLE_BUTTONS = 2, REMOTECMD_SET_PILED = 3, REMOTECMD_GET_PILED = 4 };

using namespace std;

struct RemoteCommand
{
	short	cmdId;
	string	cmdParams;

	int ToString(unsigned char* buf)
	{
		buf[0] = (unsigned char) cmdId;
		memcpy(&buf[1], cmdParams.c_str(), cmdParams.length());

		return (cmdParams.length() + 1);
	}

};

class RemoteCommands
{
	protected:
		ControlBox* box;
		bool done;
		Logger* log;
		Settings* ini;
		UDPSendSocket* _sendSock;
		UDPRecvSocket* _recvSock;
		unsigned char recvBuf[recvBufSize+1];
		unsigned char sendBuf[recvBufSize + 1];

		unsigned long Pull(void);
		unsigned long Push(void);

		std::thread threadPush;
		void spawnPushThread()
		{
			threadPush = std::thread(&RemoteCommands::Push, this);
		};

		std::thread threadPull;
		void spawnPullThread()
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

		//Lock Handling
		void go();
	public:
		RemoteCommands(ControlBox* pBox);
		~RemoteCommands();

		void stop();
		void start();
		int send(RemoteCommand);
};
