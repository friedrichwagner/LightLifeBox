#pragma once

#include "tcpconnector.h"
#include "settings.h"

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
	
public:
	TestClient(string name);
	~TestClient();
	bool connect();
	void send(string);
	string receive(int* len);
};


