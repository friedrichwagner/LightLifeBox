#include "PILight.h"
#include "helpers.h"
#include "LightLifeLogger.h"
#include <vector>



PILight::PILight(std::string pSection)
{
	ini = Settings::getInstance();
	log = Logger::getInstance();

	Section=pSection;

	this->Name = ini->ReadAttrib<string>(pSection,"name","btn");
	this->ID = ini->ReadAttrib<int>(pSection,"id",0);
	this->groupid = ini->Read<int>(pSection, "groupid", 0);

	defaultBrightness = 255;
	defaultCct = 2700;

	string tmp = ini->Read<string>(pSection, "Default", "3000,255");
	splitstring s(tmp);
	vector<string> flds = s.split(',');
	if (flds.size() >= 2)
	{
		defaultCct = atoi(flds[0].c_str());
		defaultBrightness = atoi(flds[1].c_str());		
	}

	brightness = 255;
	cct = 2700;
	xy[0] = 0; xy[1] = 0;
	rgb[0] = 0; rgb[1] = 0; rgb[2] = 0;
	fadetime = 0;
}

PILight::~PILight() 
{ 

}

void PILight::addComClient(IBaseClient* c)
{		
	if (c != NULL)
	{
		//zu steuernde Gruppe setzen
		c->setGroup(groupid);
		ComClients.push_back(c);
		log->cout("added Client:" + c->getName());
	}
}

void PILight::removeComClients()
{
	//löscht alle Clients
	ComClients.clear();
}

void PILight::removeComClient(IBaseClient* c)
{

	//only last element will be removed here
	//logClients.pop_back();

	for(vector<IBaseClient*>::const_iterator iter = ComClients.begin(); iter != ComClients.end(); ++iter)
    {
		if (*iter == c)
		{
			log->cout("remove Client:" + c->getName());
#ifdef WIN32
			//FW 21.11.2014 - funktionier nicht in g++4.7 ??
			ComClients.erase(iter);
#endif
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

void PILight::setBrightness(unsigned int val)
{
	if (val <= 255) 
		brightness = val;

	setLog();

	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
			ComClients[i]->setBrightness(brightness);
	}
}

void PILight::setCCT(unsigned int val)
{
	cct = (unsigned int) ((6500.0f - 2700.0f) / 255.0f * val + 2700);
	setLog();	

	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
			ComClients[i]->setCCT(cct);
	}
}

void PILight::setRGB(unsigned int prgb[])
{
	rgb[0] = prgb[0]; rgb[1] = prgb[1]; rgb[2] = prgb[2];
	setLog();

	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
			ComClients[i]->setRGB(rgb);
	}
}

void PILight::setXY(float pxy[])
{
	xy[0] = pxy[0]; xy[1] = pxy[1];
	setLog();

	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
			ComClients[i]->setXY(xy);
	}
}

void PILight::setFadeTime(unsigned int val)
{
	fadetime = val;
	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
			ComClients[i]->setFadeTime(val);
	}
}


void PILight::setBrightnessUpDown(int diff)
{
	if ((brightness + diff)>0 && (brightness + diff) < 255)
	{
		brightness = brightness + diff;
		setBrightness(brightness);
	}
}

void PILight::setCCTUpDown(int diff)
{
	if ((cct + diff) >= 2700 && (cct + diff) < 6500)
	{
		cct = cct + diff;
		setCCT(cct);
	}
}

void PILight::setRGBUpDown(int deltargb[])
{
	for (int i = 0; i<3; i++)
	{
		if ((rgb[i] + deltargb[i])>0 && (rgb[i] + deltargb[i]) < 255) rgb[i] = rgb[i] + deltargb[i];
	}
		

	setRGB(rgb);
}

void PILight::setXYUpDown(float [])
{
}


void PILight::resetDefault()
{
	setCCT(255); //hier darf nicht CCT angegeben werden, sondern die POTI Stellung
	setBrightness(defaultBrightness);	
}

void PILight::lockCurrState()
{
	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
		{
			if (ComClients[i]->getType() == CLIENT_LIGHTLIFE)
			{
				LightLifeLogger* p = static_cast<LightLifeLogger*>(ComClients[i]);
				if (p)
					p->setLocked();
			}				
		}			
	}	
}

void PILight::setGroup(unsigned char val)
{
	groupid = val;

	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
			ComClients[i]->setGroup(groupid);
	}
}

unsigned char PILight::getGroup()
{
	return this->groupid;
}



void PILight::setLog()
{
	sslog.str(""); sslog.clear();
	sslog << "b:" << brightness << " cct:" << cct << " x:" << xy[0] << " y:" << xy[1] << " r:" << rgb[0] << " g:" << rgb[1] << " b:" << rgb[2];
	log->cout(sslog.str());
}
