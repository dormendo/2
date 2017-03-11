#pragma once

enum class WriteLineBufferStatus
{
	// ����� ����
	NotWritten,
	// ������ ������� �������� � �����
	Complete,
	// ������ �������� �������� � �����
	Incomplete,
	// ����� ����������
	Overflown,
	// ����� ����������, ���������� ����� ������ ��������� ������
	OverflownLineIncomplete
};


class CReadLineData
{
public:
    CReadLineData(unsigned int readfilebufcapacity);
	~CReadLineData(void);

	bool Initialize(void);
    bool ResetForNewLine(char* buf, const unsigned int bufsize);
	
	inline bool GetInitialized(void) const
	{
        return this->_isInitialized;
	}
	
	inline bool StatusNotComplete(void) const
	{
        return this->Status == WriteLineBufferStatus::NotWritten || this->Status == WriteLineBufferStatus::Incomplete;
	}

public:
	char *WriteLineBuf;
	char *ReadFileBuf;
    unsigned int WriteLineBufCapacity;
    unsigned int ReadFileBufCapacity;
    unsigned int ReadPos;
    unsigned int ReadFileBufSize;
    unsigned int BytesWritten;
	WriteLineBufferStatus Status;

private:
    bool _isInitialized;
};

