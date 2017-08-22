# Mixed Reality MIDI Keyboard

This Mixed Reality MIDI Keyboard is an open source system for integrating a MIDI Keyboard into a virtual environment. It includes the design and programming of both the physical keyboard, and the virtual reality environment through which it is interacted with. The system uses a number of devices including the HTC Vive, Vive Tracker, and Leap Motion to locate the user's hands and keyboard.

The keyboard is built from a salvaged keybed, Arduino Mega 2560, and a number of other analog and digital transducers. It communicates using the MIDI protocol, over either MIDI or USB cables allowing it to interface with other stanardized MIDI instruments and digital audio workstations.

## Folders

### Arduino
Program for implementation of physical hardware using Arduino Mega 2560.

### Design Files
3D CAD files, drawings, electrical routing required to build the Keyboard frame and assemble components.

### Unity
Contains required files for Unity project - requires a number of plugins and programs to function properly as listed below:

#### Required Unity Plugins
 - NVIDIA VRWorks
 - TextMesh Pro
 - Leap Motion SDK, hand module and interaction engine module
 
#### Required Programs
 - Unity 2017.1
 - Leap Motion Orion
 - LoopMIDI
 - Hairless MIDIserial
 - Any DAW (Digital Audio Workstation) such as Bitwig Studio or Ableton Live


## Setup

## Operation


---

## Updates

### 10/Aug/2017 - Version 2 - Audio Mostly
This update aligns the software with Version 2 of the physical keyboard. The keyboard has been improved with:
 - matte black interaction surfaces
 - angled upper deck
 - Additional potentiometer and rotary encoder
 - tactile buttons
 - Infrared LEDs with improved, more precise localization capablity

### 27/Mar/2017 - Version 1

#### Changes:
 - Changed to external pull-down resistor configuration in order to simultaneously scan both MK and BR matrices
 - Inputs read directly from registers to reduce scanning time
 - Scanning code optimized to reduce scanning time (currently as low as 470 microseconds when all other functions disabled)
 - MIDI output now sends over USB and MIDI port (TX1)
 - MIDI input (RX1) forwarding to USB
 - Dedicated send/receive MIDI functions
 - #define used in place of constants where possible
 - all variables minimized to take up as little space as possible (eg. using a byte as a boolean array)

#### Additions:
 - 12 input MPR121 Capacitive Array via I2C
 - 60 pixel Adafruit Neopixel LED Strip 
 - Fast LED library to minimize impact on scanning function
 - Pitch Bend control via IR proximity sensor (in progress, may be used as modulation control instead)
 - "Abstraction Layer" slider added
 
#### To Do:
 - Complete Hardware Design & Build
 - Develop Unity based VR platform
 - Integrate MIDI, HTC Vive and Leap Motion
 - Continued Optimization

---

### 27/Feb/2017
Currently a fully functional 61-key midi keyboard with velocity sensing (code is set to 9600 baud for testing)
