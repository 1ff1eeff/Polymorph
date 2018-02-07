#pragma once
#include <string>
#include <iostream>
class XorStr_c
{
	public:
		XorStr_c();
		std::string XorStr(const char* data, const char* key);
};
extern XorStr_c XORENGINE;