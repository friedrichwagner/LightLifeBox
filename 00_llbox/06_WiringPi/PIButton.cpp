#include <map>
//#include <math.h>
#include <stdlib.h> 
#include <iostream>
#include <chrono>
#include "Settings.h"

#include "PIButton.h"
#include "Button.h"

#if defined (RASPI)
	#include <wiringPi.h>
#endif

using namespace std;

//vector<PIButton*> pibuttons;
map<int, PIButton*> pibuttons;


//Brightnss Button
void isr_BrightnessUpDown(void);
void isr_BrightnessPressed(void);

void isr_CCTUpDown(void);
void isr_CCTPressed(void);

void isr_JuddUpDown(void);
void isr_JuddPressed(void);

void isr_Lock1(void);
void isr_Lock2(void);

void InitWiringPi()
{
#if defined (RASPI)
	//Init Library
	wiringPiSetup();
#endif

}

static std::chrono::time_point<std::chrono::system_clock> last;


void InitPIButton(PIButton* p, string Section, int btntype)
{
	static int lock1 = 0;
	last = std::chrono::system_clock::now();

	if (p != NULL)
	{
		Settings* ini = Settings::getInstance();
		p->value = 0;
		p->lastvalue = 0;
		p->lastEncoded = 0;
		p->portUp = ini->Read<unsigned int>(Section, "PortUp", 0);
		p->portDown = ini->Read<unsigned int>(Section, "PortDown", 0);
		p->portPressed = ini->Read<unsigned int>(Section, "PortPressed", 0);
		p->portLED = ini->Read<unsigned int>(Section, "PortLED", 0);
		p->deltaCnt = ini->Read<unsigned int>(Section, "DeltaCnt", 1);
		p->factor = ini->Read<unsigned int>(Section, "Factor", 1);
		p->enablePressedEvent = ini->Read<bool>(Section, "EnablePortPressed", 1);

#if defined (RASPI)
		if (p->portUp>0) pinMode(p->portUp, INPUT); 
		if (p->portDown>0) pinMode(p->portDown, INPUT);
		if (p->portPressed>0) pinMode(p->portPressed, INPUT);
		if (p->portLED>0) pinMode(p->portLED, OUTPUT);
#endif

		switch (btntype)
		{
		case BRIGHTNESS:
			p->pibtnType = PIBUTTON_BRIGHTNESS;
			p->isr_UpDown = isr_BrightnessUpDown;
			if (p->enablePressedEvent)
				p->isr_Pressed = isr_BrightnessPressed;
			else
				p->isr_Pressed = NULL;
			break;
		case CCT:
			p->pibtnType = PIBUTTON_CCT;
			p->isr_UpDown = isr_CCTUpDown;

			if (p->enablePressedEvent)
				p->isr_Pressed = isr_CCTPressed;
			else
				p->isr_Pressed = NULL;
			break;

		case JUDD:
			p->pibtnType = PIBUTTON_JUDD;
			p->isr_UpDown = isr_JuddUpDown;
			if (p->enablePressedEvent)
				p->isr_Pressed = isr_JuddPressed;
			else
				p->isr_Pressed = NULL;
			break;

		case LOCK:
			if (lock1 == 0)
			{
				p->pibtnType = PIBUTTON_LOCK1;
				p->isr_UpDown = NULL; p->isr_Pressed = &isr_Lock1;

				lock1 = 1;
			}
			else
			{
				p->pibtnType = PIBUTTON_LOCK2;
				p->isr_UpDown = NULL; p->isr_Pressed = &isr_Lock2;
			}

			

			break;

		}

#if defined (RASPI)
		if ((p->portUp>0) && p->isr_UpDown != NULL) wiringPiISR(p->portUp, INT_EDGE_BOTH, p->isr_UpDown);
		if ((p->portDown>0) && p->isr_UpDown != NULL) wiringPiISR(p->portDown, INT_EDGE_BOTH, p->isr_UpDown);
		if ((p->portPressed>0) && p->isr_Pressed != NULL) 
		{
			//cout << "Port pressed:" << p->portPressed << "\r\n";
			wiringPiISR(p->portPressed, INT_EDGE_FALLING, p->isr_Pressed);
		}
#endif

		pibuttons[p->pibtnType] = p;

	}
}


