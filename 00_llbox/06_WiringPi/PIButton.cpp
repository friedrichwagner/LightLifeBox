#include <vector>
#include <math.h>

#include "PIButton.h"

#if !defined (RASPI)
	//#define INT_EDGE_FALLING 1	
	//int  wiringPiISR(int pin, int mode, void(*function)(void));
#else
	#include "wiringPI.h"
#endif

using namespace std;

struct PIButton
{
	int portLED;
	int portUp;
	int portDown;
	int portPressed;
	PIButtonTyp pibtnType;
	volatile bool change;
	int cnt;
	int delta;
	WiringPiISRFunc isr_Up;
	WiringPiISRFunc isr_Down;
	WiringPiISRFunc isr_Pressed;

	//WiringTastButtonPiEvent TastButtonEvent;
	Button* btn;
	WiringPiButtonEvent ButtonEvent;
};

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

//WiringPiButtonEvent _LockButton1Event;
//WiringPiButtonEvent _LockButton2Event;

void InitWiringPi();

//void InitPIButtons(WiringPiButtonEvent LockButton1Event, WiringPiButtonEvent LockButton2Event, WiringTastButtonPiEvent BrightnessEvent, WiringTastButtonPiEvent CCTEvent, WiringTastButtonPiEvent JuddEvent)
void InitPIButtons(WiringPiButtonEvent ButtonEvent, Button *_btnLock1, Button *_btnLock2, Button *potiBrightness, Button *potiCCT, Button *potiJudd)
{

	PIButton btnBrightness, btnCCT, btnJudd, btnLock1, btnLock2;

#if defined (RASPI)
	InitWiringPi();
#endif

	//Brightness
	btnBrightness.pibtnType =PIBUTTON_BRIGHTNESS; btnBrightness.cnt = 0; btnBrightness.delta = 1; btnBrightness.change = false;
	btnBrightness.portUp = A2; btnBrightness.portDown = B2; btnBrightness.portPressed = TAST2; btnBrightness.portLED = LED2;
	btnBrightness.isr_Up = &isr_BrightnessUp; btnBrightness.isr_Down = &isr_BrightnessDown; btnBrightness.isr_Pressed = &isr_BrightnessPressed;
	btnBrightness.ButtonEvent = ButtonEvent; btnBrightness.btn = potiBrightness;
	pibuttons.push_back(&btnBrightness);

	//CCT
	btnCCT.pibtnType = PIBUTTON_CCT; btnCCT.cnt = 0; btnCCT.delta = 1; btnCCT.change = false;
	btnCCT.portUp = A3; btnCCT.portDown = B3; btnCCT.portPressed = TAST3; btnCCT.portLED = LED3;
	btnCCT.isr_Up = &isr_CCTUp; btnCCT.isr_Down = &isr_CCTDown; btnCCT.isr_Pressed = &isr_CCTPressed;
	btnCCT.ButtonEvent = ButtonEvent; btnCCT.btn = potiCCT;
	pibuttons.push_back(&btnCCT);

	//Judd
	btnJudd.pibtnType = PIBUTTON_JUDD; btnJudd.cnt = 0; btnJudd.delta = 1; btnJudd.change = false;
	btnJudd.portUp = A3; btnJudd.portDown = B3; btnJudd.portPressed = TAST3; btnJudd.portLED = LED3;
	btnJudd.isr_Up = &isr_JuddUp; btnJudd.isr_Down = &isr_JuddDown; btnJudd.isr_Pressed = &isr_JuddPressed;
	btnJudd.ButtonEvent = ButtonEvent; btnJudd.btn = potiJudd;
	pibuttons.push_back(&btnJudd);

	//Lock1
	btnLock1.pibtnType = PIBUTTON_LOCK1; btnLock1.cnt = 0; btnLock1.delta = 1; btnLock1.change = false;
	btnLock1.portUp = -1; btnLock1.portDown = -1; btnLock1.portPressed = -1; btnLock1.portLED = -1;
	btnLock1.isr_Up = nullptr; btnLock1.isr_Down = NULL; btnLock1.isr_Pressed = &isr_Lock;
	btnLock1.ButtonEvent = ButtonEvent; btnLock2.btn = _btnLock1;
	pibuttons.push_back(&btnLock1);

	//Lock2
	btnLock2.pibtnType = PIBUTTON_LOCK1; btnLock2.cnt = 0; btnLock2.delta = 1; btnLock2.change = false;
	btnLock2.portUp = -1; btnLock2.portDown = -1; btnLock2.portPressed = -1; btnLock2.portLED = -1;
	btnLock2.isr_Up = NULL; btnLock2.isr_Down = NULL; btnLock2.isr_Pressed = &isr_Lock;
	btnLock2.ButtonEvent = ButtonEvent; btnLock2.btn = _btnLock2;
	pibuttons.push_back(&btnLock2);

	for (unsigned int i = 0; i < pibuttons.size(); i++)
	{
#if defined (RASPI)
		wiringPiISR(pibuttons[i]->portUp, INT_EDGE_FALLING, pibuttons[i]->isr_Up);
		wiringPiISR(pibuttons[i]->portDown, INT_EDGE_FALLING, pibuttons[i]->isr_Down);
		wiringPiISR(pibuttons[i]->portPressed, INT_EDGE_FALLING, pibuttons[i]->isr_Pressed);
#endif
	}
}


