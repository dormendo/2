// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the SORTALGORITHMS_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SORTALGORITHMS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef SORTALGORITHMS_EXPORTS
#define SORTALGORITHMS_API __declspec(dllexport)
#define EXPIMP_TEMPLATE
#else
#define SORTALGORITHMS_API __declspec(dllimport)
#define EXPIMP_TEMPLATE extern
#endif

#include "stdafx.h"

EXPIMP_TEMPLATE template class SORTALGORITHMS_API std::vector<int>;


class SORTALGORITHMS_API CSortBase
{
public:
	CSortBase(std::vector<int>& v) :
		data(v)
	{
	}

	virtual ~CSortBase(void) = default;

	virtual void Sort(void) = 0;
	virtual void PrintResult(void) const = 0;

protected:
	std::vector<int> data;
};

class SORTALGORITHMS_API CQuickSort : public CSortBase
{
public:
	CQuickSort(std::vector<int>& v) :
		CSortBase(v)
	{
	}

	virtual ~CQuickSort(void) = default;

	virtual void Sort(void);
	virtual void PrintResult(void) const;
};
