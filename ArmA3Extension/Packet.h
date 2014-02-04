#pragma once
class Packet
{
public:
	Packet(const char* _data, int _size);
	~Packet();

	int getSize();
	String getData();

private:
	int size;
	String* data;
};

