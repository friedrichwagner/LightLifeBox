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
	Active = true; //TEST

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->btntype = (LightLifeButtonType)ini->Read<int>(pSection, "LightLifeButtonType", (int)(NONE));

	InitPIButton();
}

Button::~Button() 
{ 
	delete tc;
}

void Button::InitPIButton()
{
	int lock1 = 0;
	this->pibtn.cnt = 0;
	this->pibtn.change = false;
	this->pibtn.portUp = ini->Read<unsigned int>(Section, "PortUp", A2);
	this->pibtn.portDown = ini->Read<unsigned int>(Section, "PortDown", B2);
	this->pibtn.portPressed = ini->Read<unsigned int>(Section, "PortPressed", TAST2);
	this->pibtn.portLED = ini->Read<unsigned int>(Section, "PortLED", LED2);	
	this->pibtn.deltaCnt = ini->Read<unsigned int>(Section, "DeltaCnt", 1);
	this->pibtn.factor = ini->Read<unsigned int>(Section, "Factor", 1);

	this->pibtn.ButtonEvent = &Button::ButtonEvent;
	this->pibtn.btn = this;

	switch (btntype)
	{
	case BRIGHTNESS:
		this->pibtn.pibtnType = PIBUTTON_BRIGHTNESS;
		this->pibtn.isr_Up = isr_BrightnessUp;this->pibtn.isr_Down = isr_BrightnessDown;this->pibtn.isr_Pressed = isr_BrightnessPressed;
		break;
	case CCT:
		this->pibtn.pibtnType = PIBUTTON_CCT;
		this->pibtn.isr_Up = isr_CCTUp; this->pibtn.isr_Down = isr_CCTDown; this->pibtn.isr_Pressed = isr_CCTPressed;
		break;

	case JUDD:
		this->pibtn.pibtnType = PIBUTTON_JUDD;
		this->pibtn.isr_Up = isr_JuddUp; this->pibtn.isr_Down = isr_JuddDown; this->pibtn.isr_Pressed = isr_JuddPressed;
		break;

	case LOCK:
		if (lock1 == 0)
		{
			this->pibtn.pibtnType = PIBUTTON_LOCK1;
			lock1 = 1;
		}
		else
			this->pibtn.pibtnType = PIBUTTON_LOCK1;

		this->pibtn.isr_Up = NULL; this->pibtn.isr_Down = NULL; this->pibtn.isr_Pressed = isr_Lock;
		
		break;

	}

#if defined (RASPI)
	if (pibtn.portUp>0) wiringPiISR(pibtn.portUp, INT_EDGE_FALLING, pibtn.portUp.isr_Up);
	if (pibtn.portDown>0) wiringPiISR(pibtn.portDown, INT_EDGE_FALLING, pibtn.portDown.isr_Down);
	if (pibtn.portPressed>0) wiringPiISR(pibtn.portPressed, INT_EDGE_FALLING, pibtn.portPressed.isr_Pressed);
#endif

	pibtn.index = pibuttons.size();
	pibuttons.push_back(&pibtn);

#if defined(_DEBUG) && !defined(RASPI)
	string mockObjectName = ini->Read<string>(Section, "Testing", "");
	tc = new TestClient(mockObjectName, pibtn.index);
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

