#include "ControlBox.h"
#include "helpers.h"

ControlBox* ControlBox::_instance = NULL;

ControlBox::ControlBox(std::string pName)
{
	isDone = false;

	this->ID = 0;
	this->Name = pName;
	
	ini = Settings::getInstance();
	log = Logger::getInstance();

	isDone = !this->Init();

	//setup TastButtons, Buttons from ini File
}

ControlBox* ControlBox::getInstance(string pName)
{

    if(_instance==NULL)
    {
        _instance = new ControlBox(pName);
        return _instance;
    }
    else
    {
        return _instance;
	}
}

ControlBox* ControlBox::getInstance()
{
	return _instance;
}


ControlBox::~ControlBox() 
{ 
	if (!isDone) stopEventLoop();
}

bool ControlBox::Init() 
{
	vector<string> flds;

	//If name not found in settings return immediately
	string s = ini->ReadAttribute("ControlBox", "name", "");
	if (ini->ReadAttribute("ControlBox", "name","") != this->Name) return false;

	this->ID = ini->Read<int>("ControlBox", "BoxNr", 0);

	//1. get the Buttons
	ini->ReadStringVector("ControlBox", "Buttons", "", &flds);
	for (unsigned int i = 0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i], "id", 0) > 0)
		{
			Buttons.push_back(new Button(flds[i]));
			Buttons[i]->addClient(this);
			Buttons[i]->start();
		}
	}

	//2. Get the Potis
	ini->ReadStringVector("ControlBox", "Potis","", &flds);
	for (unsigned  int i=0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i], "id", 0) > 0)
		{
			Potis.push_back(new TastButton(flds[i]));
			Potis[i]->addClient(this);
			Potis[i]->start();
		}			
	}

	//3. get the Lights
	ini->ReadStringVector("ControlBox", "PILights","", &flds);
	for (unsigned  int i=0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i],"id",0) > 0)
			Lights.push_back(new PILight(flds[i]));
	}

	threadSleepTime = ini->Read<int>("ControlBox", "Sleep", 100);
	return true;
}

bool ControlBox::EventLoop() 
{ 
	//int c = 0;
	log->cout("------------- Controlbox::EventLoop-BEGIN ------------- ");
	while (!isDone)
	{
		//log->cout("------------Controlbox::EventLoop-------------");

#ifdef _DEBUG
	//c= lumitech::waitOnKeyPress();
	//if (c == 3) isDone = true;
#endif

		lumitech::sleep(threadSleepTime);
	}

	//stopEventLoop(); this is done from Ctr-C Handler
	log->cout("-------------  Controlbox::EventLoop-END ------------- ");

	return true;
}

void ControlBox::stopEventLoop() 
{ 
	isDone = true;
	unsigned int i = 0;
	unsigned int cnt = Potis.size();

	for (i = 0; i< cnt; i++)
	{
		if (Potis[i] != NULL)
			Potis[i]->stop();
	}
		
	cnt = Buttons.size();
	for (i = 0; i< cnt; i++)
	{
		if (Buttons[i] != NULL)
			Buttons[i]->stop();
	}
	
}

void ControlBox::Beep(int freq, int time) 
{ 
	log->cout("beep");
}

string ControlBox::getName()
{
	return this->Name;
}

void ControlBox::notify(void* sender, enumButtonEvents event, long val)
{
	string result;
	string btnName = ((Button*)sender)->getName();

	if (Lights.size() == 0) return;

	switch (event)
	{
	case BUTTON_DOWN:
		break;

	case BUTTON_UP:
		if (btnName == "btnLock") {
			Lights[0]->lockCurrState();
		}
		else {
			Lights[0]->resetDefault();
		}

		break;

	case BUTTON_PRESSED:
		//ss1 << btnName << "(pressed): val=" << val; 	log->cout(ss1.str());
		break;

	case BUTTON_CHANGE:
		//ss1 << btnName << "(change): val=" << val; log->cout1(ss1.str());

		if (btnName == "btnLock") Lights[0]->resetDefault();
		if (btnName == "btnBrightness") Lights[0]->setBrightness(val);
		if (btnName == "btnCCT") Lights[0]->setCCT(val);
		//if (btnName == "btnJudd") Lights[0]->setXY(2700);
		break;
		break;

	default:
		break;
	}
}
