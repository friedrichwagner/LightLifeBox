#include "NeoLinkClient.h"
#include "helpers.h"
#include "Photometric.h"
#include <cstring> //for memset under gcc

#pragma region Contructor/Destructor

NeoLinkClient::NeoLinkClient()
{
	clientType = CLIENT_NEOLINK;
	Name= ini->ReadAttribute("NeoLink", "name", "NeoLinkClient");
	
	vector<string> flds;

	//es kann nur einen geben
	//flds.clear();
	ini->ReadStringVector("NeoLink", "UDP-Address","", &flds);
	IPPort = ini->Read<int>("NeoLink", "UDP-Port", 1025);

	//Kann es nur einen geben ?
	if (flds.size() >=1 ) 
		IPClients.push_back(new UDPSendSocket(flds[0], IPPort));

	nlframe = new NeoLinkData();
	nlframe->byAddress = NL_GROUP_BROADCAST;
}

NeoLinkClient::~NeoLinkClient()
{
	delete nlframe;
}

#pragma region Member Function


void NeoLinkClient::setBrightness(unsigned int val)
{	
	nlframe->byMode = NL_BRIGHTNESS;
	
	//brightness
	nlframe->data[0] = (char)val;

	//fadetime
	nlframe->data[1] = byfadetime[0];
	nlframe->data[2] = byfadetime[1];

	send();
}

void NeoLinkClient::setCCT(unsigned int cct, float xy[])
{
	std::vector<unsigned char> v;
	nlframe->byMode = NL_CCT;

	//cct in mirek
	unsigned int mirek = (unsigned int)(1e6 / cct);

	v = lumitech::intToBytes(mirek);
	nlframe->data[0] = v[0];
	nlframe->data[1] = v[1];

	//fadetime
	nlframe->data[2] = byfadetime[0];
	nlframe->data[3] = byfadetime[1];

	send();
}

void NeoLinkClient::setRGB(unsigned int rgb[3])
{
	nlframe->byMode = NL_RGB;

	nlframe->data[0] = rgb[0];
	nlframe->data[1] = rgb[1];
	nlframe->data[2] = rgb[2];

	//fadetime
	nlframe->data[3] = byfadetime[0];
	nlframe->data[4] = byfadetime[1];


	send();
}

void NeoLinkClient::setXY(float val[2])
{
	std::vector<unsigned char> v;
	nlframe->byMode = NL_XY;

	v = lumitech::intToBytes((int)(65536 * val[0]));
	nlframe->data[0] = v[0];
	nlframe->data[1] = v[1];

	v = lumitech::intToBytes((int)(65536 * val[1]));
	nlframe->data[2] = v[0];
	nlframe->data[3] = v[1];

   send();
}

void NeoLinkClient::setCCTDuv(unsigned int cct, float duv, float xy[2])
{
	fCieCoords_t cie;
	/*float xy[2];

	if (CCTDuv2xy(cct, duv, &cie) == 0)
	{
		xy[0] = cie.x; xy[1] = cie.y;
		setXY(xy);
	}*/

	setXY(xy);
}

void NeoLinkClient::setFadeTime(unsigned int millisec)
{
	std::vector<unsigned char> v;

	fadetime = millisec;

	v = lumitech::intToBytes(fadetime / 100);
	byfadetime[0] = v[0];
	byfadetime[1] = v[1];
}

void NeoLinkClient::setGroup(unsigned char group)
{
	nlframe->byAddress = group;
}

void NeoLinkClient::send()
{
	unsigned char *pBuf = nlframe->ToByteArray();
	SendUDP(pBuf, NL_BUFFER_SIZE);
}

#pragma endregion
