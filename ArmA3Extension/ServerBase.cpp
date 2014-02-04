#include "stdafx.h"
#include "ServerBase.h"


ServerBase::ServerBase()
{
	packets = new std::queue<Packet*>();
}


ServerBase::~ServerBase()
{
	while (packets->size()) 
	{
		Packet* packet = packets->front();
		delete packet;
		packets->pop();
	}
}


int ServerBase::pending()
{
	return packets->size();
}

void ServerBase::read(char* output, int outputSize)
{
	if (!packets->size())
	{
		strncpy_s(output, outputSize, "nil", _TRUNCATE);
		return;
	}

	Packet* packet = packets->front();
	strncpy_s(output, outputSize, packet->getData().c_str(), _TRUNCATE);
	delete packet;
	packets->pop();
}

void ServerBase::write(const char* data, int size)
{
	Packet* packet = new Packet(data, size);
	packets->push(packet);
}
