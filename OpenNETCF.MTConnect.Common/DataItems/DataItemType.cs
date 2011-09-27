// -------------------------------------------------------------------------------------------------------
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
    public enum DataItemSubtype
    {
        NONE,
        ALTERNATING,
        DIRECT,
        ACTUAL,
        COMMANDED,
        OVERRIDE,
        PROBE,
        TARGET,
        NO_SCALE,
        A_SCALE,
        B_SCALE,
        C_SCALE,
        D_SCALE,
        ROTARY,
        LINEAR,
        MAXIMUM,
        MINIMUM,
        GOOD,
        BAD,
        ALL,
        LINE,
        CONTROL,
        OTHER,
    }

    public enum DataItemType
    {
        OTHER,
        /// <summary>
        /// Rate of change of velocity
        /// </summary>
        ACCELERATION,
        /// <summary>
        /// The measurement of accumulated time associated with a Component
        /// </summary>
        ACCUMULATED_TIME,
        /// <summary>
        /// Rate of change of angular velocity
        /// </summary>
        ANGULAR_ACCELERATION,
        /// <summary>
        /// Rate of change of angular position
        /// </summary>
        ANGULAR_VELOCITY,
        /// <summary>
        /// The measurement of AC or DC current
        /// </summary>
        AMPERAGE,
        /// <summary>
        /// The angular position of a component relative to the parent.
        /// </summary>
        ANGLE,
        /// <summary>
        /// The feedrate of a linear axis
        /// </summary>
        AXIS_FEEDRATE,
        /// <summary>
        /// The reading of a timing device at a specific point in time. Clock time MUST be reported in W3C ISO 8601 format
        /// </summary>
        CLOCK_TIME,
        /// <summary>
        /// Percentage of one component within a mixture of components
        /// </summary>
        CONCENTRATION,
        /// <summary>
        /// The ability of a material to conduct electricity
        /// </summary>
        CONDUCTIVITY,
        /// <summary>
        /// The displacement as the change in position of an object
        /// </summary>
        DISPLACEMENT,
        /// <summary>
        /// The measurement of electrical energy consumption by a component
        /// </summary>
        ELECTRICAL_ENERGY,
        /// <summary>
        /// The measurement of the amount of a substance remaining compared to the planned maximum amount of that substance
        /// </summary>
        FILL_LEVEL,
        /// <summary>
        /// The rate of flow of a fluid
        /// </summary>
        FLOW,
        /// <summary>
        /// The measurement of the number of occurrences of a repeating event per unit time
        /// </summary>
        FREQUENCY,
        /// <summary>
        /// The measure of the push or pull introduced by an actuator or exerted on an object
        /// </summary>
        LINEAR_FORCE,
        /// <summary>
        /// The measurement of the percentage of the standard rating of a device
        /// </summary>
        LOAD,
        /// <summary>
        /// The measurement of the mass of an object(s) or an amount of material
        /// </summary>
        MASS,
        /// <summary>
        /// The feedrate of the tool path
        /// </summary>
        PATH_FEEDRATE,
        /// <summary>
        /// The current program control point or program coordinate in WORK coordinates. The coordinate system will revert to MACHINE coordinates if WORK coordinates are not available
        /// </summary>
        PATH_POSITION,
        /// <summary>
        /// The measure of the acidity or alkalinity
        /// </summary>
        PH,
        /// <summary>
        /// The position of the Component. Defaults to MACHINE coordinates.
        /// </summary>
        POSITION,
        /// <summary>
        /// The measurement of the ratio of real power flowing to a load to the apparent power in that AC circuit
        /// </summary>
        POWER_FACTOR,
        /// <summary>
        /// The force per unit area exerted by a gas or liquid
        /// </summary>
        PRESSURE,
        /// <summary>
        /// The measurement of the degree to which an object opposes an electric current through it
        /// </summary>
        RESISTANCE,
        /// <summary>
        /// The rotational speed of a rotary axis
        /// </summary>
        ROTARY_VELOCITY,
        /// <summary>
        /// Measurement of a sound level or sound pressure level relative to atmospheric pressure
        /// </summary>
        SOUND_LEVEL,
        /// <summary>
        /// Strain is the amount of deformation per unit length of an object when a load is applied
        /// </summary>
        STRAIN,
        /// <summary>
        /// The measurement of temperature
        /// </summary>
        TEMPERATURE,
        /// <summary>
        /// A measurement of angular displacement
        /// </summary>
        TILT,
        /// <summary>
        /// The turning force exerted on an object or by an object
        /// </summary>
        TORQUE,
        /// <summary>
        /// The measure of the apparent power in an electrical circuit, equal to the product of root-mean-square (RMS) voltage and RMS current’ (commonly referred to as VA)
        /// </summary>
        VOLT_AMPERE,
        /// <summary>
        /// The measurement of reactive power in an AC electrical circuit (commonly referred to as VAR)
        /// </summary>
        VOLT_AMPERE_REACTIVE,
        /// <summary>
        /// The rate of change of position
        /// </summary>
        VELOCITY,
        /// <summary>
        /// A measurement of a fluid’s resistance to flow
        /// </summary>
        VISCOSITY,
        /// <summary>
        /// The measurement of electrical potential between two points
        /// </summary>
        VOLTAGE,
        /// <summary>
        /// The measurement of power consumed or dissipated by an electrical circuit or device
        /// </summary>
        WATTAGE,


        /// <summary>
        /// The state of the Actuator - ACTIVE or INACTIVE
        /// </summary>
        ACTUATOR_STATE,
        /// <summary>
        /// The set of axes associated with a Path that the Controller is controlling. If this DataItem is not provided, it will be assumed the Controller is controlling all axes
        /// </summary>
        ACTIVE_AXES,
        /// <summary>
        /// Represents the ability of a Component to communicate. This MUST be provided for a Device and MAY be provided for any other Component. AVAILABLE or UNAVAILABLE
        /// </summary>
        AVAILABILITY,
        /// <summary>
        /// Describes the way the axes will be associated to each other. This is used in conjunction with COUPLED_AXES to indicate the way they are interacting. The possible values are: TANDEM, SYNCHRONOUS, MASTER, and SLAVE. The coupling MUST be viewed from the perspective of the axis, therefore a MASTER coupling indicates that this axis is the master of the COUPLED_AXES
        /// </summary>
        AXIS_COUPLING,
        /// <summary>
        /// The block of code being executed. Block contains the entire expression for a line of program code
        /// </summary>
        BLOCK,
        /// <summary>
        /// The current mode of the Controller. AUTOMATIC, MANUAL, MANUAL_DATA_INPUT, or SEMI_AUTOMATIC
        /// </summary>
        CONTROLLER_MODE,
        /// <summary>
        /// Refers to the set of associated axes. The value will be a space delimited set of axes names
        /// </summary>
        COUPLED_AXES,
        /// <summary>
        /// The direction of motion. CLOCKWISE or COUNTER_CLOCKWISE
        /// </summary>
        DIRECTION,
        /// <summary>
        /// The opened or closed state of the door. OPEN, UNLATCHED, or CLOSED
        /// </summary>
        DOOR_STATE,
        /// <summary>
        /// The current state of the emergency stop actuator. ARMED (the circuit is complete and the device is operating) or TRIGGERED (the circuit is open and the device MUST cease operation).
        /// </summary>
        EMERGENCY_STOP,
        /// <summary>
        /// The execution status of the Controller. READY, ACTIVE, INTERRUPTED, FEED_HOLD, or STOPPED
        /// </summary>
        EXECUTION,
        /// <summary>
        /// The current line of code being executed
        /// </summary>
        LINE,
        /// <summary>
        /// An uninterpreted textual notification
        /// </summary>
        MESSAGE,
        /// <summary>
        /// The identifier for the pallet currently in use for a given Path
        /// </summary>
        PALLET_ID,
        /// <summary>
        /// The current count of parts produced as represented by the Controller. MUST be an integer value
        /// </summary>
        PART_COUNT,
        /// <summary>
        /// An identifier of the current part in the device
        /// </summary>
        PART_ID,
        /// <summary>
        /// The operational mode for this Path. SYNCHRONOUS, MIRROR, or INDEPENDENT. Default value is INDEPENDENT if not specified
        /// </summary>
        PATH_MODE,
        /// <summary>
        /// The ON or OFF status of the Component. 
        /// </summary>
        /// <remarks>DEPRECATION WARNING: MAY be deprecated in the future</remarks>
        POWER_STATE,
        /// <summary>
        /// The name of the program being executed
        /// </summary>
        PROGRAM,
        /// <summary>
        /// The mode for the Rotary axis. SPINDLE, INDEX, or CONTOUR
        /// </summary>
        ROTARY_MODE,
        /// <summary>
        /// The identifier of an individual tool asset
        /// </summary>
        TOOL_ASSET_ID,
        /// <summary>
        /// The identifier of a tool provided by the device controller
        /// </summary>
        TOOL_NUMBER,
        /// <summary>
        /// The identifier for the workholding currently in use for a given Path
        /// </summary>
        WORKHOLDING_ID,


        /// <summary>
        /// An actuator related condition.
        /// </summary>
        ACTUATOR,
        COMMUNICATIONS,
        DATA_RANGE,
        LOGIC_PROGRAM,
        MOTION_PROGRAM,
        HARDWARE,
        SYSTEM,


        
        [Obsolete("Use CONDITION")]
        ALARM,
        [Obsolete()]
        CODE,
        [Obsolete()]
        POWER_STATUS,
        [Obsolete("Use ROTARY_VELOCITY")]
        SPINDLE_SPEED,
        [Obsolete("Use TOOL_ASSET_ID")]
        TOOL_ID,
        [Obsolete("Use FILL_LEVEL")]
        LEVEL,
        
    }
}
