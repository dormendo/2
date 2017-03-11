#include "StdAfx.h"
#include "FileReader.h"
#include "LogReader.h"
#include "Filter.h"

CLogReader::CLogReader(void) :
	_filter(nullptr),
	_fileReader(nullptr)
{
}

CLogReader::~CLogReader(void)
{
	this->Close();

    if (this->_filter != nullptr)
    {
        delete this->_filter;
        this->_filter = nullptr;
    }
}

// Открывает файл
bool CLogReader::Open(const char *filename)
{
    this->_fileReader = new (std::nothrow) CFileReader;
    if (this->_fileReader == nullptr)
    {
        return false;
    }

    if (!this->_fileReader->Open(filename))
    {
        return false;
    }

    return true;
}

// Закрывает файл
void CLogReader::Close()
{
    if (this->_fileReader != nullptr)
    {
        delete this->_fileReader;
        this->_fileReader = nullptr;
    }
}

// Устанавливает фильтр
bool CLogReader::SetFilter(const char *filter)
{
    if (filter == nullptr || this->_filter != nullptr)
    {
        return false;
    }

    this->_filter = new (std::nothrow) CFilter;
    if (this->_filter == nullptr)
    {
        return false;
    }

    if (!this->_filter->Initialize(filter))
    {
        delete this->_filter;
        this->_filter = nullptr;
        return false;
    }

    return true;
}

// Записывает в буфер следующую строку, соответствующую фильтру и целиком помещающуюся в буфере
bool CLogReader::GetNextLine(char *buf, const int bufsize)
{
    if (buf == nullptr || bufsize == 0 || this->_filter == nullptr || this->_fileReader == nullptr)
    {
        return false;
    }

    do
    {
        memset(buf, 0, bufsize);
        if (!this->_fileReader->GetNextLine(buf, bufsize))
        {
            return false;
        }
    }
    while (!this->_filter->MatchFilter(buf));

    return true;
}