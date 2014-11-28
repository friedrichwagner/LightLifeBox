#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include "baseClient.h"


enum enumPILEDMode { CCT_MODE=1, XY_MODE=2, RGB_MODE=3};

using namespace std;

class PILight
{
protected:

	//Fields
	unsigned int ID;
	string Name;
	string Section;
	string IP;
	unsigned int brightness;
	unsigned int cct;
	float xy[2];
	unsigned int rgb[3];
	unsigned int fadetime;
	std::vector<IBaseClient*> ComClients;
	enumPILEDMode piledMode;

	unsigned int defaultBrightness;
	unsigned int defaultCct;

	Settings* ini;
	Logger* log;

	//Functions
public:
	PILight(std::string pName); //private contructor
	~PILight();	

	void setBrightness(unsigned int);	
	void setCCT(unsigned int);	
	void setRGB(unsigned int[]);
	void setXY(float[]);	
	void setFadeTime(unsigned int val);

	void setBrightnessUpDown(int);	
	void setCCTUpDown(int);	
	void setRGBUpDown(int[]);	
	void setXYUpDown(float[]);	

	void resetDefault();
	void lockCurrState();

	void addComClient(IBaseClient*);
	void removeComClient(IBaseClient*);
	void removeComClients();
	void updateClients();
};
