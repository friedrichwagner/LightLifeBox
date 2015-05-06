#include "Button.h"
#include "helpers.h"

#if defined (RASPI)
	#include "wiringPi.h"
#endif

#define BLINK_DELAY 400


Button::Button(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	done = false;
	isPressed = false;
	Section=pSection;
	Active = false;

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->btntype = (LightLifeButtonType)ini->Read<int>(pSection, "LightLifeButtonType", (int)(NONE));

	PIButtonInit();

	doneBlink = true;
}

Button::~Button() 
{ 
	setActive(false);

#if defined(_DEBUG) && !defined(RASPI)
	delete tc;
#endif
}

void Button::PIButtonInit()
{
	InitPIButton(&pibtn, Section, (int)btntype);
	pibtn.ButtonEvent = &Button::ButtonEvent;
	pibtn.btn = this;

#if defined(_DEBUG) && !defined(RASPI)
	string mockObjectName = ini->Read<string>(Section, "Testing", "");
	tc = new TestClient(mockObjectName, pibtn.pibtnType);
#endif
}

void Button::ButtonEvent(PIButtonTyp t, int delta)
{
	if (notifyClients[0] != NULL)
	{
		if (Active)
		{
			if (delta == 0)
				notifyClients[0]->notify(this, BUTTON_PRESSED, 0);
			else
			{
				notifyClients[0]->notify(this, BUTTON_CHANGE, delta);
			}
		}
		else
			log->cout("Button: " + this->Name + "=inActive \t delta=" + lumitech::itos(delta));
	}
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

bool Button::setActive(bool b)
{
	Active = b;
#if defined (RASPI)
	//Set LED 
	if (Active)
	{
		if (this->pibtn.portLED>0)
			digitalWrite(this->pibtn.portLED, HIGH);
		log->cout(this->Name + "= ACTIVE" );
	}
	else
	{
		if (this->pibtn.portLED>0)
			digitalWrite(this->pibtn.portLED, LOW);
		log->cout(this->Name + "= IN-ACTIVE");
	}
#else
	//Set LED 
	if (Active)
		log->cout(this->Name + "= ACTIVE");
	else
		log->cout(this->Name + "= IN-ACTIVE");
#endif

	return Active;
}

void Button::startBlink(bool start)
{
	if (doneBlink && start)
	{
		doneBlink = false;
		spawnBlinkThread();
	}

	if (!start)
	{
		doneBlink = true;
		if (threadBlink.joinable())  threadBlink.join();
	}

}

unsigned long Button::blinkLED()
{

	while (!doneBlink)
	{
#if defined (RASPI)
		if (this->pibtn.portLED>0)
			digitalWrite(this->pibtn.portLED, HIGH);
		lumitech::sleep(BLINK_DELAY);

		if (this->pibtn.portLED>0)
			digitalWrite(this->pibtn.portLED, LOW);
		lumitech::sleep(BLINK_DELAY);
#else
		log->cout(this->Name + "= BLINK ACTIVE");
		lumitech::sleep(BLINK_DELAY);

		log->cout(this->Name + "= BLINK IN-ACTIVE");
		lumitech::sleep(BLINK_DELAY);
#endif
	}


	return 0;
}

