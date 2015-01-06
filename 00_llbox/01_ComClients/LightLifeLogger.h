#pragma once

#include "baseClient.h"
#include "helpers.h"


//must correspond to PILEDServer "enum PILEDMode" in file UDPServer.cs and lines in table LLPILedMode
enum PILEDMode : char
{
	PILED_SET_BRIGHTNESS = 1,
	PILED_SET_CCT = 2,
	PILED_SET_XY = 3,
	PILED_SET_RGB = 4,
	PILED_SET_LOCKED = 99,
};

enum LLMsgType
{
	LL_SET_LIGHTS = 10,
};

struct LightLifeData
{
	int groupid;
	int cct;
	int brightness;
	int	rgb[3];
	float xy[2];
	PILEDMode mode;
	bool locked;
	string sender;
	string receiver;
	LLMsgType msgtype;

	int roomid;
	int userid;
	int vlid;
	int sceneid;
	int sequenceid;
	int stepid;

	LightLifeData(string pSender)
	{
		groupid = 0;
		cct = 2700;
		brightness = 255;
		rgb[0] = 0; rgb[1] = 0; rgb[2] = 0;
		xy[0] = 0.0f; xy[1] = 0.0f;
		mode = PILED_SET_BRIGHTNESS;
		msgtype = LL_SET_LIGHTS;	//Controlboxes always send to Lights
		receiver = "LIGHTS";		//Controlboxes always send to Lights
		sender = pSender;			//This is the name of the Box
	}

	string ToURLString()
	{
		ostringstream s;

		s << "groupid=" << groupid << "&mode=" << mode << "&brightness=" << brightness << "&cct=" << cct << "&xy=" << xy[0] << ";" << xy[1] << "&rgb=" << rgb[0] << ";" << rgb[1] << ";" << rgb[2];
		s << "&sender=" << sender << "&receiver=" << receiver << "&msgtype=" << msgtype;
		return s.str();
	}

	string ToJSonString()
	{
		//"{\"brigthness\":0,\"cct\":0,\"groupid\":0,\"locked\":false,\"mode\":0,\"rgb\":[0,0,0],\"xy\":[0,0]}"
		//"{\"groupid\":0,\"mode\":3,\"brightness\":255,\"cct\":6500,\"xy\":[0,0]\"rgb\":[0,0,0]\"locked\":false}"

		ostringstream s;
		s << "{";
		s << "\"groupid\":" << groupid << ",";
		s << "\"mode\":" << mode << ",";
		s << "\"brightness\":" << brightness << "," ;
		s << "\"cct\":" << cct << ",";
		s << "\"xy\":" << "[" << xy[0] << "," << xy[1] << "]," ;
		s << "\"rgb\":" << "[" << rgb[0] << "," << rgb[1] << "," << rgb[2] << "],";
		s << "\"sender\":" << "\"" << sender << "\"" << ",";
		s << "\"receiver\":" << "\"" << receiver << "\"" << ",";
		s << "\"msgtype\":" << msgtype;
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
	LightLifeLogger(string);
	~LightLifeLogger();

	void setLocked();
};
