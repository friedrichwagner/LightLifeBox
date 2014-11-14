#pragma once

#include <string>
#include "ftd2xx.h"
#include "Logger.h"

using namespace std;

struct FTDIDevice
{
	int nr;
	string Description;
	string SerialNumber;
	long Type;

	FTDIDevice()
	{
		nr=0; Description = ""; SerialNumber=""; Type= 0;
	}

	FTDIDevice(int pNr, char* d, char* s, long  t)
	{
		nr=pNr; Description = d; SerialNumber=s; Type= t;
	}
};

class ftdi
{
	volatile bool done;
	static DWORD WINAPI StaticThreadStart(void* Param)
    {
        ftdi* This = (ftdi*) Param;
        return This->writeDataToUSB();
    }

#ifdef WIN32
	//Windows USB Thread Handling
	HANDLE hUSBThread;
	void startMyThread()
    {
       DWORD ThreadID;
       hUSBThread=CreateThread(NULL, 0, StaticThreadStart, (void*) this, 0, &ThreadID);
    }
#endif

#ifdef CYGWIN
	 std::thread hUSBThread;

	void startMyThread()
    {
       hUSBThread = std::thread(StaticThreadStart, this);     
    }

#endif

private:
	string Name;
	bool isConnected;
	HANDLE handle;
	FT_STATUS ftStatus;
	int bytesWritten;
	Logger* log;

	FT_STATUS ResetDevice();
	FT_STATUS doOpen(bool startThread);

	//static, private	
	static int numUSBDevices;
	static int getDeviceInfo(string name);

	//USB Handling
	int thisDeviceNr;
	void ftdiSomething();
	unsigned char* mBuffer;
	int mCnt;
	unsigned long writeDataToUSB(void);
public:
	ftdi(string name);
	~ftdi();

	bool getIsConnected();
	int getBytesWritten();
	int getStatus();
	int write(unsigned char*, int);

	//static, public
	static std::vector<FTDIDevice*> FTDIDevices;
	static int CreateDeviceInfoList();
	string getPort();
	
};
