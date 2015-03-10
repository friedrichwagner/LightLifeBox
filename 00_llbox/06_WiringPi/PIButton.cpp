#include "PIButton.h"
#include "wiringPI.h"
#include <math.h>

//Forward Declarations

//Brightnss Button
volatile bool _changeBrightness;
int _cntBrightness;
int _deltaBrightness;
void isr_BrightnessUp(void);
void isr_BrightnessDown(void);
void isr_BrightnessPressed(void);
WiringTastButtonPiEvent _BrightnessEvent;

volatile bool _changeCCT;
int _cntCCT;
int _deltaCCT;
void isr_CCTUp(void);
void isr_CCTDown(void);
void isr_CCTPressed(void);
WiringTastButtonPiEvent _CCTEvent;

volatile bool _changeJudd;
int _cntJudd;
int _deltaJudd;
void isr_JuddUp(void);
void isr_JuddDown(void);
void isr_JuddPressed(void);
WiringTastButtonPiEvent _JuddEvent;

WiringPiButtonEvent _LockButton1Event;
WiringPiButtonEvent _LockButton2Event;


void InitWiringPi(WiringPiButtonEvent LockButton1Event, WiringPiButtonEvent LockButton2Event, WiringTastButtonPiEvent BrightnessEvent, WiringTastButtonPiEvent CCTEvent, WiringTastButtonPiEvent JuddEvent)
{
	wiringPiSetup();

	pinMode(A1, INPUT);
	pinMode(B1, INPUT);
	pinMode(TAST1, INPUT);

	pinMode(A2, INPUT);
	pinMode(B2, INPUT);
	pinMode(TAST2, INPUT);

	pinMode(A3, INPUT);
	pinMode(B3, INPUT);
	pinMode(TAST3, INPUT);

	pinMode(EINGABETAST, INPUT);
	pinMode(EINGABETAST2, INPUT);

	pinMode(LED1, OUTPUT);
	pinMode(LED2, OUTPUT);
	pinMode(LED3, OUTPUT);

	//ISRs Brightness
	wiringPiISR(A2, INT_EDGE_FALLING, &isr_BrightnessUp);
	wiringPiISR(B2, INT_EDGE_FALLING, &isr_BrightnessDown);
	wiringPiISR(TAST2, INT_EDGE_FALLING, &isr_BrightnessPressed);
	_BrightnessEvent = BrightnessEvent;

	// CCT
	wiringPiISR(A3, INT_EDGE_FALLING, &isr_CCTUp);
	wiringPiISR(B3, INT_EDGE_FALLING, &isr_CCTDown);
	wiringPiISR(TAST3, INT_EDGE_FALLING, &isr_CCTPressed);
	_CCTEvent = CCTEvent;

	//pinMode(SUMMER, OUTPUT);
	// int softToneCreate(int pin);

	// ISRs Plankdistance
	wiringPiISR(A1, INT_EDGE_FALLING, &isr_JuddUp);
	wiringPiISR(B1, INT_EDGE_FALLING, &isr_JuddDown);
	wiringPiISR(TAST1, INT_EDGE_FALLING, &isr_JuddPressed);
	_JuddEvent = JuddEvent;

	//ISRs EINGABESTASTER
	//wiringPiISR(EINGABETAST, INT_EDGE_FALLING, &isr_EINGABETAST);
	_LockButton1Event = LockButton1Event;
	_LockButton2Event = LockButton2Event;
}

//----------------------------
//--------Brightness --------- 
//----------------------------
void BrightnessISR(int dir)
{
	if (!_changeBrightness)
	{
		if ((dir > 0) && _cntBrightness < 0) _cntBrightness = 0;
		else if ((dir < 0) && _cntBrightness > 0) _cntBrightness = 0;

		_cntBrightness = _cntBrightness + dir; //dir = -1 oder +1
		_changeBrightness = true;

		if ((_BrightnessEvent != nullptr) && (abs(_cntBrightness) >= _deltaBrightness))
		{
			TastButton* instance;
			(instance->*_BrightnessEvent)(PIBUTTON_BRIGHTNESS, dir);
			_deltaBrightness = 0;
		}

	}
	else
		_changeBrightness = false;
}

void isr_BrightnessUp(void)
{
	BrightnessISR(+1);
}

void isr_BrightnessDown(void)
{
	BrightnessISR(-1);
}

void isr_BrightnessPressed(void)
{
	if (_BrightnessEvent != NULL)
	{
		TastButton* instance;
		(instance->*_BrightnessEvent)(PIBUTTON_BRIGHTNESS, 0);
	}
}

//----------------------------
//--------       CCT --------- 
//----------------------------
void CCTISR(int dir)
{
	if (!_changeCCT)
	{
		if ((dir > 0) && _cntCCT < 0) _cntCCT = 0;
		else if ((dir < 0) && _cntCCT > 0) _cntCCT = 0;

		_cntCCT = _cntCCT + dir; //dir = -1 oder +1
		_changeCCT = true;

		if ((_CCTEvent != nullptr) && (abs(_cntCCT) >= _deltaCCT))
		{
			TastButton* instance;
			(instance->*_CCTEvent)(PIBUTTON_CCT, dir);
			_deltaCCT = 0;
		}

	}
	else
		_changeBrightness = false;
}

void isr_CCTUp(void)
{
	CCTISR(+1);
}

void isr_CCTDown(void)
{
	CCTISR(-1);
}

void isr_CCTPressed(void)
{
	if (_CCTEvent != NULL)
	{
		TastButton* instance;
		(instance->*_CCTEvent)(PIBUTTON_BRIGHTNESS, 0);
	}
}
/*
PIButton::PIButton(PIButtonTyp t, int portA, int portB, int portPress, int delta, WiringPiEvent cb)
{
	_type = t;
	_portA = portA;
	_portB = portB;
	_portPress = portPress;
	_delta = delta;
	_cb = cb;
	_change = false;
	_cnt = 0;

	pinMode(_portA, INPUT);
	pinMode(_portB, INPUT);
	pinMode(_portPress, INPUT);

	wiringPiISR(_portA, INT_EDGE_FALLING, &_isr_A);
	wiringPiISR(_portB, INT_EDGE_FALLING, &_isr_B);
	wiringPiISR(_portPress, INT_EDGE_FALLING, &_isr_press);
}

void PIButton::_isr_A(void)
{
	if (!_change)
	{
		if (_cnt < 0) _cnt = 0;
		_cnt++;
		_change = true;

		if ((_cb != nullptr) && (_cnt >= _delta))
		{
			WiringPiEvent(_type, 1, false);
			_delta = 0;
		}
			
	}
	else
		_change = false;
}


void PIButton::_isr_B(void)
{
	if (!_change)
	{
		if (_cnt > 0) _cnt = 0;
		_cnt--;
		_change = true;

		if ((_cb != nullptr) && (abs(_cnt) >= _delta))
		{
			WiringPiEvent(_type, 1, false);
			_delta = 0;
		}
	}
	else
		_change = false;
}
*/