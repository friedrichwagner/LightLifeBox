#include "DebugServer.h"
#include "Settings.h"
#include "PlatformCygwin.h"

DebugServer* DebugServer::_instance = NULL;

DebugServer::DebugServer()
{
	try
	{
		Settings* ini = Settings::getInstance();
		log = Logger::getInstance();

		listenPort= ini->Read<int>("Logging", "debugport", 9999);

		spawn();
	}
	catch (exception& ex)
	{
		log->cout(ex.what());
	}
}


DebugServer* DebugServer::getInstance()
{

    if(_instance==NULL)
    {
        _instance = new DebugServer();
        return _instance;
    }
    else
    {
        return _instance;
	}
}


DebugServer::~DebugServer()
{
	unsigned int i = 0;
	unsigned int cnt = DebugClients.size();

	for (i = 0; i<cnt; i++)
	{
		//Notify Clients of closing
		DebugClients[i]->doShutdown();

		//Close all client sockets
		delete DebugClients[i];
	}

	try
	{
		//Closes Listening Thread
		delete listener;		
	}
	catch(...)
	{
		//silently ignore errors here.
	}

	log->cout("DebugServer: waiting for listening thread to stop...");

	if (listenThread.joinable())  
		listenThread.join();
	log->cout("DebugServer: stopped!");
}

unsigned long DebugServer::startListen()
{
	TCPStream* stream = NULL;
	
	listener = new TCPAcceptor(listenPort);
  
	log->cout("DebugServer: start listening...");
    if (listener->start() == 0) 
	{
		while (1) 
		{			
			stream = listener->accept();			
			if (stream != NULL) 
			{
				log->cout("DebugServer: client added");
				DebugClients.push_back(stream);
			}
			else
			{
				//we shutdown the server
				break;
			}
		}
	}

	return 0;
}

void DebugServer::updateClient(string s2send) 
{
	unsigned int cnt = DebugClients.size();
	for (unsigned int i = 0; i<cnt; i++)
	{
		int ret = DebugClients[i]->send(s2send.c_str(), s2send.size());	

		//Client down
		//cout << "Client ret=" << ret << endl;
		if (ret < 0)
		{			
			listofDowns.push_back(i);			
		}
	}

	for (unsigned int i = 0; i < listofDowns.size(); i++)
	{
		cout << "DebugServer: client removed\r\n ";
		DebugClients.erase(DebugClients.begin() + listofDowns[i]);
	}

	listofDowns.clear();
}
