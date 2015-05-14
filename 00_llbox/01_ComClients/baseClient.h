#pragma once

#include <string>
#include <vector>
#include "Settings.h"
#include "Logger.h"
#include "helpers.h"
#include "serialib.h"
//#include "PILEDScene.h"
//#include "ftdi.h"

enum enumClientType { CLIENT_NONE = 1, CLIENT_DMX = 2, CLIENT_DALI = 3, CLIENT_ZIGBEE = 4, CLIENT_ZLL = 5, CLIENT_WEBAPI = 6,  CLIENT_NEOLINK = 7, CLIENT_LIGHTLIFE=8 };
//enum enumInterfaceType {IF_NONE, IF_USB, IF_UDP };

struct UDPSendSocket
{
	bool isValid;

	std::string IPAddress;
	int IPPort;
	int _socket;
	struct sockaddr_in serverAddress; //sendto address

	UDPSendSocket(std::string paramIPAddress, int paramIPPort)
	{
		isValid = false;
		setServerIPPort(paramIPAddress, paramIPPort);
	}

	~UDPSendSocket()
	{
#ifdef WIN32
		if (isValid) closesocket(_socket);
#else
		if(isValid) close(_socket);
#endif
	}

	/*void setServerAddress(sockaddr_in addr)
	{
		serverAddress.sin_family = addr.sin_family;
		serverAddress.sin_addr = addr.sin_addr;
		serverAddress.sin_port = addr.sin_port;
	}*/

	void setServerIPPort(std::string paramIPAddress, int paramIPPort)
	{
#ifdef WIN32
		if (isValid) closesocket(_socket);
#else
		if (isValid) close(_socket);
#endif

		IPAddress = paramIPAddress;
		IPPort = paramIPPort;
		_socket = socket(AF_INET, SOCK_DGRAM, 0); //UDP
		isValid = false;

		//if (socketNr == INVALID_SOCKET)
		if (_socket < 0)
		{
			isValid = false;
			cout << "UDPSendSocket invalid!!";
		}

		serverAddress.sin_family = AF_INET;
		serverAddress.sin_addr.s_addr = inet_addr(IPAddress.c_str());
		serverAddress.sin_port = htons(IPPort);

		isValid = true;
	}

	int send(unsigned char* cdata, int cnt)
	{
		int ret=0;

		if (isValid)
		{
			//ret=sendto(socketNr,cdata, cnt,0, (struct sockaddr *)&serverAddress,sizeof(serverAddress));			
			ret = sendto(_socket, (char*)cdata, cnt, 0, reinterpret_cast<sockaddr*>(&serverAddress), sizeof(serverAddress));

			//Do I need the close here ?
			//closesocket(socketNr);
		}

		return ret;
	}
};

struct UDPRecvSocket
{
	bool isValid;

	std::string IPAddress;
	int IPPort;
	int _socket;
	struct sockaddr_in serverAddress; //sendto address
	struct sockaddr_in remoteAddress;

	UDPRecvSocket(int paramIPPort)
	{
		//IPAddress = INADDR_ANY;
		IPPort = paramIPPort;
		_socket = socket(AF_INET, SOCK_DGRAM, 0); //UDP
		isValid = false;

		//if (socketNr == INVALID_SOCKET)
		if (_socket < 0) return;
	
		serverAddress.sin_family = AF_INET;
		serverAddress.sin_addr.s_addr = INADDR_ANY;
		serverAddress.sin_port = htons(IPPort);

		if (::bind(_socket, (struct sockaddr *)&serverAddress, sizeof(serverAddress)) < 0)
		{
#ifdef WIN32
			int err = GetLastError();
#else
			//int err=errno;
#endif
			return;
		}

		isValid = true;
	}

	~UDPRecvSocket()
	{
#ifdef WIN32
		if (isValid) closesocket(_socket);
#else
		if (isValid) 
		{
			shutdown(_socket,2);
			close(_socket);
		}
#endif
	}

	int receive(unsigned char* cdata, int cnt)
	{
		int ret = -1;
		int addrLength = sizeof(remoteAddress);


		//FW 9.4.2015 - Wollen sende Adresse wissen --> ausprobieren ob funtkioniert
		ret = recvfrom(_socket, (char*)cdata, cnt, 0, (struct sockaddr *)(&remoteAddress), (socklen_t*)&addrLength);
		//ret = recv(_socket, (char*)cdata, cnt, 0);
		
		if (ret>0)
			cdata[ret] = '\0';

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
	std::vector<UDPSendSocket*> IPClients;
	int IPPort;
	Logger* log;


	bool useFTDI;
#ifdef USE_FTDI
	std::vector<ftdi*> FTDIClients;
#endif
	std::vector<serialib*> USBClients;

	int SendUDP(unsigned char* data, int cnt);	
	int SendUSB(unsigned char* data, int cnt);
public:
	std::string getName();
	int getType();

	IBaseClient();
	virtual ~IBaseClient();
    //virtual void updateData(struct PILEDScene* scene)=0;
	int getCntClients();

	virtual void setBrightness(unsigned int) = 0;
	virtual void setCCT(unsigned int, float[]) = 0;
	virtual void setRGB(unsigned int[]) = 0;
	virtual void setXY(float[]) = 0;
	virtual void setFadeTime(unsigned int val) = 0;
	virtual void setGroup(unsigned char val) = 0;

	virtual void setCCTDuv(unsigned int, float duv, float xy[2]) = 0;
};

