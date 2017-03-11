#pragma once

class CFilterPart;

class CFilterPartCollection
{
public:
    CFilterPartCollection();
    ~CFilterPartCollection();

    bool Initialize(const unsigned int capacity = DEFAULT_CAPACITY);
    bool AddFilterPart(CFilterPart *filterPart);
    unsigned int GetSize(void) const;
    CFilterPart* GetFilterPart(const unsigned int index) const;

private:
    static const unsigned int DEFAULT_CAPACITY = 10;

    bool _isInitialized;
    unsigned int _size;
    unsigned int _capacity;
    CFilterPart ** _collection;

    bool SetCapacity(unsigned int newCapacity);
    void ReleaseFilterParts(void);
};

