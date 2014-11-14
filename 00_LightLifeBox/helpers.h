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


using namespace std;

class IObserver
{
public:
    virtual void updateClient(std::string) = 0;
	virtual ~IObserver() {};
};


class splitstring : public string {
    vector<string> flds;
public:
    splitstring(char *s) : string(s) { };
	splitstring(string s) : string(s) { };
    vector<string>& split(char delim, int rep=0);
};
