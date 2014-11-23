#pragma once

#ifdef CYGWIN

#include <string>
#include <sys/socket.h>
#include <arpa/inet.h> //inet_addr
#include <unistd.h>		//close(socket)
#include <thread> 
#include <sstream>

namespace lumitech
{
	extern std::string gExeName;

	int PlatformInit(char*);
	void PlatformClose();

	std::string getComputerNamePlatform();
	std::string getFileNamePlatform();
	std::string getFilePathPlatform();
	
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
	std::vector<unsigned char> intToBytes(uint16_t paramInt);

	//Sequence Runner Thread
	void startSequenceThread(int (*funcPointer)(char*), char* seqName);
	void waitSequenceThread();
	void setSequencePointer(void*);

	//Ctrl-C Handler
	void sigproc(int);
}

template <typename T>
std::string toString(T Number )
{
	std::ostringstream ss;
	ss << Number;
	return ss.str();
}

template <class T>
T fromString(const std::string& s)
{
	std::istringstream ss(s);
	T result;
	ss >> result;    // TODO handle errors

	return result;
}



#endif


