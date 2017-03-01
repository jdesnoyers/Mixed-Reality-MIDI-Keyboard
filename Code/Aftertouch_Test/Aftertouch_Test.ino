/*
 * Aftertouch Test
 * 
 */

const int aftertouchPin = A0;

void setup() {

  Serial.begin(9600);
}

void loop() {

  int a = analogRead(aftertouchPin);
  int b = constrain(map(a,10,440,0,127),0,127);
  Serial.print(a);
  Serial.print(',');
  Serial.println(b);
  delay(200);

}
