#pragma once

#include "SdpDisplayer.h"
#include "SdpPacker.h"
#include "SdpUnpacker.h"
template <typename T>
std::string sdpToString(const T &t)
{
	SdpPacker packer;
	packer.pack(t);
	return packer.getData();
}

template <typename T>
void stringToSdp(const std::string &s, T &t)
{
	SdpUnpacker unpacker(s);
	unpacker.unpack(t);
}

template <typename T>
std::string printSdp(const T &t)
{
	ostringstream os;
	SdpDisplayer displayer(os);
	displayer.display(t);
	return os.str();
}

