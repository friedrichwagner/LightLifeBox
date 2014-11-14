#include "serialib.h"
#include "helpers.h"
#include "ftd2xx.h"


serialib::serialib(string paramPort, int paramBaud, bool paramUseDMXWrite)
{
	log = Logger::getInstance();
	useDMXWrite = paramUseDMXWrite;

#if defined (_WIN32) || defined( _WIN64)
	port="\\\\.\\"+paramPort;
#endif

#ifdef CYGWIN
	port=paramPort;
#endif

	baud=paramBaud;
	isOpen = false;

	int status = Open(); 
	if (status == 1) 
	{ 			
		log->info("COM thread \"" + port + "\" started");
		startMyThread();
	}
	else
	{
		cout << "baud:" << baud << endl;
		log->error("COM \"" + port + "\" not connected");
	}

}

serialib::~serialib()
{
	done=true;

#if defined (_WIN32) || defined( _WIN64)
	WaitForSingleObject(hComThread, 200);
#else
	hComThread.join();
#endif

    Close();
}



//_________________________________________
// ::: Configuration and initialization :::



/*!
     \brief Open the serial port
     \param Device : Port name (COM1, COM2, ... for Windows ) or (/dev/ttyS0, /dev/ttyACM0, /dev/ttyUSB0 ... for linux)
     \param Bauds : Baud rate of the serial port.

                \n Supported baud rate for Windows :
                        - 110
                        - 300
                        - 600
                        - 1200
                        - 2400
                        - 4800
                        - 9600
                        - 14400
                        - 19200
                        - 38400
                        - 56000
                        - 57600
                        - 115200
                        - 128000
                        - 256000

               \n Supported baud rate for Linux :\n
                        - 110
                        - 300
                        - 600
                        - 1200
                        - 2400
                        - 4800
                        - 9600
                        - 19200
                        - 38400
                        - 57600
                        - 115200

     \return 1 success
     \return -1 device not found
     \return -2 error while opening the device
     \return -3 error while getting port parameters
     \return -4 Speed (Bauds) not recognized
     \return -5 error while writing port parameters
     \return -6 error while writing timeout parameters
  */

