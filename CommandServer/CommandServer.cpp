// CommandServer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <NamedPipeServer.h>


int main(int argc, TCHAR* argv[])
{
	NamedPipeServer server(L"\\\\.\\pipe\\Articulate");
	server.start();
	std::cout << "Server started...\n";

	char buffer[12 * 1024] = {};
	while (1)
	{
		if (!server.pending()) {
			std::this_thread::sleep_for(std::chrono::milliseconds(500));
			continue;
		}

		server.read(buffer, sizeof(buffer));
		printf("%s", buffer);
	}
	return 0;
}

