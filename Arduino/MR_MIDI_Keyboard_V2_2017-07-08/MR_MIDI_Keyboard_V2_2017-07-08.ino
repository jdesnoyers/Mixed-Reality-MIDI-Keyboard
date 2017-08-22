#include <Wire.h>
#include <Adafruit_MPR121.h>
#include <FastLED.h>

#define USB_DEBUG_MODE 0    //1 activates USB debug mode, 0 deactivates for MIDI transmission
#define CAP_ARRAY_ACTIVE 1 //1 activates capacitive sensor array (blocks startup if disconnected), 0 turns off
#define LEFT_LED_ACTIVE 1 //1 activates the left (fourth) LED, 0 deactivates

#define NUM_LEDS 52             //Number of LEDs in Neopixel strip
#define START_KEYLEDS 0         //Start of Keyboard aligned LEDs
#define NUM_KEYLEDS 50          //Number of Keyboard aligned LEDs
#define DELTA_T_MAX 106608      //set max velocity sensing time
#define DELTA_T_MIN 2416        //set min veloicty sensing time
#define SMOOTHING_ARRAY_SIZE 10 //smoothing array moving average
#define ENCODER_ARRAY_SIZE 8    //size of encoder control array
#define ENCODER_ACCEL_TIME 50 //what is the threshold for a "fast" count vs a "slow" count
#define ENCODER_INCREMENT_MAX 16 //maximum increment per encoder pulse

//analog pin definitions
#define ABSTRACTION_POT_PIN A0  //"Abstraction" Linear Potentiometer analog input pin
#define CUTOFF_POT_PIN A1       //Frequency cutoff rotary potentiometer pin
#define RESONANCE_POT_PIN A2    //Resonance rotary potentiometer pin
#define PITCHBEND_PIN A3        //IR pitchbend sensor pin
#define AFTERTOUCH_PIN A15      //Keyboard aftertouch pin

//digital pin definitions - NOTE: PINS 24-49 dedicated to keyboard
#define ENCODER_CHAN_A 2        //Rotary encoder channel A
#define ENCODER_CHAN_B 3        //Rotary encoder channel B
#define LED_DATA_PIN 8          //LED serial communication pin
#define IR_LED_PIN_A 9          //Infrared LED pin A (RIGHT)
#define IR_LED_PIN_B 10         //Infrared LED pin B (MIDDLE)
#define IR_LED_PIN_C 11         //Infrared LED pin C (LEFT)
#define ABSTRACTION_LED_PIN 12  //"Abstraction" Linear Potentiometer LED output pin
#define ARCADE_BUTTON_01 50     //Arcade button pin 1
#define ARCADE_BUTTON_02 51     //Arcade button pin 2
#define ARCADE_BUTTON_03 52     //Arcade button pin 3
#define ENCODER_BUTTON 53        //Rotary encoder button

/* 61 key Mixed Reality MIDI Keyboard
 * 
 * FOR ARDUINO MEGA 2560
 * 
 * John Desnoyers-Stewart
 * 
 * V2 August 6, 2017
 * V1 April 5, 2017
 * 
 * Use of KD, BR and MK correspond to keyboard matrix PCB
 * KD is output (activated rows) and BR and MK are input (scanned columns)
 * Two parallel 8x8 matrices, KDxBR and KDxMK
 */

//prototypes

void scanRow(byte row);                             //scans one row of key matrix
byte calculateVelocity(unsigned long keytimeCalc);  //calculates velocity based on switch timing
void speedCheck();                                  //used to test speed of the device

void midiOn(byte key,byte vel);   //generate MIDI on signal
void midiOff(byte key,byte vel);  //generate MIDI off signal
void midiAftertouch();            //convert aftertouch information to MIDI data
void midiCutoffFrequency();       //convert analog input to MIDI cutoff freq.
void midiResonance();             //convert analog input to MIDI resonance freq.
void midiPitchBend();             //convert IR prox sensor to MIDI pitch bend
void midiAbstractionLayer();      //send abstraction layer data over MIDI
void midiCapArray();              //send capacitive touch data over MIDI
void midiButtonRead();            //send button presses over MIDI
void encoderPinA();               //interrupt function for encoder pin a
void encoderPinB();               //interrupt function for encoder pin b
void midiEncoder();               //send encoder data over MIDI;
void smoothAnalogReadMIDI(uint16_t readarray[], byte& index, byte& smoothedvalue, byte factor, byte pin, int maxvolt = 1023);   //reads an analog value and smooths the number via an array

