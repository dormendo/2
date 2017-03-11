// FileParser.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "LogReader.h"

int _tmain(int argc, _TCHAR* argv[])
{
	if (argc == 1)
	{
		printf("File name and filter string have not been set\r\n");
		printf("Format: FileParser.exe <filename> <filter> [<linebuffersize>]\r\n");
		return 1;
	}
	if (argc == 2)
	{
		printf("Filter string has not been set\r\n");
		printf("Format: FileParser.exe <filename> <filter> [<linebuffersize>]\r\n");
		return 1;
	}

	char *filename = argv[1];
	char *filter = argv[2];

	int bufsize = 4096;
	int minbufsize = 32;
	int maxbufsize = 1024 * 1024;

	if (argc > 3)
	{
		int parsedsize = atoi(argv[3]);
		if (parsedsize < minbufsize)
		{
			parsedsize = minbufsize;
		}
		else if (parsedsize > maxbufsize)
		{
			parsedsize = maxbufsize;
		}
		bufsize = parsedsize;
	}

	char* buffer = new (std::nothrow) char[bufsize + 1];
    if (buffer == nullptr)
    {
        printf("Can't allocate buffer for log records\r\n");
        return 1;
    }

	memset(buffer, 0, bufsize + 1);

    {
        CLogReader reader;
	
	    if (!reader.SetFilter(filter))
	    {
		    printf("Incorrect filter");
		    return 2;
	    }
	
	    if (!reader.Open(filename))
	    {
		    printf("File open error");
		    return 3;
	    }

	    clock_t start, finish;
	    start = clock();
	    int i = 0;
	    while (reader.GetNextLine(buffer, bufsize))
	    {
		    printf("%s\r\n", buffer);
		    i++;
	    }
        finish = clock();
        printf("%i, %f\r\n", i, static_cast<double>(finish - start) / CLOCKS_PER_SEC);

	    reader.Close();
    }

    if (buffer != nullptr)
    {
	    delete [] buffer;
    }

	return 0;
}

