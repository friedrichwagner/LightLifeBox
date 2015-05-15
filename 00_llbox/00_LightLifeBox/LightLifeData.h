#pragma once

#include "baseClient.h"
#include "helpers.h"

#define DEFAULT_NEOLINK_FADETIME 300
#define MINIMUM_SEND_TIME 100
#define DEFAULT_CCT 4000
#define DEFAULT_BRIGHTNESS 50

enum ActivationState
{
	none = 0,
	activating = 1,
	relaxing = 2
};

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
	//Box --> PILEDServer
	LL_NONE = 0,
	LL_SET_LIGHTS = 10,
	LL_CALL_SCENE = 20,

	/*LL_START_TESTSEQUENCE = 30,
	LL_STOP_TESTSEQUENCE = 31,
	LL_PAUSE_TESTSEQUENCE = 32,
	LL_NEXT_TESTSEQUENCE_STEP = 33,
	LL_PREV_TESTSEQUENCE_STEP = 34,
	LL_RELOAD_TESTSEQUENCE = 35,*/

	//AdminConsole -->Box
	LL_DISCOVER = 50,
	LL_ENABLE_BUTTONS = 51,
	LL_SET_PILED = 52,
	LL_GET_PILED=53,
	LL_SET_SEQUENCEDATA = 54,
	LL_START_DELTATEST = 55,
	LL_STOP_DELTATEST = 56,

	//Box ->AdminConsole
	LL_SET_LOCKED = 100,
	LL_SET_DEFAULT = 101,	
	LL_SET_LOCKED_DELTATEST = 102,
	LL_AFTER_WAIT_TIME = 103,
	LL_AFTER_FADE_TIME = 104
};

struct LightLifeData
{
	int roomid;
	int userid;
	int vlid;
	int cycleid;
	int sceneid;
	int sequenceid;
	int stepid;
	int posid;

	int groupid;
	int cct;
	int brightness;	
	int	rgb[3];
	float xy[2];
	int fadetime;		//FW 9.4.2015
	PILEDMode mode;
	bool locked;
	string sender;
	string receiver;
	LLMsgType msgtype;

	float duv;
	ActivationState activationState;

	LightLifeData(string pSender)
	{
		roomid = 0;		userid = 0;			vlid = 0;		posid = 0;
		cycleid = 0;	sceneid = 0;	sequenceid = 0;		stepid = 0;		activationState = none;

		groupid = 0;	brightness = 255; 
		cct = 2700;  	duv = 0.0f; 	fadetime = 0;		
		xy[0] = 0.0f;	xy[1] = 0.0f;
		rgb[0] = 0;		rgb[1] = 0;		rgb[2] = 0;

		mode = PILED_SET_BRIGHTNESS;	msgtype = LL_SET_LIGHTS;
		sender = pSender;				receiver = "LIGHTS";		
	}

	LightLifeData(LightLifeData* old)
	{
		roomid = old->roomid;		userid = old->userid;			vlid = old->vlid;	posid = old->posid;
		cycleid = old->cycleid;		 sceneid = old->sceneid;		sequenceid = old->sequenceid;	stepid = old->stepid;		activationState = old->activationState;

		groupid = old->groupid;	brightness = old->brightness;
		cct = old->cct;  	duv = old->duv; 	fadetime = old->fadetime;
		xy[0] = old->xy[0];	xy[1] = old->xy[1];
		rgb[0] = old->rgb[0];		rgb[1] = old->rgb[1];		rgb[2] = old->rgb[2];

		mode = old->mode;		msgtype = old->msgtype;
		sender = old->sender;	receiver = old->receiver;
	}

	string ToSplitString()
	{
		ostringstream s;

		//LightLifeData
		s << "roomid=" << roomid << ";userid=" << userid << ";vlid=" << vlid << ";sceneid=" << sceneid << ";sequenceid=" << sequenceid << ";stepid=" << stepid << ";posid=" << posid << ";cycleid=" << cycleid;
		//PILEDData
		s << ";groupid=" << groupid << ";mode=" << mode << ";brightness=" << brightness << ";cct=" << cct << ";duv=" << duv << ";x=" << xy[0] << ";y=" << xy[1] << ";r=" << rgb[0] << ";g=" << rgb[1] << ";b=" << rgb[2] << ";fadetime=" << fadetime;
		//Others
		s << ";sender=" << sender << ";receiver=" << receiver << ";msgtype=" << msgtype << ";activationstate=" << activationState;
		return s.str();
	}
};
