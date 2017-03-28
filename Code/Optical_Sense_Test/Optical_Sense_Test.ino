

void setup() {

  Serial.begin(38400);
  
}

void loop() {

  Serial.println(analogRead(A0));
  
  delay(200);  

}
