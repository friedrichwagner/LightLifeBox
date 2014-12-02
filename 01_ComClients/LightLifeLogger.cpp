#include "LightLifeLogger.h"
#include "helpers.h"

#pragma region Contructor/Destructor

LightLifeLogger::LightLifeLogger()
{
	clientType = CLIENT_ZLL;
	Name= ini->ReadAttribute("LIGHTLIFESERVER", "name", "LightLife1");
	
	vector<string> flds;

	//es kann nur einen geben
	//flds.clear();
	ini->ReadStringVector("LIGHTLIFESERVER", "IP-Address","", &flds);
	IPPort = ini->Read<int>("LIGHTLIFESERVER", "IP-Port", 1025);

	//Kann es nur einen geben ?
	if (flds.size() >=1 ) 
		IPClients.push_back(new IPSocket(flds[0], IPPort));

	lldata = new LightLifeData();
}

LightLifeLogger::~LightLifeLogger()
{
	delete lldata;
}

#pragma region Member Function


void LightLifeLogger::setBrightness(unsigned int val)
{	
	lldata->mode = LL_SET_BRIGHTNESS;
	lldata->brightness= val;
	send();
}

void LightLifeLogger::setCCT(unsigned int cct)
{
	lldata->mode = LL_SET_CCT;
	lldata->cct = cct;
	send();
}

void LightLifeLogger::setRGB(unsigned int rgb[3])
{
	lldata->mode = LL_SET_RGB;
	lldata->rgb[0] = rgb[0];
	lldata->rgb[1] = rgb[1];
	lldata->rgb[2] = rgb[2];
	send();
}

void LightLifeLogger::setXY(float xy[2])
{
	lldata->mode = LL_SET_XY;
	lldata->xy[0] = xy[0];
	lldata->xy[1] = xy[1];
	
	send();
}

void LightLifeLogger::setFadeTime(unsigned int millisec)
{
	//Nothing to do
}

void LightLifeLogger::setGroup(unsigned char group)
{
	lldata->groupid = group;
}

void LightLifeLogger::send()
{
	string s = lldata->ToJSonString();
	SendUDP((unsigned char*) s.c_str(), s.length());
}

#pragma endregion
