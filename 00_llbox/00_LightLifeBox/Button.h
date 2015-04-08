#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include <time.h>
#include "IPClient.h"
#include "PIButton.h"


using namespace std;

enum LightLifeButtonType : int { NONE=0, LOCK=1, BRIGHTNESS=2, CCT=3, JUDD=4};

class Button
{
protected:
	//Fields
	volatile bool done;
	unsigned int ID;
	LightLifeButtonType btntype;
	string	Name;
	string	Section;
	Settings* ini;
	Logger* log;
	bool isPressed;
	bool Active;

	//Testing
	TestClient* tc;

	//Observer Pattern
	vector<IButtonObserver*> notifyClients;

	//Hardware Button
	PIButton pibtn;
	void InitPIButton();
public:
	Button(std::string pName);
	~Button();	
	void ButtonEvent(PIButtonTyp t, int delta);

	bool enablePressedEvent;
	string getName();
	LightLifeButtonType getBtnType();
	int getID();

	bool setActive(bool);
	void addClient(IButtonObserver* obs);
};
