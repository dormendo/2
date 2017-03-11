#pragma once
class CFilterPart
{
public:
    CFilterPart();
    ~CFilterPart();

    // Слово поиска
    char* Word;

    // Размер слова поиска
    unsigned int WordLen;

    // Признак символа '*' перед словом
    bool LeadingAsterisk;

    // Количество символов, которые нужно пропустить перед словом
    unsigned int CharsToSkip;

    // Информация о минимальном количестве символов после данного блока для удовлетворения условиям фильтрации
    unsigned int ExactCharsToEnd;

 
    bool SetWord(const char* filter, const unsigned int startIndex, const unsigned int length);
    bool TestLastPartWord(const char *buf, const int linelen) const;
    bool TestPartWord(const char *buf, const int firstcharindex, const int offset = 0) const;

};