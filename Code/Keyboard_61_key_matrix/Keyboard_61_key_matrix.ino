/* 61 key keyboard matrix to midi
 * 
 * FOR ARDUINO MEGA
 * 
 * John Desnoyers-Stewart
 * 200218553
 * March 1, 2017
 */

/*constants - use of KD, BR and MK correspond to keyboard matrix PCB
 *KD is input (scanned columns) and BR and MK are output (activated rows)
 *Two parallel 8x8 matrices, KDxBR and KDxMK
 */

//prototypes
void scanRowBR(byte);
void scanRowMK(byte);
void midiOn(byte,byte);
void midiOff(byte,byte);
void midiAftertouch();
byte calculateVelocity(unsigned long keytimeCalc);

long loopTime = 0;

//midi channel
const byte midiChannel = 0;

//scanned columns KD
const byte matrixKD[8] = {29,31,33,35,37,39,41,43};

//scanned rows BR - array 0-8 corresponds to board numbers 3-10
const byte matrixBR[8] = {22,26,30,34,38,42,46,45};

//scanned rows MK - array 0-8 corresponds to board numbers 3-10;
const byte matrixMK[8] = {23,24,28,32,36,40,44,47};

//refresh interval
const long sendTime = 1000;
const long scanTime = 200;

//time tracking long variables
unsigned long prevsendTime = 0;
unsigned long prevscanTime = 0;
unsigned long currentTime;
unsigned long currentMillis;

//current row number for activating BR 3-10/MK3-10
byte rownum = 0;
byte colmax = 8;

//read/write bytes
byte inputKD = 0;

//Keypress arrays (each bit is treated as a boolean)
byte keypressBR[8] = {0};
byte keypressMK[8] = {0};
byte keystate[8] = {0};

//key velocity array
byte keyvelocity[64];

//velocity calculating array
unsigned long keytime[64];

//midi settings
const unsigned int velocityMax = 10000;
const unsigned int velocityMin = 1040;
byte octShift = 12;
float midiSlope = 127/(velocityMax-velocityMin);
float midiOffset = midiSlope*velocityMax;

void setup() {

  for(byte i=0; i<8; i++)
  {
    //Activate KD columns as pull up inputs
    pinMode(matrixKD[i], INPUT_PULLUP);
    
    //Activate BR, MK as outputs
    pinMode(matrixBR[i], OUTPUT);
    pinMode(matrixMK[i], OUTPUT);

    //set all BR and MK to high (inactive)
    digitalWrite(matrixBR[i], HIGH);  
    digitalWrite(matrixMK[i], HIGH);  

      
  }

  
  //Activate serial
  Serial.begin(38400);

}

void loop() {


  if(rownum==7)
    colmax = 5;
  else
    colmax = 8;
  
  scanRowBR(rownum); 
  scanRowMK(rownum);

  
  currentTime = micros();
  currentMillis = millis();



  //increment rownum through 0-7 
  if (rownum >= 7)
  {
    rownum = 0;
    /*loopTime = micros()-loopTime;
    Serial.println(loopTime);
    loopTime = micros();*/
    //midiAftertouch();
    
  }
  else
    rownum ++;  

}



