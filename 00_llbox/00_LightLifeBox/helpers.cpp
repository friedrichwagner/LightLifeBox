#include "helpers.h"

// split: receives a char delimiter; returns a vector of strings
// By default ignores repeated delimiters, unless argument rep == 1.
vector<string>& splitstring::split(char delim, int rep) 
{
    if (!flds.empty()) flds.clear();  // empty vector if necessary
    string work = data();
    string buf = "";
    unsigned int i = 0;
    while (i < work.length()) {
        if (work[i] != delim)
            buf += work[i];
        else if (rep == 1) {
            flds.push_back(buf);
            buf = "";
        } else if (buf.length() > 0) {
            flds.push_back(buf);
            buf = "";
        }
        i++;
    }
    if (!buf.empty())
        flds.push_back(buf);
    return flds;
}

map<string, string>& splitstring::split2map(char delimOuter, char delimInner)
{
	mapdata.clear();
	vector<string> myflds = split(delimOuter); //-->param1=1234
	for (unsigned int i = 0; i < myflds.size(); i++)
	{
		splitstring s = myflds[i];
		vector<string> myflds1 = s.split(delimInner);
		if (myflds1.size() == 2)
		{
			mapdata[myflds1[0]]= myflds1[1];
		}
	}

	return mapdata;
}

