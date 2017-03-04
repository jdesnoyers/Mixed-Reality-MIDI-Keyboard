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

  //Read KD pins: 7,6,5,4,3,2,1,0 = 43,41,39,37,35,33,31,29 = L6,G0,G2,C0,C2,C4,C6,A7
  inputKD = ((PINL << 1)&128)|((PING <<6)&64)|((PING<<3)& 32)|((PINC<<4) & 16)|((PINC<<1)&8)|((PINC>>2)&4)|((PINC>>5)&2)|((PINA >> 7)&1);
  
  //loop through columns MK0-7
  for(byte col = 0; col < colmax; col++)
  {
    
    int i=col+row*8; //calculate keyindex
    
    if(!bitRead(inputKD,col))
    {
      if (!bitRead(keypressBR[row],col) && !bitRead(keystate[row],col))
      {
        keytime[i] = micros(); //if key is newly pressed, track time to obtain velocity
      }
    }
    else 
    {
      if (bitRead(keypressBR[row],col) && bitRead(keystate[row],col))
      {
        //calculate velocity - may produce errors if MK wasn't triggered
        keyvelocity[i] = calculateVelocity(keytime[i]);

        bitClear(keystate[row],col); //track note as off
        midiOff(i,keyvelocity[i]);  //send midi off
          
      }
    }
          
  }
  
  keypressBR[row] = ~inputKD;//set keypress latch equal to inverse of current key state)
  
  digitalWrite(matrixBR[row], HIGH);   //deactivate BR
}

void scanRowMK(byte row)
{
  digitalWrite(matrixMK[row], LOW);   //activate MK

  //Read KD pins: 7,6,5,4,3,2,1,0 = 43,41,39,37,35,33,31,29 = L6,G0,G2,C0,C2,C4,C6,A7
  inputKD = ((PINL << 1)&128)|((PING <<6)&64)|((PING<<3)& 32)|((PINC<<4) & 16)|((PINC<<1)&8)|((PINC>>2)&4)|((PINC>>5)&2)|((PINA >> 7)&1);   

  //loop through columns MK0-7
  for(byte col = 0;col < colmax; col++)
  {
    
    int i= col+row*8; //calculate key index
    if (!bitRead(inputKD,col))
    {
      if(!bitRead(keypressMK[row],col) && !bitRead(keystate[row],col))
      {
        //Serial.println(micros()-keytime[i]);
        keyvelocity[i]= calculateVelocity(keytime[i]);      //calculate velocity, set time to zero, send midi signal
        
        //keytime[i] = 0;      //set time to zero
        bitSet(keystate[row],col); //track note as on
        midiOn(i,keyvelocity[i]); //send midi on
        
      }
    }
    else
    {
      if(bitRead(keypressMK[row],col) && bitRead(keystate[row],col))
      {
        //keytime[i] = micros();      //if key is newly released, track time to obtain velocity
      }
      else
      {
        if(bitRead(keypressBR[row],col) && !bitRead(keystate[row],col) && (micros()-keytime[i] > velocityMax))
        {
          bitSet(keystate[row],col);  //track note as on
          midiOn(i,0);  //send midi on
        }
      }

    }
         
  }

  keypressMK[row] = ~inputKD;//set keypress latch equal to inverse of current key state)

  digitalWrite(matrixMK[row], HIGH); //turn off MK
}



//Calculate velocity and check that it is within bounds
byte calculateVelocity(unsigned long keytimeCalc)
{
  //check if micros has rolled over, if so use alternate calcutaion, if not, 
  /*if(micros() < keytimeCalc)
    keytimeCalc = 4294967295 - keytimeCalc + micros(); //accounts for rollover
  else*/
    keytimeCalc = micros()-keytimeCalc; //typical calculation
    
  //velocity going over 127!!!
  long velCalc = map(keytimeCalc,velocityMax,velocityMin,0,127);

  //float velCalc = (keytimeCalc-velocityMax)*127/(velocityMin-velocityMax);
  
  //byte velCalc = ((velocityMax-(keytimeCalc-velocityMin))*127)/(velocityMax-velocityMin); //calculate velocity

  //ensure velocity is within bounds
  /*if (velCalc == 0)
    return 0;
  else if (velCalc > 127)
    return 127;
  else*/
    return velCalc;  
      
}
