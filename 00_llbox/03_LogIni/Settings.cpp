#include "Settings.h"

#include <iostream>
#include <fstream>
#include <sstream>
#include <stdlib.h> 
#include <vector> 
#include "helpers.h"
#include <iostream>
#include <algorithm>

using namespace std;

Settings* Settings::_instance = NULL;

Settings::Settings()
{
	ComputerName= lumitech::getComputerNamePlatform();	
	FileName = ComputerName + "_settings.xml";
	FullName = lumitech::getFilePathPlatform() + FileName;

	//Check for Computerspecific _settings.xml
	if (!checkFilExists(FullName))
	{
		//otherwise use "standard file" = settings.xml
		FileName="settings.xml";
		FullName = lumitech::getFilePathPlatform() + FileName;
	}	
	//cout << FileName << endl;
	//cout << FullName << endl;

	xMainNode=iDom.parseFile(FullName.c_str(), "Settings", &xmlResult);
}

Settings::~Settings()
{
	
}

bool Settings::checkFilExists(string filename)
{
	ifstream f(filename);

	return f.good();
}

Settings* Settings::getInstance()
{
    if(_instance==NULL)
    {
        _instance = new Settings();
        return _instance;
    }
    else
    {
        return _instance;
	}
}

string Settings::getFileName(bool full)
{
	if (full)
		return FullName;
	else
		return FileName;
}

string Settings::ReadString(string Section, string name, string defaultValue)
{
	string s(defaultValue);

	if (!xMainNode.isEmpty())
	{
		try
		{
			xNode=xMainNode.getChildNode(Section.c_str()).getChildNode(name.c_str());
			if (!xNode.isEmpty())
			{
				s = (char*) xNode.getAttribute("value");				
			}
		}
		catch(...)
		{
			return defaultValue;
		}
	
	}

	return s;
}

string Settings::ReadAttribute(string Section, string attr, string defaultValue)
{
	string s(defaultValue);

	if (!xMainNode.isEmpty())
	{
		try
		{
			xNode=xMainNode.getChildNode(Section.c_str());
			if (!xNode.isEmpty())
			{
				s = (char*) xNode.getAttribute(attr.c_str());				
			}
		}
		catch(...)
		{
			return defaultValue;
		}
	
	}

	return s;
}

bool Settings::to_bool(std::string str) 
{
    std::transform(str.begin(), str.end(), str.begin(), ::tolower);
    std::istringstream is(str);
    bool b;
    is >> std::boolalpha >> b;
    return b;
}


int Settings::ReadInt(string Section, string name, int defaultValue)
{
	string s;
	int ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = lumitech::stoi(s);			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

short Settings::ReadShort(string Section, string name, short defaultValue)
{
	string s;
	short ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = (short) lumitech::stoi(s);			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

long Settings::ReadLong(string Section, string name, long defaultValue)
{
	string s;
	long ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = (long) lumitech::stol(s);			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

bool Settings::ReadBool(string Section, string name, bool defaultValue)
{
	string s;
	bool  ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");
		std::transform(s.begin(), s.end(),s.begin(), ::toupper);

		if (s == "TRUE"  || s=="YES") ret=true;
		else ret=false;			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

unsigned char Settings::ReadByte(string Section, string name, unsigned char  defaultValue)
{
	string s;
	unsigned char ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = lumitech::stoi(s);

		if (ret<0 || ret>255) return defaultValue;
			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

unsigned int Settings::ReadUInt(string Section, string name, unsigned int defaultValue)
{
	string s;
	unsigned int ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = lumitech::stoi(s);

		if (ret<0) return defaultValue;
			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}



unsigned short Settings::ReadUShort(string Section, string name, unsigned short defaultValue)
{
	string s;
	unsigned short ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = (unsigned short) lumitech::stoi(s);

		if (ret<0) return defaultValue;			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

unsigned long Settings::ReadULong(string Section, string name, unsigned long defaultValue)
{
	string s;
	unsigned long ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = lumitech::stol(s);
		if(ret<0) return defaultValue;
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

float Settings::ReadFloat(string Section, string name, float defaultValue)
{
	string s;
	float ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = lumitech::stof(s);			
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

double Settings::ReadDouble(string Section, string name, double defaultValue)
{
	string s;
	double ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret =lumitech::stod(s);
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}

int Settings::ReadStringVector(string Section, string name, string defaultValue, vector<string> *ret)
{
	if (ret == 0) return 0;
	ret->clear();

	string tmp= ReadString(Section, name, defaultValue);	
	splitstring s(tmp);
	vector<string> flds = s.split(',');

	if (flds.size()==0 && defaultValue.length()>0)
		flds.push_back(defaultValue);

	for (unsigned int i=0; i < flds.size(); i++)
		ret->push_back(flds[i]);

	return flds.size();
}


/*
std::time_t Settings::ReadDateTime(string Section, string name, std::time_t defaultValue)
{
	string s;
	double ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = (double) std::stof(s);
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
}
*/



