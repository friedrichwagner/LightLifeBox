#include "PILEDTestRunner.h"
#include "Settings.h"
#include <sstream>
#include "helpers.h"
#include <climits>	

//Static constants
const std::string PILEDSequenceRunner::sMainNodeName="PILEDSequencesTable";
const std::string PILEDSequenceRunner::sSquenceNodeName="PILEDSequence";


#pragma region Contructor/Destructor
PILEDSequenceRunner::PILEDSequenceRunner()
{
	Init("autorun");
}

PILEDSequenceRunner::PILEDSequenceRunner(string SequenceName)
{
	Init(SequenceName);
}

PILEDSequenceRunner::~PILEDSequenceRunner()
{
	
}

void PILEDSequenceRunner::Init(string SequenceName)
{
	isRunning=false;
	isLoaded=false;
	stopit=false;
	actualSequenceName=SequenceName;
	cntSequences=0;
	repeatCntSequence=1;
	actualRepeatCnt=0;
	

	Settings* s=Settings::getInstance();
	string fname=s->getFileName(true);
	log=Logger::getInstance();

	if(fname.length() > 0)
	{
		//xMainNode=iDom.parseFile(fname.c_str(), "Settings", &xmlResult);
		xMainNode=iDom.parseFile(fname.c_str(), PILEDSequenceRunner::sMainNodeName.c_str(), &xmlResult);
		cntSequences=xMainNode.nChildNode(PILEDSequenceRunner::sSquenceNodeName.c_str());
	}

	if (!xMainNode.isEmpty())
	{
		loadSequence(actualSequenceName);
	}
}
#pragma endregion


#pragma region get/set
int PILEDSequenceRunner::getSequenceCnt()
{
	return cntSequences;
}

int PILEDSequenceRunner::getActualSceneCnt()
{
	return cntActualScenes;
}

bool PILEDSequenceRunner::getIsRunning()
{
	return isRunning;
}

string PILEDSequenceRunner::getActualSequenceName()
{
	return actualSequenceName;
}

bool PILEDSequenceRunner::getIsLoaded()
{
	return isLoaded;
}
#pragma endregion


#pragma region methods
bool PILEDSequenceRunner::loadSequence(string SequenceName)
{
	string s;
	int i;
	bool ret=false;

	try
	{
		isLoaded=false;

		i=-1;
		xSquenceNode.emptyNode();
		cntActualScenes=0;
		actualSequenceName="";
		actualSceneNr=0;

		// <PILEDSequence name="autorun" repeat="true">
		for (i=0; i<cntSequences; i++)
		{
			s="";
			getAttribute(xMainNode.getChildNode(PILEDSequenceRunner::sSquenceNodeName.c_str(),i),"name", &s);

			if (s == SequenceName) break;
		}

		if (i < cntSequences)
		{
			xSquenceNode = xMainNode.getChildNode(PILEDSequenceRunner::sSquenceNodeName.c_str(),i);
			cntActualScenes = xSquenceNode.nChildNode(PILEDScene::xNodeName.c_str());
			
			//Name of the Sequence in attributte "name"
			actualSequenceName="SequenceName";
			getAttribute(xSquenceNode, "name", &actualSequenceName);
			
			//RepeatCount; -1 is endless
			try
			{
				repeatCntSequence=1;
				getAttribute(xSquenceNode,"repeat", &s);
				repeatCntSequence = lumitech::stoi(s);

				//If < 0 it means run forever
				if (repeatCntSequence < 0)
					repeatCntSequence = LONG_MAX;
			}
			catch(...)
			{
				repeatCntSequence=1;
			}
			

			log->info("Sequence load:" + actualSequenceName);

			isLoaded=true;
			ret=true;
		}
	}
	catch (std::exception& e)
	{
		log->error(e.what());
	}

	return ret;
}

bool PILEDSequenceRunner::startSequence()
{
	return startSequence(actualSequenceName);
}

bool PILEDSequenceRunner::stopSequence()
{
	//1. do Something to stop it
	stopit=true;

	//2. Wait for Sequence to stop
	/*while (isRunning)
	{
		//wait 100ms
		//lumitech::sleep(100);
	}*/

	return isRunning;
}

bool PILEDSequenceRunner::startSequence(string SequenceName)
{
	bool ret=false;
	stopit=false;
	actualSceneNr=0;
	this->actualRepeatCnt=repeatCntSequence;
	

	if (actualSequenceName != SequenceName) loadSequence(SequenceName);

	if (cntActualScenes>0)
	{
		log->cout(printSequence(actualSequenceName));

		while(actualRepeatCnt-- && !stopit)
		{
			isRunning=true;

			while (actualScene.getData(xSquenceNode, actualSceneNr) && !stopit)
			{
				//update listed Clients
				log->cout("\t" + actualScene.printData());
				
				//Iterate over all "registered" Clients and send data
				updateClients();
				

				if (actualScene.waitOnKeypress)
					lumitech::waitOnKeyPress();
				else
					lumitech::sleep(actualScene.waittime);
				
				actualSceneNr++;
			}

			if (stopit) break;
			else
				log->cout("Sequence finished: " + actualSequenceName);

			actualSceneNr=0;
		}
	}

	log->cout("Sequence stopped: " + actualSequenceName);

	isRunning=false;

	return ret;
}

string PILEDSequenceRunner::printSequence(string SequenceName)
{
	stringstream ss("nothing to print");

	if (actualSequenceName != SequenceName) loadSequence(SequenceName);

	if (isLoaded)
	{
		//ss << "SequenceName:" << getActualSequenceName() << "\t" "SequenceCount:" << lumitech::itos(getSequenceCnt()) << "\t" <<  "SceneCount:" << lumitech::itos(getActualSceneCnt()) << endl;		
		ss << "Sequence:" << getActualSequenceName() << "\t" <<  "SceneCount:" << lumitech::itos(getActualSceneCnt());// << endl;		
	}

	return ss.str();
}

string PILEDSequenceRunner::printScenes(string SequenceName)
{
	stringstream ss("nothing to print");
	
	if (actualSequenceName != SequenceName) loadSequence(SequenceName);

	if (cntActualScenes>0)
	{
		while (actualScene.getData(xSquenceNode, actualSceneNr))
		{

			ss << actualScene.printData();		
			actualSceneNr++;
		}
	}


	return ss.str();
}

#pragma endregion

#pragma region Observer Pattern Handling

void PILEDSequenceRunner::addClient(IBaseClient* c)
{
	log->info("added Client:" + c->getName());
	Clients.insert(c);
}

void PILEDSequenceRunner::removeClient(IBaseClient* c)
{
	std::set<IBaseClient*>::iterator it;

	it = Clients.find (c);

	if (it != Clients.end())
	{
		log->info("remove Client:" + c->getName());
		Clients.erase(it);
	}
}

void PILEDSequenceRunner::updateClients()
{
	for(set<IBaseClient*>::const_iterator iter = Clients.begin(); iter != Clients.end(); ++iter)
    {
        if(*iter != 0)
        {
			//log->info("Update Client:" + (*iter)->getName() + " Scene:" + actualSequenceName + "/" + lumitech::itos(actualScene.nr) );
			//log->info("Client:" + (*iter)->getName());
            (*iter)->updateData(&actualScene);
        }
    }
}

bool PILEDSequenceRunner::getAttribute(ITCXMLNode xMainNode, string attr, string* retStr)
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

#pragma endregion


