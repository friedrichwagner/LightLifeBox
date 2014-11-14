#pragma once

#include <string>
#include "Logger.h"

// Include for windows
#if defined (_WIN32) || defined( _WIN64)
    // Accessing to the serial port under Windows
    #include <windows.h>
	#include <time.h>
#endif

// Include for Linux
#ifdef CYGWIN
	#include <sys/time.h>
    #include <stdlib.h>
    #include <sys/types.h>
    #include <sys/shm.h>
    #include <termios.h>
    #include <string.h>
    #include <iostream>
    // File control definitions
    #include <fcntl.h>
    #include <unistd.h>
    #include <sys/ioctl.h>		
#endif


class serialib
{
private:

	volatile bool done;

#ifdef WIN32
	//Windows USB Thread Handling
	static DWORD WINAPI StaticThreadStart(void* Param)
    {
        serialib* This = (serialib*) Param;
        return This->writeData();
    }

	HANDLE hComThread;
	void startMyThread()
    {
       DWORD ThreadID;
		hComThread=CreateThread(NULL, 0, StaticThreadStart, (void*) this, 0, &ThreadID);
    }
#endif

#ifdef CYGWIN
	std::thread hComThread;
	void startMyThread()
    {
		hComThread = std::thread(&serialib::writeData, this);     
    }

#endif

	unsigned long writeDataToPort(void);

#if defined (_WIN32) || defined( _WIN64)
    HANDLE          hSerial;
    COMMTIMEOUTS    timeouts;
#endif

#ifdef CYGWIN
    int             fd;
#endif

private:
	unsigned char* mBuffer;
	int mCnt;

	std::string port;
	unsigned int baud;
	bool isOpen;
	bool useDMXWrite;
	Logger* log;

    //char    Open        (const char *Device,const unsigned int Bauds);
	char	Open();
    void    Close();
    int	_write(const unsigned char *buffer, const unsigned int NbBytes);
	int	_writeDMX(const unsigned char *buffer, const unsigned int NbBytes);

    unsigned long    writeData();
	//unsigned long	writeDataToDMX();

	int     Read        (void *Buffer,unsigned int MaxNbBytes,const unsigned int TimeOut_ms=0);
    void    FlushReceiver();
    // Return the number of bytes in the received buffer
    int     Peek();
public:
    serialib    (std::string port, int baud, bool useDMXWrite);
    ~serialib   ();

	bool getIsOpen();
	std::string getPort();

	int writeBuf(unsigned char*, int);
};

class TimeOut
{
public:

    // Constructor
    TimeOut();

    // Init the timer
    void                InitTimer();

    // Return the elapsed time since initialization
    unsigned long int   ElapsedTime_ms();

private:    
    struct timeval PreviousTime;
};



