#pragma once


#include <Ws2tcpip.h>
#include <winsock2.h>

#include <string>
#include <windows.h>
#include <time.h> 
#include <vector>
#include <sstream>

namespace lumitech
{
	extern std::string gExeName;
	
	int PlatformInit(char*);
	void PlatformClose();

	std::string getComputerNamePlatform();
	std::string getFileNamePlatform();
	std::string getFilePathPlatform();

	//template<typename T> T FromString(const std::string& str);
	
	int stoi(std::string s);
	long stol(std::string s);
	float stof(std::string s);
	double stod(std::string s);
	bool stob(std::string s);

	std::string itos(int i);
	std::string dtos(double t);

	int waitOnKeyPress();
	void sleep(unsigned long miliseceonds);

	int gettimeofdayLT(struct timeval* tp, void* tzp);
	const std::string currentDateTime();
	std::vector<unsigned char> intToBytes(UINT16 paramInt);

	//Ctrl-C Handler
	BOOL CtrlHandler( DWORD fdwCtrlType ); 
	
	//Sequence Runner Thread
	void startSequenceThread(LPTHREAD_START_ROUTINE funcPointer, char* seqName);
	void waitSequenceThread();
	void setSequencePointer(void*);
}


template <class T>
T fromString(const std::string& s)
{
    std::istringstream ss(s);
    T result;
    ss >> result;    // TODO handle errors

    return result;
}




