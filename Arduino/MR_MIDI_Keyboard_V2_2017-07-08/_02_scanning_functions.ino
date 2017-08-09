/*Scan all of the columns of the specified row to see which keys in the matrix have been pressed
 *the columns correspond to the sequential physical key, 0-7
 *the rows correspond to groups of keys, 0-7
 *row 0, col 0 = C1 on the keyboard, row 1, col 0 = C1#... row 0, col 1 is then G#1 and so on.
 *
 *BR and MK are parallel matrices used to measure velocity. The key pressed will always activate switch BR then MK
 *by measuring the time between activations we can derive a realtive velocity. The reverse is true for off signals.
 *
 *The matrix scan operates as follows:
 *all rows are set low as the system uses pull down resistor configuration
 *One KD row is set high, all MK and BR columns are then scanned, if any have BR activated then a timer for that key is started.
 *If MK is activated then the time is captured and a key on signal sent over MIDI. 
 *Key off events are handled in the reverse order, when an MK is deactivated, a timer is started, 
 *when BR becomes deactivated the time is captured and KEY OFF sent.
 */

void scanRow(byte row)
{

  //activate KD(n) - direct access causes glitch on key 55 (row 7; col 6; KD7,MK/BR9) only increases speed by 20 microseconds
  //KD 0,1,2,3,4,5,6,7 = 31,33,35,37,39,41,43,45 = C6,C4,C2,C0,G2,G0,L6,L4
  
  digitalWrite(matrixKD[row], 1); //activate KD(n)
  
  /*switch (row)
  {
    case 0: PORTC |= B01000000;
      break;
    case 1: PORTC |= B00010000;
      break;
    case 2: PORTC |= B00000100;
      break;
    case 3: PORTC |= B00000001;
      break;
    case 4: PORTG |= B00000100;
      break;
    case 5: PORTG |= B00000001;
      break;
    case 6: PORTL |= B01000000;
      break;
    case 7: PORTL |= B00010000;
      break;
  }*/

  /*Read MK,BR pins via registers:
   *BR 10,9,8,7,6,5,4,3 = 47,48,44,40,36,32,28,24 = L2,L1,L5,G1,C1,C5,A6,A2
   *MK 10,9,8,7,6,5,4,3 = 49,46,42,38,34,30,26,25 = L0,L3,L7,D7,C3,C7,A4,A3
   */
  inputBR = ((PINL << 5) & B11000000)|(PINL & B00100000)|((PING << 3) & B00010000)|((PINC << 2) & B00001000)|((PINC >> 3) & B00000100)|((PINA >> 5) & B00000010)|((PINA >> 2) & B00000001);
  inputMK = ((PINL << 7) & B10000000)|((PINL << 3) & B01000000)|((PINL >> 2) & B00100000)|((PIND >> 3) & B00010000)|(PINC & B00001000)|((PINC >> 5) & B00000100)|((PINA >> 3) & B00000011);


  //loop through columns MK0-7
  for(byte col = 0; col < 8; col++)
  {
    
    uint8_t i=row+col*8; //calculate keyindex

    if(bitRead(inputBR,col))  //if first input is active
    {
      if (!bitRead(keypressBR[row],col) && !bitRead(keystate[row],col))
      {
        keytime[i] = micros(); //if key is newly pressed, track time to obtain velocity
      }
      
      if (bitRead(inputMK,col)) //if second input (MK) is active
      {
        if(!bitRead(keypressMK[row],col) && !bitRead(keystate[row],col))
        {
          keyvelocity[i]= calculateVelocity(keytime[i]);      //calculate velocity, set time to zero, send midi signal
          
          bitSet(keystate[row],col);      //track note as on
          midiOn(i,keyvelocity[i]);       //send midi on
          activateLED(i,keyvelocity[i]);  //turn on LED
          
        }
      }
      else if(bitRead(keypressMK[row],col) && bitRead(keystate[row],col))
      {
          keytime[i] = micros();      //if key is newly released, track time to obtain velocity
        
      }
    }
    else if (bitRead(keypressBR[row],col) && bitRead(keystate[row],col))
    {
      //calculate velocity
      keyvelocity[i] = calculateVelocity(keytime[i]);

      bitClear(keystate[row],col);  //track note as off
      midiOff(i,keyvelocity[i]);    //send midi off
      deactivateLED(i);             //turn off LED
    }        
  }

  //set keypress latch equal to current key state
  keypressBR[row] = inputBR;
  keypressMK[row] = inputMK;

  digitalWrite(matrixKD[row], 0); //deactivate KD(n)

  //deactivate KD(n)
  //KD 0,1,2,3,4,5,6,7 = 31,33,35,37,39,41,43,45 = C6,C4,C2,C0,G2,G0,L6,L4
  
  /*switch (row)
  {
    case 0: PORTC &= B10111111;
      break;
    case 1: PORTC &= B11101111;
      break;
    case 2: PORTC &= B11111011;
      break;
    case 3: PORTC &= B11111110;
      break;
    case 4: PORTG &= B11111011;
      break;
    case 5: PORTG &= B11111110;
      break;
    case 6: PORTL &= B10111111;
      break;
    case 7: PORTL &= B11101111;
      break;
  }*/
}


//Calculate velocity and check that it is within bounds
byte calculateVelocity(unsigned long keytimeCalc)
{
  //check if micros has rolled over, if so use alternate calcutaion 
  if(micros() < keytimeCalc)
    keytimeCalc = 4294967295 - keytimeCalc + micros(); //accounts for rollover
  else
    keytimeCalc = micros()-keytimeCalc; //typical calculation


  if (!velCurveMode)
    velCalc = map(keytimeCalc,DELTA_T_MAX,DELTA_T_MIN,0,127); //linear velocity
  else
    velCalc = pow(2.71828,(float) (124100-keytimeCalc)/25000)-3; //approximate exponential velocity

  
  return constrain(velCalc,0,127);//return velocity within bounds of midi spec;  
      
}
