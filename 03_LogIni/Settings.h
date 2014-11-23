#pragma once

#include "helpers.h"
#include <vector>
#include <string>
#include <ctime>
#include "IXMLParser.h"

using namespace std;

// <<Singleton>>
class Settings
{
private:
	static Settings* _instance;	//Class property!
	string FileName;		//Filename+Extension
	string FullName;		//Path and Filename+ Extension
	string ComputerName;	
	Settings();				//private contructor for Singleton Pattern

	IXMLDomParser iDom;
	ITCXMLNode xMainNode;
	ITCXMLNode xNode;
	IXMLResults xmlResult;

	string getComputerName();
	string getFilePath();
	bool checkFilExists(string filename);
public:
	static Settings* getInstance();
	~Settings();

	string getFileName(bool);

	string ReadAttribute(string Section, string attr, string defaultValue);
	string ReadString(string Section, string name, string defaultValue);

	int ReadInt(string Section, string name, int defaultValue);
	short ReadShort(string Section, string name, short defaultValue);
	long ReadLong(string Section, string name, long defaultValue);
	bool ReadBool(string Section, string name, bool defaultValue);
	unsigned char ReadByte(string Section, string name, unsigned char defaultValue);

	unsigned int ReadUInt(string Section, string name, unsigned int defaultValue);
	unsigned short ReadUShort(string Section, string name, unsigned short defaultValue);
	unsigned long ReadULong(string Section, string name, unsigned long defaultValue);

	float ReadFloat(string Section, string name, float defaultValue);
	double ReadDouble(string Section, string name, double defaultValue);

	
	bool to_bool(std::string str);

	//std::time_t ReadDateTime(string Section, string name, std::time_t defaultValue);
	int ReadStringVector(string Section, string name, string defaultValue, vector<string> *flds);

	//Templates
	template <typename T> T Read(string Section, string name, T defaultValue);	
	template <typename T> 	T ReadAttrib(string Section, string attr, T defaultValue);
	//template <class T> void Write(string Section, string name, T Value);

};


template <typename T>
T Settings::Read(string Section, string name, T defaultValue)
{
	string s;
	T ret=defaultValue;

	try
	{
		s=ReadString(Section, name, "");		
		ret = fromString<T>(s);
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
};

template <typename T>
T Settings::ReadAttrib(string Section, string name, T defaultValue)
{
	string s;
	T ret=defaultValue;

	try
	{
		s=ReadAttribute(Section, name, "");		
		ret = fromString<T>(s);
	}
	catch(...)
	{
		return defaultValue;
	}

	return ret;
};
