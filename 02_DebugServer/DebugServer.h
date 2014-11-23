#pragma once

#include "tcpacceptor.h"
#include "tcpstream.h"
#include "helpers.h"
#include <vector>
#include "Logger.h"
#include <thread>


class DebugServer : IObserver
{
	volatile bool done;
	static DebugServer* _instance;

	std::thread listenThread;
	void spawn()
	{
		listenThread = std::thread(&DebugServer::startListen, this);
		//listenThread = std::move(	std::thread( [this] { this->startListen(); } ));
	};

private:
	int listenPort;
	Logger* log;
	
	TCPAcceptor* listener;
	std::vector<TCPStream*> DebugClients;
	std::vector<int> listofDowns;
	unsigned long startListen(void);
	DebugServer(); //private constructor
  public:
    static DebugServer* getInstance(); //Singleton does not work on Linux. Thread always crashes
	
	~DebugServer();
	void updateClient(std::string);
};

