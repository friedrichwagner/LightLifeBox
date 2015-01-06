#pragma once

#include <string>
#include "tcpstream.h"

using namespace std;

class TCPAcceptor
{
    int    m_lsd;
    int    m_port;
    string m_address;
    bool   m_listening;
    
  public:
    TCPAcceptor(int port, const char* address="");
    ~TCPAcceptor();

    int        start();
    TCPStream* accept();

  private:
	  TCPAcceptor() {};
};

