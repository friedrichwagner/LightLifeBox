#pragma once

#include "baseClient.h"
#include "helpers.h"

#define NL_BUFFER_SIZE 30
#define DATA_SIZE		24

enum NeoLinkGroups : unsigned char
{
	NL_GROUP_BROADCAST = 0,
	NL_GROUP_1 = 201,
	NL_GROUP_2 = 202,
	NL_GROUP_3 = 203,
	NL_GROUP_4 = 204,
	NL_GROUP_5 = 205,
	NL_GROUP_6 = 206,
	NL_GROUP_7 = 207,
	NL_GROUP_8 = 208,
	NL_GROUP_9 = 209,
	NL_GROUP_10 = 210,
	NL_GROUP_11 = 211,
	NL_GROUP_12 = 212,
	NL_GROUP_13 = 213,
	NL_GROUP_14 = 214,
	NL_GROUP_15 = 215,
	NL_GROUP_16 = 216
};

enum NeoLinkMode : char
{
	//ZLL
	NL_STARTUP = 1,
	NL_NETWORK_STATUS = 2,
	NL_NETWORK = 3,
	NL_IDENTIFY = 4,
	NL_GROUPCONFIG = 5,
	NL_GROUP_REFRESH_ADDRESS = 6,
	NL_GROUP_REFRESH_NAME = 7,
	NL_BRIGHTNESS = 10,
	NL_CCT = 11,
	NL_RGB = 12,
	NL_SCENES = 13,
	NL_SEQUENCES_CALL = 14,
	NL_SEQUENCES_SET = 15,
	NL_XY = 16			//tbd
};

enum NeoLinkSubMode_NETWORK : char
{
	//ZLL
	NL_NETWORK_TOUCHLINK = 0,
	NL_NETWORK_RESETNEW = 1,
	NL_NETWORK_CLASSICAL = 2,
	NL_NETWORK_RELEASELAMP = 3
};

enum NeoLinkSubMode_SCENES : char
{
	//ZLL
	NL_SCENES_ADD = 0,
	NL_SCENES_DELETE = 1,
	NL_SCENES_DELETE_ALL = 2,
	NL_SCENES_CALL = 3
};

struct NeoLinkData
{
	unsigned char byStart;
	unsigned char byMode;
	unsigned char byAddress;

	unsigned char data[DATA_SIZE];

	unsigned char byGroupUpdate;
	unsigned char byCRC;
	unsigned char byStop;
	//private byte isvalid;

	unsigned char byArrBuffer[NL_BUFFER_SIZE];

	NeoLinkData()
	{
		byStart = 0x02;
		byMode = 0x00;
		byAddress = 0x00;

		byGroupUpdate = 0x00;
		byCRC = 0x00;
		byStop = 0x03;
	}

	unsigned char* ToByteArray()
	{
		//std::vector<unsigned char> v;
		int i = 0;

		byArrBuffer[i++] = byStart;
		byArrBuffer[i++] = byMode;
		byArrBuffer[i++] = byAddress;

		for (int k = 0; k < DATA_SIZE; k++) byArrBuffer[i++] = 0;

		byArrBuffer[i++] = byGroupUpdate;
		byArrBuffer[i++] = calcCRC();
		byArrBuffer[i++] = byStop;

		return &byArrBuffer[0];
	}


	unsigned char calcCRC()
	{
		int num = 24; // CRC Berechnung von Byte 4(=Brightness) bis 27 (=vor CRC Byte)
		int j, k, crc8, m, x;

		crc8 = 0;
		//Achtung: m startet mit 4 (Brightness), nicht 0
		for (m = 4; m < num; m++)
		{
			x = byArrBuffer[m];
			for (k = 0; k < 8; k++)
			{
				j = 1 & (x ^ crc8);
				crc8 = (crc8 / 2) & 0xFF;
				x = (x / 2) & 0xFF;
				if (j != 0)
					crc8 = crc8 ^ 0x8C;
			}
		}
		byCRC = crc8;

		return byCRC;
	}
};

class NeoLinkClient : public IBaseClient
{
private:
	NeoLinkData* nlframe;

	char groupAddress;
	unsigned int fadetime;
	char byfadetime[2];

	void setBrightness(unsigned int);
	void setCCT(unsigned int);
	void setRGB(unsigned int[3]);
	void setXY(float[2]);
	void setFadeTime(unsigned int);
	void setGroup(unsigned char);
	void send();

public:
	NeoLinkClient();
	~NeoLinkClient();

	
	void setFadetime(unsigned int);
};
