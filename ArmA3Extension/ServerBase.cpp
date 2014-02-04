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
	packet->getData().copy(output, outputSize);
	delete packet;
	packets->pop();
}

void ServerBase::write(TCHAR* data, int size)
{
	Packet* packet = new Packet((const char*)data, size);
	packets->push(packet);
}
