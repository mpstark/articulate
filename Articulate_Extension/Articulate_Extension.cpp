// Articulate_Extension.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <iostream>
#include <thread>
#include <queue>
#include <string>
#include <memory>

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

queue<auto_ptr<string>> PendingCommands;

void __stdcall RVExtension(char* output, int outputSize, const char *function) {

	if(!strcmp(function, "version")) strncpy(output, PLUGIN_VERSION, outputSize);
	else if(!strcmp(function, "start")) {
		if(namedPipe == NULL) {
			strncpy_s(output, outputSize, "409", 3);
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
			strncpy(output, "404", 3);
			namedPipe = NULL;
			return;
		}

		thread listenerThread(PipeListenerTask, &namedPipe);
	}
	else if(!strcmp(function, "read")) {
		if(PendingCommands.empty()) {
			strncpy_s(output, outputSize, "204", 3);
			return;
		}

		auto_ptr<string> currentCommand = PendingCommands.front();
		PendingCommands.pop();

		currentCommand->copy(output, outputSize, 0);

		currentCommand.release();
	}
}

void PipeListenerTask(HANDLE* namedPipe) {
	char buffer[COMMAND_BUFFER_SIZE];
	
	DWORD bytesRead = 0;
	
	string pendingData;

	while(*namedPipe) {
		BOOL result = ReadFile(*namedPipe, buffer, COMMAND_BUFFER_SIZE, &bytesRead, NULL);

		if(result) {			
			pendingData.append(buffer, bytesRead);

			int commandEndIndex = pendingData.find('\1');
			PendingCommands.push(auto_ptr<string>(new string(pendingData.substr(0, commandEndIndex))));

			if(commandEndIndex == pendingData.length() - 1) pendingData.clear();
			else pendingData = pendingData.substr(commandEndIndex + 1, pendingData.length() - commandEndIndex - 1);
		} else {
			CloseHandle(*namedPipe);
			*namedPipe = NULL;
		}
	}
}