#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include <time.h>
#include "IPClient.h"
#include <thread>

using namespace std;

enum LightLifeButtonType : int { NONE=0, LOCK=1, BRIGHTNESS=2, CCT=3, JUDD=4};

class Button
{
protected:
	//Fields
	volatile bool done;
	unsigned int ID;
	LightLifeButtonType btntype;
	//unsigned int PortNr;
	int PortVal;
	string	Name;
	string	Section;
	Settings* ini;
	Logger* log;
	bool isPressed;
	clock_t tstart, tstop;
	float elapsedTime;
	//int threadSleepTime;

	//Functions
	int getPortVal();
	void ButtonDown(void);
	void ButtonUp(void);
	void ButtonPressed(void);
	virtual unsigned long startListen(void);

	//Observer Pattern
	vector<IButtonObserver*> notifyClients;

	//Testing
	TestClient* tc;

	std::thread thisThread;
	virtual void spawn()
	{
		thisThread = std::thread(&Button::startListen, this);
	};

public:
	Button(std::string pName);
	~Button();	

	bool Active;
	bool getIsPressed();
	string getName();
	LightLifeButtonType getBtnType();
	int getID();
	void stop();
	void start();

	void addClient(IButtonObserver* obs);
};
