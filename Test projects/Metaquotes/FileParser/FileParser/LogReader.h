#pragma once

class CFilter;
class CFileReader;


class CLogReader
{
public:
	CLogReader(void);
	~CLogReader(void);

	bool Open(const char *filename);
	void Close();
	bool SetFilter(const char *filter);
	bool GetNextLine(char *buf, const int bufsize);

private:
    // ���������� � �������
    CFilter* _filter;

    // �������� ���������� ����� �� �����
    CFileReader* _fileReader;
};

