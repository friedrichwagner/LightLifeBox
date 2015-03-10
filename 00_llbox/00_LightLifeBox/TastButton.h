#pragma once

#include <string>
#include "Button.h"

class TastButton :  public Button
{
protected:
	//Fields
	unsigned long delta;
	unsigned long actualValue;
	int PortVal2;
	

	//Functions
	/*void OnChange(long delta);
	virtual unsigned long startListen(void);

	int getPortVal2();
	void spawn()
	{
		thisThread = std::thread(&TastButton::startListen, this);
	};*/

public:
	TastButton(std::string pName);
	~TastButton();	

	unsigned long getValue();
	void setValue(unsigned long);
};
