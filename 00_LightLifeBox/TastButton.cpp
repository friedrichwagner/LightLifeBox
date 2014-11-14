#include "TastButton.h"
#include "helpers.h"
#include <math.h>

using namespace std;

TastButton::TastButton(string pName)
	:Button(pName)
{
	delta = 10;
	actualValue=0;
	PortVal2=0;

	spawn();
}

TastButton::~TastButton() 
{ 
	done = true;
}

unsigned long TastButton::getValue()
{
	return actualValue;
}

void TastButton::setValue(unsigned long val)
{
	actualValue = val;
}


unsigned long TastButton::startListen()
{
  
	cout << this->Name <<": start listening..." << endl;
	while (!done) 
	{			
		if (!isPressed && PortVal < 100) ButtonDown();
		else if (isPressed && PortVal < 100) ButtonPressed();
		else if (isPressed && PortVal > 1000) ButtonUp();

		if  ( labs((long)(actualValue - PortVal2)) > delta)
		{
			OnChange(actualValue-PortVal2);
		}

		lumitech::sleep(100);
	}
	return 0;
}

void TastButton::OnChange(long delta)
{	
	//call ControlBox Callback Function
	actualValue=PortVal2;
}