char serialib::Open()
{

#if defined (_WIN32) || defined( _WIN64)

    // Open serial port
	//port = "FT232R USB UART";
    hSerial = CreateFileA(port.c_str(),GENERIC_READ | GENERIC_WRITE,0,0,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,0);
    if(hSerial==INVALID_HANDLE_VALUE) 
	{
        if(GetLastError()==ERROR_FILE_NOT_FOUND)
            return -1;                                                  
        return -2;                                                      
    }

    // Set parameters
    DCB dcbSerialParams = {0};                                          
	// FTDCB dcbSerialParams;
    dcbSerialParams.DCBlength=sizeof(dcbSerialParams);
    if (!GetCommState(hSerial, &dcbSerialParams))                       
        return -3;  	

    switch (baud)                                                   
    {
		case 110  :     dcbSerialParams.BaudRate=CBR_110; break;
		case 300  :     dcbSerialParams.BaudRate=CBR_300; break;
		case 600  :     dcbSerialParams.BaudRate=CBR_600; break;
		case 1200 :     dcbSerialParams.BaudRate=CBR_1200; break;
		case 2400 :     dcbSerialParams.BaudRate=CBR_2400; break;
		case 4800 :     dcbSerialParams.BaudRate=CBR_4800; break;
		case 9600 :     dcbSerialParams.BaudRate=CBR_9600; break;
		case 14400 :    dcbSerialParams.BaudRate=CBR_14400; break;
		case 19200 :    dcbSerialParams.BaudRate=CBR_19200; break;
		case 38400 :    dcbSerialParams.BaudRate=CBR_38400; break;
		case 56000 :    dcbSerialParams.BaudRate=CBR_56000; break;
		case 57600 :    dcbSerialParams.BaudRate=CBR_57600; break;
		case 115200 :   dcbSerialParams.BaudRate=CBR_115200; break;
		case 128000 :   dcbSerialParams.BaudRate=CBR_128000; break;
		case 256000 :   dcbSerialParams.BaudRate=CBR_256000; break;
		default : return -4;
	}    
    dcbSerialParams.ByteSize=DATABITS_8;
	dcbSerialParams.Parity=NOPARITY;
    dcbSerialParams.StopBits=ONESTOPBIT;

	dcbSerialParams.fOutX = false;
    dcbSerialParams.fInX = false;
    dcbSerialParams.fErrorChar = false;
	dcbSerialParams.fAbortOnError = false;
    dcbSerialParams.fBinary = true;

    dcbSerialParams.fRtsControl = true;
	//dcbSerialParams.fDtrControl = true;
    
    	
    if(!SetCommState(hSerial, &dcbSerialParams))                        
        return -5;                                                     

    // Set TimeOut
    timeouts.ReadIntervalTimeout=0;                                     
    timeouts.ReadTotalTimeoutConstant=MAXDWORD;                       
    timeouts.ReadTotalTimeoutMultiplier=0;
    timeouts.WriteTotalTimeoutConstant=MAXDWORD;
    timeouts.WriteTotalTimeoutMultiplier=0;
    //if(!FT_W32_SetCommTimeouts(hSerial, &timeouts))   return -6;                                                      

	PurgeComm(hSerial, PURGE_TXCLEAR |PURGE_RXCLEAR);

	Sleep(1000);
    
	isOpen=true;

	return 1;                                                          

#endif

#ifdef CYGWIN    
    struct termios options;                                             // Structure with the device's options

    // Open device
    fd = open(port.c_str(), O_RDWR | O_NOCTTY | O_NDELAY);                    // Open port
    if (fd == -1) return -2;                                            // If the device is not open, return -1
    fcntl(fd, F_SETFL, FNDELAY);                                        // Open the device in nonblocking mode

    // Set parameters
    tcgetattr(fd, &options);                                            // Get the current options of the port
    bzero(&options, sizeof(options));                                   // Clear all the options
    speed_t         Speed;
    switch (baud)                                                      // Set the speed (Bauds)
    {
		case 110  :     Speed=B110; break;
		case 300  :     Speed=B300; break;
		case 600  :     Speed=B600; break;
		case 1200 :     Speed=B1200; break;
		case 2400 :     Speed=B2400; break;
		case 4800 :     Speed=B4800; break;
		case 9600 :     Speed=B9600; break;
		case 19200 :    Speed=B19200; break;
		case 38400 :    Speed=B38400; break;
		case 57600 :    Speed=B57600; break;
		case 115200 :   Speed=B115200; break;
		case 230400 :   Speed=B230400; break;
		default : return -4;
	}
    
	cfsetispeed(&options, Speed);                                       // Set the baud rate at 115200 bauds
    cfsetospeed(&options, Speed);
    
	options.c_cflag |= ( CLOCAL | CREAD |  CS8);                        // Configure the device : 8 bits, no parity, no control
    options.c_iflag |= ( IGNPAR | IGNBRK );
    options.c_cc[VTIME]=0;                                              // Timer unused
    options.c_cc[VMIN]=0;                                               // At least on character before satisfy reading
    

	/*options.c_cflag &= ~CSIZE;
	options.c_cflag |= CS8;
	options.c_cflag &= ~(PARENB|PARODD);
	options.c_cflag |=CSTOPB;
	options.c_cflag &= ~CRTSCTS;*/

	
	tcsetattr(fd, TCSANOW, &options);                                   // Activate the settings

	tcflush(fd,TCOFLUSH);
    
	isOpen=true;
	return 1;                                                    
#endif
	
}

void serialib::Close()
{
#if defined (_WIN32) || defined( _WIN64)
    CloseHandle(hSerial);
#endif
#ifdef CYGWIN
    close (fd);
#endif

	isOpen=false;
}

bool serialib::getIsOpen()
{
	return isOpen;
}

std::string serialib::getPort()
{
	return port;
}

int serialib::writeBuf(unsigned char* buffer, int cnt)
{
	mBuffer=buffer; mCnt=cnt;
	if (!isOpen)
		log->error("COM \"" + port + "\" not connected");

	return mCnt;
}

unsigned long serialib::writeData()
{
	done=false;
	int ret=0;

	while (!done)
	{

		if (isOpen)
		{
			if (useDMXWrite)
				ret=_writeDMX(mBuffer, mCnt); 
			else
				ret=_write(mBuffer, mCnt); 

			//cout << "ret=" << ret <<endl;

			lumitech::sleep(100);
		}
	}

	return 0;
}

