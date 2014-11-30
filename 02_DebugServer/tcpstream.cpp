//http://vichargrave.com

#include "tcpstream.h"

TCPStream::TCPStream(int sd, struct sockaddr_in* address) : m_sd(sd) 
{
    char ip[50];
#ifdef WIN32
    InetNtop(PF_INET, (struct in_addr*)&(address->sin_addr.s_addr), ip, sizeof(ip)-1);
#else
	inet_ntop(PF_INET, (struct in_addr*)&(address->sin_addr.s_addr), ip, sizeof(ip)-1);
#endif
	m_address = address;
    m_peerIP = ip;
    m_peerPort = ntohs(address->sin_port);
}

TCPStream::~TCPStream()
{
#ifdef WIN32
        closesocket(m_sd);
#else
		close(m_sd);
#endif
}

void TCPStream::doShutdown()
{
#ifdef WIN32
	shutdown(m_sd, SD_BOTH);
#else
	shutdown(m_sd, SHUT_RDWR);
	//close(m_lsd);
#endif
}

int TCPStream::send(const char* buffer, size_t len) 
{
#ifdef WIN32
	return sendto(m_sd,buffer, len, 0, reinterpret_cast<sockaddr*>(&m_address),sizeof(m_address));
#else
    return write(m_sd, buffer, len);
#endif
}

int TCPStream::receive(char* buffer, size_t len) 
{
    return recv(m_sd, buffer, len, 0);
}

string TCPStream::getPeerIP() 
{
    return m_peerIP;
}

int TCPStream::getPeerPort() 
{
    return m_peerPort;
}

