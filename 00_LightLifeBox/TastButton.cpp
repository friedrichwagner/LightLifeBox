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
}

TastButton::~TastButton() 
{ 
	done = true;
}

int TastButton::getPortVal2()
{
#ifdef _DEBUG
	return tc->getPortVal2();
#endif

	return 0;
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
  
	log->cout(this->Name + ": TastButton start listening");
	while (!done) 
	{	
		PortVal = getPortVal();
		//PortVal2 = getPortVal2();

		if (!isPressed && PortVal < 10) ButtonDown();
		else if (isPressed && PortVal < 10) ButtonPressed();
		else if (isPressed && PortVal > 10) ButtonUp();

		if  ( labs((long)(actualValue - PortVal2)) > delta)
		{
			OnChange(actualValue-PortVal2);
		}

		//lumitech::sleep(threadSleepTime);
	}
	return 0;
}

void TastButton::OnChange(long delta)
{	
	log->cout(this->Name + ": OnChange");
	//call ControlBox Callback Function
	actualValue=PortVal2;
	for (unsigned int i = 0; i < notifyClients.size(); i++)
		notifyClients[i]->notify(BUTTON_UP, PortVal2);
}


