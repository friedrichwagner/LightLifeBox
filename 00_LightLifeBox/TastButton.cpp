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
	if (!done) stop();
}

int TastButton::getPortVal2()
{
#ifdef _DEBUG
	if (tc->connected())
	{
		return tc->getPortVal2();
	}
#endif

	return -100;
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

		if (!isPressed && PortVal == -1) ButtonDown();
		//else if (isPressed && PortVal < 10) ButtonPressed();
		else if (isPressed && PortVal == 1001) ButtonUp();

		//if  ( (unsigned long)labs((long)(actualValue - PortVal2)) > delta)
		if ((PortVal >=0) && (PortVal <= 1000))
		{
			OnChange(PortVal);
		}

		//lumitech::sleep(threadSleepTime);
	}
	return 0;
}

void TastButton::OnChange(long delta)
{	
	//log->cout(this->Name + ": OnChange");
	actualValue=delta;
	try
	{

		for (unsigned int i = 0; i < notifyClients.size(); i++)
		{
			if (notifyClients[i] != NULL)
				notifyClients[i]->notify(this, BUTTON_CHANGE, PortVal);
		}
	}
	catch (exception& ex)
	{
		log->cout(ex.what());
	}
}


