#include "ZLLClient.h"
#include "helpers.h"
#include "serialib.h"
#include <cstring> //for memset under gcc

#pragma region Contructor/Destructor

ZLLClient::ZLLClient()
{
	clientType = CLIENT_ZLL;
	Name= ini->ReadAttribute("ZLL", "name", "ZLLClient");
	
	vector<string> flds;

	//es kann nur einen geben
	//flds.clear();
	ini->ReadStringVector("ZLL", "USBCom","", &flds);
	if (flds.size() >=2 ) 
		USBClients.push_back(new serialib(flds[0], lumitech::stoi(flds[1]), false ));

	data = new PILEDInterfaceData();
}

ZLLClient::~ZLLClient()
{
	delete data;
}

#pragma region Member Function


void ZLLClient::setBrightness(unsigned int val)
{
	data->byCmd = SET_BRIGHTNESS;
	data->brightness = val;

	send();
}

void ZLLClient::setCCT(unsigned int val)
{
	data->byCmd = SET_CCT;
	data->cct = (unsigned int)(1e6/val); //in Mired

	send();
}

void ZLLClient::setRGB(unsigned int val[3])
{
	data->byCmd = SET_RGB;
	data->byR = val[0];
	data->byG = val[1];
	data->byB = val[2];

	send();
}

void ZLLClient::setXY(float val[2])
{
	data->byCmd = SET_XY;

	data->ciex = val[0];
	data->ciey = val[1];

   send();
}

void ZLLClient::setFadeTime(unsigned int val)
{
	data->fadetime = val;

	send();
}

void ZLLClient::setGroup(unsigned char val)
{
	//Nothing to do here
	//ZLlCleint connects directly to one PI-LED LMU, so no group is required
}


void ZLLClient::send()
{
	unsigned char *pBuf = data->ToByteArray();
	SendUSB(pBuf, ZLL_BUFFER_SIZE);
}


/*
void ZLLClient::updateData(PILEDScene* scene)
{
	setFadeTime(scene->fadetime);	

	if (scene->pmode == PILED_BRIGHTNESS)
	{
		setBrightness(scene->brightness);
	}

	if (scene->pmode == PILED_XY)
	{
		setXY(scene->xy);		
		//setBrightness(scene->brightness);
	}

	if (scene->pmode == PILED_RGB)
	{
		setRGB(scene->rgb);
	}

	if (scene->pmode == PILED_CCT)
	{
		setCCT(scene->cct);		
		//setBrightness(scene->brightness);
	}

	unsigned char *pBuf = data.ToByteArray();
	SendUSB(pBuf, ZLL_BUFFER_SIZE);

	//Send Brightness always in those 2 modes
	if (scene->pmode == PILED_XY || scene->pmode == PILED_CCT)
	{
		if (scene->brightness != data.brightness)
		{
			setBrightness(scene->brightness);
			pBuf =  data.ToByteArray();
			SendUSB(pBuf, ZLL_BUFFER_SIZE);
		}

	}
}
*/

#pragma endregion
