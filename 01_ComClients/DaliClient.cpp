#include "DaliClient.h"
#include "helpers.h"
#include "serialib.h"

#pragma region Contructor/Destructor

DaliClient::DaliClient()
{	
	clientType = CLIENT_DALI;
	Name= ini->ReadAttribute("DALI", "name","DaliClient");
	
	vector<string> flds;
	//es kann nur einen geben
	//flds.clear();
	ini->ReadStringVector("DALI", "USBCom","", &flds);

	//if (flds.size() >=2 ) USBClients.push_back(new serialib(flds[0], lumitech::stoi(flds[1])));
}

DaliClient::~DaliClient()
{
}

#pragma region Member Function


void DaliClient::setBrightness(unsigned int val)
{

}

void DaliClient::setCCT(unsigned int val)
{


}

void DaliClient::setRGB(unsigned int val[3])
{

}

void DaliClient::setXY(float val[2])
{

}

void DaliClient::setFadeTime(unsigned int val)
{

}


void DaliClient::updateData(PILEDScene* scene)
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

	//SendUSB("12", 2);
}

#pragma endregion
