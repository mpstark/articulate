#include "ArmACommands.h"

#include <string>

using namespace std;

void GenerateScript(ArticulateCommand* command, char* output, int outputSize)
{
	std::string script;
	
	if(command->Subjects & ARTICULATE_SUBJECT_ALL)
		script.append("_units = units group player;\n");
	else
	{
		script.append(" _units = [];\n");
		if(command->Subjects & 0xff000000) { // Team selection
			script.append("{ if(");		
			bool hasTeam = false;
			if(command->Subjects & ARTICULATE_SUBJECT_TEAM_RED) {
				script.append("assignedTeam _x == \"RED\"");
				hasTeam = true;
			}
			
			if(command->Subjects & ARTICULATE_SUBJECT_TEAM_BLUE) {
				if(hasTeam)
					script.append(" || ");
				script.append("assignedTeam _x == \"BLUE\"");
				hasTeam = true;
			}
			
			if(command->Subjects & ARTICULATE_SUBJECT_TEAM_GREEN) {
				if(hasTeam)
					script.append(" || ");
				script.append("assignedTeam _x == \"GREEN\"");
				hasTeam = true;
			}
			
			if(command->Subjects & ARTICULATE_SUBJECT_TEAM_YELLOW) {
				if(hasTeam)
					script.append(" || ");
				script.append("assignedTeam _x == \"YELLOW\"");
				hasTeam = true;
			}
			
			if(command->Subjects & ARTICULATE_SUBJECT_TEAM_WHITE) {
				if(hasTeam)
					script.append(" || ");
				script.append("assignedTeam _x == \"MAIN\"");
			}

			script.append(") then { _units set [count _units, _x]; }; } forEach units group player;\n");			
		}

	}

	script.append("{ player groupSelectUnit [_x, true]; } forEach _units;\n");	

	switch(command->Command) {


		// TARGETTING COMMANDS

		case ARTICULATE_COMMAND_TARGET_CANCEL:
			script.append("_units commandWatch objNull;");
			break;
		case ARTICULATE_COMMAND_TARGET_CURSOR:
			script.append("{ cursorTarget player commandTarget _x; } forEach _units;");
			break;

		// ENGAGEMENT COMMANDS
		case ARTICULATE_COMMAND_ENGAGE_FIRE:
			script.append("_units commandFire objNull;");
			break;
	}
	
	script.append("\n{ player groupSelectUnit [_x, false]; } forEach groupSelectedUnits player;\n");	

	strncpy(output, script.c_str(), outputSize);
}