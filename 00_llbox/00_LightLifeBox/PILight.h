#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include "baseClient.h"
#include <sstream>


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
	unsigned int groupid;

	std::vector<IBaseClient*> ComClients;
	enumPILEDMode piledMode;
	ostringstream sslog;

	unsigned int defaultBrightness;
	unsigned int defaultCct;

	Settings* ini;
	Logger* log;

	//Functions
	void setLog();
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
	void setGroup(unsigned char);
	unsigned char getGroup();

	void resetDefault();
	void lockCurrState();

	void addComClient(IBaseClient*);
	void removeComClient(IBaseClient*);
	void removeComClients();
	void updateClients();
	IBaseClient* getComClient(int index);

	string getFullState();
};
