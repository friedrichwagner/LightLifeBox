#include "DMXClient.h"
#include "helpers.h"
#include "serialib.h"
#include <cstring> //for memset under gcc

#pragma region Contructor/Destructor

DMXClient::DMXClient()
{	
	clientType = CLIENT_DMX;
	iDMXBroadCastStartAddress= 501;
	InitBuffers();

	Name= ini->ReadAttribute("DMX", "name","DMXClient");
	IPPort= ini->ReadInt("DMX", "IP-Port",12345);	
	//IPPort= ini->Read<int>("DMX", "IP-Port",12345);	
	useFTDI= ini->ReadBool("DMX", "UseFTDI", true);	

	//Getting DMX Start Addresses from INI File (can be more than on separated by ",")
	vector<string> flds;
	ini->ReadStringVector("DMX", "StartAddress","1", &flds);		
	for (unsigned int i=0; i <flds.size(); i++)
		StartAddresses.push_back(lumitech::stoi(flds[i]));					

	//Getting DMX IP Addresses from INI File (can be more than on separated by ",")
	//flds.clear();
	ini->ReadStringVector("DMX", "IP-Address","", &flds);
	for (unsigned  int i=0; i< flds.size(); i++)
		IPClients.push_back(new IPSocket(flds[i], IPPort));
	
	//Getting USB Addresses from INI File (can be more than on separated by ",")
	if (useFTDI)
	{
		ini->ReadStringVector("DMX", "USBName","", &flds);
#ifdef USE_FTDI
		 if (ftdi::FTDIDevices.size() >0)
			for (unsigned  int i=0; i< flds.size(); i++)
				FTDIClients.push_back(new ftdi(flds[i]));
#endif

	}
	else
	{
		//es kann nur einen geben
		ini->ReadStringVector("DMX", "USBCom","", &flds);
		if (flds.size() >=2 ) USBClients.push_back(new serialib(flds[0], lumitech::stoi(flds[1]), true));
	}
}

DMXClient::~DMXClient()
{
}

void DMXClient::InitBuffers()
{
	memset(buffer, 0, sizeof(buffer));
	memset(artNetData, 0, sizeof(artNetData));

#ifdef WIN32
	strcpy_s(artNetName, 8, "Art-Net");
	memcpy_s(&artNetData[0], sizeof(artNetData), artNetName,sizeof(artNetName));
#endif

#ifdef CYGWIN
	strcpy(artNetName, "Art-Net");
	memcpy(&artNetData[0], artNetName,sizeof(artNetName));
#endif

	// OpCode
    artNetData[8] = 0x00;
    artNetData[9] = 0x50;

    // ProtVerH
    artNetData[10] = 0x00;
    //ProtVer
    artNetData[11] = 0x0E;

    // Sequence
    artNetData[12] = 0x00;

    // Physical
    artNetData[13] = 0x00;

    // Universe
    artNetData[14] = 0x00; // <- Universe Setting
    artNetData[15] = 0x00;

    // LengthHi
    artNetData[16] = 0x02; // Length High Byte 512 Byte always
    // Length
    artNetData[17] = 0x00; // Length Low Byte


}
#pragma endregion

#pragma region Member Function

void DMXClient::setDMXValue(int offset, int value)
{
	for (unsigned int i=0; i < StartAddresses.size(); i++)
	{
		//Channel between 1 and 512
		int channel = StartAddresses[i]+offset;

		if ( channel >= 1 && channel <= 512)
		{
			//buffer[channel] = value;
			buffer[channel] = value;
			artNetData[ARTNET_HEADER_SIZE + channel-1] = value;    
		}
	 }
}

void DMXClient::setBrightness(unsigned int val)
{
	setDMXValue(1, val);

	send();
}

void DMXClient::setCCT(unsigned int val)
{
	int dmxcct = (int)(((float)val - MIN_CCT) / (MAX_CCT - MIN_CCT) * 255.0f);
	setDMXValue(0, DMXVAL_CCT_MODE);
	setDMXValue(2, dmxcct);

	send();
}

void DMXClient::setRGB(unsigned int val[])
{
	setDMXValue(0, DMXVAL_RGB_MODE);
	setDMXValue(1, val[0]);
	setDMXValue(2, val[1]);
	setDMXValue(3, val[2]);

	send();
}

void DMXClient::setXY(float val[2])
{
   int x = (int)((val[0] - 0.17) / 0.00188);
   int y = (int)((val[1] - 0.08) / 0.00153);

	setDMXValue(0, DMXVAL_XY_MODE);
	setDMXValue(2, x);
	setDMXValue(3, y);

	send();
}

void DMXClient::send()
{
	SendUDP(&artNetData[0], sizeof(artNetData));
	SendUSB(&buffer[0], sizeof(buffer));
}

void DMXClient::setFadeTime(unsigned int val)
{
	//Nothing to do in DMX with Fading
}


/*
void DMXClient::updateData(PILEDScene* scene)
{
	setFadeTime(scene->fadetime);	

	if (scene->pmode == PILED_XY)
	{
		setXY(scene->xy);		
		setBrightness(scene->brightness);
	}

	if (scene->pmode == PILED_RGB)
	{
		setRGB(scene->rgb);
	}

	if (scene->pmode == PILED_CCT)
	{
		setCCT(scene->cct);		
		setBrightness(scene->brightness);
	}

	//Maybe run this in a thread to not block the other observers ????
	SendUDP(&artNetData[0], sizeof(artNetData));
	SendUSB(&buffer[0], sizeof(buffer));
}
*/

#pragma endregion
