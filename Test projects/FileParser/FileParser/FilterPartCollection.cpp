#include "stdafx.h"
#include "FilterPartCollection.h"
#include "FilterPart.h"


CFilterPartCollection::CFilterPartCollection() :
_isInitialized(false),
_size(0),
_capacity(0),
_collection(nullptr)
{
}

bool CFilterPartCollection::Initialize(unsigned int capacity)
{
    if (!this->SetCapacity(capacity))
    {
        return false;
    }

    this->_isInitialized = true;
    return true;
}

CFilterPartCollection::~CFilterPartCollection()
{
    this->ReleaseFilterParts();
}

bool CFilterPartCollection::AddFilterPart(CFilterPart *filterPart)
{
    if (!this->_isInitialized || filterPart == nullptr)
    {
        return false;
    }

    if (this->_size == this->_capacity && !this->SetCapacity(this->_capacity * 2))
    {
        return false;
    }

    this->_collection[this->_size] = filterPart;
    this->_size++;

    return true;
}

bool CFilterPartCollection::SetCapacity(const unsigned int newCapacity)
{
    if (this->_capacity == newCapacity)
    {
        return true;
    }

    if (newCapacity < this->_size)
    {
        return false;
    }

    CFilterPart** newCollection = new (std::nothrow) CFilterPart*[newCapacity];
    if (newCollection == nullptr)
    {
        return false;
    }

    if (this->_collection != nullptr && this->_size > 0)
    {
        memcpy(newCollection, this->_collection, this->_size * sizeof(CFilterPart*));
        delete[] this->_collection;
        this->_collection = nullptr;
    }

    this->_collection = newCollection;
    this->_capacity = newCapacity;

    return true;
}

void CFilterPartCollection::ReleaseFilterParts(void)
{
    if (this->_collection != nullptr)
    {
        for (unsigned int i = 0; i < this->_size; i++)
        {
            delete this->_collection[i];
        }

        delete[] this->_collection;
        this->_collection = nullptr;
        this->_size = 0;
        this->_capacity = 0;
    }
}

unsigned int CFilterPartCollection::GetSize(void) const
{
    return this->_size;
}

CFilterPart* CFilterPartCollection::GetFilterPart(const unsigned int index) const
{
    if (!this->_isInitialized || this->_size <= index)
    {
        return nullptr;
    }

    return this->_collection[index];
}
