#include <Wire.h>
#include <Adafruit_MPR121.h>
#include <CapSense.h>
#include <FastLED.h>

#define NUM_LEDS 60
#define LED_DATA_PIN 2
#define ABSTRACTION_LED_PIN 12
#define ABSTRACTION_POT_PIN A0
#define PITCHBEND_PIN A1
#define AFTERTOUCH_PIN A15

/* 61 key MIDI Keyboard for input prototyping
 * 
 * FOR ARDUINO MEGA
 * 
 * John Desnoyers-Stewart
 * 200218553
 * March 25, 2017
 */

/*Use of KD, BR and MK correspond to keyboard matrix PCB
 *KD is output (activated rows) and BR and MK are input (scanned columns)
 *Two parallel 8x8 matrices, KDxBR and KDxMK
 */


//prototypes
void scanRow(byte row);
byte calculateVelocity(unsigned long keytimeCalc);
void speedCheck();

void midiOn(byte key,byte vel);
void midiOff(byte key,byte vel);
void midiAftertouch();
void midiPitchBend();
void midiAbstractionLayer();
void midiCapArray();

void sendMidiData(byte midiStatus,byte midiDataA,byte midiDataB);
void sendMidiData(byte midiStatus,byte midiDataA);

void activateLED(byte led, byte vel);
void deactivateLED(byte led);

const byte matrixKD[8] = {31,33,35,37,39,41,43,45};

//scanned rows BR,MK pins - array 0-8 corresponds to board numbers 3-10
//const byte matrixBR[8] = {24,28,32,36,40,44,48,47};
//const byte matrixMK[8] = {25,26,30,34,38,42,46,49};

//time tracking long variables
long loopTime = 0;

byte rownum = 0;  //current row number for activating BR 3-10/MK3-10

//read/write bytes
byte inputBR = 0; //each bit captures the current state of a key within the current scan
byte inputMK = 0;

//Key arrays
byte keypressBR[8] = {0};   //MK keypress latch (each bit as treated as a boolean)
byte keypressMK[8] = {0};   //MK keypress latch (each bit as treated as a boolean)
byte keystate[8] = {0};     //tracks last midi state sent (0=off,1=on) (each bit as treated as a boolean)
byte keyvelocity[64];       //tracks key velocity to send over midi
unsigned long keytime[64];  //tracks first switch trigger time to calculate velocity
byte velCalc = 0;           //byte to calculate velocity;

//midi settings

const long deltaTmax = 106608;  //set max velocity sensing time
const long deltaTmin = 2416;    //set min veloicty sensing time
byte midiChannel = 0;           //set midi channel
byte octShift = 24;             //current keyboard shift from C0
byte velCurveMode = 1;          //velocity curve, 0=linear, 1= exponential


//smoothing variables
const byte smmax = 10;
byte smindex = 0;

//midi controls
uint16_t pitchBend = 8192;
uint16_t pitchRead = 8192;
uint16_t abstractionRead[smmax+1] = {0};
byte abstractionSmooth = 0;
byte abstractionLayer = 0;

//capacitive sensor array
Adafruit_MPR121 capArray = Adafruit_MPR121();
uint16_t capread = 0;
uint16_t capkeystate = 0;

CRGB ledRGB[NUM_LEDS];
CHSV ledHSV[NUM_LEDS];








  
