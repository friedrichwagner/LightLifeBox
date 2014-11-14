#include "Logger.h"
#include "Settings.h"

#include <iostream>
#include <fstream>
#include <iomanip>
#include <ctime>
#include <stdlib.h> 
#include <vector> 
#include "helpers.h"
#include <iostream>

using namespace std;

Logger* Logger::_instance = NULL;
int Logger::LogLevel = 10;

Logger::Logger()
{
	LogFileName = lumitech::getFileNamePlatform();
	ComputerName= lumitech::getComputerNamePlatform();
	Settings* ini=Settings::getInstance();

	LogLevel = ini->ReadInt("Logging", "loglevel", 10);
	//LogLevel = ini->Read<int>("Logging", "loglevel", 10);
}

Logger::~Logger()
{
	
}

Logger* Logger::getInstance()
{
   _instance = getInstance(Logger::LogLevel);
   return _instance;
}

Logger* Logger::getInstance(int pLogLevel)
{
	Logger::LogLevel= pLogLevel;

    if(_instance==NULL)
    {
        _instance = new Logger();
        return _instance;
    }
    else
    {
        return _instance;
	}
}

void Logger::info(string msg)
{
	//"[INFO]" wird nur geloggt, wenn Level=10
	log(msg,"[INFO]", "", 9);
}

void Logger::debug(string msg)
{
	//"[DEBUG]" wird nur geloggt, wenn Level=10
	log(msg,"[DEBUG]", "",9);
}

void Logger::warn(string msg)
{
	log(msg,"[WARN]", "",5);
}

void Logger::error(string msg)
{
	//"[ERROR]" wird immer geloggt
	log(msg,"[ERROR]", "",0);
}

void Logger::fatal(string msg)
{
	//"[FATAL]" wird immer geloggt
	log(msg,"[FATAL]", "",0);
}

void Logger::log(string msg, int level)
{
	//"[log]" user defined logging
	log(msg,"[LOG]", "", level);
}

void Logger::cout(string msg)
{
	std::ostringstream ss;

	ss <<  msg <<  endl;
	std::cout << ss.str();
	for (unsigned int i=0; i<logClients.size(); i++)
		logClients[i]->updateClient(ss.str());
}

void Logger::addClient(IObserver* obs)
{
	logClients.push_back(obs);
}

 void Logger::log(string msg, string type, string pwhere, int actualLevel)
{
    if (actualLevel <= LogLevel)
    {
		try
		{
			std::ostringstream ss1;
			ss1 << lumitech::currentDateTime() << "\t" << msg;
			cout(ss1.str());
			
#if !defined(RASPI)
			ss1 << endl;
			cout("Should not come here on RASPI");
			fstream fs(LogFileName, std::fstream::out | std::fstream::app);

			if (fs.is_open())
			{
				fs << ss1.str();
				fs.close();
			}
#endif
		}
		catch(...)
		{
			//silently ignore
		}
		
    }
}
