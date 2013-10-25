# Articulate_Extension
This extension provides Named Pipe command formatting and forwarding for ArmA games (> v1.61) to permit Articulate to perform more complex operations than are available using the standard keybinds.

When combined with Articulate's Addon, this extension makes it possible to send advanced commands to ArmA through a named pipe (\\.\pipe\Articulate) using a predetermined format.

```cpp
struct ArticulateCommand {
	uint Subjects;
	uint Command;
}

#define ARTICULATE_SUBJECT_ALL 0x80000000
#define ARTICULATE_SUBJECT_TEAM_RED 0x01000000
#define ARTICULATE_SUBJECT_TEAM_BLUE 0x02000000
#define ARTICULATE_SUBJECT_TEAM_GREEN 0x04000000
#define ARTICULATE_SUBJECT_TEAM_YELLOW 0x08000000
#define ARTICULATE_SUBJECT_TEAM_WHITE 0x10000000

#define ARTICULATE_UNIT_2 0x2
#define ARTICULATE_UNIT_3 0x4
#define ARTICULATE_UNIT_4 0x8
#define ARTICULATE_UNIT_5 0x10
#define ARTICULATE_UNIT_6 0x20
#define ARTICULATE_UNIT_7 0x40
#define ARTICULATE_UNIT_8 0x80
#define ARTICULATE_UNIT_9 0x100
#define ARTICULATE_UNIT_10 0x200
#define ARTICULATE_UNIT_11 0x400
#define ARTICULATE_UNIT_12 0x800
#define ARTICULATE_UNIT_13 0x1000
#define ARTICULATE_UNIT_14 0x2000
#define ARTICULATE_UNIT_15 0x4000
#define ARTICULATE_UNIT_16 0x8000
#define ARTICULATE_UNIT_17 0x10000
#define ARTICULATE_UNIT_18 0x20000
#define ARTICULATE_UNIT_19 0x40000
#define ARTICULATE_UNIT_20 0x80000
#define ARTICULATE_UNIT_21 0x100000
#define ARTICULATE_UNIT_22 0x200000
#define ARTICULATE_UNIT_23 0x400000
#define ARTICULATE_UNIT_24 0x800000
```

The available commands list is still being finalized due to limitations in the available control over units using SQF functions. They will (hopefully) at some point be as complete as the standard command menu.