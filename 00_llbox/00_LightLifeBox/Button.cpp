#include "Button.h"
#include "helpers.h"


Button::Button(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	done = false;
	isPressed = false;
	Section=pSection;
	Active = true;

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->PortNr = ini->Read<int>(pSection,"PortNr",0);
	threadSleepTime = ini->Read<int>(pSection, "Sleep", 100);
	
	
	elapsedTime=0;
	tstart=0;
	tstop=0;
	PortVal = 1000;

#ifdef _DEBUG
	string mockObjectName = ini->Read<string>(pSection, "Testing", "");
	tc = new TestClient(mockObjectName);

	if (tc->connected())
	{
		tc->send(mockObjectName);
	}
#endif
}

Button::~Button() 
{ 
	if (!done) stop();

#ifdef _DEBUG
	delete tc;
#endif
}

int Button::getPortVal()
{
#ifdef _DEBUG
	if (tc->connected())
	{
		int len = -1;
		int val = tc->getPortVal(&len); //this is blocking when the TestServer is running
		//if (len < 0) done = true;
		return val;
	}
	else
		done = true;

#endif

	return -100;
}

bool Button::getIsPressed()
{
	return isPressed;
}

unsigned long Button::startListen()
{  
	log->cout(this->Name+": Button start listening ...");
	while (!done)
	{
		//log->cout(this->Name + ": waiting...");
		PortVal = getPortVal();

		if (Active)
		{
			if (!isPressed && PortVal == -1) ButtonDown();
			//else if (isPressed && PortVal < 10) ButtonPressed();
			else if (isPressed && PortVal == 1001) ButtonUp();
		}
		//lumitech::sleep(threadSleepTime);
	}

	log->cout(this->Name + ": stop listening...");
	
	return 0;
}

void Button::start()
{
	spawn();
}

void Button::stop()
{
	done = true;
	if (thisThread.joinable()) thisThread.join();
}


void Button::ButtonDown(void)
{
	isPressed = true;
	tstart=clock();

	//log->cout(this->Name + ": ButtonDown");
	for (unsigned int i = 0; i < notifyClients.size(); i++)
	{
		if (notifyClients[i] != NULL)
			notifyClients[i]->notify(this, BUTTON_DOWN, PortVal);
	}
		
	//call ControlBox Callback Function
}

void Button::ButtonPressed(void)
{
	//log->cout(this->Name + ": ButtonPressed");

	tstop=clock();
	elapsedTime = (((float)tstop)-((float)tstart)); 
	isPressed = true;

	for (unsigned int i = 0; i < notifyClients.size(); i++)
		if (notifyClients[i] != NULL)
			notifyClients[i]->notify(this, BUTTON_PRESSED, PortVal);
	//call ControlBox Callback Function
}

void Button::ButtonUp(void)
{
	//log->cout(this->Name + ": ButtonUp");
	isPressed = false;
	//call ControlBox Callback Function
	for (unsigned  int i = 0; i < notifyClients.size(); i++)
		if (notifyClients[i] != NULL)
			notifyClients[i]->notify(this, BUTTON_UP, PortVal);

}

void Button::addClient(IButtonObserver* obs)
{
	notifyClients.push_back(obs);
}

string Button::getName()
{
	return Name;
}

int Button::getID()
{
	return ID;
}
