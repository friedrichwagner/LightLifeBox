#pragma once

#ifdef WIN32
	#include <Ws2tcpip.h>
	#include <winsock2.h>
#else
	#include <netinet/in.h>
	#include <sys/types.h>
	#include <sys/socket.h>
	#include <unistd.h>
	#include <arpa/inet.h>
#endif

#include <string>

using namespace std;

class TCPStream
{
    int     m_sd;
    string  m_peerIP;
    int     m_peerPort;
	struct sockaddr_in* m_address;

  public:
    friend class TCPAcceptor;
    friend class TCPConnector;

    ~TCPStream();

    int send(const char* buffer, size_t len);
    int receive(char* buffer, size_t len);

    string getPeerIP();
    int    getPeerPort();
	void doShutdown();

  private:
    TCPStream(int sd, struct sockaddr_in* address);
    TCPStream();
    TCPStream(const TCPStream& stream);
};

