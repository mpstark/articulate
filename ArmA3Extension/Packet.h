#pragma once
class Packet
{
public:
	Packet(char* _data, int _size);
	~Packet();

	int getSize();
	std::string getData();

private:
	int size;
	std::string* data;
};