#if !defined (RASPI)
int  wiringPiISR(int pin, int mode, void(*function)(void))
{

	return 1;
}
#endif


//http://theatticlight.net/posts/Reading-a-Rotary-Encoder-from-a-Raspberry-Pi/
void isr_General2(int btntype)
{
	int dir=+1;

#if defined (RASPI)
	int MSB = digitalRead(pibuttons[btntype]->portUp);
	int LSB = digitalRead(pibuttons[btntype]->portDown);
	

	int encoded = (MSB << 1) | LSB;
	int sum = (pibuttons[btntype]->lastEncoded << 2) | encoded;

	//if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011) pibuttons[btntype]->value++;
	//if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000) pibuttons[btntype]->value--;

	//Only one increment per detent
	if (sum == 0b0100 || sum == 0b1011) pibuttons[btntype]->value++;
	if (sum == 0b0111 || sum == 0b1000) 
	{
		pibuttons[btntype]->value--;
		dir=-1;
	}

	//if (sum == 0b0100 || sum == 0b1011) pibuttons[btntype]->value = +1;
	//if (sum == 0b0111 || sum == 0b1000) pibuttons[btntype]->value = -1;

	pibuttons[btntype]->lastEncoded = encoded;	
#else
	if ((pibuttons[btntype]->value - pibuttons[btntype]->lastvalue) < 0)
		dir = -1;
#endif
	
	//cout << pibuttons[btntype]->lastvalue << ";" << pibuttons[btntype]->value << "\r\n";
	unsigned int delta = (unsigned int)abs(pibuttons[btntype]->lastvalue  - pibuttons[btntype]->value);
	//if ((pibuttons[btntype]->value >= (int)pibuttons[btntype]->deltaCnt) || (pibuttons[btntype]->value <= (int)pibuttons[btntype]->deltaCnt))
	if (delta >= pibuttons[btntype]->deltaCnt)
	{
		if (pibuttons[btntype]->ButtonEvent != 0)
		{
			//Das ist echt grauslich: Pointer to member function aus vector
			Button* instance = pibuttons[btntype]->btn;
			int val = dir * pibuttons[btntype]->factor;
			//cout << pibuttons[btntype]->lastvalue << ";" << pibuttons[btntype]->value << "\r\n";
			(instance->*(pibuttons[btntype]->ButtonEvent))((PIButtonTyp)btntype, val);

			pibuttons[btntype]->lastvalue = pibuttons[btntype]->value;
		}
	}
#if defined(RASPI)
	delay(20);
#endif

}

void isr_PressedGeneral(int btntype)
{
	//De-bouncing
	std::chrono::duration<double> elapsed_seconds = std::chrono::system_clock::now() - last;
	cout << elapsed_seconds.count() << "\n";
	if (elapsed_seconds.count() < 0.5) return;	
	last = std::chrono::system_clock::now();

	if ((pibuttons[btntype]->ButtonEvent != NULL) && pibuttons[btntype]->enablePressedEvent)
	{
		Button* instance = pibuttons[btntype]->btn;
		(instance->*(pibuttons[btntype]->ButtonEvent))((PIButtonTyp)btntype, 0);
	}	

#if defined(RASPI)
	//delay(200);
#endif

}

//----------------------------
//--------Brightness --------- 
//----------------------------
void isr_BrightnessUpDown(void)
{
	isr_General2(PIBUTTON_BRIGHTNESS);
}

void isr_BrightnessPressed(void)
{
	isr_PressedGeneral(PIBUTTON_BRIGHTNESS);
}

//----------------------------
//--------       CCT --------- 
//----------------------------
void isr_CCTUpDown(void)
{
	isr_General2(PIBUTTON_CCT);
}


void isr_CCTPressed(void)
{
	isr_PressedGeneral(PIBUTTON_CCT);
}

//----------------------------
//--------  Judd    --------- 
//----------------------------
void isr_JuddUpDown(void)
{
	isr_General2(PIBUTTON_JUDD);
}

void isr_JuddPressed(void)
{
	isr_PressedGeneral(PIBUTTON_JUDD);
}

//----------------------------
//--------  Lock Button    --------- 
//----------------------------
void isr_Lock1(void)
{
	isr_PressedGeneral(PIBUTTON_LOCK1);
}

void isr_Lock2(void)
{
	isr_PressedGeneral(PIBUTTON_LOCK2);
}
