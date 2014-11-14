#include "button.h"
#include "helpers.h"


Button::Button(std::string pSection)
{
	done = false;
	isPressed = false;
	Section=pSection;

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->PortNr = ini->Read<int>(pSection,"PortNr",0);
	
	
	ini = Settings::getInstance();
	log = Logger::getInstance();

	elapsedTime=0;
	tstart=0;
	tstop=0;

	spawn();
}

Button::~Button() 
{ 
	done = true;
}

bool Button::getIsPressed()
{
	return isPressed;
}

unsigned long Button::startListen()
{
  
	cout << this->Name <<": start listening..." << endl;
	while (!done) 
	{			
		if (!isPressed && PortVal < 100) ButtonDown();
		else if (isPressed && PortVal < 100) ButtonPressed();
		else if (isPressed && PortVal > 1000) ButtonUp();

		lumitech::sleep(100);
	}
	return 0;
}

void Button::ButtonDown(void)
{
	isPressed = true;
	tstart=clock();

	//call ControlBox Callback Function
}

void Button::ButtonPressed(void)
{
	tstop=clock();
	elapsedTime = (((float)tstop)-((float)tstart)); 

	isPressed = true;
	//call ControlBox Callback Function
}

void Button::ButtonUp(void)
{
	isPressed = false;
	//call ControlBox Callback Function
}

