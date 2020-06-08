#include "Default/DefaultStart.as"
#include "Default/DefaultLoaders.as"

void Configure()
{
	s_soundon = 1; // sound on
	v_driver = 5;  // default video driver
}

void InitializeGame()
{
	print("INTIALIZING TEST CLIENT");
	LoadDefaultMapLoaders();
	LoadDefaultMenuMusic();
	ConnectLocalhost();
}
