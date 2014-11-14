#pragma once

#include <string>
#include "IXMLParser.h"

enum TPILEDMode { PILED_NONE, PILED_BRIGHTNESS, PILED_CCT, PILED_XY, PILED_RGB };

struct PILEDScene
{
	static const std::string xNodeName;
	enum TPILEDMode pmode;
	unsigned int nr;
	unsigned int brightness;
	unsigned int cct;
	unsigned int rgb[3];
	float xy[2];
	unsigned long fadetime;
	unsigned long waittime;
	bool waitOnKeypress;

	bool getData(ITCXMLNode, int);
	bool getAttribute(ITCXMLNode xMainNode, std::string, std::string*);
	std::string printData();

	PILEDScene()
	{
		//Set Standard values;
		pmode=PILED_NONE;
		nr=0;
		brightness=255;
		cct=3000;
		rgb[0]=0;rgb[1]=0;rgb[2]=0;
		xy[0]=0.333f;xy[1]=0.333f;
		fadetime=100;
		waittime=3000;
		waitOnKeypress=false;
	}
};
