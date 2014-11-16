#include "button.h"
#include "helpers.h"


Button::Button(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	done = false;
	isPressed = false;
	Section=pSection;

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->PortNr = ini->Read<int>(pSection,"PortNr",0);
	threadSleepTime = ini->Read<int>(pSection, "Sleep", 100);
	
	
	elapsedTime=0;
	tstart=0;
	tstop=0;
	PortVal = 1000;

#ifdef _DEBUG

	//tc = TestClient(ini->Read<string>(pSection, "Testing", ""));
#endif

	spawn();
}

Button::~Button() 
{ 
	done = true;

#ifdef WIN32
	WaitForSingleObject(thisThread, INFINITE);
#else
	//close(socket_id) does not close the accept, therefore it would hang here if I wait on the join
	//listenThread.join();
#endif
}

int Button::getPortVal()
{
#ifdef _DEBUG
	cin >> PortVal;

	return PortVal;
#else
#endif

}

bool Button::getIsPressed()
{
	return isPressed;
}

unsigned long Button::startListen()
{  
	log->cout(this->Name+": start listening...");
	while (!done) 
	{	
		//log->cout(this->Name + ": waiting...");
		getPortVal();

		if (!isPressed && PortVal < 100) ButtonDown();
		else if (isPressed && PortVal < 100) ButtonPressed();
		else if (isPressed && PortVal > 1000) ButtonUp();

		lumitech::sleep(threadSleepTime);
	}
	log->cout(this->Name + ": stop listening...");
	return 0;
}

void Button::ButtonDown(void)
{
	isPressed = true;
	tstart=clock();

	log->cout(this->Name + ": ButtonDown");
	for (unsigned int i = 0; i < notifyClients.size(); i++)
		notifyClients[i]->notify(BUTTON_DOWN, 0);
	//call ControlBox Callback Function
}

void Button::ButtonPressed(void)
{
	log->cout(this->Name + ": ButtonPressed");

	tstop=clock();
	elapsedTime = (((float)tstop)-((float)tstart)); 
	isPressed = true;

	for (unsigned int i = 0; i < notifyClients.size(); i++)
		notifyClients[i]->notify(BUTTON_PRESSED, 0);
	//call ControlBox Callback Function
}

void Button::ButtonUp(void)
{
	log->cout(this->Name + ": ButtonUp");
	isPressed = false;
	//call ControlBox Callback Function
	for (unsigned  int i = 0; i < notifyClients.size(); i++)
		notifyClients[i]->notify(BUTTON_UP, 0);

}

void Button::addClient(IButtonObserver* obs)
{
	notifyClients.push_back(obs);
}

