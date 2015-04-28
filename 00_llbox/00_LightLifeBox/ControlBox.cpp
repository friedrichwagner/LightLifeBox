#include "ControlBox.h"
#include "helpers.h"
#include "NeoLinkClient.h"
#include "LightLifeLogger.h"
#include "Timer.h"
#include "DeltaTest.h"

ControlBox* ControlBox::_instance = NULL;

void delay1(ControlBox* p);
void delay2(ControlBox* p);

int psychoTestDelayTimeinSecs = 30000;

ControlBox::ControlBox(std::string pName)
{
	isDone = false;
	DeltaTestInProgress = false;

	this->ID = 0;
	this->Name = pName;
	this->buttonActive = -1;
	
	ini = Settings::getInstance();
	log = Logger::getInstance();

	isDone = !this->Init();

	//Add ComClients
	if (!isDone)
	{
		addComClients();
	}

	//TEST TEST
	//StartDeltaTest(123, 100, 4000, DTEST_CCT_UP);
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

	unsigned int idx = Buttons.size();
	
	//log->cout("Size=" + lumitech::itos(idx));
	for (unsigned int i = 0; i < idx; i++)		
			Buttons[i]->setActive(false);

	int cnt = Lights.size();
	for (int i = 0; i < cnt; i++)
		Lights[i]->removeComClients();

	delete rmCmd;
}

