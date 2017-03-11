#include "stdafx.h"
#include "Filter.h"
#include "FilterPartCollection.h"
#include "FilterPart.h"


CFilter::CFilter() :
    _isInitialized(false),
    _minLineLen(0),
    _collection(nullptr)
{
}


CFilter::~CFilter()
{
}

bool CFilter::Initialize(const char* filter)
{
    if (this->_isInitialized)
    {
        return false;
    }
    
    if (!CFilter::IsFilterCorrect(filter))
    {
        return false;
    }

    if (!this->SetFilterParts(filter))
    {
        return false;
    }

    this->_isInitialized = true;
    return true;
}

bool CFilter::IsFilterCorrect(const char* filter)
{
    if (filter == nullptr)
    {
        return false;
    }

    for (const char *ch = filter; *ch != '\0'; ch++)
    {
        if (*ch != ASTERISK && *ch != QMARK)
        {
            return true;
        }
    }

    return false;
}


// ������ ������������� � ������ ������. ���� ������� �� ���� ������: ����� ���������� � �����.
// ����� ���������� �������� � ���� ������������������ �������� '*' � '?' � ��������� ������ ��������,
// �� ���������� ����������. ����� - ������������������ ��������, ������������ � ������ �������, ����� '*' � '?',
// � ������������� �������� '*'.
// ���, ������ order???new???*?*old*close****** ������� �� ��������� �����:
// 1. �����: order???new???
// 2. ���������: *?*, �����: old
// 3. ���������: *, �����: close
// 4. ���������: ******
bool CFilter::SetFilterParts(const char *filter)
{
    if (filter == nullptr)
    {
        return false;
    }

    if (this->_collection != nullptr)
    {
        delete this->_collection;
        this->_collection = nullptr;
    }
    
    this->_collection = new (std::nothrow) CFilterPartCollection;
    if (this->_collection == nullptr)
    {
        return false;
    }

    if (!this->_collection->Initialize())
    {
        return false;
    }

    if (!this->FillFilterPartCollection(filter))
    {
        delete this->_collection;
        this->_collection = nullptr;
        return false;
    }

    return true;
}

// ��������� ������ ������ �������
bool CFilter::FillFilterPartCollection(const char *filter)
{
    int partindex = 0;
    bool asteriskcontext = true;
    
    CFilterPart* part = new (std::nothrow) CFilterPart;
    if (part == nullptr || !this->_collection->AddFilterPart(part))
    {
        return false;
    }

    unsigned int wordstart = -1;
    unsigned int i = 0;

    for (; filter[i] != 0; i++)
    {
        char ch = filter[i];

        if (ch != ASTERISK)
        {
            this->_minLineLen++;
        }

        if (asteriskcontext)
        {
            if (ch == ASTERISK)
            {
                part->LeadingAsterisk = true;
            }
            else if (ch == QMARK)
            {
                part->CharsToSkip++;
            }
            else
            {
                asteriskcontext = false;
                wordstart = i;
            }
        }
        else if (ch == ASTERISK)
        {
            if (!part->SetWord(filter, wordstart, i - wordstart))
            {
                return false;
            }

            asteriskcontext = true;

            part = new (std::nothrow) CFilterPart;
            if (part == nullptr || !this->_collection->AddFilterPart(part))
            {
                return false;
            }

            part->LeadingAsterisk = true;
        }
    }

    if (!asteriskcontext)
    {
        if (!part->SetWord(filter, wordstart, i - wordstart))
        {
            return false;
        }
    }

    // ���� ��������� ����� �������� '*', ������������ ����� ������� ������� ����� ����� �� ������ ������.
    // ��� ����, ����� ������� ������� ���������� ������, �������������� ���������� �������� � ������,
    // ������ ����������� �� ������� �������, ������� �� ����� �������� �����
    unsigned int exactcharssum = 0;
    for (int i = this->_collection->GetSize() - 1; i >= 0; i--)
    {
        CFilterPart *part = this->_collection->GetFilterPart(i);
        exactcharssum += part->WordLen;
        part->ExactCharsToEnd = exactcharssum;
        exactcharssum += part->CharsToSkip;
    }

    return true;
}


// ��������� ������ �� ������������ �������
bool CFilter::MatchFilter(const char *buf) const
{
    unsigned int linelen = strlen(buf);
    if (linelen == 0 || linelen < this->_minLineLen)
    {
        return false;
    }

    int lastpartstoskip = 0;

    // ���� ��������� ���� ������� �������� �����, ���������� ��������� ����� ������ �� ������������ �������
    if (this->_collection->GetSize() > 1)
    {
        int lastparttestoffset = 0;
        CFilterPart* lastpart = this->_collection->GetFilterPart(this->_collection->GetSize() - 1);

        if (lastpart->Word != NULL || lastpart->LeadingAsterisk)
        {
            lastpartstoskip = 1;
        }

        if (lastpart->Word != NULL && !lastpart->TestLastPartWord(buf, linelen))
        {
            return false;
        }
    }

    unsigned int curpos = 0;

    for (unsigned int i = 0; i < this->_collection->GetSize() - lastpartstoskip; i++)
    {
        CFilterPart* part = this->_collection->GetFilterPart(i);
        curpos += part->CharsToSkip;

        if (part->LeadingAsterisk) // ������ ����� �������� ������ '*'. ��������� ����� ���������
        {
            while (true)
            {
                const char* firstcharptr = strchr(buf + curpos, *(part->Word));
                if (firstcharptr == NULL)
                {
                    return false;
                }

                curpos = firstcharptr - buf;
                if (curpos > linelen - part->ExactCharsToEnd)
                {
                    return false;
                }

                if (part->TestPartWord(buf, curpos, 1))
                {
                    break;
                }

                curpos++;
            }

            curpos += part->WordLen;
        }
        else // ������ ����� �� �������� ������� '*'. �������� ������� ���������.
        {
            if (*(buf + curpos) == *(part->Word) && part->TestPartWord(buf, curpos))
            {
                curpos += part->WordLen;
            }
            else
            {
                return false;
            }
        }
    }

    return true;
}
