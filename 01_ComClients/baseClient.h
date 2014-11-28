#pragma once

#include <string>
#include <vector>
#include "Settings.h"
#include "Logger.h"
#include "helpers.h"
#include "serialib.h"
#include "PILEDScene.h"
//#include "ftdi.h"

enum enumClientType { CLIENT_NONE = 1, CLIENT_DMX = 2, CLIENT_DALI = 3, CLIENT_ZIGBEE = 4, CLIENT_ZLL = 5, CLIENT_WEBAPI = 6 };
//enum enumInterfaceType {IF_NONE, IF_USB, IF_UDP };

struct IPSocket
{
	bool isValid;
	std::string IPAddress;
	int IPPort;
	int socketNr;
	struct sockaddr_in serverAddress;

	IPSocket(std::string paramIPAddress, int paramIPPort)
	{
		IPAddress= paramIPAddress;
		IPPort = paramIPPort;
		socketNr=socket(AF_INET,SOCK_DGRAM,0); //UDP
		isValid=false;

		//if (socketNr == INVALID_SOCKET)
		if (socketNr < 0 ) 
		{
			isValid=false;
		}

		serverAddress.sin_family = AF_INET;
		serverAddress.sin_addr.s_addr=inet_addr(IPAddress.c_str());
		serverAddress.sin_port=htons(IPPort);

		isValid=true;
	}

	~IPSocket()
	{
#ifdef WIN32
		if(isValid) closesocket(socketNr);
#else
		if(isValid) close(socketNr);
#endif
	}

	int send(unsigned char* cdata, int cnt)
	{
		int ret=0;

		if (isValid)
		{
			//ret=sendto(socketNr,cdata, cnt,0, (struct sockaddr *)&serverAddress,sizeof(serverAddress));			
			ret=sendto(socketNr, (char*) cdata, cnt,0, reinterpret_cast<sockaddr*>(&serverAddress),sizeof(serverAddress));

			//Do I need the close here ?
			//closesocket(socketNr);
		}

		return ret;
	}
};

class IBaseClient
{
private:

protected:
	std::string Name;
	enumClientType clientType;
	//enumInterfaceType ifType;
	Settings* ini;
	//std::vector<std::string> IPAddresses;
	std::vector<IPSocket*> IPClients;
	int IPPort;
	Logger* log;


	bool useFTDI;
#ifdef USE_FTDI
	std::vector<ftdi*> FTDIClients;
#endif
	std::vector<serialib*> USBClients;

	int SendUDP(unsigned char* data, int cnt);	
	int SendUSB(unsigned char* data, int cnt);

	virtual void setBrightness(unsigned int)=0;	
	virtual void setCCT(unsigned int)=0;	
	virtual void setRGB(unsigned int[])=0;	
	virtual void setXY(float[])=0;	
	virtual void setFadeTime(unsigned int val)=0;
public:
	std::string getName();

	IBaseClient();
	virtual ~IBaseClient();
    virtual void updateData(struct PILEDScene* scene)=0;
	int getCntClients();
};

