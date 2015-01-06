
#include "PILEDScene.h"
#include "helpers.h"
#include <sstream>


//Static constants
const std::string PILEDScene::xNodeName="PILEDScene";

#pragma region struct Scene

bool PILEDScene::getData(ITCXMLNode xSequenceNode, int iSceneNr)
{
	bool ret=false;	
	string tmp;

	try
	{
		//Selects the next item in the xml, not necessarily the next sccene with nr= iSceneNr
		ITCXMLNode xMainNode=xSequenceNode.getChildNode(PILEDScene::xNodeName.c_str(), iSceneNr);
		pmode = PILED_NONE;

		if  (!xMainNode.isEmpty())
		{
			//1. Nr must be there because we loook for it in the xml file
			if (getAttribute(xMainNode, "nr", &tmp))					
				nr=lumitech::stoi(tmp);
						
			//2. brightness
			if (getAttribute(xMainNode, "brightness", &tmp))	
			{
				brightness = lumitech::stoi(tmp) % 256;	
				pmode = PILED_BRIGHTNESS;
			}
			//3. CCT
			if (getAttribute(xMainNode, "cct", &tmp))
			{
				cct = lumitech::stoi(tmp);
				if (cct > 2000 && cct < 10000)	
					pmode =PILED_CCT;
			}

			//4. xy
			if (getAttribute(xMainNode,"xy", &tmp))
			{
				splitstring s(tmp);
				vector<string> flds = s.split(',');

				if (flds.size() ==2)
				{
					xy[0]=lumitech::stof(flds[0]);
					xy[1]=lumitech::stof(flds[1]);

					if ((xy[0] > 0) && (xy[0] < 1) && (xy[1] > 0) && (xy[1] < 1))
						pmode =PILED_XY;   
				}				
			}

			//5. RGB
			if (getAttribute(xMainNode,"rgb", &tmp))
			{
				splitstring s(tmp);
				vector<string> flds = s.split(',');

				if (flds.size() ==3)
				{
					rgb[0]=lumitech::stoi(flds[0]) % 256;
					rgb[1]=lumitech::stoi(flds[1]) % 256;
					rgb[2]=lumitech::stoi(flds[2]) % 256;

					if ((rgb[0] >= 0 && rgb[1] >= 0 && rgb[2] >= 0) && (rgb[0] <= 255 && rgb[1] <= 255 && rgb[2] <= 255))
						pmode =PILED_RGB; 
				}				
			}

			if (getAttribute(xMainNode,"fadetime", &tmp))
			{
				fadetime = lumitech::stol(tmp);
			}

			if (getAttribute(xMainNode,"waittime", &tmp))
			{
				waittime = lumitech::stol(tmp);
			}

			if (getAttribute(xMainNode,"waitkeypress", &tmp))
			{
				waitOnKeypress = lumitech::stob(tmp);
			}
			else
				waitOnKeypress = false;

			
			ret=true;
		}

		return ret;
	}
	catch(...)
	{
		return false;
	}
}


bool PILEDScene::getAttribute(ITCXMLNode xMainNode, string attr, string* retStr)
{
	bool ret=false;

	IXMLCStr s=xMainNode.getAttribute(attr.c_str());
	
	if (s != 0)
	{
		*retStr = xMainNode.getAttribute(attr.c_str());

		if (retStr->length()>0)
		{
			ret=true;
		}
	}
	
	return ret;
}

string PILEDScene::printData()
{
	std::stringstream ss;
	ss << nr;
	
	if (pmode==PILED_CCT)
		 ss << " : b=" << brightness << " cct=" << cct;

	if (pmode==PILED_XY)	
		ss << " : b=" << brightness << " xy=" << xy[0] << "," << xy[1];
	
	if (pmode==PILED_RGB)	
		ss << " : rgb=" << rgb[0] << "," <<  rgb[1] << "," <<  rgb[2];
	
	if (pmode!=PILED_NONE)	
	{
		ss << "; fade=" << fadetime << "; wait=";
		if (waitOnKeypress)
			ss << "keypress";
		else
			ss <<  waittime;
	}

	//ss << endl;

	return ss.str();
}

#pragma endregion
