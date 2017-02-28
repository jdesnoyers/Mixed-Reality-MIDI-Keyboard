/* 61 key keyboard matrix to midi
 * 
 * FOR ARDUINO MEGA
 * 
 * John Desnoyers-Stewart
 * 200218553
 * February 13, 2017
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
byte calculateVelocity(unsigned long keytimeCalc);


//midi channel
const byte midiChannel = 0;

//scanned columns KD
const byte matrixKD[8] = {40,38,36,34,32,30,28,26};

//scanned rows BR - array 0-8 corresponds to board numbers 3-10
const byte matrixBR[8] = {47,43,39,35,31,27,23,24};

//scanned rows MK - array 0-8 corresponds to board numbers 3-10;
const byte matrixMK[8] = {46,45,41,37,33,29,25,22};

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
  Serial.begin(9600);

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
    rownum = 0;
  else
    rownum ++;
  

}

/*Scan all of the columns of the specified row to see which keys in the matrix have been pressed
 *the columns correspond to the sequential physical key, 0-7
 *the rows correspond to groups of keys, 0-7
 *row 0, col 0 = C1 on the keyboard, row 0, col 1 = C1#... row 1, col 0 is then G#1 and so on.
 *
 *BR and MK are parallel matrices used to measure velocity. The key pressed will always activate switch BR then MK
 *by measuring the time between activations we can derive a realtive velocity. The reverse is true for off signals.
 *MK is not always activated so an on or off signal with zero velocity is sent after a few moments .
 *
 *The matrix scan operates as follows:
 *all rows are set typically high as the system uses pull up resistor configuration
 *One BR row is set low, all columns are then scanned, if any have activated then a timer for that key is started
 *The corresponding MK row is set low, all columns scanned. If any are activated then the velocity is captured from the difference in activation time
 *Some error is inherent as the activation may occur while other rows are being scanned, but this is imperceptible
 *
 */

void scanRowBR(byte row)
{

  digitalWrite(matrixBR[row], LOW);   //activate BR


  //loop through columns MK0-7
  for(byte col = 0; col < colmax; col++)
  {
    
    int i=col+row*8; //calculate keyindex
    
    if(digitalRead(matrixKD[col]) == LOW)
    {
      if ((bitRead(keypressBR[row],col) == 0) && (bitRead(keystate[row],col) == 0))
      {
        keytime[i] = micros(); //if key is newly pressed, track time to obtain velocity
      }
    }
    else 
    {
      if ((bitRead(keypressBR[row],col) == 1) && (bitRead(keystate[row],col) == 1))
      {
        //calculate velocity - may produce errors if MK wasn't triggered
        keyvelocity[i] = calculateVelocity(keytime[i]);

        bitClear(keystate[row],col); //track note as off
        midiOff(i,keyvelocity[i]);  //send midi off
          
      }
    }
      
    bitWrite(keypressBR[row],col,!digitalRead(matrixKD[col])); //set keypress latch equal to inverse of current key state)
    
  }

  digitalWrite(matrixBR[row], HIGH);   //deactivate BR
}

void scanRowMK(byte row)
{
  digitalWrite(matrixMK[row], LOW);   //activate MK
  
  //loop through columns MK0-7
  for(byte col = 0;col < colmax; col++)
  {
    
    int i= col+row*8; //calculate key index
    
    if (digitalRead(matrixKD[col]) == LOW)
    {
      if((bitRead(keypressMK[row],col) == 0) && (bitRead(keystate[row],col) == 0))
      {
     
        keyvelocity[i]= calculateVelocity(keytime[i]);      //calculate velocity, set time to zero, send midi signal
        
        //keytime[i] = 0;      //set time to zero
        bitSet(keystate[row],col); //track note as on
        midiOn(i,keyvelocity[i]); //send midi on
        
      }
    }
    else 
    {
      if((bitRead(keypressMK[row],col) == 1) && (bitRead(keystate[row],col) == 1))
      {
        //keytime[i] = micros();      //if key is newly released, track time to obtain velocity
      }
      else
      {
        if((bitRead(keypressBR[row],col) == 1) && (bitRead(keystate[row],col) == 0)&& (micros()-keytime[i] > velocityMax))
        {
          bitSet(keystate[row],col);  //track note as on
          midiOn(i,0);  //send midi on
        }
      }

    }
    
    bitWrite(keypressMK[row],col,!digitalRead(matrixKD[col]));
        
  }

  digitalWrite(matrixMK[row], HIGH); //turn off MK
}

/*Send 3-byte midi off signal 
 * 1000nnnn 0kkkkkkk 0vvvvvvv
 * 128-143  0-127    0-127
 */
void midiOff(byte key,byte vel)
{

  if (key + octShift > 127)
    return; //return if key is out of range
  else if (key <=1)
    vel = 0; //keys 0,1 are defective, set velocity to zero
  
  Serial.print(128+midiChannel);
  Serial.print(" ");
  Serial.print(key+octShift); //align octave shift
  Serial.print(" ");
  Serial.println(vel);
}

/*Send 3-byte midi on signal 
 * 1000nnnn 0kkkkkkk 0vvvvvvv
 * 144-159  0-127    0-127
 */
void midiOn(byte key,byte vel)
{
 
  if (key + octShift > 127)
    return; //return if key is out of range
  else if (key <=1)
    vel = 0; //keys 0,1 are defective, set velocity to zero
    
  Serial.print(144+midiChannel); //inlcude channel
  Serial.print(" ");
  Serial.print(key+octShift); //align octave shift
  Serial.print(" ");
  Serial.println(vel);
}

//Calculate velocity and check that it is within bounds
byte calculateVelocity(unsigned long keytimeCalc)
{
  //check if micros has rolled over, if so use alternate calcutaion, if not, 
  if(micros() < keytimeCalc)
    keytimeCalc = 4294967295 - keytimeCalc + micros(); //accounts for rollover
  else
    keytimeCalc = micros()-keytimeCalc; //typical calculation
    
  //velocity going over 127!!!  
  byte velCalc = ((velocityMax-(keytimeCalc-velocityMin))*127)/(velocityMax-velocityMin); //calculate velocity

  //ensure velocity is within bounds
  if (velCalc == 0)
    return 0;
  else if (velCalc > 127)
    return 127;
  else
    return velCalc;  
      
}

