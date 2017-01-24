#pragma once
#include "SdpHead.h"

class SdpDisplayer
{
public:
	explicit SdpDisplayer(std::ostream &os, bool bWithOpt = true) : m_os(os), m_bWithOpt(bWithOpt), m_tab(0) {}

	// redirection
	template <typename T>
	void visit(uint32_t /*tag*/, bool /*require*/, const char *name, const T &val)
	{
		display(name, val);
	}

	// display interface
	template <typename T>
	void display(const T &val)
	{
		display(NULL, val);
	}

	void display(const char *name, bool val)
	{
		pf(name);
		m_os << (val ? "T" : "F") << std::endl;
	}
	void display(const char *name, char val)
	{
		pf(name);
		m_os << (uint8_t)val << std::endl;
	}
	void display(const char *name, int8_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, uint8_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, int16_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, uint16_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, int32_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, uint32_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, int64_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, uint64_t val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, float val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, double val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	void display(const char *name, const std::string &val)
	{
		pf(name);
		m_os << val << std::endl;
	}
	template <typename Alloc>
	void display(const char *name, const std::vector<char, Alloc> &val)
	{
		pf(name);
		m_os.write(&val[0], val.size());
		m_os << std::endl;
	}

	template <typename T, typename Alloc>
	void display(const char *name, const std::vector<T, Alloc> &val)
	{
		pf(name);
		m_os << val.size() << ", [";
		if (!val.empty())
		{
			m_os << std::endl;
			inctab(1);
			for (uint32_t i = 0; i < val.size(); ++i)
			{
				display(val[i]);
			}
			inctab(-1);
			tab();
		}
		m_os << "]" << std::endl;
	}

	template <typename T>
	void display(const char *name, const SdpVectorProxy<T> &val)
	{
		pf(name);
		m_os << val.under.size() << ", [";
		if (val.under.size() != 0)
		{
			m_os << std::endl;
			inctab(1);
			while (val.under.next())
			{
				val.under.visit(*this, 0, true, NULL);
			}
			inctab(-1);
			tab();
		}
		m_os << "]" << std::endl;
	}

	template <typename Key, typename Value, typename Compare, typename Alloc>
	void display(const char *name, const std::map<Key, Value, Compare, Alloc> &val)
	{
		pf(name);
		m_os << val.size() << ", {";
		if (!val.empty())
		{
			m_os << std::endl;
			inctab(1);
			for (typename std::map<Key, Value, Compare, Alloc>::const_iterator first = val.begin(), last = val.end(); first != last; ++first)
			{
				pf(NULL); m_os << "(" << std::endl;
				inctab(1);
				display(first->first);
				display(first->second);
				inctab(-1);
				pf(NULL); m_os << ")" << std::endl;
			}
			inctab(-1);
			tab();
		}
		m_os << "}" << std::endl;
	}

	template <typename T>
	void display(const char *name, const SdpMapProxy<T> &val)
	{
		pf(name);
		m_os << val.under.size() << ", {";
		if (val.under.size() != 0)
		{
			m_os << std::endl;
			inctab(1);
			while (val.under.next())
			{
				pf(NULL); m_os << "(" << std::endl;
				inctab(1);
				val.under.visitKey(*this, 0, true, NULL);
				val.under.visitVal(*this, 0, true, NULL);
				inctab(-1);
				pf(NULL); m_os << ")" << std::endl;
			}
			inctab(-1);
			tab();
		}
		m_os << "}" << std::endl;
	}

	template <typename T>
	void display(const char *name, const T &val)
	{
		if (m_tab == 0 && name == NULL)
		{
			val.visit(*this, m_bWithOpt);
		}
		else
		{
			pf(name); m_os << "{" << std::endl;
			inctab(1);
			val.visit(*this, m_bWithOpt);
			inctab(-1);
			pf(NULL); m_os << "}" << std::endl;
		}
	}

	template <typename T>
	void display(const char *name, const SdpStructProxy<T> &val)
	{
		if (m_tab == 0 && name == NULL)
		{
			val.under.visit(*this, m_bWithOpt);
		}
		else
		{
			pf(name); m_os << "{" << std::endl;
			inctab(1);
			val.under.visit(*this, m_bWithOpt);
			inctab(-1);
			pf(NULL); m_os << "}" << std::endl;
		}
	}

	void inctab(int32_t n)
	{
		m_tab += n;
	}

private:
	void tab()
	{
		for (uint32_t i = 0; i < m_tab; ++i)
		{
			m_os << "\t";
		}
	}
	void pf(const char *name)
	{
		tab();
		if (name)
		{
			m_os << name << ": ";
		}
	}

private:
	std::ostream 	&m_os;
	bool		m_bWithOpt;
	uint32_t	m_tab;
};
