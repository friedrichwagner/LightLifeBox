#include "ftdi.h"
#include "helpers.h"
#include <malloc.h>

using namespace std;

//static member initialization
std::vector<FTDIDevice*> ftdi::FTDIDevices;
int ftdi::numUSBDevices=0;

#pragma region Contructor/Destructor

ftdi::ftdi(string name)
{
	Name=name;
	isConnected=false;
	handle=0;
	thisDeviceNr=ftdi::getDeviceInfo(Name);
	log = Logger::getInstance();
	mCnt=0;
	ftStatus=FT_OK;

	if ( thisDeviceNr > -1)
	{
		ftStatus = doOpen(true);
	}
}

ftdi::~ftdi()
{
	done=true;

#ifdef WIN32
	WaitForSingleObject(hUSBThread, 200);
#else
	hUSBThread.join();
#endif

	ResetDevice();
	FT_Close(handle);

	handle=0;
	isConnected=false;
}
	
#pragma endregion

#pragma region Member Functions

FT_STATUS ftdi::ResetDevice()
{
	ftStatus = FT_ResetDevice(handle);
    ftStatus = FT_Purge(handle, FT_PURGE_TX | FT_PURGE_RX);

	return ftStatus;
}

FT_STATUS ftdi::doOpen(bool startThread)
{
	ResetDevice();
	ftStatus= FT_OpenEx((PVOID) Name.c_str(), FT_OPEN_BY_DESCRIPTION, &handle); 

	if (ftStatus == FT_OK) 
	{ 			
		isConnected=true;
		if (startThread)
		{
			log->info("USB thread \"" + Name + "\" started");
			startMyThread();
		}
	}
	else
		log->error("USB \"" + Name + "\" not connected");

	return ftStatus;
}

string ftdi::getPort()
{
	return Name;
}


//this is static
int ftdi::CreateDeviceInfoList()
{
	stringstream ss;
	Logger* thisLog=Logger::getInstance();

	DWORD numDevs=0;
	FT_STATUS ftStatus;
	

	ftStatus = FT_CreateDeviceInfoList(&numDevs);
	ss << "Number FTDI Devices:" << numDevs;
	thisLog->info(ss.str());

	if (ftStatus == FT_OK) 
	{ 
		FT_DEVICE_LIST_INFO_NODE *devInfo;
		devInfo = (FT_DEVICE_LIST_INFO_NODE*)malloc(sizeof(FT_DEVICE_LIST_INFO_NODE)*numDevs);
		
		ftStatus = FT_GetDeviceInfoList(devInfo,&numDevs); 
		if (ftStatus == FT_OK) 
		{ 
			for (unsigned int i=0; i< numDevs; i++)
			{
				string tmpStr(devInfo[i].Description);
				if (tmpStr.length() > 0)
				{
					FTDIDevices.push_back(new FTDIDevice(i, devInfo[i].Description, devInfo[i].SerialNumber, devInfo[i].Type ));
					ss.clear();
					ss << "FTDI Device added:" << devInfo[i].Description << "; " << devInfo[i].SerialNumber << "; " << devInfo[i].Type;
					thisLog->info(ss.str());
				}
				else
				{
					ss.clear();
					ss << "FTDI DeviceInfoList description is empty!" << endl << " Did you forget to unload ftdi_sio and usbserial ?";
					thisLog->error(ss.str());
				}
					
			}
		}

#ifdef NO
		char  descr[64];
		char  serial[64];

		for (unsigned int i=0; i< numDevs; i++)
		{
			ftStatus = FT_ListDevices((PVOID) i,descr,FT_LIST_BY_INDEX|FT_OPEN_BY_DESCRIPTION); 
			if (ftStatus == FT_OK) 
			{
				ftStatus = FT_ListDevices((PVOID) i,serial,FT_LIST_BY_INDEX|FT_OPEN_BY_SERIAL_NUMBER); 
				if (ftStatus == FT_OK) 
				{
					cout << "desc:" << descr << endl;
					cout << "serial:" << serial << endl;

					FTDIDevices.push_back(new FTDIDevice(i, descr, serial, 3));
				}					
			}
		}
#endif
	}

	ftdi::numUSBDevices = numDevs;

	return numDevs;
}

//this is static
int ftdi::getDeviceInfo(string name)
{
	for (int i=0; i< ftdi::numUSBDevices; i++)
	{
		//cout << ftdi::FTDIDevices[i]->Description << "==" << name <<endl;

		if (ftdi::FTDIDevices[i]->Description == name)
			return i;
	}

	return -1;
}

bool ftdi::getIsConnected()
{
	return isConnected;
}

int ftdi::getBytesWritten()
{
	return bytesWritten;
}

int ftdi::getStatus()
{
	return ftStatus;
}

void ftdi::ftdiSomething()
{
	//ftStatus = FT_SetBitMode(handle, 0x00,0);
    //ftStatus=FT_ResetDevice(handle);
    //ftStatus=FT_SetDivisor(handle, (char)12);  // set baud rate
	ftStatus=FT_SetDivisor(handle, 12);  // set baud rate
	//ftStatus=FT_SetBaudRate(handle, FT_BAUD_115200);  // set baud rate
	ftStatus=FT_SetDataCharacteristics(handle, FT_BITS_8, FT_STOP_BITS_1, 0);
    ftStatus=FT_SetFlowControl(handle, (char)FT_FLOW_NONE, 0, 0);
    ftStatus=FT_ClrRts(handle);    

	ftStatus=FT_SetBreakOn(handle);
    ftStatus=FT_SetBreakOff(handle);
}


int ftdi::write(unsigned char* buffer, int cnt)
{
	mBuffer=buffer; mCnt=cnt;

	if ( (!isConnected) | (ftStatus != FT_OK))
	{
		log->error("USB \"" + this->Name + "\" not connected or status != FT_OK");
		log->error("try reopen");
		ftStatus = doOpen(false);
	}

	return mCnt;
}

//this is the thread function
unsigned long ftdi::writeDataToUSB(void)
{
	DWORD dw=0;
	done=false;

	while (!done)
	{
		ftdiSomething();

		if (isConnected && ftStatus==FT_OK && mCnt > 0)
		{
			ftStatus = FT_Write(handle, mBuffer, mCnt, &dw); 
			bytesWritten=(int) dw;
			lumitech::sleep(100);
		}
	}

	return 0;
}


#pragma endregion



