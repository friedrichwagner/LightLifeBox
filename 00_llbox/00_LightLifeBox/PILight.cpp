#include "PILight.h"
#include "helpers.h"
#include "LightLifeLogger.h"
#include "Photometric.h"
#include <iomanip>
#include <vector>

using namespace std::chrono;

static milliseconds lastsend;

PILight::PILight(std::string pSection)
{
	//lastsend = duration_cast<milliseconds>(high_resolution_clock::now().time_since_epoch());
	lastsend = std::chrono::milliseconds(0);

	ini = Settings::getInstance();
	log = Logger::getInstance();

	Section = pSection;

	this->Name = ini->ReadAttrib<string>(pSection, "name", "btn");
	this->ID = ini->ReadAttrib<int>(pSection, "id", 0);

	defaultBrightness = DEFAULT_BRIGHTNESS;
	defaultCct = DEFAULT_CCT;
	duv = 0.0f;

	stringstream ss;
	ss << defaultCct << "," << defaultBrightness;
	
	string tmp = ini->Read<string>(pSection, "Default", ss.str());
	splitstring s(tmp);
	vector<string> flds = s.split(',');
	if (flds.size() >= 2)
	{
		defaultCct = atoi(flds[0].c_str());
		defaultBrightness = atoi(flds[1].c_str());
	}

	MinVal = MIN_CCT;
	MaxVal = MAX_CCT;

	brightness = defaultBrightness;
	cct = defaultCct;
	fCieCoords_t cie = CCT2xy(defaultCct);
	defaultXy[0] = cie.x; defaultXy[1] = cie.y;
	xy[0] = defaultXy[0]; xy[1] = defaultXy[1];

	duv = 0.0f;
	rgb[0] = 0; rgb[1] = 0; rgb[2] = 0;
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

		//Set default Values on Startup
		setFadeTime(DEFAULT_NEOLINK_FADETIME);

		setCCT(defaultCct);
		//setXY(defaultXy);
		lumitech::sleep(DEFAULT_NEOLINK_FADETIME + 30);
		setBrightness(defaultBrightness);
	}
}

void PILight::removeComClients()
{
	//löscht alle Clients
	int cnt = ComClients.size();
	for (int i = 0; i < cnt; i++)
		delete ComClients[i];

	ComClients.clear();
}

void PILight::removeComClient(int index)
{

	//only last element will be removed here
	//logClients.pop_back();
	ComClients.erase(ComClients.begin()+ index);
}

IBaseClient* PILight::getComClient(int index)
{
	if (ComClients.size() > (unsigned int) index)
		return ComClients[index];

	return NULL;
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

bool PILight::skip()
{
	milliseconds now = duration_cast<milliseconds>(high_resolution_clock::now().time_since_epoch());
	if ((now - lastsend).count() < (long)(DEFAULT_NEOLINK_FADETIME + 20))
		//if ((now - lastsend).count() < (long)MINIMUM_SEND_TIME)
	{
		//Silently ignore this messages
		//toString(true);
		return true;
	}

	return false;
}

void PILight::Send2ComClients()
{
	toString();

	for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
		{
			switch (piledMode)
			{
			case PILED_MODE_NONE:
				break;
			case PILED_SET_BRIGHTNESS:
				ComClients[i]->setBrightness(brightness);
				break;
			case PILED_SET_CCT:
				ComClients[i]->setCCT(cct, xy);
				break;
			case PILED_SET_XY:
				ComClients[i]->setXY(xy);
				break;
			case PILED_SET_RGB:
				ComClients[i]->setRGB(rgb);
				break;
			case PILED_SET_DUV:
				ComClients[i]->setCCTDuv(cct, duv, xy);
				break;
			default:
				break;
			}
			
		}			
	}

	lastsend = duration_cast<milliseconds>(high_resolution_clock::now().time_since_epoch());	
}


void PILight::setBrightness(unsigned int val)
{
	if (skip()) return;

	brightness = val;
	piledMode = PILED_SET_BRIGHTNESS;

	if (brightness > 255) brightness = 255;
	if (brightness < 25) brightness = 25;

	Send2ComClients();
}

void PILight::setCCT(unsigned int val)
{
	if (skip()) return;

	//cct = (unsigned int)((MaxVal - MinVal) * delta + MinVal);
	cct = val;
	piledMode = PILED_SET_CCT;
	if (cct > (unsigned int)MaxVal) cct = MaxVal;
	if (cct < (unsigned int)MinVal) cct = MinVal;

	//Umrechunung CCT (CIE1964) to x,y Koordinaten
	fCieCoords_t cie=CCT2xy(cct);
	xy[0] = cie.x; xy[1] = cie.y;

	duv = 0.0f; //Das ist Absicht. Wenn man CCT verändert, will man kein duv haben.

	Send2ComClients();
}

void PILight::setRGB(unsigned int prgb[])
{
	if (skip()) return;

	rgb[0] = prgb[0]; rgb[1] = prgb[1]; rgb[2] = prgb[2];
	piledMode = PILED_SET_RGB;
	for (int i = 0; i<3; i++)
	{
		if (rgb[i]>255) rgb[i] = 255;
		if (rgb[i]<0) rgb[i] = 0;
	}

	Send2ComClients();
}

void PILight::setXY(float pxy[])
{
	if (skip()) return;

	xy[0] = pxy[0]; xy[1] = pxy[1];
	piledMode = PILED_SET_XY;
	if (xy[0]>1.0f) xy[0] = 1.0f; if (xy[0]<0.0f) xy[0] = 0.0f;
	if (xy[1]>1.0f) xy[1] = 1.0f; if (xy[1]<0.0f) xy[1] = 0.0f;

	Send2ComClients();
}

