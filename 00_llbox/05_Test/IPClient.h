#pragma once

#include "tcpconnector.h"
#include "Settings.h"
#include "Logger.h"
#include "PIButton.h"
#include <thread>

using namespace std;

class TestClient
{
protected:
	TCPConnector* connector;
	TCPStream* stream;
	string name;
	string IP;
	int Port;
	Settings* ini;
	Logger* log;

	bool connect();

	bool done;
	virtual unsigned long startListen(void);
	std::thread thisThread;
	virtual void spawn()
	{
		thisThread = std::thread(&TestClient::startListen, this);
	}; 
	
	string receive(int* len);
	unsigned int PIButtonIndex;

	int actualValue;
	int lastValue;
public:
	TestClient(string name, unsigned int _PIButtonIndex);
	~TestClient();

	void send(string);
	bool connected();
	int getPortVal();
};


