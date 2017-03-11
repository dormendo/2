#include "stdafx.h"
#include "FilterPart.h"
#include "Filter.h"


CFilterPart::CFilterPart() :
    Word(nullptr),
    WordLen(0),
    LeadingAsterisk(false),
    CharsToSkip(0),
    ExactCharsToEnd(0)
{
}

CFilterPart::~CFilterPart()
{
    if (this->Word != nullptr)
    {
        delete[] this->Word;
        this->Word = nullptr;
    }
}

// Назначает блоку фильтра слово поиска
bool CFilterPart::SetWord(const char* filter, const unsigned int startIndex, const unsigned int length)
{
    if (length == 0)
    {
        return false;
    }

    this->WordLen = length;
    this->Word = new (std::nothrow) char[this->WordLen + 1];
    if (this->Word == nullptr)
    {
        return false;
    }

    memcpy(this->Word, filter + startIndex, this->WordLen);
    this->Word[this->WordLen] = '\0';

    return true;
}

// Проверяет строку на соответствие последнему блоку фильтра в коллекции
bool CFilterPart::TestLastPartWord(const char *buf, const int linelen) const
{
    return this->TestPartWord(buf, linelen - this->WordLen, 0);
}

// Проверяет строку на соответствие блоку фильтра
bool CFilterPart::TestPartWord(const char *buf, const int firstcharindex, const int offset) const
{
    unsigned int wordindex = offset;
    unsigned int lineindex = firstcharindex + offset;

    for (; wordindex < this->WordLen; wordindex++, lineindex++)
    {
        char wordch = this->Word[wordindex];
        char linech = buf[lineindex];

        if (wordch != CFilter::QMARK && wordch != linech)
        {
            return false;
        }
    }

    return true;
}