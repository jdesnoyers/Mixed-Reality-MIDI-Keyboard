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

  uint16_t pitchRead = map(analogRead(PITCHBEND_PIN),0,1023,0,16383); //read current pitch bend and map to 14 bit MIDI data integer;

  if (pitchBend != pitchRead)
  {
    pitchBend = pitchRead;

    byte pitchLSB = pitchRead & B01111111;        //capture 7 least significant bits
    byte pitchMSB = (pitchRead >> 7) & B01111111; // capture 7 most significant bits

    sendMidiData(B11100000,pitchLSB,pitchMSB);

  }

}

void midiAbstractionLayer()
{

  
  abstractionRead[smindex] = analogRead(ABSTRACTION_POT_PIN);

  if (smindex>smmax)
  {
    uint16_t sum = 0;
    
    for (byte i = 0; i<smmax; i++)
    {
      sum += abstractionRead[i] ;
    }
    abstractionSmooth = map(sum/smmax,0,1023,0,127);
    
    smindex = 0;
  }
  else
    smindex++;
  
  if (abstractionLayer != abstractionSmooth)
  {
    
    abstractionLayer = abstractionSmooth;
    
    analogWrite(ABSTRACTION_LED_PIN,abstractionLayer); //change slider LED PWM value;
    
    sendMidiData(B10110000,20,abstractionLayer);
    
  }
  
}

/* Capacitive array using the Adafruit MPR121 Breakout
 * Using I2C communication
 * 
 * Based on MPR121test by limor Fried Ladyada for Adafruit Industries
 */
void midiCapArray()
{

  capread = capArray.touched(); //capture current capacitive sensor values

  
  //for now just sending on off signals on unused MIDI channels 102-113)
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

//Send MIDI data - two data bytes
void sendMidiData(byte midiStatus,byte midiDataA,byte midiDataB)
{
    //send over USB
    Serial.print(midiStatus+midiChannel); //include channel
    Serial.print(" ");
    Serial.print(midiDataA);
    Serial.print(" ");
    Serial.println(midiDataB);

    //send over MIDI
    Serial1.write(midiStatus+midiChannel); //include channel
    Serial1.write(midiDataA);
    Serial1.write(midiDataB);
}

//Send MIDI data - one data bytes
void sendMidiData(byte midiStatus,byte midiDataA)
{
    //send over USB
    Serial.print(midiStatus+midiChannel); //include channel
    Serial.print(" ");
    Serial.println(midiDataA);

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
