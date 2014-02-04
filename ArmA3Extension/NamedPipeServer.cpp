#include "stdafx.h"
#include "NamedPipeServer.h"

using namespace std;

void createPipe(ServerBase* server, LPCWSTR name, HANDLE* pipe);
void listen(ServerBase* server, LPCWSTR name, HANDLE* pipe);
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
	thread t(listen, this, name, &pipe);
	t.detach();
}

BOOL NamedPipeServer::active()
{
	return !!pipe;
}

void createPipe(ServerBase* server, LPCWSTR name, HANDLE* pipe)
{
	*pipe = CreateNamedPipe(
		name,
		PIPE_ACCESS_INBOUND,
		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT,
		PIPE_UNLIMITED_INSTANCES,
		0,
		PIPE_BUFFER_SIZE,
		0,
		NULL
	);

	if (*pipe == NULL || *pipe == INVALID_HANDLE_VALUE)
	{
		*pipe = 0;
		char* error = "articulateError = \"Failed to open named pipe\"";
		server->write(error, strnlen_s(error, 1024));
		return;
	}
}

void listen(ServerBase* server, LPCWSTR name, HANDLE *pipe)
{
	BOOL connected;
	while (true)
	{
		createPipe(server, name, pipe);
		connected = ConnectNamedPipe(*pipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
		if (connected)
		{
			std::thread t(onClient, server, *pipe);
			t.detach();
		}
		else 
		{
			char* error = "articulateError = \"Command client couldn't connect to the pipe\"";
			server->write(error, strnlen_s(error, 1024));
			CloseHandle(pipe);
			pipe = NULL;
		}

		std::this_thread::sleep_for(std::chrono::milliseconds(500));
	}
}

void onClient(ServerBase* server, HANDLE port)
{
	HANDLE hHeap = GetProcessHeap();
	char* buffer = (char*)HeapAlloc(hHeap, 0, PIPE_BUFFER_SIZE * sizeof(char));
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
			PIPE_BUFFER_SIZE * sizeof(char),
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