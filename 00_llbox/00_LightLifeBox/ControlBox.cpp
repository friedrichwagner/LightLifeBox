#include "ControlBox.h"
#include "helpers.h"
//#include "PIButton.h"

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

	delete rmCmd;
}

bool ControlBox::Init() 
{
	vector<string> flds;

	//If name not found in settings return immediately
	string s = ini->ReadAttribute(this->Name, "name", "");
	if (s.length() == 0) return false;

	this->ID = ini->Read<int>(this->Name, "BoxNr", 0);

	//1. get the Buttons
	ini->ReadStringVector("ControlBox_General", "Buttons", "", &flds);
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
	ini->ReadStringVector("ControlBox_General", "Potis","", &flds);
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
	ini->ReadStringVector("ControlBox_General", "PILights","", &flds);
	for (unsigned  int i=0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i],"id",0) > 0)
			Lights.push_back(new PILight(flds[i]));
	}

	threadSleepTime = ini->Read<int>("ControlBox_General", "Sleep", 100);

	rmCmd = new RemoteCommands(this);
	rmCmd->start();

	if (Buttons.size() >= 2 && Potis.size() >= 3)
		InitWiringPi(Buttons[0]->ButtonEvent, Buttons[1]->ButtonEvent, Potis[0]->ButtonEvent, Potis[1]->ButtonEvent, Potis[2]->ButtonEvent);
	else
		log->cout("Cannot Init Wiring Pi ");

	return true;
}

bool ControlBox::EventLoop() 
{ 
	//int c = 0;
	log->cout("------------- Controlbox::EventLoop-BEGIN ------------- ");
	rmCmd->NowAddLLLogger(); //ziemlich unsauber
	while (!isDone)
	{
		//log->cout("------------Controlbox::EventLoop-------------");

#ifdef _DEBUG
	//char c= lumitech::waitOnKeyPress();
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
	LightLifeButtonType btntype = ((Button*)sender)->getBtnType();

	if (Lights.size() == 0) return;

	switch (event)
	{
	case BUTTON_DOWN:
		break;

	case BUTTON_UP:
		if (btntype == LOCK) 
		{
			Lights[0]->lockCurrState();
			
			rmCmd->SendRemoteCommand(REMOTE_SENDCMD_LOCK, "");

			Lights[0]->resetDefault();

		}
		else {
			Lights[0]->resetDefault();
		}

		break;

	case BUTTON_PRESSED:
		break;

	case BUTTON_CHANGE:
		//ss1 << btnName << "(change): val=" << val; log->cout1(ss1.str());
		if (btntype == BRIGHTNESS)	Lights[0]->setBrightness(val);
		if (btntype == CCT)			Lights[0]->setCCT(val);
		if (btntype == JUDD)		Lights[0]->setXY(new float[2] {0.333f, 0.333f});
		break;

	default:
		break;
	}
}
