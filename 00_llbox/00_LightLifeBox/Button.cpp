#include "Button.h"
#include "helpers.h"

#if defined (RASPI)
	#include "wiringPi.h"
#endif


Button::Button(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	done = false;
	isPressed = false;
	Section=pSection;
	Active = false; // ini->Read<bool>(pSection, "ButtonsDefaultActive", false);

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->btntype = (LightLifeButtonType)ini->Read<int>(pSection, "LightLifeButtonType", (int)(NONE));

	InitPIButton();
}

Button::~Button() 
{ 
	setActive(false);
	delete tc;
}

void Button::InitPIButton()
{
	static int lock1 = 0;
	this->pibtn.cnt = 0;
	this->pibtn.change = false;
	this->pibtn.portUp = ini->Read<unsigned int>(Section, "PortUp", A2);
	this->pibtn.portDown = ini->Read<unsigned int>(Section, "PortDown", B2);
	this->pibtn.portPressed = ini->Read<unsigned int>(Section, "PortPressed", TAST2);
	this->pibtn.portLED = ini->Read<unsigned int>(Section, "PortLED", LED2);	
	this->pibtn.deltaCnt = ini->Read<unsigned int>(Section, "DeltaCnt", 1);
	this->pibtn.factor = ini->Read<unsigned int>(Section, "Factor", 1);

	enablePressedEvent = ini->Read<bool>(Section, "EnablePortPressed", 1);

	this->pibtn.ButtonEvent = &Button::ButtonEvent;
	this->pibtn.btn = this;

	switch (btntype)
	{
	case BRIGHTNESS:
		this->pibtn.pibtnType = PIBUTTON_BRIGHTNESS;
		this->pibtn.isr_Up = isr_BrightnessUp;this->pibtn.isr_Down = isr_BrightnessDown;
		if (enablePressedEvent)
			this->pibtn.isr_Pressed = isr_BrightnessPressed;
		else
			this->pibtn.isr_Pressed = NULL;
		break;
	case CCT:
		this->pibtn.pibtnType = PIBUTTON_CCT;
		this->pibtn.isr_Up = isr_CCTUp; this->pibtn.isr_Down = isr_CCTDown; 
		
		if (enablePressedEvent)
			this->pibtn.isr_Pressed = isr_CCTPressed;
		else
			this->pibtn.isr_Pressed = NULL;
		break;

	case JUDD:
		this->pibtn.pibtnType = PIBUTTON_JUDD;
		this->pibtn.isr_Up = isr_JuddUp; this->pibtn.isr_Down = isr_JuddDown; 
		if (enablePressedEvent)
			this->pibtn.isr_Pressed = isr_JuddPressed;
		else
			this->pibtn.isr_Pressed = NULL;
		break;

	case LOCK:
		if (lock1 == 0)
		{
			this->pibtn.pibtnType = PIBUTTON_LOCK1;
			lock1 = 1;
		}
		else
			this->pibtn.pibtnType = PIBUTTON_LOCK2;

		this->pibtn.isr_Up = NULL; this->pibtn.isr_Down = NULL; this->pibtn.isr_Pressed = &isr_Lock;
		
		break;

	}

#if defined (RASPI)
	if ((pibtn.portUp>0) && pibtn.isr_Up != NULL) wiringPiISR(pibtn.portUp, INT_EDGE_FALLING, pibtn.isr_Up);
	if ((pibtn.portDown>0) && pibtn.isr_Down != NULL) wiringPiISR(pibtn.portDown, INT_EDGE_FALLING, pibtn.isr_Down);
	if ((pibtn.portPressed>0)  &&  pibtn.isr_Pressed!= NULL) wiringPiISR(pibtn.portPressed, INT_EDGE_FALLING, pibtn.isr_Pressed);
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