int serialib::_write(const unsigned char *buffer, const unsigned int NbBytes)
{
#if defined (_WIN32) || defined( _WIN64)
	DWORD dwBytesWritten; 

    if(!WriteFile(hSerial, buffer, NbBytes, &dwBytesWritten, NULL))   
        return -1;                                                      

    return NbBytes;                                                         
#endif

#ifdef CYGWIN
    if (write (fd,buffer,NbBytes)!=(ssize_t)NbBytes)                             
        return -1;                                                      
    return NbBytes;                                                           
#endif
}



int serialib::_writeDMX(const unsigned char *buffer, const unsigned int NbBytes)
{
#if defined (_WIN32) || defined( _WIN64)
	BOOL ret;

	 // set RS485 for sendin
   ret=EscapeCommFunction(hSerial, CLRRTS);

	ret=SetCommBreak(hSerial);
    ret=ClearCommBreak(hSerial);         
#endif

#ifdef CYGWIN
	// set RS485 for sendin
	int status=0;
	status &= ~TIOCM_RTS;	
	ioctl(fd, TIOCMSET, &status);
	tcsendbreak(fd,0);  //set Break
	//tcsendbreak(fd,0);	 //reset Break


	/*ioctl(fd, TIOCSBRK, 0);
    usleep(100); // should sleep for 100 microseconds.
    ioctl(fd, TIOCCBRK, 0);
    usleep(10); // mark after break
	*/

#endif

	return  _write(buffer, NbBytes);
}

int serialib::Read (void *Buffer,unsigned int MaxNbBytes,unsigned int TimeOut_ms)
{
#if defined (_WIN32) || defined(_WIN64)
    DWORD dwBytesRead = 0;
    timeouts.ReadTotalTimeoutConstant=(DWORD)TimeOut_ms;   

    if(!SetCommTimeouts(hSerial, &timeouts))                           
        return -1;                                

    if(!ReadFile(hSerial,Buffer,(DWORD)MaxNbBytes,&dwBytesRead, NULL))  
        return -2;                                                      

    if (dwBytesRead!=(DWORD)MaxNbBytes) return 0;     

    return 1;                                                          
#endif

#ifdef CYGWIN
    TimeOut          Timer;                                             // Timer used for timeout
    Timer.InitTimer();                                                  // Initialise the timer
    unsigned int     NbByteRead=0;
    while (Timer.ElapsedTime_ms()<TimeOut_ms || TimeOut_ms==0)          // While Timeout is not reached
    {
        unsigned char* Ptr=(unsigned char*)Buffer+NbByteRead;           // Compute the position of the current byte
        int Ret=read(fd,(void*)Ptr,MaxNbBytes-NbByteRead);              // Try to read a byte on the device
        if (Ret==-1) return -2;                                         // Error while reading
        if (Ret>0) {                                                    // One or several byte(s) has been read on the device
            NbByteRead+=Ret;                                            // Increase the number of read bytes
            if (NbByteRead>=MaxNbBytes)                                 // Success : bytes has been read
                return 1;
        }
    }
    return 0;                                                           // Timeout reached, return 0
#endif
}


void serialib::FlushReceiver()
{
#ifdef CYGWIN
    tcflush(fd,TCIFLUSH);
#endif
}

int serialib::Peek()
{
    int Nbytes=0;
#ifdef CYGWIN
    ioctl(fd, FIONREAD, &Nbytes);
#endif
    return Nbytes;
}

// ******************************************
//  Class TimeOut
// ******************************************
TimeOut::TimeOut()
{}
void TimeOut::InitTimer()
{
    lumitech::gettimeofdayLT(&PreviousTime, NULL);
}

//Return the elapsed time since initialization
unsigned long int TimeOut::ElapsedTime_ms()
{
    struct timeval CurrentTime;
    int sec,usec;
    lumitech::gettimeofdayLT(&CurrentTime, NULL);                                   // Get current time
    sec=CurrentTime.tv_sec-PreviousTime.tv_sec;                         // Compute the number of second elapsed since last call
    usec=CurrentTime.tv_usec-PreviousTime.tv_usec;                      // Compute
    if (usec<0) {                                                       // If the previous usec is higher than the current one
        usec=1000000-PreviousTime.tv_usec+CurrentTime.tv_usec;          // Recompute the microseonds
        sec--;                                                          // Substract one second
    }
    return sec*1000+usec/1000;
}

