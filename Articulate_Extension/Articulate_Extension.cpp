// Articulate_Extension.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <iostream>
#include <thread>
#include <queue>

using namespace std;
#define PLUGIN_VERSION "1.0"
#define PIPE_NAME L"\\\\.\\pipe\\Articulate"
#define COMMAND_BUFFER_SIZE 4096

extern "C" {
	__declspec(dllexport) void __stdcall RVExtension(char *output, int outputSize, const char *function); 
};

/**
* PRIVATE VARIABLES
**/
HANDLE namedPipe = NULL;


/**
* PRIVATE FUNCTIONS
**/
void PipeListenerTask(HANDLE* namedPipe);

/**
* INTERNAL STRUCTURES
**/

std::queue<char*> PendingCommands;

void __stdcall RVExtension(char* output, int outputSize, const char *function) {

	if(!strcmp(function, "version")) strncpy(output, PLUGIN_VERSION, outputSize);
	else if(!strcmp(function, "start")) {
		if(namedPipe == NULL) {
			strncpy_s(output, outputSize, "Already connected to Articulate", 32);
			return;
		}

		namedPipe = CreateFile(PIPE_NAME,
			GENERIC_READ,
			FILE_SHARE_READ | FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			NULL);

		if(namedPipe == INVALID_HANDLE_VALUE) {
			strncpy(output, "Articulate is not running", 25);
			namedPipe = NULL;
			return;
		}

		thread listenerThread(PipeListenerTask, &namedPipe);
	}
	else if(!strcmp(function, "read")) {
		if(PendingCommands.empty()) {
			strncpy_s(output, outputSize, "{}", 2);
			return;
		}

		char* currentCommand = PendingCommands.front();
		PendingCommands.pop();
		strncpy_s(output, outputSize, currentCommand, strlen(currentCommand));
		free(currentCommand);
	}
}

void PipeListenerTask(HANDLE* namedPipe) {
	char buffer[COMMAND_BUFFER_SIZE];
	
	DWORD bytesRead = 0;

	while(*namedPipe) {
		BOOL result = ReadFile(*namedPipe, buffer, COMMAND_BUFFER_SIZE, &bytesRead, NULL);

		if(result) {
			char* command = (char*)malloc(bytesRead);
			strncpy(command, buffer, bytesRead);
			PendingCommands.push(command);
		} else {
			CloseHandle(*namedPipe);
			*namedPipe = NULL;
		}
	}
}