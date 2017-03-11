#pragma once
class CFilterPart
{
public:
    CFilterPart();
    ~CFilterPart();

    char* Word;
    unsigned int WordLen;
    bool LeadingAsterisk;
    unsigned int CharsToSkip;
    unsigned int ExactCharsToEnd;

    bool SetWord(const char* filter, const unsigned int startIndex, const unsigned int length);

    bool TestLastPartWord(const char *buf, const int linelen) const;
    bool TestPartWord(const char *buf, const int firstcharindex, const int offset = 0) const;

};