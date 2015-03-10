#include "Button.h"
#include "helpers.h"


Button::Button(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	done = false;
	isPressed = false;
	Section=pSection;
	Active = false; //Box muss von Admin Console aktiviert werden

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->btntype = (LightLifeButtonType)ini->Read<int>(pSection, "LightLifeButtonType", (int)(NONE));
	//this->PortNr = ini->Read<int>(pSection,"PortNr",0);
	//threadSleepTime = ini->Read<int>(pSection, "Sleep", 100);

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

void Button::start()
{
	//spawn();
}

void Button::stop()
{
	done = true;
	//if (thisThread.joinable()) thisThread.join();
}


bool Button::getIsPressed()
{
	return isPressed;
}

/*
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
			else if (isPressed && PortVal == 1001) ButtonUp();
		}
		else
			log->cout("Button(" + this->Name + ") InActive! Val=" + lumitech::itos(PortVal));
	}

	log->cout(this->Name + ": stop listening...");
	
	return 0;
}

int Button::getPortVal()
{
#if defined(_DEBUG) && defined(WIN32)
	if (tc->connected())
	{
		int len = -1;
		int val = tc->getPortVal(&len); //this is blocking when the TestServer is running
		if (len < 0) done = true;
		return val;
	}
	else
		done = true;
#else

#endif

	return -100;
}

void Button::ButtonDown(void)
{
	isPressed = true;
	tstart=clock();

	//Gibt eh nur ControlBox
	if (notifyClients[0] != NULL)
		notifyClients[0]->notify(this, BUTTON_DOWN, PortVal);
}

void Button::ButtonPressed(void)
{
	//log->cout(this->Name + ": ButtonPressed");

	tstop=clock();
	elapsedTime = (((float)tstop)-((float)tstart)); 
	isPressed = true;

	if (notifyClients[0] != NULL)
		notifyClients[0]->notify(this, BUTTON_PRESSED, PortVal);
}

void Button::ButtonUp(void)
{
	//log->cout(this->Name + ": ButtonUp");
	isPressed = false;
	if (notifyClients[0] != NULL)
		notifyClients[0]->notify(this, BUTTON_UP, PortVal);

}
*/

void Button::ButtonEvent(PIButtonTyp t, int delta)
{
	if (notifyClients[0] != NULL)
	if (delta == 0)
		notifyClients[0]->notify(this, BUTTON_PRESSED, 0);
	else
		notifyClients[0]->notify(this, BUTTON_PRESSED, delta);
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

LightLifeButtonType Button::getBtnType()
{
	return btntype;
}

