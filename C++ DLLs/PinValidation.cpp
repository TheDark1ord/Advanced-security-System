#include "pch.h"
#include "PinValidation.h"

bool validate_pin(const char* pin)
{
	const char* actual_pin = "1234";

	if (sizeof(actual_pin) == sizeof(pin))
	{
		for (int i = 0; i < (sizeof(pin) / sizeof(char)); i++)
		{
			if (pin[i] != actual_pin[i])
				return false;
		}
		return true;
	}
	return false;
}

int test(int input)
{
	std::cout << "Hello world!\n";
	return input;
}