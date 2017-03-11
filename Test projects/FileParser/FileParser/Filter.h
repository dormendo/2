#pragma once

class CFilterPartCollection;

class CFilter
{
public:
    CFilter();
    ~CFilter();

    bool Initialize(const char* filter);
    bool MatchFilter(const char *buf) const;

    static const char ASTERISK = '*';
    static const char QMARK = '?';

private:
    static bool IsFilterCorrect(const char* filter);

    bool _isInitialized;
    // Минимально необходимая длина строки, удовлетворяющая фильтру
    unsigned int _minLineLen;
    // Коллекция "составных частей" фильтра
    CFilterPartCollection* _collection;

    bool SetFilterParts(const char *filter);
    bool FillFilterPartCollection(const char *filter);
};

