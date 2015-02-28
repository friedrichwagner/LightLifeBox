#include <iostream>
#include "baseClient.h"
#include <cstring>


#pragma region Contructor/Destructor

IBaseClient::IBaseClient()
{
	ini = Settings::getInstance();
	clientType = CLIENT_NONE;
	log = Logger::getInstance();
	useFTDI = false;
}

IBaseClient::~IBaseClient()
{
	for (unsigned int i=0; i < USBClients.size(); i++)
	{
		delete USBClients[i];
	}

	for (unsigned int i=0; i < IPClients.size(); i++)
	{
		delete IPClients[i];
	}

#ifdef USE_FTDI
	for (unsigned int i=0; i < FTDIClients.size(); i++)
	{
		delete FTDIClients[i];
	}
#endif

}

#pragma endregion

#pragma region Member Function

string IBaseClient::getName()
{
	return Name;
}

int IBaseClient::getType()
{
	return clientType;
}

int IBaseClient::getCntClients()
{
	int ret=IPClients.size() +  USBClients.size();

#ifdef USE_FTDI
	ret+=FTDIClients.size();
#endif

	return ret;
}

int IBaseClient::SendUDP(unsigned char* cdata, int cnt)
{
	int ret=0;

	for (unsigned int i=0; i < IPClients.size(); i++)
	{
		//isValid wird in send gemacht
		log->info("send:" + IPClients[i]->IPAddress + ":" + lumitech::itos(IPPort) + "(" + this->Name + ")");
		ret=IPClients[i]->send(cdata, cnt);
	}

	return ret;
}

int IBaseClient::SendUSB(unsigned char* cdata, int cnt)
{
	int ret=0;

	if (useFTDI)
	{

#ifdef USE_FTDI
		for (unsigned int i=0; i < FTDIClients.size(); i++)
		{
			log->info("send:" +FTDIClients[i]->getPort() + "(" + this->Name + ")");
			//if (clientType == CLIENT_DMX)
			//	ret=FTDIClients[i]->WriteDMX(cdata, cnt);
			//else
				ret=FTDIClients[i]->write(cdata, cnt);
		}
#endif

	}
	else
	{
		for (unsigned int i=0; i < USBClients.size(); i++)
		{
			log->info("send:" +USBClients[i]->getPort() + "(" + this->Name + ")");
			ret=USBClients[i]->writeBuf(cdata, cnt);			
		}
	}
	return ret;
}

#pragma endregion
