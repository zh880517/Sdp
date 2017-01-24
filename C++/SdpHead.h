#pragma once
#include <vector>
#include <string>
#include <sstream>
#include <map>
#include <set>
#include <list>
#include <stdexcept>

enum SdpPackDataType
{
	SdpPackDataType_Integer_Positive = 0,
	SdpPackDataType_Integer_Negative = 1,
	SdpPackDataType_Float = 2,
	SdpPackDataType_Double = 3,
	SdpPackDataType_String = 4,
	SdpPackDataType_Vector = 5,
	SdpPackDataType_Map = 6,
	SdpPackDataType_StructBegin = 7,
	SdpPackDataType_StructEnd = 8,
};


struct SdpException : public std::exception
{
	explicit SdpException(const std::string &msg) : sWhat(msg) {}
	virtual ~SdpException() throw() {}
	void trace(uint32_t tag, const char *name)
	{
		if (name)
		{
			if (!sWhat.empty())
			{
				sWhat.append(" <- ");
			}

			char buf[20];
			snprintf(buf, sizeof(buf), "%u", tag);
			sWhat.append(buf);
			sWhat.append(":");
			sWhat.append(name);
		}
	}

	const char *what() const throw() { return sWhat.c_str(); }

	std::string sWhat;
};

#define _SDPUNPACKER_EXCEPT_TRY_ 			try {
#define _SDPUNPACKER_EXCEPT_CATCH_THROW_	} catch (SdpException &e) { e.trace(tag, name); throw; }


template <typename Under>
struct SdpStructProxy
{
	Under under;
};

template <typename Under>
struct SdpVectorProxy
{
	Under under;
};

template <typename Under>
struct SdpMapProxy
{
	Under under;
};

namespace assign
{

	template <typename T1, typename T2>
	struct sdp_assign_from_imp
	{
		sdp_assign_from_imp(T1 &a, const T2 &b)
		{
			a = b;
		}
	};

	template <typename T1, typename T2>
	void sdp_assign_from(T1 &a, const T2 &b)
	{
		sdp_assign_from_imp<T1, T2>(a, b);
	}

	template <typename T1, typename Alloc1, typename T2, typename Alloc2>
	struct sdp_assign_from_imp <std::vector<T1, Alloc1>, std::vector<T2, Alloc2> >
	{
		sdp_assign_from_imp(std::vector<T1, Alloc1> &a, const std::vector<T2, Alloc2> &b)
		{
			a.resize(b.size());
			for (unsigned i = 0; i < a.size(); ++i)
			{
				sdp_assign_from(a[i], b[i]);
			}
		}
	};

	template <typename Key1, typename Value1, typename Compare1, typename Alloc1, typename Key2, typename Value2, typename Compare2, typename Alloc2>
	struct sdp_assign_from_imp <std::map<Key1, Value1, Compare1, Alloc1>, std::map<Key2, Value2, Compare2, Alloc2> >
	{
		sdp_assign_from_imp(std::map<Key1, Value1, Compare1, Alloc1> &a, const std::map<Key2, Value2, Compare2, Alloc2> &b)
		{
			a.clear();
			for (typename std::map<Key2, Value2, Compare2, Alloc2>::const_iterator first = b.begin(), last = b.end(); first != last; ++first)
			{
				std::pair<Key1, Value1> kv;
				sdp_assign_from(kv.first, first->first);
				sdp_assign_from(kv.second, first->second);
				a.insert(kv);
			}
		}
	};

	template <typename T1, typename T2>
	struct sdp_assign_to_imp
	{
		sdp_assign_to_imp(T1 &a, const T2 &b)
		{
			a = b;
		}
	};

	template <typename T1, typename T2>
	void sdp_assign_to(T1 &a, const T2 &b)
	{
		sdp_assign_to_imp<T1, T2>(a, b);
	}


	template <typename T1, typename Alloc1, typename T2, typename Alloc2>
	struct sdp_assign_to_imp <std::vector<T1, Alloc1>, std::vector<T2, Alloc2> >
	{
		sdp_assign_to_imp(std::vector<T1, Alloc1> &a, const std::vector<T2, Alloc2> &b)
		{
			a.resize(b.size());
			for (unsigned i = 0; i < a.size(); ++i)
			{
				sdp_assign_to(a[i], b[i]);
			}
		}
	};

	template <typename Key1, typename Value1, typename Compare1, typename Alloc1, typename Key2, typename Value2, typename Compare2, typename Alloc2>
	struct sdp_assign_to_imp <std::map<Key1, Value1, Compare1, Alloc1>, std::map<Key2, Value2, Compare2, Alloc2> >
	{
		sdp_assign_to_imp(std::map<Key1, Value1, Compare1, Alloc1> &a, const std::map<Key2, Value2, Compare2, Alloc2> &b)
		{
			a.clear();
			for (typename std::map<Key2, Value2, Compare2, Alloc2>::const_iterator first = b.begin(), last = b.end(); first != last; ++first)
			{
				std::pair<Key1, Value1> kv;
				sdp_assign_to(kv.first, first->first);
				sdp_assign_to(kv.second, first->second);
				a.insert(kv);
			}
		}
	};

}


