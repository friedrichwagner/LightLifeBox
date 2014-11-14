#pragma once

#include <string>
#include "Button.h"

class TastButton :  public Button
{
protected:
	//Fields
	unsigned long delta;
	unsigned long actualValue;
	unsigned long PortVal2;
	

	//Functions
	void OnChange(long delta);
	virtual unsigned long startListen(void);
public:
	TastButton(std::string pName);
	~TastButton();	

	unsigned long getValue();
	void setValue(unsigned long);
};
