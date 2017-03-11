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


// Фильтр компилируется в массив блоков. Блок состоит из двух частей: набор операторов и слово.
// Набор операторов включает в себя последовательность символов '*' и '?' и ограничен первым символом,
// не являющимся оператором. Слово - последовательность символов, начинающаяся с любого символа, кроме '*' и '?',
// и завершающаяся символом '*'.
// Так, строка order???new???*?*old*close****** делится на следующие блоки:
// 1. Слово: order???new???
// 2. Операторы: *?*, слово: old
// 3. Операторы: *, слово: close
// 4. Операторы: ******
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

// Заполняет массив блоков фильтра
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

    // Если операторы блока содержат '*', производится поиск первого символа слова вперёд по буферу строки.
    // Для того, чтобы оценить границы возможного поиска, рассчитывается количество символов в строке,
    // строго необходимых по условию фильтра, начиная со слова текущего блока
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


// Проверяет строку на соответствие фильтру
bool CFilter::MatchFilter(const char *buf) const
{
    unsigned int linelen = strlen(buf);
    if (linelen == 0 || linelen < this->_minLineLen)
    {
        return false;
    }

    int lastpartstoskip = 0;

    // Если последний блок фильтра содержит слово, необходимо проверить конец строки на соответствие фильтру
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

        if (part->LeadingAsterisk) // Начало блока содержит символ '*'. Необходим поиск подстроки
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
        else // Начало блока не содержит символа '*'. Возможно простое сравнение.
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
