#pragma once
#include "Packet.h"

class ServerBase
{
public:
	ServerBase();
	~ServerBase();
	int pending();
	virtual void start() = 0;
	void read(char* output, int outputSize);
	void write(TCHAR* data, int outputSize);
	
private:
	std::queue<Packet*>* packets;
};

