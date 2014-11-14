#include "DebugServer.h"
#include "Settings.h"

DebugServer* DebugServer::_instance = NULL;

DebugServer::DebugServer() 
{
	Settings* ini = Settings::getInstance();

	listenPort= ini->ReadInt("Logging", "debugport", 9999);
	//listenPort= ini->Read<int>("Logging", "debugport", 9999);

#ifdef WIN32
	spawn();
#else
	listenThread = spawn();
#endif
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
  	for (unsigned int i=0;i<DebugClients.size(); i++)
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

	//Waiting for Listening Thread to shutdown
#ifdef WIN32
	WaitForSingleObject(listenThread, INFINITE);
#else
	//close(socket_id) does not close the accept, therefore it would hang here if I wait on the join
	//listenThread.join();
#endif
	}
	catch(...)
	{
		//silently ignore errors here.
	}

	
}

unsigned long DebugServer::startListen()
{
	TCPStream* stream = NULL;

	listener = new TCPAcceptor(listenPort);
   
	cout << "Start Listening..." << endl;
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
