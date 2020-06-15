#include "Default/DefaultStart.as"
#include "Default/DefaultLoaders.as"

void Configure()
{
	s_soundon = 0; // disable audio
	v_driver = 0;  // disable video
}

void InitializeGame()
{
	print("INITIALIZING TEST SERVER");
	LoadDefaultMapLoaders();
	//LoadDefaultMenuMusic();
	RunServer();
}
