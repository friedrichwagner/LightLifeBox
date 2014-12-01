#pragma once

#include "baseClient.h"
#include "helpers.h"

#define ZLL_BUFFER_SIZE 30
#define FILLER_SIZE		12

enum ZLLCommand : char
{
    //ZLL
    SET_BRIGHTNESS = 1,
    SET_XY = 2,
    SET_CCT = 3,
	SET_RGB = 4,

    GET_CCT_MIN_MAX = 20,
    GET_MODEL = 21,
};

enum InterfaceIdentifier : char
{
    IF_TEST = 0,
    IF_ZLL = 1,
    IF_DALI = 2
};

struct PILEDInterfaceData
{    
    unsigned char byStart;        
    unsigned char byCmd;
    unsigned char bySubCmd;
    InterfaceIdentifier byIF;

    unsigned int brightness;
    unsigned int fadetime;

    float ciex;
    float ciey;
    unsigned int cct;

    unsigned char byR;
    unsigned char byG;
    unsigned char byB;

    //unsigned char filler[FILLER_SIZE]; //muss 8 bytes sein

    unsigned char byCRC;
    unsigned char byStop;
    //private byte isvalid;

    unsigned char byArrBuffer[ZLL_BUFFER_SIZE];

    PILEDInterfaceData()
    {       
        byStart = 0x02;            
        byCmd = 0x00;
        bySubCmd = 0x00;
        byIF = IF_ZLL;
        brightness = 0;
        fadetime = 0; // in ms, nur 100ms Schritte
        ciex = 0.0f;
        ciey = 0.0f;
        cct = unsigned int (1e6/6500);
        byR = 0;
        byG = 0;
        byB = 0;
        //memset(filler ,0, FILLER_SIZE);
        byCRC = 0;
        byStop = 0x03;      
    }

	unsigned char* ToByteArray()
	{
        std::vector<unsigned char> v;
        int i = 0;

        byArrBuffer[i++] = byStart;            
        byArrBuffer[i++] = byCmd;
        byArrBuffer[i++] = bySubCmd;
        byArrBuffer[i++] = (char)byIF;
        byArrBuffer[i++] = brightness;

		v= lumitech::intToBytes(fadetime / 100);

        byArrBuffer[i++] = v[0];
        byArrBuffer[i++] = v[1];

        v= lumitech::intToBytes((int)(65536*ciex));
        byArrBuffer[i++] = v[0];
        byArrBuffer[i++] = v[1];

        v= lumitech::intToBytes((int)(65536*ciey));
        byArrBuffer[i++] = v[0];
        byArrBuffer[i++] = v[1];

        v= lumitech::intToBytes(cct);
        byArrBuffer[i++] = v[0];
        byArrBuffer[i++] = v[1];

        byArrBuffer[i++] = byR;
        byArrBuffer[i++] = byG;
        byArrBuffer[i++] = byB;

        for (int k = 0; k < FILLER_SIZE; k++) byArrBuffer[i++] = 0;

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

class ZLLClient : public IBaseClient
{
private:
	PILEDInterfaceData* data;

	void setBrightness(unsigned int);	
	void setCCT(unsigned int);	
	void setRGB(unsigned int[3]);	
	void setXY(float[2]);	
	void setFadeTime(unsigned int);
	void send();
	
public:
	ZLLClient();
	~ZLLClient();
    //void updateData(PILEDScene* scene);          
};
