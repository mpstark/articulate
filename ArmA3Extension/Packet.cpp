#include "stdafx.h"
#include "Packet.h"


Packet::Packet(char* _data, int _size)
{
	data = new std::string(_data);
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

std::string Packet::getData()
{
	return *data;
}