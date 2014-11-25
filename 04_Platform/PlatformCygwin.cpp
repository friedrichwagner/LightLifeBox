#include <iostream>
#include <vector>
#include <sstream>
#include <algorithm>
#include <iostream>
#include "ControlBox.h"

#ifdef CYGWIN
#include <thread>
#include "PlatformCygwin.h"
#include <unistd.h>
#include <termios.h>  //_getch, usleep
#include <sys/time.h>
#include <signal.h> //raspberry pi

#define MAX_PATH 260
namespace lumitech {

std::string gExeName;
std::thread hSequenceThread;
//to be able to stop the Sequence in the Ctr-C Handler
ControlBox* thisBox = 0;

int PlatformInit(char* argv0)
{
	gExeName = argv0;

	//Install Ctrl-C Handler
	signal(SIGINT, sigproc);
	return 0;
}
void PlatformClose()
{

}

std::string getComputerNamePlatform()
{
	char mname[MAX_PATH];
	int ret=-1;
	std::string retstr("MyMachine");

	ret= gethostname(mname, MAX_PATH);	
	//cout<< ret << std::endl;

	if (ret==0) 
	{
		retstr = mname;
	}

	return retstr;
}


std::string getFileNamePlatform()
{
	std::vector<char> executablePath(gExeName.begin(), gExeName.end());
	std::string retstr= gExeName;	
	retstr.append(".log");

	return retstr;
}

std::string getFilePathPlatform()
{
	std::string retstr= gExeName;
	std::string::size_type pos = retstr.find_last_of( "\\/" );
	retstr= retstr.substr( 0, pos+1);

	return retstr;
}

template <class T>
T fromString(const std::string& s)
{
    std::istringstream ss(s);
    T result;
    ss >> result;    // TODO handle errors

    return result;
}

int stoi(std::string s)
{
	return fromString<int>(s);
}

long stol(std::string s)
{
	return fromString<long>(s);
}

float stof(std::string s)
{
	return fromString<float>(s);
}

double stod(std::string s)
{
	return fromString<double>(s);
}

bool stob(std::string s)
{
	bool ret=false;

	std::transform(s.begin(), s.end(),s.begin(), ::toupper);
	if (s.compare("TRUE")==0 || s.compare("1")==0) ret=true;

	return ret;
}

std::string itos(int i)
{
	return toString<int>(i);
}

std::string dtos(double d)
{
	return toString<double>(d);
}

char getch()
{
	struct termios oldt,
	newt;
	int ch;
	tcgetattr( STDIN_FILENO, &oldt );
	newt = oldt;
	newt.c_lflag &= ~( ICANON | ECHO );
	tcsetattr( STDIN_FILENO, TCSANOW, &newt );
	ch = getchar();
	tcsetattr( STDIN_FILENO, TCSANOW, &oldt );
	
	return ch;
 }

int waitOnKeyPress()
{
	int c;
	c=getch();
	//c=std::cin.get();

	return c;
}

void sleep(unsigned long miliseceonds)
{
	usleep(miliseceonds*1000);
}

int gettimeofdayLT(struct timeval* tp, void* tzp) 
{
#ifdef CYGWIN
	return gettimeofday(tp, (void*) tzp);
#else
	return gettimeofday(tp, ( __timezone_ptr_t) tzp);
#endif
}

// Get current date/time, format is YYYY-MM-DD.HH:mm:ss
const std::string currentDateTime() 
{
    time_t     now = time(0);
    struct tm  tstruct;
    char       buf[80];
    tstruct = *localtime(&now);
    // Visit http://en.cppreference.com/w/cpp/chrono/c/strftime
    // for more information about date/time format
    strftime(buf, sizeof(buf), "%Y.%m.%d %X", &tstruct);

    return buf;
}

std::vector<unsigned char> intToBytes(uint16_t paramInt)
{
	int size = sizeof(uint16_t);

     std::vector<unsigned char> arrayOfByte(size);
     for (int i = 0; i < size; i++)
         arrayOfByte[size - i] = (paramInt >> (i * 8));
     return arrayOfByte;
}


void startSequenceThread(int (*funcPointer)(char*), char* seqName)
{
	hSequenceThread = std::thread(funcPointer, seqName);
}

void waitSequenceThread()
{
	hSequenceThread.join();
}

void setSequencePointer(void* p)
{
	if (p != 0) thisBox = (ControlBox*)p;
}

void sigproc(int pi)
{ 		 
	signal(SIGINT, sigproc); /*  */
	/* NOTE some versions of UNIX will reset signal to default
	after each call. So for portability reset signal each time */
 
	cout << "Ctrl-C Handler" << endl;

	if (thisBox != 0) 	
	{
		thisBox->stopEventLoop();
	}
}


}

#endif
