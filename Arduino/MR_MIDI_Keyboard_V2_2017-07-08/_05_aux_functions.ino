//test current scanning rate
void speedCheck()
{
  loopTime = micros()-loopTime;
  Serial.println(loopTime);
  loopTime = micros();
}

void activateLED(byte led, byte vel)
{

  led = map(led,0,60,START_KEYLEDS,NUM_KEYLEDS+START_KEYLEDS-1);
  
  ledHSV[led].val = vel*2;        //set HSV model value equal to velocity
  ledRGB[led] = ledHSV[led];   //set RGB output equal to HSV
  ledAvgCount++;
  ledAverageRGB = ((ledAverageRGB*(ledAvgCount-1))+ledRGB[led])/ledAvgCount;
}

void deactivateLED(byte led)
{
  
  led = map(led,0,60,START_KEYLEDS,NUM_KEYLEDS+START_KEYLEDS-1);
  ledAvgCount--;
  
  if(ledAvgCount>0)
    ledAverageRGB = ((ledAverageRGB*(ledAvgCount+1))-ledRGB[led])/ledAvgCount; 
  else
    ledAverageRGB = CRGB(0,0,0);
    
  ledRGB[led] = CRGB(0,0,0);
}

void colorizeEnds()
{
    
  if((buttonToggle>>3)&1)
  {
    fill_solid(&ledRGB[START_KEYLEDS],NUM_KEYLEDS,CRGB(30,144,255));
    //fill_rainbow(&ledRGB[START_KEYLEDS],NUM_KEYLEDS,0,51);
  }
  else
  {
    fill_solid(&ledRGB[START_KEYLEDS],NUM_KEYLEDS,ledAverageRGB);
  }
}

/** Encoder reading based on Interrupt-based Rotary Encoder Sketch by Simon Merrett, based on insight from Oleg Mazurov, Nick Gammon, rt, Steve Spence
  * Repurposed for Arduino MEGA 2560 and MR MIDI keyboard
  */
  
void encoderPinA()
{
  cli(); //stop interrupts happening before we read pin values
  reading = PINE & B00110000; // read all eight pin values then strip away all but pinA and pinB's values
  if(reading == B00110000 && aFlag) { //check that we have both pins at detent (HIGH) and that we are expecting detent on this pin's rising edge
    if(encoderPos < 127)
    {
      //check if spinning faster than threshold, if it is, increment the accelerator, if not decrement
      if ((millis() - encoderDeltaT < ENCODER_ACCEL_TIME) && (encoderIncrement < ENCODER_INCREMENT_MAX))
      {
        encoderIncrement ++;
      }
      else if(encoderIncrement > 1)
      {
        encoderIncrement --;
      }
      
      encoderPos += encoderIncrement; //increment the encoder's position count

      //if encoderPos incremented past it's boundary, return it to the boundary and reset the incrementer
      if (encoderPos > 127)
      {
        encoderPos = 127;
        encoderIncrement =1;
      }
    }
    encoderDeltaT = millis();
    bFlag = 0; //reset flags for the next turn
    aFlag = 0; //reset flags for the next turn
  }
  else if (reading == B00010000) bFlag = 1; //signal that we're expecting pinB to signal the transition to detent from free rotation
  sei(); //restart interrupts
}

void encoderPinB()
{
  cli(); //stop interrupts happening before we read pin values
  reading = PINE & B00110000; //read all eight pin values then strip away all but pinA and pinB's values
  if (reading == B00110000 && bFlag) { //check that we have both pins at detent (HIGH) and that we are expecting detent on this pin's rising edge
    if(encoderPos > 0)
    {
      //check if spinning faster than threshold, if it is, increment the accelerator, if not decrement
      if ((millis() - encoderDeltaT < ENCODER_ACCEL_TIME) && (encoderIncrement < ENCODER_INCREMENT_MAX))
      {
        encoderIncrement ++;
      }
      else if(encoderIncrement > 1)
      {
        encoderIncrement --;
      }
      
      encoderPos -= encoderIncrement; //increment the encoder's position count
      
      if (encoderPos > 127)  //check if encoderPos wrapped around 0
      {
        encoderPos = 0; //bound the encoder value
        encoderIncrement = 1; //avoid overcounting
      }
    }
    
    encoderDeltaT = millis();
    bFlag = 0; //reset flags for the next turn
    aFlag = 0; //reset flags for the next turn
  }
  else if (reading == B00100000) aFlag = 1; //signal that we're expecting pinA to signal the transition to detent from free rotation
  sei(); //restart interrupts
}
