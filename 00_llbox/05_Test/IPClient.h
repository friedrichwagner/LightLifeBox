#pragma once

#include "tcpconnector.h"
#include "Settings.h"

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

	bool connect();
	
	string receive(int* len);
	
public:
	TestClient(string name);
	~TestClient();

	void send(string);
	bool connected();
	int getPortVal(int* len);
	int getPortVal2();
};


