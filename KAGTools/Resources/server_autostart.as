#include "Default/DefaultStart.as"
#include "Default/DefaultLoaders.as"

void Configure()
{
	s_soundon = 0; // disable audio
	v_driver = 0;  // disable video

	if(sv_rconpassword == "")
	{
		sv_rconpassword = "test";
	}
}

void InitializeGame()
{
	print("INTIALIZING TEST SERVER");
	LoadDefaultMapLoaders();
	RunServer();
}
