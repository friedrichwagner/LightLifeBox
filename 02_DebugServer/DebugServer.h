#pragma once

#include "tcpacceptor.h"
#include "tcpstream.h"
#include "helpers.h"
#include <vector>


class DebugServer : IObserver
{
	volatile bool done;
	static DebugServer* _instance;

#ifdef WIN32
	//Windows USB Thread Handling
	static DWORD WINAPI StaticThreadStart(void* Param)
    {
        DebugServer* This = (DebugServer*) Param;
        return This->startListen();
    }

	HANDLE listenThread;
	void spawn()
    {
       DWORD ThreadID;
       listenThread=CreateThread(NULL, 0, StaticThreadStart, (void*) this, 0, &ThreadID);
    };
#endif

#ifdef CYGWIN
	std::thread listenThread;
	std::thread spawn() 
	{
		return std::thread(&DebugServer::startListen, this);
	};

#endif

	
private:
	int listenPort;
	
	TCPAcceptor* listener;
	std::vector<TCPStream*> DebugClients;
	std::vector<int> listofDowns;
	unsigned long startListen(void);

	DebugServer(); //private constructor

  public:
    static DebugServer* getInstance();

	~DebugServer();
	void updateClient(std::string);
};

