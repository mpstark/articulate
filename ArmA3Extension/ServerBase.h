#pragma once
#include "Packet.h"

class ServerBase
{
public:
	ServerBase();
	~ServerBase();
	int pending();
	virtual void read(char* output, int outputSize);

protected:
	void write(char* data, int outputSize);

private:
	std::queue<Packet*>* packets;
};

