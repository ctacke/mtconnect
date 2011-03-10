﻿// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://mtconnect.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.MTConnect
{
    public enum DataItemType
    {
        ACCELERATION,
        ACTIVE_AXES,
        ALARM,
        AMPERAGE,
        ANGLE,
        ANGULAR_ACCELERATION,
        ANGULAR_VELOCITY,
        AVAILABILITY,
        BLOCK,
        CODE,
        DISPLACEMENT,
        DIRECTION,
        DOOR_STATE,
        EMERGENCY_STOP,
        EXECUTION,
        FREQUENCY,
        PART_COUNT,
        PART_ID,
        PATH_FEEDRATE,
        PATH_POSITION,
        AXIS_FEEDRATE,
        LINE,
        CONTROLLER_MODE,
        LOAD,
        MESSAGE,
        POSITION,
        POWER_STATUS,
        POWER_STATE,
        PRESSURE,
        PROGRAM,
        ROTARY_MODE,
        COUPLED_AXES,
        AXIS_COUPLING,
        SPINDLE_SPEED,
        TEMPERATURE,
        TORQUE,
        TOOL_ID,
        VELOCITY,
        VIBRATION,
        VOLTAGE,
        WATTAGE,
        WORKHOLDING_ID,
        COMMUNICATIONS,
        LOGIC_PROGRAM,
        MOTION_PROGRAM,
        HARDWARE,
        SYSTEM,
        LEVEL,
        ACTUATOR,
    }

    public enum DataItemSubtype
    {
        ACTUAL,
        COMMANDED,
        MAXIMUM,
        MINIMUM,
        OTHER,
        OVERRIDE,
        PROBE,
        TARGET,
        GOOD,
        BAD,
        ALL,
        LINE,
        CONTROL,
    }
}