void sendMidiData(byte midiStatus,byte midiDataA,byte midiDataB); //send three byte midi signal
void sendMidiData(byte midiStatus,byte midiDataA);                //overload to send two byte midi signal

void activateLED(byte led, byte vel); //activate LED based on key press
void deactivateLED(byte led);         //deactivate on release

const byte matrixKD[8] = {31,33,35,37,39,41,43,45}; //define matrix rows

//scanned rows BR,MK pins - array 0-8 corresponds to board numbers 3-10
//const byte matrixBR[8] = {24,28,32,36,40,44,48,47};
//const byte matrixMK[8] = {25,26,30,34,38,42,46,49};

long loopTime = 0;  //time tracking variable

byte rownum = 0;  //current row number for activating BR/MK3-10

//matrix read/write bytes
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
byte midiChannel = 0;           //set midi channel
byte octShift = 24;             //current keyboard shift from C0
byte velCurveMode = 1;          //velocity curve, 0=linear, 1= exponential

//midi controls
uint16_t pitchBend = 8192;  //pitchbend store
uint16_t pitchRead = 8192;  //pitchbend read
byte pitchLatch = 1;        //pitchbend latch

uint16_t abstractionRead[SMOOTHING_ARRAY_SIZE] = {0}; //abstraction input array
byte abstractionSmooth = 0; //smoothed input
byte abstractionLayer = 0;  //stored/sent abstraction value

uint16_t cutoffRead[SMOOTHING_ARRAY_SIZE] = {0};  //cutoff input array
byte cutoffSmooth = 0;  //smoothed input
byte cutoffFrequency = 0; //stored/sent cutoff

uint16_t resonanceRead[SMOOTHING_ARRAY_SIZE] = {0};  //cutoff input array
byte resonanceSmooth = 0;  //smoothed input
byte resonanceValue = 0; //stored/sent cutoff


//smoothing increments
byte absSmIndex = 0;
byte cutSmIndex = 0;
byte resSmIndex = 0;

Adafruit_MPR121 capArray = Adafruit_MPR121(); //capacitive sensor array (i2c)
uint16_t capread = 0;       //capsense input read register
uint16_t capkeystate = 0;   //capsense input state register

byte buttonRead = 0;  //button toggle register
byte buttonState = 0;  //button toggle register
byte buttonToggle = B00001111;  //button toggle register

//encoder variables - based on Interrupt-based Rotary Encoder Sketch by Simon Merrett et al.
volatile byte aFlag = 0; // let's us know when we're expecting a rising edge on pinA to signal that the encoder has arrived at a detent
volatile byte bFlag = 0; // let's us know when we're expecting a rising edge on pinB to signal that the encoder has arrived at a detent (opposite direction to when aFlag is set)
volatile byte encoderPos = 0; //this variable stores our current value of encoder position. Change to int or uin16_t instead of byte if you want to record a larger range than 0-255
volatile byte oldEncPos = 0; //stores the last encoder position value so we can compare to the current reading and see if it has changed (so we know when to print to the serial monitor)
volatile byte reading = 0; //somewhere to store the direct values we read from our interrupt pins before checking to see if we have moved a whole detent
volatile byte encoderIncrement = 1; //how much to increment values from the encoder
volatile unsigned long encoderDeltaT = 0; //how long it has been since last pulse

byte encoderFunction = 0; //encoder function tracker
byte encoderArray[ENCODER_ARRAY_SIZE] = {0};

//Neopixel variables
CRGB ledRGB[NUM_LEDS];  //RGB color array (LED state)
CRGB ledAverageRGB;     //RGB averager
CHSV ledHSV[NUM_LEDS];  //HSV color array (for quick color calculations)
byte ledAvgCount = 0;   //count of activated LEDs








  
