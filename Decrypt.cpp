#include "Decrypt.h"

#include <iostream>

using namespace std;

XorStr_c XORENGINE;

XorStr_c::XorStr_c()
{
	storage.reserve(STR_VALUE);
}

char* XorStr_c::XorStr(size_t index, size_t size, const char* data)
{
	std::vector< size_t >::iterator it = std::find(ids.begin(), ids.end(), index);
	if (it == ids.end())
	{
		std::string buffer; 
		buffer.resize(size + 1);
		for (int i = 0; i < size; i++)
		{
			char read = data[i];
			//buffer[i] = read ^ key[i % strlen(key)];
			buffer[i] = ( read ^ ( (XORKEY + i) % 0xFF) );
		}
		buffer[size] = '\0';
		ids.push_back(index);
		storage.push_back(StrStorage_s(buffer));
		return (char*)storage.back().s.c_str();
	}

    /*std::string str = "";
	for (size_t i = 0; i < strlen(data); i++)
	{
		str += data[i] ^ key[i % strlen(key)];
	}	
	
	return str;*/	
	
	return (char*)storage[std::distance(ids.begin(), it)].s.c_str();
}