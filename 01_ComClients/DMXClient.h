#pragma once

#include "baseClient.h"

#define DMX_BUFFER_SIZE 512 
#define ARTNET_HEADER_SIZE  18
#define MIN_CCT	2700.0
#define MAX_CCT	6500.0

#define DMXVAL_CCT_MODE	1
#define DMXVAL_RGB_MODE	101
#define DMXVAL_XY_MODE	201

class DMXClient : public IBaseClient
{
private:
	int iDMXBroadCastStartAddress;
	char artNetName[8];
	std::vector<int> StartAddresses;
    unsigned char buffer[DMX_BUFFER_SIZE +1]; ///DMX Start Code byte[0]=0 muss hier mitgeschickt werden, d.h. eigentlich werden 513 bytes geschickt
    unsigned char artNetData[ARTNET_HEADER_SIZE + DMX_BUFFER_SIZE];
	void InitBuffers();

	void setDMXValue(int addr, int val);

	void setBrightness(unsigned int);	
	void setCCT(unsigned int);	
	void setRGB(unsigned int[3]);	
	void setXY(float[2]);	
	void setFadeTime(unsigned int);
	void send();
public:
	DMXClient();
	~DMXClient();
    //void updateData(PILEDScene* scene);          
};
