#include "StdAfx.h"
#include "ReadLineData.h"


CReadLineData::CReadLineData(unsigned int readfilebufcapacity) :
WriteLineBuf(nullptr),
ReadFileBuf(nullptr),
WriteLineBufCapacity(0),
ReadFileBufCapacity(readfilebufcapacity),
ReadPos(0),
ReadFileBufSize(0),
BytesWritten(0),
Status(WriteLineBufferStatus::NotWritten),
_isInitialized(false)
{	
}


CReadLineData::~CReadLineData(void)
{
    if (this->ReadFileBuf != nullptr)
	{
        delete[] this->ReadFileBuf;
        this->ReadFileBuf = nullptr;
	}
}


bool CReadLineData::Initialize(void)
{
    if (this->ReadFileBufCapacity == 0)
	{
		return false;
	}
	
    this->ReadFileBuf = new (std::nothrow) char[this->ReadFileBufCapacity];
    if (this->ReadFileBuf == nullptr)
	{
		return false;
	}

    this->_isInitialized = true;
	return true;
}


bool CReadLineData::ResetForNewLine(char* buf, const unsigned int bufsize)
{
    if (buf == nullptr || bufsize == 0)
    {
        return false;
    }

    this->WriteLineBuf = buf;
    this->WriteLineBufCapacity = bufsize;

    this->BytesWritten = 0;
    this->Status = WriteLineBufferStatus::NotWritten;

    return true;
}