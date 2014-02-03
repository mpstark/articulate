// ArmA3Extension.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

extern "C"
{
	__declspec (dllexport) void __stdcall RVExtension(char *output, int outputSize, const char *function);
}

void __stdcall RVExtension(char *output, int outputSize, const char *function)
{
	if (!strcmp(function, "version")) strncpy_s(output, outputSize, "1.0", _TRUNCATE);
	else strncpy_s(output, outputSize, "IT WORKS!", _TRUNCATE);
}