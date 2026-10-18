#pragma once

// ray-cimgui 1.92.1 uses the pre-1.92.9 C wrapper shape. Keep the upstream
// backend source intact while adapting the two changed ImGui convenience APIs.
#ifndef CIMGUI_DEFINE_ENUMS_AND_STRUCTS
#define CIMGUI_DEFINE_ENUMS_AND_STRUCTS
#endif
#include "cimgui.h"

#define igGetContentRegionAvail(out) (*(out) = igGetContentRegionAvail())
#define CmdListsCount CmdLists.Size