bool ControlBox::Init() 
{
	vector<string> flds;

	//If name not found in settings return immediately
	string s = ini->ReadAttribute(this->Name, "name", "");
	if (s.length() == 0) return false;

	this->ID = ini->Read<int>(this->Name, "BoxNr", 0);

	bool defaultActiveControls = ini->Read<bool>(this->Name, "ControlsDefaultActive", false);
	testWithoutConsole = ini->Read<bool>("ControlBox_General", "TestWithoutConsole", false);
	psychoTestDelayTimeinSecs = ini->Read<int>("ControlBox_General", "DelayTimeForPsychoTestSecs", 30) * 1000;

	//1. Get the Potis	
	ini->ReadStringVector("ControlBox_General", "Potis", "", &flds);
	for (unsigned int i = 0; i< flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i], "id", 0) > 0)
		{
			Buttons.push_back(new Button(flds[i]));
			Buttons[i]->addClient(this);
			if (testWithoutConsole)
			{
				buttonActive = 0;
				//Nur ersten Button auf Active setzen, weitere dann bei jedem "Lock"
				if (i == 0) Buttons[i]->setActive(true);
				else Buttons[i]->setActive(false);
			}
			else
				Buttons[i]->setActive(defaultActiveControls);
		}
	}

	//2. get the Buttons
	int idx = Buttons.size();
	ini->ReadStringVector("ControlBox_General", "Buttons", "", &flds);
	for (unsigned int i = 0; i < flds.size(); i++)
	{
		if (ini->ReadAttrib<int>(flds[i], "id", 0) > 0)
		{
			Buttons.push_back(new Button(flds[i]));
			Buttons[idx+i]->addClient(this);

			if (testWithoutConsole)
				Buttons[idx + i]->setActive(true);
			else
				Buttons[idx+i]->setActive(defaultActiveControls);
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

	return true;
}

int ControlBox::addComClients()
{
	//ACHTUNG! Im Settings File muss "LightLifeServer" immer als erste Client angegeben werden
	//damit in der Funktion rmCmd->NowAddLLLogger im dieser Client "gezogen" wird
	//Hack, aber ist halt jetzt so

	int defaultgroupid = ini->Read<int>(this->Name, "DefaultGroup", 0); // Default = broadcast, wird mit RemoteCommand "Discover" gesetzt  ini->Read<int>(pSection, "groupid", 0);

	vector<string> flds;
	ini->ReadStringVector("ControlBox_General", "Clients", "", &flds);
	for (unsigned int i = 0; i< flds.size(); i++)
	{
		IBaseClient* clnt = NULL;
		if (flds[i] == "NeoLink")
		{
			clnt = new NeoLinkClient();			
		}

		if (flds[i] == "LightLifeServer")
		{
			clnt = new LightLifeLogger(Name);
		}

		for (unsigned int k = 0; k < Lights.size(); k++)
		{
			if ((clnt != NULL) && (clnt->getCntClients() > 0))
			{				
				Lights[k]->setGroup(defaultgroupid);
				Lights[k]->addComClient(clnt);
			}
				
		}
	}

	return flds.size();
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
	if (DeltaTestInProgress)
	{
		StopDeltaTest();
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

void ControlBox::setButtons(bool b[])
{
	for (unsigned int i = 0; i < Buttons.size(); i++)
		Buttons[i]->setActive(b[i]);
}

void ControlBox::notify(void* sender, enumButtonEvents event, int delta)
{
	LightLifeButtonType btntype = ((Button*)sender)->getBtnType();

	if (Lights.size() == 0) return;

	switch (event)
	{
	case BUTTON_PRESSED:
		if (btntype == LOCK)
		{
			if (DeltaTestInProgress)
			{
				log->cout("Stopping Delta Test.....");
				deltaTest->stop();
				rmCmd->SendRemoteCommand(LL_SET_LOCKED_DELTATEST, "");
				StopDeltaTest();
			}
			else
			{
				Lights[0]->lockCurrState();
				rmCmd->SendRemoteCommand(LL_SET_LOCKED, "");

				if (testWithoutConsole)
				{
					//Disable all Buttons
					bool b[5] = { false, false, false, false, false };
					setButtons(b);

					//Wait 30 secs
					log->cout("Waiting 30 secs...");
					Later Delay1(psychoTestDelayTimeinSecs, true, &delay1, this);
				}
			}
		}

		break;

	case BUTTON_CHANGE:
		//ss1 << btnName << "(change): val=" << val; log->cout1(ss1.str());
		if (btntype == BRIGHTNESS)	Lights[0]->setBrightnessUpDown(delta);
		if (btntype == CCT)			Lights[0]->setCCTUpDown(delta);
		if (btntype == JUDD)		Lights[0]->setDuvUpDown(delta);
		break;

	default:
		break;
	}
}


//Nach 30 Sekunden statischer Lichtszene --> Fade auf Ausgangszustand (fadetime=30sek)
void delay1(ControlBox* p)
{	
	if (p != NULL)
	{
		//30 seconds fade
		p->Lights[0]->setFadeTime(psychoTestDelayTimeinSecs);
		p->Lights[0]->resetDefault();
		cout << "Fading 30 secs..\r\n";
		Later Delay2(psychoTestDelayTimeinSecs+2, true, &delay2, p);
	}
}

void delay2(ControlBox* p)
{
	bool b[5] = { false, false, false, true, true };
	if (p != NULL)
	{
		p->buttonActive++;
		//0=Brightess, 1=CCT, 2=JUDD, 3=ALLE Buttons klein Box, 4=ALLE Buttons grosser Raum
		if (p->buttonActive > 4)  p->buttonActive = 0;

		if (p->buttonActive >= 3)
		{
			b[0] = true; b[1] = true; b[2] = true;
			p->setButtons(b);
		}
		else
		{
			b[p->buttonActive] = true;
			p->setButtons(b);
		}
		p->Lights[0]->setFadeTime(DEFAULT_NEOLINK_FADETIME);
	}
}

void ControlBox::StartDeltaTest(int userid, int b0, int cct0, TestMode mode)
{
	deltaTest = new DeltaTest();

	log->cout("Start Delta Test _______________");

	//1. Enable only LOCK Buttons
	bool b[5] = { false, false, false, true, true };
	setButtons(b);

	//2. Run Test
	DeltaTestInProgress = true;
	deltaTest->start(Lights[0], userid, b0, cct0, mode);
}

void ControlBox::StopDeltaTest()
{
	if (deltaTest != NULL)
	{
		bool b[5] = { false, false, false, true, true };

		DeltaTestInProgress = false;

		delete deltaTest;
		deltaTest = NULL;

		if (testWithoutConsole)
		{
			b[0] = true; b[3] = true; b[4] = true;
		}
		else
		{
			bool defaultActiveControls = ini->Read<bool>(this->Name, "ControlsDefaultActive", false);
			b[0] = defaultActiveControls; b[1] = defaultActiveControls; b[2] = defaultActiveControls;  b[3] = defaultActiveControls; b[4] = defaultActiveControls;
		}

		setButtons(b);

		log->cout("Finished Delta Test!_______________");
	}
}