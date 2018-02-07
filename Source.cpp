#include <iostream>
#include <string>
#include "Decrypt.h"

using namespace std;

//enc_string_enable

int main()
{
	cout << XORENGINE.XorStr("\x25\x08\x01\x01\x02\x41\x4D\x3A\x02\x1F\x01\x09\x4C","m");
	cout << "Dat lines\n";

	system("PAUSE");

	return 0;
}