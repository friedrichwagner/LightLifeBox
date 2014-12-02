#pragma once

#include "baseClient.h"
#include "helpers.h"


enum LLMode : char
{
	LL_SET_BRIGHTNESS = 1,
	LL_SET_XY = 2,
	LL_SET_CCT = 3,
	LL_SET_RGB = 4,
};

struct LightLifeData
{
	int groupid;
	int cct;
	int brightness;
	int	rgb[3];
	float xy[2];
	LLMode mode;

	LightLifeData()
	{
		groupid = 0;
		cct = 2700;
		brightness = 255;
		rgb[0] = 0; rgb[1] = 0; rgb[2] = 0;
		xy[0] = 0.0f; xy[1] = 0.0f;
		mode = LL_SET_BRIGHTNESS;
	}

	string ToURLString()
	{
		ostringstream s;

		s << "groupid=" << groupid << "&mode=" << mode << "&brightness=" << brightness << "&cct=" << cct << "&xy=" << xy[0] << ";" << xy[1] << "&rgb=" << rgb[0] << ";" << rgb[1] << ";" << rgb[2];
		return s.str();
	}

	string ToJSonString()
	{
		ostringstream s;
		s << "{" << endl;
		s << "\"groupid\":" << groupid << ","<< endl;
		s << "\"mode\":" << mode << "," << endl;
		s << "\"brightness\":" << brightness << "," << endl;
		s << "\"cct\":" << cct << "," << endl;
		s << "\"xy\":" << "[" << xy[0] << "," << xy[1] << "]" << endl;
		s << "\"rgb\":" << "[" << rgb[0] << "," << rgb[1] << "," << rgb[2] << "]" << endl;
		s << "}";
		
		return s.str();
	}

};

class LightLifeLogger : public IBaseClient
{
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
	LightLifeLogger();
	~LightLifeLogger();
};
