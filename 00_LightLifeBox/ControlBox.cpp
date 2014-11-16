#include "ControlBox.h"
#include "helpers.h"

ControlBox* ControlBox::_instance = NULL;

ControlBox::ControlBox(std::string pName)
{
	isDone = false;

	this->ID = 1;
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
	stopEventLoop();
}

bool ControlBox::Init() 
{
	vector<string> flds;

	//If name not found in settings return immediately
	if (ini->ReadAttribute("ControlBox", "name","") != this->Name) return false;

	//1. Get the Potis
	ini->ReadStringVector("ControlBox", "Potis","", &flds);
	for (unsigned  int i=0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i], "id", 0) > 0)
		{
			Potis.push_back(new TastButton(flds[i]));
			Potis[i]->addClient(this);
		}
			
	}

	//2. get the Buttons
	ini->ReadStringVector("ControlBox", "Buttons","", &flds);
	for (unsigned  int i=0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i], "id", 0) > 0)
		{
			Buttons.push_back(new Button(flds[i]));
			Buttons[i]->addClient(this);
		}
			
	}

	//2. get the Lights
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
	int c = 0;
	log->cout("Controlbox::EventLoop-BEGIN");
	while (!isDone)
	{
		log->cout("------------Controlbox::EventLoop-------------");

#ifdef _DEBUG
	//c= lumitech::waitOnKeyPress();
	//if (c == 3) isDone = true;
#endif

		lumitech::sleep(1000);
	}

	stopEventLoop();
	log->cout("Controlbox::EventLoop-END");

	return true;
}

void ControlBox::stopEventLoop() 
{ 
	isDone = true;

	for (unsigned int i = 0; i< Potis.size(); i++)
		Potis[i]->done = true;

	for (unsigned int i = 0; i< Buttons.size(); i++)
		Buttons[i]->done = true;
}

void ControlBox::Beep(int freq, int time) 
{ 
	log->cout("beep");
}

void ControlBox::notify(enumButtonEvents event, long val)
{
	log->cout("notify");
}
