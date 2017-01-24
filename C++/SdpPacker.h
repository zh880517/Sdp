#pragma once
#include "SdpHead.h"

class SdpPacker
{
public:
	void swap(std::string &data) { std::swap(m_data, data); }
	std::string &getData() { return m_data; }
	const std::string &getData() const { return m_data; }

	// redirection
	template <typename T>
	void visit(uint32_t tag, bool require, const char * /*name*/, const T &val)
	{
		pack(tag, require, val);
	}

	// packing interface
	template <typename T>
	void pack(const T &val)
	{
		pack(0, true, val);
	}
	template <typename T>
	void pack(uint32_t tag, const T &val)
	{
		pack(tag, true, val);
	}

	void pack(uint32_t tag, bool /*require*/, bool val)
	{
		pack(tag, (uint32_t)val ? 1 : 0);
	}
	void pack(uint32_t tag, bool /*require*/, char val)
	{
		pack(tag, (uint32_t)val);
	}
	void pack(uint32_t tag, bool /*require*/, int8_t val)
	{
		pack(tag, (int32_t)val);
	}
	void pack(uint32_t tag, bool /*require*/, uint8_t val)
	{
		pack(tag, (uint32_t)val);
	}
	void pack(uint32_t tag, bool /*require*/, int16_t val)
	{
		pack(tag, (int32_t)val);
	}
	void pack(uint32_t tag, bool /*require*/, uint16_t val)
	{
		pack(tag, (uint32_t)val);
	}
	void pack(uint32_t tag, bool /*require*/, int32_t val)
	{
		pack(tag, val);
	}
	void pack(uint32_t tag, bool /*require*/, uint32_t val)
	{
		pack(tag, val);
	}
	void pack(uint32_t tag, bool /*require*/, int64_t val)
	{
		pack(tag, val);
	}
	void pack(uint32_t tag, bool /*require*/, uint64_t val)
	{
		pack(tag, val);
	}
	void pack(uint32_t tag, int32_t val)
	{
		if (val < 0)
		{
			packHeader(tag, SdpPackDataType_Integer_Negative);
			packNumber((uint32_t)-val);
		}
		else
		{
			pack(tag, (uint32_t)val);
		}
	}
	void pack(uint32_t tag, uint32_t val)
	{
		packHeader(tag, SdpPackDataType_Integer_Positive);
		packNumber(val);
	}
	void pack(uint32_t tag, int64_t val)
	{
		if (val < 0)
		{
			packHeader(tag, SdpPackDataType_Integer_Negative);
			packNumber((uint64_t)-val);
		}
		else
		{
			pack(tag, (uint64_t)val);
		}
	}
	void pack(uint32_t tag, uint64_t val)
	{
		packHeader(tag, SdpPackDataType_Integer_Positive);
		packNumber(val);
	}
	void pack(uint32_t tag, bool /*require*/, float val)
	{
		packHeader(tag, SdpPackDataType_Float);
		union { float f; uint32_t i; };
		f = val;
		packNumber(i);
	}
	void pack(uint32_t tag, bool /*require*/, double val)
	{
		packHeader(tag, SdpPackDataType_Double);
		union { double d; uint64_t i; };
		d = val;
		packNumber(i);
	}

	void pack(uint32_t tag, bool /*require*/, const std::string &val)
	{
		packHeader(tag, SdpPackDataType_String);
		packNumber(val.size());
		packData(val.c_str(), (uint32_t)val.size());
	}
	template <typename Alloc>
	void pack(uint32_t tag, bool /*require*/, const std::vector<char, Alloc> &val)
	{
		packHeader(tag, SdpPackDataType_String);
		packNumber(val.size());
		packData(&val[0], val.size());
	}

	template <typename T, typename Alloc>
	void pack(uint32_t tag, bool /*require*/, const std::vector<T, Alloc> &val)
	{
		packHeader(tag, SdpPackDataType_Vector);
		packNumber(val.size());
		for (unsigned i = 0; i < val.size(); ++i)
		{
			pack(val[i]);
		}
	}

	template <typename T>
	void pack(uint32_t tag, bool /*require*/, const SdpVectorProxy<T> &val)
	{
		packHeader(tag, SdpPackDataType_Vector);
		packNumber(val.under.size());
		while (val.under.next())
		{
			val.under.visit(*this, 0, true, NULL);
		}
	}

	template <typename Key, typename Value, typename Compare, typename Alloc>
	void pack(uint32_t tag, bool /*require*/, const std::map<Key, Value, Compare, Alloc> &val)
	{
		packHeader(tag, SdpPackDataType_Map);
		packNumber(val.size());
		for (typename std::map<Key, Value, Compare, Alloc>::const_iterator first = val.begin(), last = val.end(); first != last; ++first)
		{
			pack(first->first);
			pack(first->second);
		}
	}

	template <typename T>
	void pack(uint32_t tag, bool /*require*/, const SdpMapProxy<T> &val)
	{
		packHeader(tag, SdpPackDataType_Map);
		packNumber(val.under.size());
		while (val.under.next())
		{
			val.under.visitKey(*this, 0, true, NULL);
			val.under.visitVal(*this, 0, true, NULL);
		}
	}

	template <typename T>
	void pack(uint32_t tag, bool require, const T &val)
	{
		if (require)
		{
			packHeader(tag, SdpPackDataType_StructBegin);
			val.visit(*this, false); // without optional field with default value
			packHeader(0, SdpPackDataType_StructEnd);
		}
		else
		{
			std::string::size_type size1 = m_data.size();

			packHeader(tag, SdpPackDataType_StructBegin);
			std::string::size_type size2 = m_data.size();

			val.visit(*this, false); // without optional field with default value
			if (size2 == m_data.size())
			{
				m_data.resize(size1);
			}
			else
			{
				packHeader(0, SdpPackDataType_StructEnd);
			}
		}
	}

	template <typename T>
	void pack(uint32_t tag, bool require, const SdpStructProxy<T> &val)
	{
		if (require)
		{
			packHeader(tag, SdpPackDataType_StructBegin);
			val.under.visit(*this, false);
			packHeader(0, SdpPackDataType_StructEnd);
		}
		else
		{
			std::string::size_type size1 = m_data.size();

			packHeader(tag, SdpPackDataType_StructBegin);
			std::string::size_type size2 = m_data.size();

			val.under.visit(*this, false);
			if (size2 == m_data.size())
			{
				m_data.resize(size1);
			}
			else
			{
				packHeader(0, SdpPackDataType_StructEnd);
			}
		}
	}

	// data operation
	void packData(const void *p, uint32_t size)
	{
		m_data.append((const char *)p, size);
	}
	void packHeader(uint32_t tag, SdpPackDataType type)
	{
		uint8_t header = type << 4;
		if (tag < 15)
		{
			header |= tag;
			packData(&header, 1);
		}
		else
		{
			header |= 0xf;
			packData(&header, 1);
			packNumber(tag);
		}
	}
	void packNumber(uint32_t val)
	{
		uint8_t bytes[5];
		uint32_t n = 0;
		while (val > 0x7f)
		{
			bytes[n++] = ((uint8_t)(val) & 0x7f) | 0x80;
			val >>= 7;
		}
		bytes[n++] = (uint8_t)val;
		packData(bytes, n);
	}
	void packNumber(uint64_t val)
	{
		uint8_t bytes[10];
		uint32_t n = 0;
		while (val > 0x7f)
		{
			bytes[n++] = ((uint8_t)(val) & 0x7f) | 0x80;
			val >>= 7;
		}
		bytes[n++] = (uint8_t)val;
		packData(bytes, n);
	}

private:
	std::string m_data;
};
