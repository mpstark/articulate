#pragma once
#include "ServerBase.h"

#define PIPE_BUFFER_SIZE 12 * 1024

class NamedPipeServer :
	public ServerBase
{
public:
	NamedPipeServer(LPCWSTR pipeName);
	~NamedPipeServer();
	void start() override;

private:
	LPCWSTR name;
	HANDLE pipe;
	void createPipe();
};

