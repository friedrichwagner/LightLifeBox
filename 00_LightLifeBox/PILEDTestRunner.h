	#pragma once

#include <string>
#include <set>
#include "IXMLParser.h"
#include "Logger.h"
#include "baseClient.h"
#include "PILEDScene.h"

using namespace std;

//enum enumWhatHasChanged { CHANGE_NOTHING=1, CHANGE_BRIGHTNESS=2, CHANGE_CCT=4, CHANGE_RGB=8, CHANGE_XY=16};

class PILEDSequenceRunner
{
private:
	static const string sMainNodeName;
	static const string sSquenceNodeName;

	string actualSequenceName;	
	int cntSequences;
	long repeatCntSequence;
	long actualRepeatCnt;

	bool isRunning;
	bool isLoaded;
	std::set<IBaseClient*> Clients;

	volatile bool stopit;

	IXMLDomParser iDom;
	ITCXMLNode xMainNode;
	ITCXMLNode xSquenceNode;
	IXMLResults xmlResult;

	PILEDScene actualScene;
	int cntActualScenes;
	int actualSceneNr;
	Logger* log;
	void Init(string);

	bool getAttribute(ITCXMLNode xMainNode, string attr, string* retStr);
public:
	PILEDSequenceRunner();
	PILEDSequenceRunner(string SequenceName);
	~PILEDSequenceRunner();	

	string getActualSequenceName();
	int getActualSceneCnt();
	int getSequenceCnt();
	bool getIsLoaded();
	bool getIsRunning();

	void addClient(IBaseClient*);
	void removeClient(IBaseClient*);
	void updateClients();

	
	bool stopSequence();	
	bool startSequence(string);
	bool startSequence();
	bool loadSequence(string);

	string printSequence(string);
	string printScenes(string);
};

