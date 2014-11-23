#include "PILight.h"
#include "helpers.h"
#include "BaseClient.h"
#include <vector>


PILight::PILight(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	Section=pSection;

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	//this->PortNr = ini->Read<int>(pSection,"PortNr",0);
}

PILight::~PILight() 
{ 

}

void PILight::addComClient(IBaseClient* c)
{
	log->info("added Client:" + c->getName());
	ComClients.push_back(c);
}

void PILight::removeComClient(IBaseClient* c)
{
	for(vector<IBaseClient*>::const_iterator iter = ComClients.begin(); iter != ComClients.end(); ++iter)
    {
		if (*iter == c)
		{
			log->info("remove Client:" + c->getName());
			//FW 21.11.2014 - funktionier nicht in g++4.7 ??
			//ComClients.erase(iter);
			return;
		}
	}
}

void PILight::updateClients()
{
	for(vector<IBaseClient*>::const_iterator iter = ComClients.begin(); iter != ComClients.end(); ++iter)
    {
        if(*iter != 0)
        {
            //(*iter)->updateData(&actualScene);
        }
    }
}

void PILight::setBrightness(unsigned int)
{
}

void PILight::setCCT(unsigned int)
{
}

void PILight::setRGB(unsigned int[])
{
}

void PILight::setXY(float[])
{
}

void PILight::setFadeTime(unsigned int val)
{
}


void PILight::setBrightnessUpDown(int)
{
}

void PILight::setCCTUpDown(int)
{
}

void PILight::setRGBUpDown(int[])
{
}

void PILight::setXYUpDown(float[])
{
}


void PILight::resetDefault()
{
}

void PILight::lockCurrState()
{
}