#if defined (RASPI)

void InitWiringPi()
{
	//Init Library
	wiringPiSetup();

	//Config Port as Input, Output
	//Brightness
	pinMode(A2, INPUT); pinMode(B2, INPUT); pinMode(TAST2, INPUT);
	//CCT
	pinMode(A3, INPUT); pinMode(B3, INPUT);	pinMode(TAST3, INPUT);
	//Judd
	pinMode(A1, INPUT); pinMode(B1, INPUT); pinMode(TAST1, INPUT); 

	//Lock1, Lock 2
	pinMode(EINGABETAST, INPUT); pinMode(EINGABETAST2, INPUT);

	//LEDs
	pinMode(LED1, OUTPUT); pinMode(LED2, OUTPUT); pinMode(LED3, OUTPUT);
}
#endif

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
			pibuttons[index]->change = true;

			if ((pibuttons[index]->ButtonEvent != 0) && (abs(pibuttons[index]->cnt) >= pibuttons[index]->delta))
			{
				//Das ist echt grauslich: Pointer to member functio aus vecotr
				Button* instance = pibuttons[index]->btn;
				(instance->*(pibuttons[index]->ButtonEvent))(PIBUTTON_BRIGHTNESS, 0);
				pibuttons[index]->delta = 0;
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
		(instance->*(pibuttons[index]->ButtonEvent))(PIBUTTON_BRIGHTNESS, 0);
	}
}

//----------------------------
//--------Brightness --------- 
//----------------------------
void isr_BrightnessUp(void)
{
	isr_General(PIBUTTON_BRIGHTNESS, +1);
}

void isr_BrightnessDown(void)
{
	isr_General(PIBUTTON_BRIGHTNESS, -1);
}

void isr_BrightnessPressed(void)
{
	isr_PressedGeneral(PIBUTTON_BRIGHTNESS);
}

//----------------------------
//--------       CCT --------- 
//----------------------------
void isr_CCTUp(void)
{
	isr_General(PIBUTTON_CCT, +1);
}

void isr_CCTDown(void)
{
	isr_General(PIBUTTON_CCT, +1);
}

void isr_CCTPressed(void)
{
	isr_PressedGeneral(PIBUTTON_CCT);
}

//----------------------------
//--------  Judd    --------- 
//----------------------------
void isr_JuddUp(void)
{
	isr_General(PIBUTTON_JUDD, +1);
}

void isr_JuddDown(void)
{
	isr_General(PIBUTTON_JUDD, +1);
}

void isr_JuddPressed(void)
{
	isr_PressedGeneral(PIBUTTON_JUDD);
}

//----------------------------
//--------  Lock Button    --------- 
//----------------------------
void isr_Lock(void)
{
	isr_PressedGeneral(PIBUTTON_LOCK1);
}

