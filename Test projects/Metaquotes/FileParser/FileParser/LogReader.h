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
    // Информация о фильтре
    CFilter* _filter;

    // Механизм считывания строк из файла
    CFileReader* _fileReader;
};

