#pragma once

#include <string>
#include <vector>
#include <map>
#include <iostream>

#ifdef WIN32
	#include "PlatformWin32.h"
#endif

#ifdef CYGWIN
	#include "PlatformCygwin.h"
#endif

enum enumButtonEvents {BUTTON_PRESSED, BUTTON_CHANGE};


using namespace std;

class IObserver
{
public:
    virtual void updateClient(std::string) = 0;
	virtual ~IObserver() {};
};

class IButtonObserver
{
public:
	virtual void notify(void* sender, enumButtonEvents event, int delta) = 0;
	virtual ~IButtonObserver() {};
};


class splitstring : public string 
{
    vector<string> flds;
	map<string, string> mapdata;
public:
	splitstring() : string() { };
    splitstring(char *s) : string(s) { };
	splitstring(string s) : string(s) { };
    vector<string>& split(char delim, int rep=0);
	map<string, string>& split2map(char delimOuter, char delimInner); //param1=123;param2=456;param3=789 --> delimOuter=';' delimInner='="
};
