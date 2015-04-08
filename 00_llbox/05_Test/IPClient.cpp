#include "IPClient.h"
#include "helpers.h"

TestClient::TestClient(std::string pName, unsigned int _PIButtonIndex)
{
	log = Logger::getInstance();
	ini = Settings::getInstance();
	name = pName;
	done = false;
	PIButtonIndex = _PIButtonIndex;
	actualValue = 0;
	lastValue = 0;

	this->IP = ini->Read<string>(pName, "IP", "127.0.0.1");
	this->Port = ini->Read<int>(pName, "Port", 10000);
	connector = new TCPConnector();
	stream = NULL;

	if (connect())
	{
		send(name);
		spawn();
	}
}

TestClient::~TestClient()
{
	done = true;
	delete stream;
	delete connector;

	if (thisThread.joinable()) thisThread.join();
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
			line[*len] = 0;
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

int TestClient::getPortVal()
{
	int len;
	string s;

	s = this->receive(&len);
	int i = atoi(s.c_str());

	if (len == -1)
	{
		done = true;
		return -9999;
	}

	return i;
}

unsigned long TestClient::startListen()
{
	int dir = +1;

	log->cout(this->name + ": start listening ...");
	while (!done)
	{
		//Blocking Call
		actualValue = getPortVal();

		if (actualValue == 0)
		{
			isr_PressedGeneral(PIButtonIndex);
			actualValue = 0;
		}
		else if (actualValue == -9999)
		{
			break; // for debugging purpose
		}
		else
		{
			dir = +1;
			if (actualValue - lastValue < 0) dir = -1;

			pibuttons[PIButtonIndex]->value = dir;
			isr_General2(PIButtonIndex);
			
			lastValue = actualValue;
		}		
	}
	
	log->cout(this->name + ": stop listening ...");

	return 0;
}




