#pragma once

#include <string>
#include "PILight.h"
#include "Settings.h"
#include "Logger.h"
#include "TastButton.h"

using namespace std;

class ControlBox : IButtonObserver
{
protected:
	static ControlBox* _instance;	//Class property!

	//Fields
	unsigned int ID;	
	string Name;
	string IP;
	vector<TastButton*> Potis;
	vector<Button*> Buttons;
	int threadSleepTime;
	stringstream sslog;
	
	Settings* ini;
	Logger* log;

	//Functions
	bool Init();

	ControlBox(std::string pName); //private contructor	
public:
	static ControlBox* getInstance(string);
	static ControlBox* getInstance();	//should only be used when _instance is not NULL
	~ControlBox();	

	bool EventLoop();
	void stopEventLoop();
	void Beep(int freq, int time);

	vector<PILight*> Lights;
	bool isDone;

	void notify(void* sender, enumButtonEvents event, long val);
};
