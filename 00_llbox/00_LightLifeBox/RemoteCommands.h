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

enum enumRemoteReceiveCommand { REMOTE_RECVCMD_DISCOVER = 1, REMOTE_RECVCMD_ENABLE_BUTTONS = 2, REMOTE_RECVCMD_SET_PILED = 3, REMOTE_RECVCMD_GET_PILED = 4, REMOTE_RECVCMD_SET_SEQUENCE=5};
enum enumRemoteSendCommand { REMOTE_SENDCMD_LOCK = 10 };

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

class LightLifeLogger; //Forward Deklaration

class RemoteCommands
{
	protected:
		ControlBox* box;
		LightLifeLogger* lllogger;
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

		int send(RemoteCommand);

		queue<RemoteCommand>* cmdQ;
		void ExecuteReceiveCommands(RemoteCommand);

		//Individual Commands
		void DiscoverCommand(RemoteCommand);
		void EnableButtonsCommand(RemoteCommand);
		void SetPILEDCommand(RemoteCommand);
		void GetPILEDCommand(RemoteCommand);
		void SequenceHandlingCommand(RemoteCommand cmd);
		void StandardAnswer(RemoteCommand cmd);		
		

		void SendLock(enumRemoteSendCommand id, string params);

		//Lock Handling
		void go();
	public:
		RemoteCommands(ControlBox* pBox);
		~RemoteCommands();

		void stop();
		void start();

		void NowAddLLLogger();
		void SendRemoteCommand(enumRemoteSendCommand, string); // to AdminConsole
		
};
