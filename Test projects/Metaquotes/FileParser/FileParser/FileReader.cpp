#include "stdafx.h"
#include "FileReader.h"
#include "ReadLineData.h"


CFileReader::CFileReader() :
    _hFile(INVALID_HANDLE_VALUE),
    _isEof(false),
    _rlData(nullptr)
{
}


CFileReader::~CFileReader()
{
    this->Close();
}

// Открывает файл для последовательного чтения, создаёт буфер для обработки считанных данных и структуру данных для поддержки считывания
bool CFileReader::Open(const char *filename)
{
    if (filename == nullptr || filename[0] == '\0')
    {
        return false;
    }

    this->_hFile = CreateFile(filename, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING,
        FILE_FLAG_SEQUENTIAL_SCAN, NULL);
    if (this->_hFile == INVALID_HANDLE_VALUE)
    {
        return false;
    }

    if (this->_rlData != nullptr)
    {
        delete this->_rlData;
        this->_rlData = nullptr;
    }

    this->_rlData = new (std::nothrow) CReadLineData(READFILE_BUFFER_SIZE);
    if (this->_rlData == nullptr)
    {
        return false;
    }
    if (!this->_rlData->Initialize())
    {
        return false;
    }

    return true;
}


// Закрывает файл
void CFileReader::Close(void)
{
    if (this->_rlData != nullptr)
    {
        delete this->_rlData;
        this->_rlData = nullptr;
    }

    if (this->_hFile != INVALID_HANDLE_VALUE)
    {
        CloseHandle(this->_hFile);
        this->_hFile = INVALID_HANDLE_VALUE;
    }
}

// Возвращает следующую строку из файла
bool CFileReader::GetNextLine(char* buf, const unsigned int bufsize)
{
    if (buf == nullptr || bufsize == 0
        ||
        this->_hFile == INVALID_HANDLE_VALUE || this->_isEof
        ||
        this->_rlData != nullptr && !this->_rlData->GetInitialized())
    {
        return false;
    }

    ReadLineResult readResult;

    do
    {
        if (!this->_rlData->ResetForNewLine(buf, bufsize))
        {
            return false;
        }

        readResult = this->ReadNextLine();
        if (readResult == ReadLineResult::Success)
        {
            return true;
        }
    }
    while (readResult != ReadLineResult::Eof && readResult != ReadLineResult::Error);

    return false;
}


// Считывает из файла следующую строку 
ReadLineResult CFileReader::ReadNextLine()
{
    while (true)
    {
        if (this->_rlData->StatusNotComplete())
        {
            if (this->_rlData->ReadPos != this->_rlData->ReadFileBufSize)
            {
                this->WriteLineBuffer();

                if (this->_rlData->Status == WriteLineBufferStatus::Complete)
                {
                    return ReadLineResult::Success;
                }
                else if (this->_rlData->Status == WriteLineBufferStatus::Overflown)
                {
                    return ReadLineResult::BufferOverflow;
                }
            }
        }
        else if (this->_rlData->Status == WriteLineBufferStatus::OverflownLineIncomplete)
        {
            if (this->SkipToNextLine())
            {
                return ReadLineResult::BufferOverflow;
            }
        }

        bool readFileResult = this->ReadFileToBuffer();
        if (!readFileResult)
        {
            return ReadLineResult::Error;
        }

        if (this->_isEof)
        {
            if (this->_rlData->Status == WriteLineBufferStatus::Incomplete)
            {
                return (this->_rlData->BytesWritten == 0 ? ReadLineResult::Eof : ReadLineResult::Success);
            }
            else if (this->_rlData->Status == WriteLineBufferStatus::OverflownLineIncomplete)
            {
                return ReadLineResult::BufferOverflow;
            }
        }
    }

    return ReadLineResult::Error;
}

// Записывает данные из файла в буфер строки для дальнейшей обработки
void CFileReader::WriteLineBuffer()
{
    if (this->_rlData->Status == WriteLineBufferStatus::NotWritten)
    {
        // Отсекаем символы \r и \n
        for (; this->_rlData->ReadPos < this->_rlData->ReadFileBufSize; this->_rlData->ReadPos++)
        {
            char ch = this->_rlData->ReadFileBuf[this->_rlData->ReadPos];
            if (ch != CR && ch != LF)
            {
                break;
            }
        }
    }

    this->_rlData->Status = WriteLineBufferStatus::Incomplete;
    char* writebuf = this->_rlData->WriteLineBuf + this->_rlData->BytesWritten;
    const unsigned int writebufcapacity = this->_rlData->WriteLineBufCapacity - this->_rlData->BytesWritten;

    unsigned int searchlimit = this->_rlData->ReadFileBufSize - this->_rlData->ReadPos;
    bool overflowmode = false;
    if (writebufcapacity < searchlimit)
    {
        searchlimit = writebufcapacity;
        overflowmode = true;
    }

    char *bufstartpos = this->_rlData->ReadFileBuf + this->_rlData->ReadPos;
    int copysize = searchlimit;

    for (unsigned int i = 0; i < searchlimit; i++)
    {
        char ch = bufstartpos[i];
        if (ch == CR || ch == LF)
        {
            copysize = i;
            this->_rlData->Status = WriteLineBufferStatus::Complete;
            break;
        }
    }

    this->_rlData->ReadPos += copysize;

    bool writebufferoverflown = false;
    if (overflowmode && this->_rlData->Status != WriteLineBufferStatus::Complete)
    {
        this->_rlData->ReadPos++;
        char ch = this->_rlData->ReadFileBuf[this->_rlData->ReadPos];
        if (ch != CR && ch != LF)
        {
            writebufferoverflown = true;
        }
    }

    if (writebufferoverflown)
    {
        for (; this->_rlData->ReadPos < this->_rlData->ReadFileBufSize; this->_rlData->ReadPos++)
        {
            char ch = this->_rlData->ReadFileBuf[this->_rlData->ReadPos];
            if (ch == CR || ch == LF)
            {
                this->_rlData->Status = WriteLineBufferStatus::Overflown;
                return;
            }
        }

        this->_rlData->Status = WriteLineBufferStatus::OverflownLineIncomplete;
        return;
    }

    memcpy(writebuf, bufstartpos, copysize);
    this->_rlData->BytesWritten += copysize;
}

// Считывает данные из файла в буфер
bool CFileReader::ReadFileToBuffer(void)
{
    if (this->_rlData == nullptr || this->_hFile == INVALID_HANDLE_VALUE)
    {
        return false;
    }

    DWORD bytesread = 0;
    if (!ReadFile(this->_hFile, this->_rlData->ReadFileBuf, this->_rlData->ReadFileBufCapacity, &bytesread, NULL))
    {
        return false;
    }

    this->_rlData->ReadPos = 0;

    if (bytesread == 0)
    {
        this->_isEof = true;
        return false;
    }
    else
    {
        this->_rlData->ReadFileBufSize = bytesread;
    }

    return true;
}

// Достигает начала следующей строки. Используется в случае, если строка не помещается в буфер
// и небходимо пропустить остаток текущей строки
bool CFileReader::SkipToNextLine()
{
    for (; this->_rlData->ReadPos < this->_rlData->ReadFileBufSize; this->_rlData->ReadPos++)
    {
        char ch = this->_rlData->ReadFileBuf[this->_rlData->ReadPos];
        if (ch == CR || ch == LF)
        {
            return true;
        }
    }

    return false;
}
