#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include <time.h>

using namespace std;

class Button
{
protected:
	//Fields
	volatile bool done;

	unsigned int ID;
	unsigned int PortNr;
	unsigned int PortVal;
	string	Name;
	string	Section;
	Settings* ini;
	Logger* log;
	bool isPressed;
	clock_t tstart, tstop;
	float elapsedTime;

	//Functions
	void ButtonDown(void);
	void ButtonUp(void);
	void ButtonPressed(void);
	virtual unsigned long startListen(void);

#ifdef WIN32
	//Windows Thread Handling
	static DWORD WINAPI StaticThreadStart(void* Param)
    {
        Button* This = (Button*) Param;
        return This->startListen();
    }

	HANDLE thisThread;
	void spawn()
    {
       DWORD ThreadID;
       thisThread=CreateThread(NULL, 0, StaticThreadStart, (void*) this, 0, &ThreadID);
    };
#endif

#ifdef CYGWIN
	std::thread thisThread;
	void spawn() 
	{
		thisThread = std::thread(&Button::startListen, this);
	};

#endif

public:
	Button(std::string pName);
	~Button();	

	bool getIsPressed();
};
