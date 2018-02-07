#include "Decrypt.h"
#include <string>
#include <iostream>

using namespace std;

XorStr_c XORENGINE;

XorStr_c::XorStr_c()
{
	
}

std::string XorStr_c::XorStr(const char* data, const char* key)
{
	//const char* key = "m";
	std::string str = "";

	for (size_t i = 0; i < strlen(data); i++)
	{
		str += data[i] ^ key[i % strlen(key)];
	}	

	return str;
}