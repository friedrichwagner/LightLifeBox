#pragma once

#include "baseClient.h"
#include "helpers.h"
#include "LightLifeData.h"

class LightLifeLogger : public IBaseClient
{
	friend class RemoteCommands;

private:
	LightLifeData* lldata;

	void setBrightness(unsigned int);
	void setCCT(unsigned int, float[]);
	void setRGB(unsigned int[3]);
	void setXY(float[2]);
	void setFadeTime(unsigned int);
	void setGroup(unsigned char);
	void send();
	void send(LightLifeData* d);

	void setCCTDuv(unsigned int, float duv, float xy[2]);

public:
	LightLifeLogger(string);
	~LightLifeLogger();

	void setLocked();
	void resetDefault();
	void logLLEvent();
};


