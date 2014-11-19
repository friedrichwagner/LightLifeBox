#include "IPClient.h"
#include "helpers.h"

TestClient::TestClient(std::string pName)
{
	ini = Settings::getInstance();
	name = pName;

	this->IP = ini->Read<string>(pName, "IP", "127.0.0.1");
	this->Port = ini->Read<int>(pName, "Port", 10000);
	connector = new TCPConnector();
	stream = NULL;

	connect();
}

TestClient::~TestClient()
{
	delete connector;
	delete stream;
}

bool TestClient::connect()
{
	stream = connector->connect(IP.c_str(), Port);

	if (stream)
	{
		return true;
	}

	return false;
}

void TestClient::send(string msg)
{
	if (stream)
	{
		stream->send(msg.c_str(), msg.size());
	}
}

string TestClient::receive(int* len)
{
	char line[256];
	string retStr;

	if (stream)
	{
		*len = stream->receive(line, sizeof(line));
		if (*len > 0)
		{
			retStr = line;
			line[*len] = NULL;
		}
	}

	return retStr;
}

bool TestClient::connected()
{
	if (stream)
	{
		return true;
	}

	return false;
}

int TestClient::getPortVal(int* len)
{
	string s;

	s = this->receive(len);
	int i = atoi(s.c_str());

	return i;
}

int TestClient::getPortVal2()
{
	int len;
	string s;

	s = this->receive(&len);
	int i = atoi(s.c_str());

	return i;
}



