#pragma once

#include "baseClient.h"
#include "helpers.h"

//must correspond to PILEDServer "enum PILEDMode" in file UDPServer.cs and lines in table LLPILedMode
enum PILEDMode : char
{
	PILED_MODE_NONE = 0,
	PILED_SET_BRIGHTNESS = 1,
	PILED_SET_CCT = 2,
	PILED_SET_XY = 3,
	PILED_SET_RGB = 4,
	PILED_SET_DUV = 5,
};

enum LLMsgType
{
	LL_NONE = 0,
	LL_SET_LIGHTS = 10,
	LL_CALL_SCENE = 20,
	LL_START_TESTSEQUENCE = 30,
	LL_STOP_TESTSEQUENCE = 31,
	LL_PAUSE_TESTSEQUENCE = 32,
	LL_NEXT_TESTSEQUENCE_STEP = 33,
	LL_PREV_TESTSEQUENCE_STEP = 34,
	LL_SET_BUTTONS = 35,
	LL_RELOAD_TESTSEQUENCE = 36,
	LL_SET_LOCKED = 99,
};

struct LightLifeData
{
	int roomid;
	int userid;
	int vlid;
	int sceneid;
	int sequenceid;
	int stepid;

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

	float duv;

	LightLifeData(string pSender)
	{
		roomid = 0;
		userid = 0;
		vlid = 0;
		sceneid = 0;
		sequenceid = 0;
		stepid = 0;

		groupid = 0;
		cct = 2700;
		brightness = 255;
		duv = 0.0f;
		rgb[0] = 0; rgb[1] = 0; rgb[2] = 0;
		xy[0] = 0.0f; xy[1] = 0.0f;
		mode = PILED_SET_BRIGHTNESS;
		msgtype = LL_SET_LIGHTS;	//Controlboxes always send to Lights
		receiver = "LIGHTS";		//Controlboxes always send to Lights
		sender = pSender;			//This is the name of the Box
	}

	string ToSplitString()
	{
		ostringstream s;

		//LightLifeData
		s << "roomid=" << roomid << ";userid=" << userid << ";vlid=" << vlid << ";sceneid=" << sceneid << ";sequenceid=" << sequenceid << ";stepid=" << stepid;
		//PILEDData
		s << "groupid=" << groupid << ";mode=" << mode << ";brightness=" << brightness << ";cct=" << cct << ";duv=" << duv << "; x = " << xy[0] << "; y" << xy[1] << "; r = " << rgb[0] << "; g = " << rgb[1] << "; b = " << rgb[2];
		//Others
		s << ";sender=" << sender << ";receiver=" << receiver << ";msgtype=" << msgtype;
		return s.str();
	}

	/*string ToURLString()
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
		s << "\"brightness\":" << brightness << ",";
		s << "\"cct\":" << cct << ",";
		s << "\"xy\":" << "[" << xy[0] << "," << xy[1] << "],";
		s << "\"rgb\":" << "[" << rgb[0] << "," << rgb[1] << "," << rgb[2] << "],";
		s << "\"sender\":" << "\"" << sender << "\"" << ",";
		s << "\"receiver\":" << "\"" << receiver << "\"" << ",";
		s << "\"msgtype\":" << msgtype;
		s << "}";

		return s.str();
	}*/
};
