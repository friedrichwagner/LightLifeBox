#include <vector>
//#include <math.h>
#include <stdlib.h> 
#include <iostream>
#include "Settings.h"

#include "PIButton.h"
#include "Button.h"

#if defined (RASPI)
	#include <wiringPi.h>
#endif

using namespace std;

vector<PIButton*> pibuttons;

//Brightnss Button
void isr_BrightnessUp(void);
void isr_BrightnessDown(void);
void isr_BrightnessPressed(void);

void isr_CCTUp(void);
void isr_CCTDown(void);
void isr_CCTPressed(void);

void isr_JuddUp(void);
void isr_JuddDown(void);
void isr_JuddPressed(void);

void isr_Lock(void);

void InitWiringPi()
{
#if defined (RASPI)
	//Init Library
	wiringPiSetup();

	//Config Port as Input, Output
	//Brightness
	//pinMode(A2, INPUT); pinMode(B2, INPUT); pinMode(TAST2, INPUT);
	//CCT
	//pinMode(A3, INPUT); pinMode(B3, INPUT);	pinMode(TAST3, INPUT);
	//Judd
	//pinMode(A1, INPUT); pinMode(B1, INPUT); pinMode(TAST1, INPUT); 

	//Lock1, Lock 2
	//pinMode(EINGABETAST, INPUT); pinMode(EINGABETAST2, INPUT);

	//LEDs
	//pinMode(LED1, OUTPUT); pinMode(LED2, OUTPUT); pinMode(LED3, OUTPUT);
#endif

}

void InitPIButton(PIButton* p, string Section, LightLifeButtonType btntype)
{
	static int lock1 = 0;

	if (p != NULL)
	{
		Settings* ini = Settings::getInstance();
		p->cnt = 0;
		p->change = false;
		p->portUp = ini->Read<unsigned int>(Section, "PortUp", A2);
		p->portDown = ini->Read<unsigned int>(Section, "PortDown", B2);
		p->portPressed = ini->Read<unsigned int>(Section, "PortPressed", TAST2);
		p->portLED = ini->Read<unsigned int>(Section, "PortLED", LED2);
		p->deltaCnt = ini->Read<unsigned int>(Section, "DeltaCnt", 1);
		p->factor = ini->Read<unsigned int>(Section, "Factor", 1);
		p->enablePressedEvent = ini->Read<bool>(Section, "EnablePortPressed", 1);

#if defined (RASPI)
		if (portUp>0) pinMode(portUp, INPUT); 
		if (portDown>0) pinMode(portDown, INPUT);
		if (portPressed>0) pinMode(portPressed, INPUT);
		if (portLED>0) pinMode(portLED, OUTPUT);
#endif

		switch (btntype)
		{
		case BRIGHTNESS:
			p->pibtnType = PIBUTTON_BRIGHTNESS;
			p->isr_Up = isr_BrightnessUp; p->isr_Down = isr_BrightnessDown;
			if (p->enablePressedEvent)
				p->isr_Pressed = isr_BrightnessPressed;
			else
				p->isr_Pressed = NULL;
			break;
		case CCT:
			p->pibtnType = PIBUTTON_CCT;
			p->isr_Up = isr_CCTUp; p->isr_Down = isr_CCTDown;

			if (p->enablePressedEvent)
				p->isr_Pressed = isr_CCTPressed;
			else
				p->isr_Pressed = NULL;
			break;

		case JUDD:
			p->pibtnType = PIBUTTON_JUDD;
			p->isr_Up = isr_JuddUp; p->isr_Down = isr_JuddDown;
			if (p->enablePressedEvent)
				p->isr_Pressed = isr_JuddPressed;
			else
				p->isr_Pressed = NULL;
			break;

		case LOCK:
			if (lock1 == 0)
			{
				p->pibtnType = PIBUTTON_LOCK1;
				lock1 = 1;
			}
			else
				p->pibtnType = PIBUTTON_LOCK2;

			p->isr_Up = NULL; p->isr_Down = NULL; p->isr_Pressed = &isr_Lock;

			break;

		}

#if defined (RASPI)
		if ((pibtn.portUp>0) && pibtn.isr_Up != NULL) wiringPiISR(pibtn.portUp, INT_EDGE_FALLING, pibtn.isr_Up);
		if ((pibtn.portDown>0) && pibtn.isr_Down != NULL) wiringPiISR(pibtn.portDown, INT_EDGE_FALLING, pibtn.isr_Down);
		if ((pibtn.portPressed>0) && pibtn.isr_Pressed != NULL) wiringPiISR(pibtn.portPressed, INT_EDGE_FALLING, pibtn.isr_Pressed);
#endif

	}
}


