#pragma once

#include <string>
#include "PILight.h"
#include "Settings.h"
#include "Logger.h"
#include "Button.h"
#include "RemoteCommands.h"
#include "DeltaTest.h"

using namespace std;

class RemoteCommands;

class ControlBox : IButtonObserver
{
protected:
	friend class RemoteCommands;

	static ControlBox* _instance;	//Class property!

	//Fields
	unsigned int ID;	
	string Name;
	string IP;
	vector<Button*> Buttons;
	int threadSleepTime;
	//stringstream sslog;
	
	Settings* ini;
	Logger* log;

	//Functions
	bool Init();

	ControlBox(std::string pName); //private contructor		

	RemoteCommands* rmCmd;
	int addComClients();
	bool testWithoutConsole;	

	bool DeltaTestInProgress;
	DeltaTest* deltaTest;
public:
	static ControlBox* getInstance(string);
	static ControlBox* getInstance();	//should only be used when _instance is not NULL
	~ControlBox();	

	bool EventLoop();
	void stopEventLoop();
	void Beep(int freq, int time);
	string getName();
	
	vector<PILight*> Lights;
	bool isDone;

	void notify(void* sender, enumButtonEvents event, int delta);	

	int buttonActive;
	void setButtons(bool[], bool[]);

	void StartDeltaTest(int userid, int b0, int cct0, TestMode mode);
	void StopDeltaTest();
};
