#include <vector>
//#include <math.h>
#include <stdlib.h> 

#include "PIButton.h"

#if !defined (RASPI)
	//#define INT_EDGE_FALLING 1	
	//int  wiringPiISR(int pin, int mode, void(*function)(void));
#else
	#include "wiringPI.h"
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
	pinMode(A2, INPUT); pinMode(B2, INPUT); pinMode(TAST2, INPUT);
	//CCT
	pinMode(A3, INPUT); pinMode(B3, INPUT);	pinMode(TAST3, INPUT);
	//Judd
	pinMode(A1, INPUT); pinMode(B1, INPUT); pinMode(TAST1, INPUT); 

	//Lock1, Lock 2
	pinMode(EINGABETAST, INPUT); pinMode(EINGABETAST2, INPUT);

	//LEDs
	pinMode(LED1, OUTPUT); pinMode(LED2, OUTPUT); pinMode(LED3, OUTPUT);
#endif

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

			if (abs(pibuttons[index]->cnt) >= pibuttons[index]->deltaCnt)
			{
				if (pibuttons[index]->ButtonEvent != 0)
				{				
					//Das ist echt grauslich: Pointer to member function aus vector
					Button* instance = pibuttons[index]->btn;
					int val = dir * pibuttons[index]->factor;
					(instance->*(pibuttons[index]->ButtonEvent))(PIBUTTON_BRIGHTNESS, val);
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