#if !defined (RASPI)
int  wiringPiISR(int pin, int mode, void(*function)(void))
{

	return 1;
}
#endif

//----------------------------
//--------General --------- 
//----------------------------
void isr_General(int index, int dir)
{
	if (pibuttons[index] != NULL)
	{
		if (!pibuttons[index]->change)
		{
			if ((dir > 0) && pibuttons[index]->cnt < 0) pibuttons[index]->cnt = 0;
			else if ((dir < 0) && pibuttons[index]->cnt > 0) pibuttons[index]->cnt = 0;

			pibuttons[index]->cnt = pibuttons[index]->cnt + dir; //dir = -1 oder +1			
			//cout << pibuttons[index]->cnt;

			if ((unsigned int)abs(pibuttons[index]->cnt) >= pibuttons[index]->deltaCnt)
			{
				if (pibuttons[index]->ButtonEvent != 0)
				{				
					//Das ist echt grauslich: Pointer to member function aus vector
					Button* instance = pibuttons[index]->btn;
					int val = dir * pibuttons[index]->factor;
					(instance->*(pibuttons[index]->ButtonEvent))((PIButtonTyp)index, val);
				}
				pibuttons[index]->cnt = 0;
				pibuttons[index]->change = true;
			}
		}
		else
			pibuttons[index]->change = false;
	}
}

void isr_PressedGeneral(int index)
{
	if (pibuttons[index]->ButtonEvent != NULL)
	{
		Button* instance = pibuttons[index]->btn;
		(instance->*(pibuttons[index]->ButtonEvent))((PIButtonTyp)index, 0);
	}
}

//----------------------------
//--------Brightness --------- 
//----------------------------
void isr_BrightnessUp(void)
{
	isr_General(PIBUTTON_BRIGHTNESS, +1);
#if defined(RASPI)
	delay(200);
#endif
}

void isr_BrightnessDown(void)
{
	isr_General(PIBUTTON_BRIGHTNESS, -1);
#if defined(RASPI)
	delay(200);
#endif
}

void isr_BrightnessPressed(void)
{
	isr_PressedGeneral(PIBUTTON_BRIGHTNESS);
#if defined(RASPI)
	delay(200);
#endif
}

//----------------------------
//--------       CCT --------- 
//----------------------------
void isr_CCTUp(void)
{
	isr_General(PIBUTTON_CCT, +1);
#if defined(RASPI)
	delay(200);
#endif
}

void isr_CCTDown(void)
{
	isr_General(PIBUTTON_CCT, -1);
#if defined(RASPI)
	delay(200);
#endif
}

void isr_CCTPressed(void)
{
	isr_PressedGeneral(PIBUTTON_CCT);
#if defined(RASPI)
	delay(200);
#endif
}

//----------------------------
//--------  Judd    --------- 
//----------------------------
void isr_JuddUp(void)
{
	isr_General(PIBUTTON_JUDD, +1);
#if defined(RASPI)
	delay(200);
#endif
}

void isr_JuddDown(void)
{
	isr_General(PIBUTTON_JUDD, -1);
#if defined(RASPI)
	delay(200);
#endif
}

void isr_JuddPressed(void)
{
	isr_PressedGeneral(PIBUTTON_JUDD);
#if defined(RASPI)
	delay(200);
#endif
}

//----------------------------
//--------  Lock Button    --------- 
//----------------------------
void isr_Lock(void)
{
	isr_PressedGeneral(PIBUTTON_LOCK1);
#if defined(RASPI)
	delay(200);
#endif
}

