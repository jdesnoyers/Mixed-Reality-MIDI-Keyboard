/*Send 3-byte midi off signal 
 * 1000nnnn 0kkkkkkk 0vvvvvvv
 * 128-143  0-127    0-127
 */
void midiOff(byte key,byte vel)
{

  if (key + octShift > 127)
    return; //return if key is out of range

  sendMidiData(B10000000,key+octShift,vel);
}

/*Send 3-byte midi on signal 
 * 1000nnnn 0kkkkkkk 0vvvvvvv
 * 144-159  0-127    0-127
 */ 
void midiOn(byte key,byte vel)
{
 
  if (key + octShift > 127)
    return; //return if key is out of range

  sendMidiData(B10010000,key+octShift,vel);
}

//check current aftertouch pressure and send midi data over USB
void midiAftertouch()
{

  byte aftertouchRead = constrain(map(analogRead(AFTERTOUCH_PIN),10,440,0,127),0,127); //read current pressure and map to valid MIDI range

  //if velocity is greater than zero send aftertouch message
  if(aftertouchRead > 0)
  {
    sendMidiData(B11010000,aftertouchRead);
  }

}

void midiPitchBend()
{

  uint16_t pitchRead = map(analogRead(PITCHBEND_PIN),100,550,0,16383); //read current pitch bend and map to 14 bit MIDI data integer;

  if (pitchBend != pitchRead)
  {
    if (pitchRead > 16383)
    {
      pitchBend = 8192;
      if (pitchLatch ==0)
      {
        pitchLatch = 1;
        byte pitchLSB = pitchBend & B01111111;        //capture 7 least significant bits
        byte pitchMSB = (pitchBend >> 7) & B01111111; // capture 7 most significant bits
        sendMidiData(B11100000,pitchLSB,pitchMSB); 
      }
    }
    else
    {
      pitchBend = pitchRead;
      byte pitchLSB = pitchBend & B01111111;        //capture 7 least significant bits
      byte pitchMSB = (pitchBend >> 7) & B01111111; // capture 7 most significant bits
    
      sendMidiData(B11100000,pitchLSB,pitchMSB);
      pitchLatch = 0;
    }
  }

}

void midiAbstractionLayer()
{

  
  smoothAnalogReadMIDI(abstractionRead, absSmIndex, abstractionSmooth, SMOOTHING_ARRAY_SIZE, ABSTRACTION_POT_PIN);
  
  if (abstractionLayer != abstractionSmooth)    //if the value has changed
  {
    
    abstractionLayer = abstractionSmooth;               //set current abstraction value to the inputted value
    
    analogWrite(ABSTRACTION_LED_PIN,abstractionLayer);  //change slider LED PWM value;
    
    sendMidiData(B10110000,20,abstractionLayer);        //send the command
    
  }
  
}

void midiCutoffFrequency()
{

  smoothAnalogReadMIDI(cutoffRead, cutSmIndex, cutoffSmooth, SMOOTHING_ARRAY_SIZE, CUTOFF_POT_PIN);
  
  if (cutoffFrequency != cutoffSmooth)    //if the value has changed
  {
    
    cutoffFrequency = cutoffSmooth;               //set current abstraction value to the inputted value
        
    sendMidiData(B10110000,74,cutoffFrequency);        //send the command
    
  }
  
}

//smooth analog inputs mapped to midi values
void smoothAnalogReadMIDI(uint16_t readarray[], byte& index, byte& smoothedvalue, byte factor, byte pin)
{
  readarray[index] = analogRead(pin);  //read current analog value

  if (index >=factor)   //if index has incremented through the array
  {
    uint16_t sum = 0;     //declare summing variable
    
    for (byte i = 0; i<factor; i++)     //sum the array
    {
      sum += readarray[i];
    }
    smoothedvalue = map(sum/factor,0,1023,0,127); //map values to MIDI byte
    
    index = 0;
  }
  else
    index++;  //otherwise increment

}

/* Capacitive array using the Adafruit MPR121 Breakout
 * Using I2C communication
 * 
 * Based on MPR121test by limor Fried Ladyada for Adafruit Industries
 */
void midiCapArray()
{

  capread = capArray.touched(); //capture current capacitive sensor values

  
  //for now just sending on off signals on unused MIDI channels 102-113

  for(byte i = 0; i< 12; i++)
  {
    if(i==0 || i>3) //for channels 0 & 4-11, send direct touch on/off signal
    {
      if(((capread & ~capkeystate) >> i) & 1)
      {
        sendMidiData(B10110000,102+i,127);
      }
      else if(((~capread & capkeystate) >> i) & 1)
      {
        sendMidiData(B10110000,102+i,0);
      }
    }
    else //for other channels (1-3) send toggle signal (first touch = on, second = off)
    {
      if(((capread & ~capkeystate & ~capkeytoggle) >> i) & 1)
      {
        capkeytoggle |= (1 << i);
        sendMidiData(B10110000,102+i,127);
        if(i==1)  //toggle IR LEDS on pad 1 press
        {
          digitalWrite(IR_LED_PIN_A,1);
          digitalWrite(IR_LED_PIN_B,1);
        }
      }
      else if(((capread & ~capkeystate & capkeytoggle) >> i) & 1)
      {
        capkeytoggle &= ~(1<<i);
        sendMidiData(B10110000,102+i,0);
        if(i==1)  //toggle IR LEDS on pad 1 press
        {
          digitalWrite(IR_LED_PIN_A,0);
          digitalWrite(IR_LED_PIN_B,0);
        }
      }
    }
  }
  
  capkeystate = capread; //store current key states
  
}

//Send MIDI data - two data bytes
void sendMidiData(byte midiStatus,byte midiDataA,byte midiDataB)
{

    Serial.print(midiStatus+midiChannel); //include channel
    Serial.print(" ");
    Serial.print(midiDataA);
    Serial.print(" ");
    Serial.println(midiDataB);

    //send over USB
    /*Serial.write(midiStatus+midiChannel); //include channel
    Serial.write(midiDataA);
    Serial.write(midiDataB);*/

    //send over MIDI
    Serial1.write(midiStatus+midiChannel); //include channel
    Serial1.write(midiDataA);
    Serial1.write(midiDataB);
}

//Send MIDI data - one data bytes
void sendMidiData(byte midiStatus,byte midiDataA)
{
    Serial.print(midiStatus+midiChannel); //include channel
    Serial.print(" ");
    Serial.println(midiDataA);
    
    //send over USB
    //Serial.write(midiStatus+midiChannel); //include channel
    //Serial.write(midiDataA);

    //send over MIDI
    Serial1.write(midiStatus+midiChannel); //include channel
    Serial1.write(midiDataA);  
}

//On MIDI in event, forward to USB
void serialEvent1()
{
  
  byte midiIN = Serial1.read();  //read input
  Serial.println(midiIN);        //send over USB

}
