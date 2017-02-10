#include "rest_client.h"

#include <InternetButton.h>

/* Pings your endpoint, to see if you're available on Skype, Busy or Away */
InternetButton b = InternetButton();

RestClient client = RestClient("http://localhost");

void setup() {
  Serial.begin(9600);
  Serial.println("starting...");
}

String response;
void loop(){
    response = "";
    int statusCode = client.get("/path", &response);
    Serial.print("Status code from server: ");
    Serial.println(statusCode);
    Serial.print("Response body from server: ");
    Serial.println(response);
    delay(1000);
}