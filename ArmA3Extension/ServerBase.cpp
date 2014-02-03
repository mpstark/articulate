#include "stdafx.h"
#include "ServerBase.h"
#include "Packet.h"


ServerBase::ServerBase()
{
}


ServerBase::~ServerBase()
{
	while (packets.size()) 
	{
		Packet* packet = packets.front();
		delete packet;
		packets.pop();
	}
}


int ServerBase::pending()
{
	return packets.size();
}

void ServerBase::read(char* output, int outputSize)
{
	if (!packets.size()) 
	{
		strncpy_s(output, outputSize, "nil", _TRUNCATE);
		return;
	}

	Packet* packet = packets.front();
	packet->getData().copy(output, outputSize);
	delete packet;
	packets.pop();
}

void ServerBase::write(char* data, int size)
{
	Packet* packet = new Packet(data, size);
	packets.push(packet);
}
