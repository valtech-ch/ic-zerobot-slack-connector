How to run the service?
Install the Service with .net command line: installutil <yourproject>.exe
You find it usually under: "C:\Windows\Microsoft.NET\Framework64\v4.0.30319" or a different version depending on you locally installed .Net Framework
Then simply run the Sercice from your local service list (Should be called ICZeroBotSlack)

How to debug the Service?
When compiled as Debug, the service calls the Init after 10 seconds. That's the time you have to attach your source to the process called ICZeroBotSlack. 
