#pragma once

#include <string>
#include <vector>
#include <iostream>

#ifdef WIN32
	#include "PlatformWin32.h"
#endif

#ifdef CYGWIN
	#include "PlatformCygwin.h"
#endif

enum enumButtonEvents {BUTTON_DOWN, BUTTON_UP, BUTTON_PRESSED, BUTTON_CHANGE};


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
	virtual void notify(enumButtonEvents event, long val) = 0;
	virtual ~IButtonObserver() {};
};


class splitstring : public string {
    vector<string> flds;
public:
    splitstring(char *s) : string(s) { };
	splitstring(string s) : string(s) { };
    vector<string>& split(char delim, int rep=0);
};
