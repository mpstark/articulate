#include "stdafx.h"
#include "Packet.h"


Packet::Packet(const char* _data, int _size)
{
	data = new String(_data);
	size = _size;
}

Packet::~Packet()
{
	delete data;
}

int Packet::getSize()
{
	return size;
}

String Packet::getData()
{
	return *data;
}