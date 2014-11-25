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

	log->cout("DebuServer: waiting for listening thread to stop...");
	listenThread.join();
	log->cout("DebugServer: deleted!");
}

unsigned long DebugServer::startListen()
{
	TCPStream* stream = NULL;
	
	listener = new TCPAcceptor(listenPort);
  
	cout << "DebugServer: start listening..." << endl;
    if (listener->start() == 0) 
	{
		while (1) 
		{			
			stream = listener->accept();
			cout << " accept " << endl;

			if (stream != NULL) 
			{
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
	for (unsigned int i=0;i<DebugClients.size(); i++)
	{
		//cout << "Client send" << endl;
		int ret = DebugClients[i]->send(s2send.c_str(), s2send.size());	

		//Client down
		//cout << "Client ret=" << ret << endl;
		if (ret < 0)
		{
			listofDowns.push_back(i);			
		}
	}

	for (unsigned int i=0; i< listofDowns.size(); i++)
		DebugClients.erase(DebugClients.begin() + listofDowns[i]);

	listofDowns.clear();
}
