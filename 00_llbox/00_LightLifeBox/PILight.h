#pragma once

#include <string>
#include "Settings.h"
#include "Logger.h"
#include "baseClient.h"
#include <sstream>
#include "LightLifeData.h"


//enum enumPILEDMode { CCT_MODE=1, XY_MODE=2, RGB_MODE=3};
#define MIN_CCT 2500
#define MAX_CCT 7000

#define MIN_DUV -0.02f
#define MAX_DUV 0.02f

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
	float duv;

	std::vector<IBaseClient*> ComClients;
	PILEDMode piledMode;
	ostringstream sslog;

	unsigned int defaultBrightness;
	unsigned int defaultCct;
	int MinVal;
	int MaxVal;
	//float defaultXy[2];

	Settings* ini;
	Logger* log;

	//Functions
	void setLog();

	void Send2ComClients();
public:
	PILight(std::string pName); //private contructor
	~PILight();	

	//Absolute
	void setBrightness(unsigned int);	
	void setCCT(unsigned int);	
	void setRGB(unsigned int[]);
	void setXY(float[]);	
	void setCCTDuv(unsigned int, float);

	void setFadeTime(unsigned int val);

	//UpDown
	void setBrightnessUpDown(int);	
	void setCCTUpDown(int);	
	void setRGBUpDown(int[]);	
	void setXYUpDown(int[]);	
	void setDuvUpDown(int delta);
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

	void getXY(float[]);
};
