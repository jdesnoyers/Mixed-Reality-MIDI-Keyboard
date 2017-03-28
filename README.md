# Virtual Reality MIDI interface

An arduino based interactive perhiperal that allows for a augmented virtual reality experience using MIDI communication and hand tracking.
The user is able to interact via a customized prototyping MIDI keyboard, HTC Vive, and Leap Motion Controller.

---

## Updates

### 27/Mar/2017
**_Major Update_**

#### Current Status:
 - MIDI/Arduino portion mostly complete. From here on this platform will remain as a flexible test bed for various inputs in VR.


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
