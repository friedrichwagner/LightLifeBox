#include "LightLifeLogger.h"
#include "helpers.h"

#pragma region Contructor/Destructor

LightLifeLogger::LightLifeLogger(string boxname)
{
	clientType = CLIENT_LIGHTLIFE;
	Name= ini->ReadAttribute("LightLifeServer", "name", "LightLife1");
	
	vector<string> flds;

	//es kann nur einen geben
	//flds.clear();
	ini->ReadStringVector("LightLifeServer", "UDP-Address","", &flds);
	IPPort = ini->Read<int>("LightLifeServer", "UDP-Port", 1025);

	//Kann es nur einen geben ?
	if (flds.size() >=1 ) 
		IPClients.push_back(new UDPSendSocket(flds[0], IPPort));

	lldata = new LightLifeData(boxname);
}

LightLifeLogger::~LightLifeLogger()
{
	delete lldata;
}

#pragma region Member Function


void LightLifeLogger::setBrightness(unsigned int val)
{	
	lldata->mode = PILED_SET_BRIGHTNESS;
	lldata->brightness= val;
	lldata->msgtype = LL_SET_LIGHTS;
	send();
}

void LightLifeLogger::setCCT(unsigned int cct, float xy[])
{
	lldata->mode = PILED_SET_CCT;
	lldata->cct = cct;
	lldata->duv = 0.0f;
	lldata->xy[0] = xy[0]; //bereits umgerechnete CCT to xy-Koordinaten
	lldata->xy[1] = xy[1];
	lldata->msgtype = LL_SET_LIGHTS;
	send();
}

void LightLifeLogger::setCCTDuv(unsigned int cct, float duv, float xy[2])
{
	lldata->mode = PILED_SET_DUV;
	lldata->cct = cct;
	lldata->duv = duv;
	lldata->xy[0] = xy[0];
	lldata->xy[1] = xy[1];
	lldata->msgtype = LL_SET_LIGHTS;
	send();
}


void LightLifeLogger::setRGB(unsigned int rgb[3])
{
	lldata->mode = PILED_SET_RGB;
	lldata->rgb[0] = rgb[0];
	lldata->rgb[1] = rgb[1];
	lldata->rgb[2] = rgb[2];
	lldata->msgtype = LL_SET_LIGHTS;
	send();
}

void LightLifeLogger::setXY(float xy[2])
{
	lldata->mode = PILED_SET_XY;
	lldata->xy[0] = xy[0];
	lldata->xy[1] = xy[1];
	lldata->msgtype = LL_SET_LIGHTS;
	send();
}

void LightLifeLogger::setFadeTime(unsigned int millisec)
{
	//Nothing to do
	lldata->fadetime = millisec;
}

void LightLifeLogger::setGroup(unsigned char group)
{
	lldata->groupid = group;
}

void LightLifeLogger::setLocked()
{
	lldata->msgtype = LL_SET_LOCKED;
	send();
}

void LightLifeLogger::resetDefault()
{
	lldata->msgtype = LL_SET_DEFAULT;
	send();
}

void LightLifeLogger::send()
{
	string s = lldata->ToSplitString();
	SendUDP((unsigned char*) s.c_str(), s.length());
}

void LightLifeLogger::send(LightLifeData* d)
{
	string s = d->ToSplitString();
	SendUDP((unsigned char*)s.c_str(), s.length());
}


void LightLifeLogger::logLLEvent()
{
	//Copy constructor
	LightLifeData* d = new LightLifeData(lldata);

	//PILED Daten "leer" setzen
	d->brightness = 0;
	d->cct = 0;	d->duv = 0.0f;
	d->xy[0] = 0;	d->xy[1] = 0;
	d->receiver = "CONSOLE";

	send(d);

}

#pragma endregion