void PILight::setCCTDuv(unsigned int valCCT, float valDuv)
{
	if (skip()) return;

	cct = valCCT; duv = valDuv;
	if (cct > (unsigned int)MaxVal) cct = MaxVal;
	if (cct < (unsigned int)MinVal) cct = MinVal;

	piledMode = PILED_SET_DUV;

	if (duv > MAX_DUV) duv = MAX_DUV;
	if (duv < MIN_DUV) duv = MIN_DUV;
	
	fCieCoords_t cie;

	if (CCTDuv2xy(cct, duv, &cie) == 0)
	{
		xy[0] = cie.x;
		xy[1] = cie.y;
		//setXY(xy);
	}
	
	Send2ComClients();
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


void PILight::setBrightnessUpDown(int delta)
{
	if (skip()) return;

	int b = (signed)brightness + delta;
	//cout << "b=" << b << "\r\n";
	if (b < 0)
		brightness = 0;
	else
		brightness = brightness + delta;

	setBrightness(brightness);

}

void PILight::setCCTUpDown(int delta)
{
	if (skip()) return;

	cct = cct + delta;

	//FW 11.6.2015 - Wenn alle 3 gleichzeitig geändert werden, dann darf duv ja nicht auf 0 gesetzt werden
	//sonst springt er auf die Plancksche Kurve
	//setCCT(cct);
	setCCTDuv(cct, duv);
}

void PILight::setRGBUpDown(int deltargb[])
{
	if (skip()) return;

	for (int i = 0; i < 3; i++)
		rgb[i] = rgb[i] + deltargb[i];

	setRGB(rgb);
}

void PILight::setXYUpDown(int delta[])
{
	if (skip()) return;

	xy[0] = xy[0] + delta[0] / 1000; // 1/1000 = +/-0.001
	xy[1] = xy[1] + delta[1] / 1000;
	setXY(xy);
}

void PILight::setDuvUpDown(int delta)
{
	if (skip()) return;

	duv = duv + (float)delta/2000.0f; // 1/2000 = +/-0.0005
	setCCTDuv(cct, duv);
}

void PILight::resetDefault()
{
	log->cout("resetDefault()-- Start");

	//setXY statt setCCT, damit bei Judd ein Fade gemacht wird
	//aber cct auf defaultCCT Wert setzen
	this->cct = defaultCct;
	//setCCT(defaultCct); 
	setXY(defaultXy); //damit gefadet wird	

	lumitech::sleep(DEFAULT_NEOLINK_FADETIME + 30);

	setBrightness(defaultBrightness);
	duv = 0.0f;

	//FW 10.6.2015: Alle Boxen schicken direkt an NeoLink Box. Daten werden nicht (mehr) über Server geschleift
	/*for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
		{
			if (ComClients[i]->getType() == CLIENT_LIGHTLIFE)
			{
				LightLifeLogger* p = static_cast<LightLifeLogger*>(ComClients[i]);
				if (p)
				{
					p->resetDefault();
					//log->cout("resetDefault() - End");
				}
			}
		}
	}*/
}

void PILight::lockCurrState()
{
	//FW 10.6.2015: Alle Boxen schicken direkt an NeoLink Box. Daten werden nicht (mehr) über Server geschleift
	/*for (unsigned int i = 0; i < ComClients.size(); i++)
	{
		if (ComClients[i] != NULL)
		{
			if (ComClients[i]->getType() == CLIENT_LIGHTLIFE)
			{
				LightLifeLogger* p = static_cast<LightLifeLogger*>(ComClients[i]);
				if (p)
				{
					p->setLocked();
					log->cout("lockCurrState()");
				}
			}				
		}			
	}*/	
}

void PILight::setGroup(unsigned char val)
{
	groupid = val;
	log->cout("Set group=" + lumitech:: itos(groupid));
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


void PILight::toString(bool skip)
{
	string mode;

	sslog.str(""); sslog.clear();
	if (skip)
		sslog << "skip::::";

	sslog << "gr:" << groupid << " b:" << brightness << " cct:" << cct << std::fixed << setprecision(4) << " x:" << xy[0] << " y:" << xy[1] << " duv:" << duv << " ft:" << fadetime;
	switch (piledMode)
	{
	case PILED_MODE_NONE:
		mode = "NONE";
		break;
	case PILED_SET_BRIGHTNESS:
		mode = "BRIGHTNESS";
		break;
	case PILED_SET_CCT:
		mode = "CCT";
		break;
	case PILED_SET_XY:
		mode = "XY";
		break;
	case PILED_SET_RGB:
		mode = "RGB";
		break;
	case PILED_SET_DUV:
		mode = "DUV";
		break;
	default:
		mode = "NONE";
		break;
	}

	sslog <<	" mode:" << mode;
	log->cout(sslog.str());
}

string PILight::getFullState()
{
	ostringstream ss;
	ss << "mode=" << piledMode << ";brightness=" << brightness << ";cct=" << cct << ";x=" << xy[0] << ";y=" << xy[1] << ";r=" << rgb[0] << ";g=" << rgb[1] << ";b=" << rgb[2] << ";duv=" << duv
		<< ";groupid=" << groupid << ";fadetime=" << fadetime << ";defaultcct=" << defaultCct << ";defaultbrightness=" << defaultBrightness;
	return ss.str();
}

void PILight::getXY(float _xy[])
{
	_xy[0] = xy[0];
	_xy[1] = xy[1];
}