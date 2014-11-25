#pragma once

#include <string>
#include "helpers.h"
#include <sstream>
#include <vector>

// <<Singleton>>
class Logger
{
private:
	static Logger* _instance;	//Class property!
	std::string LogFileName;		//Path and Filename
	std::string ComputerName;
	static int LogLevel; 	//Class property!
	Logger();				//private contructor for Singleton Pattern

	std::string getComputerName();
	std::string getFileName();

	vector<IObserver*> logClients;	
public:
	static Logger* getInstance();
	static Logger* getInstance(int LogLevel);
	~Logger();

	void info(std::string);
	void warn(std::string);
	void debug(std::string);
	void error(std::string);
	void fatal(std::string);
	void log(std::string msg, std::string type, std::string pwhere, int level);
	void log(std::string msg, int level);

	//all the DebugServers
	void cout(std::string msg);
	void addClient(IObserver* obs);
	void removeClient(IObserver* obs);
};

