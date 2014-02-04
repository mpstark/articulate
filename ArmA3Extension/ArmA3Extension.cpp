// ArmA3Extension.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ServerBase.h"
#include "NamedPipeServer.h"
#include "Packet.h"
#include "ArmA3Extension.h"


NamedPipeServer* server = new NamedPipeServer(L"\\\\.\\pipe\\Articulate");

void __stdcall RVExtension(char *output, int outputSize, const char *function)
{
	if (!strcmp(function, "version")) strncpy_s(output, outputSize, "1.0", _TRUNCATE);
	else if (!strcmp(function, "start"))
	{
		server->start();
		strncpy_s(output, outputSize, server->active() ? "true" : "false", _TRUNCATE);
	}
	else if (!strcmp(function, "read")) server->read(output, outputSize);
	else strncpy_s(output, outputSize, "nil", _TRUNCATE);
}