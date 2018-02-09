#pragma once

#include <iostream>
#include <algorithm>
#include <vector>

class XorStr_c
{
public:
	XorStr_c();
	struct StrStorage_s
	{
		std::string s;
		StrStorage_s(std::string s)
		{
			this->s = s;
		}
	};
	std::vector< StrStorage_s > storage;
	std::vector< size_t > ids;
	char* XorStr(size_t index, size_t size, const char* data, const char* key);
private:
	const int STR_VALUE = 738;
};
extern XorStr_c XORENGINE;