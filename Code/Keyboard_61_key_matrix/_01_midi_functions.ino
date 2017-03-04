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

//check current aftertouch pressure and send midi data over USB
void midiAftertouch()
{

  int pRead = analogRead(A0);
  byte pSend = (int) constrain(map(pRead,10,440,0,127),0,127); //read current pressure and map to valid midi range

  //if velocity is greater than zero send aftertouch message
  if(pSend>0)
  {
    Serial.print(208+midiChannel); //include channel
    Serial.print(" ");
    Serial.println(pSend);    
  }

}
