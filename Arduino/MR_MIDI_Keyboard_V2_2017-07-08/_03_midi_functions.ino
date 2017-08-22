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
    
    analogWrite(ABSTRACTION_LED_PIN,pow(abstractionLayer,1.5)/11.269);  //change slider LED PWM value;
    
    sendMidiData(B10110000,20,abstractionLayer);        //send the command
    
  }
  
}

void midiCutoffFrequency()
{

  smoothAnalogReadMIDI(cutoffRead, cutSmIndex, cutoffSmooth, SMOOTHING_ARRAY_SIZE, CUTOFF_POT_PIN);
  
  if (cutoffFrequency != cutoffSmooth)    //if the value has changed
  {
    
    cutoffFrequency = cutoffSmooth;               //set current cutoff value to the inputted value
        
    sendMidiData(B10110000,74,cutoffFrequency);        //send the command
    
  }
  
}

void midiResonance()
{

  smoothAnalogReadMIDI(resonanceRead, resSmIndex, resonanceSmooth, SMOOTHING_ARRAY_SIZE, RESONANCE_POT_PIN);
  
  if (resonanceValue != resonanceSmooth)    //if the value has changed
  {
    
    resonanceValue = resonanceSmooth;               //set current resonance value to the inputted value
        
    sendMidiData(B10110000,71,resonanceValue);        //send the command
    
  }
  
}

//smooth analog inputs mapped to midi values
void smoothAnalogReadMIDI(uint16_t readarray[], byte& index, byte& smoothedvalue, byte factor, byte pin, int maxvolt = 1023)
{
  readarray[index] = analogRead(pin);  //read current analog value

  if (index >= factor - 1)   //if index has incremented through the array
  {
    uint16_t sum = 0;     //declare summing variable
    
    for (byte i = 0; i<factor; i++)     //sum the array
    {
      sum += readarray[i];
    }
    smoothedvalue = map(sum/factor,0,maxvolt,0,127); //map values to MIDI byte
    
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

  
  //sending on off signals on unused MIDI channels 102-113

  for(byte i = 0; i< 12; i++)
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
  
  capkeystate = capread; //store current key states
  
}

void midiButtonArray()
{

  //read pins 53,52,51,50 PINB 0,1,2,3
  buttonRead = PINB & B00001111;

  //if encoder pin 53 pressed increment function and send updated value
  if(buttonRead & ~buttonState & 1)
  {
    if(encoderFunction == 1)  //skip CC 74 (cutoff frequency)
    {
      encoderFunction = 3;
    }
    else if(encoderFunction >= ENCODER_ARRAY_SIZE - 1)  //loop back around at CC 79
    {
      encoderFunction = 0;
    }
    else
    {
      encoderFunction++;
    }
    sendMidiData(B10110000,114,encoderFunction);  //send the current encoder function
    encoderPos = encoderArray[encoderFunction];   //update the encoder to match current function
  }

  //toggle for buttons 52-50
  for(byte i=1 ; i < 4 ; i++)
  {
       
      if(((buttonRead & ~buttonState & ~buttonToggle) >> i) & 1)
      {
        buttonToggle |= (1 << i);
        sendMidiData(B10110000,114+i,127);
        if(i==3)  //toggle IR LEDS on button 3 press
        {
          digitalWrite(IR_LED_PIN_A,1);
          digitalWrite(IR_LED_PIN_B,1);
          #if(LEFT_LED_ACTIVE == 1)
            digitalWrite(IR_LED_PIN_C,1);
          #endif
        }
      }
      else if(((buttonRead & ~buttonState & buttonToggle) >> i) & 1)
      {
        buttonToggle &= ~(1<<i);
        sendMidiData(B10110000,114+i,0);
        if(i==3)  //toggle IR LEDS on button 3 press
        {
          digitalWrite(IR_LED_PIN_A,0);
          digitalWrite(IR_LED_PIN_B,0);
          #if(LEFT_LED_ACTIVE == 1)
            digitalWrite(IR_LED_PIN_C,0);
          #endif
        }
      }
  }
    
  buttonState = buttonRead;
}



void midiEncoder()
{
  if(oldEncPos != encoderPos)
  {
    if (encoderPos <= 127)
    {
      encoderArray[encoderFunction] = encoderPos;
      sendMidiData(B10110000,72 + encoderFunction,encoderArray[encoderFunction]);
    }
    oldEncPos = encoderPos;
  }
  else if ((encoderIncrement > 1) && (millis() - encoderDeltaT  > ENCODER_ACCEL_TIME))
  {
        encoderIncrement--; //if the encoder isn't being used, return incrementer to 1
  }
}

//Send MIDI data - two data bytes
void sendMidiData(byte midiStatus,byte midiDataA,byte midiDataB)
{

  #if(USB_DEBUG_MODE == 1)
    //debug over USB
    Serial.print(midiStatus+midiChannel); //include channel
    Serial.print(" ");
    Serial.print(midiDataA);
    Serial.print(" ");
    Serial.println(midiDataB);

  #else  
    //send over USB
    Serial.write(midiStatus+midiChannel); //include channel
    Serial.write(midiDataA);
    Serial.write(midiDataB);
  #endif
  
    //send over MIDI
    Serial1.write(midiStatus+midiChannel); //include channel
    Serial1.write(midiDataA);
    Serial1.write(midiDataB);
}

//Send MIDI data - one data bytes
void sendMidiData(byte midiStatus,byte midiDataA)
{
  #if(USB_DEBUG_MODE == 1)
    //debug over USB
    Serial.print(midiStatus+midiChannel); //include channel
    Serial.print(" ");
    Serial.println(midiDataA);

  #else
    //send over USB
    Serial.write(midiStatus+midiChannel); //include channel
    Serial.write(midiDataA);
  #endif

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
