#pragma once

enum class ReadLineResult
{
    // ��������� ������ ������� �������
    Success,
    // ����� ������ ����������
    BufferOverflow,
    // ����� �����
    Eof,
    // ������ ������
    Error
};


class CReadLineData;

class CFileReader
{
public:
    CFileReader();
    ~CFileReader();
    bool Open(const char *filename);
    void Close(void);
    bool GetNextLine(char* buf, const unsigned int bufsize);

private:
    static const char CR = '\r';
    static const char LF = '\n';
    static const unsigned int READFILE_BUFFER_SIZE = 1024 * 1024;
    
    // ���������� �����
    HANDLE _hFile;
    bool _isEof;
    CReadLineData* _rlData;

    bool ReadFileToBuffer(void);
    ReadLineResult ReadNextLine(void);
    void WriteLineBuffer(void);
    bool SkipToNextLine(void);
};

