Bio Info Terminal :
is A vocal and text based basic AI lab assistant designed with many skills to help with 
lab prep, statistical analysis and chemical recognition. It was built using Microsoft project oxford 
speech recognition for responding to using speech, Nadio speech response to let your computer respond in
 multiple voices and NHunspell dictionary tools for checking user spelling. In addition we use API links
to databases like PubChem to identify chemical compound data and Windows Presentation Foundation to build 
the UI.

To run simply open in visual studio 2017 or higher then to get speech recognition working set up an 
account with MS cognitive services you can get one here note this is free to open up but will require 
a credit card. You can start the process here.

https://azure.microsoft.com/en-us/services/cognitive-services/

Once you have your key go to the app config and place it in the location marked
<add key="MicosoftSpeechApiKey" value="# INSERT MS SPEACH API KEY CODE HERE #" />

Then simply run the program you can ask questions like 
"Tell me about cytosine"

You can further specify what other data you want to include after that such as formula
Or you can simply say - "Tell me about cytosine data full" for a set of info on the chemical compound

This basic version shows how the prototype looked before it was advanced to a full AI with a fully 
responsive system and thus only has a few skills operational they are detailed in the power point

BioInfo Terminal User Guide.ppx
 
inside the documentation section. You can also run the program with speech recognitions operational 
if you just go to the bin/Debug section and open the Bio_Info_Termianl.exe. Note you must do this before
 running is VS otherwise your new build will overwrite the exe with working API speech keys in it.

Last an installer is located in the app.publish section but may not work on all machines 

Lead Developer : Matthew Cintron
