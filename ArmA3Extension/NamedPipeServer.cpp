#include "stdafx.h"
#include "NamedPipeServer.h"

using namespace std;

void listen(ServerBase* server, HANDLE pipe);
void onClient(ServerBase* server, HANDLE port);

NamedPipeServer::NamedPipeServer(LPCWSTR pipeName)
{
	name = pipeName;
}

NamedPipeServer::~NamedPipeServer()
{
}

void NamedPipeServer::start()
{
	DWORD dwThreadId;

	createPipe();
	thread t(listen, this, pipe);
	t.detach();
}

void NamedPipeServer::createPipe()
{
	if (!pipe)
	{
		pipe = CreateNamedPipe(
			name,
			PIPE_ACCESS_INBOUND,
			PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT,
			PIPE_UNLIMITED_INSTANCES,
			0,
			PIPE_BUFFER_SIZE,
			0,
			NULL
		);

		if (pipe == NULL || pipe == INVALID_HANDLE_VALUE)
		{
			pipe = 0;
			return;
		}
	}
}

void listen(ServerBase* server, HANDLE pipe)
{
	BOOL connected;
	DWORD dwThreadId;
	while (true)
	{
		connected = ConnectNamedPipe(pipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
		if (connected)
		{
			std::thread t(onClient, server, pipe);
			t.detach();
		}
		else 
		{
			CloseHandle(pipe);
			pipe = NULL;
		}
	}
}

void onClient(ServerBase* server, HANDLE port)
{
	HANDLE hHeap = GetProcessHeap();
	TCHAR* buffer = (TCHAR*)HeapAlloc(hHeap, 0, PIPE_BUFFER_SIZE*sizeof(TCHAR));
	DWORD cbBytesRead = 0;
	BOOL fSuccess = FALSE;

	if (port == NULL)
	{
		if (buffer != NULL) HeapFree(hHeap, 0, buffer);
		return;
	}

	if (buffer == NULL) return;
	
	while (true) 
	{
		fSuccess = ReadFile(
			port,
			buffer,
			PIPE_BUFFER_SIZE * sizeof(TCHAR),
			&cbBytesRead,
			NULL
		);

		if (!fSuccess || !cbBytesRead)
			break;

		server->write(buffer, cbBytesRead);
	}

	FlushFileBuffers(port);
	DisconnectNamedPipe(port);
	CloseHandle(port);
	HeapFree(hHeap, 0, buffer);
	return;
}