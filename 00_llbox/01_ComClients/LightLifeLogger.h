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
	void setCCT(unsigned int);
	void setRGB(unsigned int[3]);
	void setXY(float[2]);
	void setFadeTime(unsigned int);
	void setGroup(unsigned char);
	void send();

public:
	LightLifeLogger(string);
	~LightLifeLogger();

	void setLocked();
	void logLLEvent();
};


