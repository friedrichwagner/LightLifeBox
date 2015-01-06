#pragma once

#include "baseClient.h"
#include "helpers.h"

class DaliClient : public IBaseClient
{
private:
	void setBrightness(unsigned int);	
	void setCCT(unsigned int);	
	void setRGB(unsigned int[3]);	
	void setXY(float[2]);	
	void setFadeTime(unsigned int);
	void setGroup(unsigned char);
public:
	DaliClient();
	~DaliClient();
    void updateData(PILEDScene* scene);          
};
