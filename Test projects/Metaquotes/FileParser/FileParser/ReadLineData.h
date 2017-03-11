#pragma once

enum class WriteLineBufferStatus
{
	// Буфер пуст
	NotWritten,
	// Строка целиком записана в буфер
	Complete,
	// Строка частично записана в буфер
	Incomplete,
	// Буфер переполнен
	Overflown,
	// Буфер переполнен, необходимо найти начало следующей строки
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
    // Буфер строки из файла
	char *WriteLineBuf;

    // Буфер данных из файла
	char *ReadFileBuf;
    
    // Размерность буфера строки
    unsigned int WriteLineBufCapacity;

    // Размерность буфера данных из файла
    unsigned int ReadFileBufCapacity;

    // Текущая позиция считывания из буфера файла
    unsigned int ReadPos;

    // Размер блока, записанного в буфер файла
    unsigned int ReadFileBufSize;

    // Количество байт, записанных в буфер строки
    unsigned int BytesWritten;

    // Статус записи данных в буфер строки
	WriteLineBufferStatus Status;

private:
    bool _isInitialized;
};

