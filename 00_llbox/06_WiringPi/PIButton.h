#pragma once

#include "Button.h"
#include "TastButton.h"

#define A1 27       // Define wiringPIPin27/Physical36  as A1
#define B1 29       // Define wiringPIPin29/Physical40  as B1
#define TAST1 28    // Define wiringPIPin28/Physical38  as TAST1

#define A2 21      // Define wiringPIPin21/Physical29 as A2
#define B2 25      // Define wiringPIPin25/Physical37 as B2
#define TAST2 22   // Define wiringPIPin22/Physical31 as TAST2

#define A3 12      // Define wiringPIPin12Physical19 as A3
#define B3 14      // Define wiringPIPin14/Physical23 as B3
#define TAST3 13   // Define wiringPIPin13/Physical21 as TAST3

#define EINGABETAST 1 // Define wiringPIPin1/Physical12 as EINGABETAST
#define EINGABETAST2 4 // Define wiringPIPin4/Physical26 as EINGABETAST2

#define SUMMER 26     // Define wiringPiPin26/Physical32 as Summer-Piezo

#define LED1 6 // Define wiringPiPin6/Physical22 as LED1
#define LED2 23 // Define wiringPiPin23/Physical33 as LED1
#define LED3 24 // Define wiringPiPin24/Physical35 as LED1

enum PIButtonTyp { PIBUTTON_BRIGHTNESS = 0, PIBUTTON_CCT = 1, PIBUTTON_JUDD = 2, PIBUTTON_LOCK1=3, PIBUTTON_LOCK2=4 };
enum PIButtonRotateDirection { DOWN = -1, UP = +1, PRESS = 99 };

class Button;
class TastButton;

typedef void (Button::*WiringPiButtonEvent)(PIButtonTyp t, int delta);
typedef void (TastButton::*WiringTastButtonPiEvent)(PIButtonTyp t, int delta);

void InitWiringPi(WiringPiButtonEvent LockButton1Event, WiringPiButtonEvent LockButton2Event, WiringTastButtonPiEvent BrightnessEvent, WiringTastButtonPiEvent CCTEvent, WiringTastButtonPiEvent JuddEvent);


/*class PIButton
{
private:
	PIButtonTyp _type;
	int _portA;
	int _portB;
	int _portPress;

	volatile bool _change;
	int _cnt;
	int _delta;

	void _isr_A(void);
	void _isr_B(void);
	void _isr_press(void);

	WiringPiEvent _cb;

public:
	//static void InitWiringPi();
	PIButton(int portA, int portB, int portPress, int delta, (Button::*WiringPiEvent)* btn);
	
};
*/