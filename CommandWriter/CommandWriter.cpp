// CommandWriter.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#define PIPE_BUFFER_SIZE 12 * 1024

void openPipe(HANDLE* pipe);

int _tmain(int argc, _TCHAR* argv[])
{
	HANDLE pipe;

	DWORD written;
	String command;
	
	while (1)
	{
		std::cout << "> ";
		std::getline(std::cin, command);
		if (!command.length()) return 0;

		openPipe(&pipe);
		if (!pipe) continue;

		BOOL wrote = WriteFile(pipe, command.c_str(), command.length() + 1, &written, NULL);
		if (!wrote) 
		{
			std::cout << "Failed to write command to Extension\n";
			printf("Error: %d", GetLastError());
		}

		CloseHandle(pipe);
		pipe = NULL;
	}

	return 0;
}

void openPipe(HANDLE* pipe)
{
	*pipe = CreateFile(
		L"\\\\.\\pipe\\Articulate",
		GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		0,
		NULL
		);

	if (*pipe == INVALID_HANDLE_VALUE)
	{
		std::cout << "Invalid Pipe Handle\n";
		*pipe = 0;
		return;
	}

	DWORD err = GetLastError();
	switch (err)
	{
		case ERROR_PIPE_BUSY:
		case ERROR_PIPE_LISTENING:
		case ERROR_PIPE_CONNECTED:
		case ERROR_SUCCESS:
		case ERROR_IO_PENDING:
			break;

		case ERROR_PIPE_NOT_CONNECTED:
			std::cout << "Failed to connect to the pipe\n";
			*pipe = 0;
			return;
		case ERROR_ACCESS_DENIED:
			std::cout << "Access denied\n";
			*pipe = 0;
			return;
	}

	/*if (!WaitNamedPipe(L"\\\\.\\pipe\\Articulate", 20000))
	{
		std::cout << "Couldn't connect to Articulate plugin\n";
		return;
	}*/

	DWORD dwMode = PIPE_TYPE_MESSAGE | PIPE_WAIT;
	SetNamedPipeHandleState(
		*pipe,
		&dwMode,
		NULL,
		NULL
	);	
}