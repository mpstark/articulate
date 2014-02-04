#pragma once
#include "Packet.h"

class ServerBase
{
public:
	ServerBase();
	~ServerBase();
	int pending();
	virtual BOOL active() = 0 { return true; }
	virtual void start() = 0;
	void read(char* output, int outputSize);
	void write(const char* data, int outputSize);
	
private:
	std::queue<Packet*>* packets;
};

