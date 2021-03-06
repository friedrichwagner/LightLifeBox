#########################
#	-o output file
#	-c compile but not link
#	-L, -I include and lib paths
#	-l link a lib
#	-g insert debugging info in your exutable
#	-Wall turn on warnings
#	-fPIC position independent code (dosen't work sometimes)
#	-O optimize
#########################
SDIR0=./00_LightLifeBox
SDIR1=./01_ComClients
SDIR2=./02_DebugServer
SDIR3=./03_LogIni
SDIR4=./04_Platform
SDIR5=./05_Test


ODIR0=$(SDIR0)/obj
ODIR1=$(SDIR1)/obj
ODIR2=$(SDIR2)/obj
ODIR3=$(SDIR3)/obj
ODIR4=$(SDIR4)/obj
ODIR5=$(SDIR5)/obj

#The LightLifeBox files
_SOURCES0=Button.cpp ControlBox.cpp helpers.cpp main.cpp PILight.cpp TastButton.cpp
OBJECTS0=$(addprefix  $(ODIR0)/, $(_SOURCES0:.cpp=.o))

#The ComClient files
#_SOURCES1=baseClient.cpp DaliClient.cpp DMXClient.cpp  serialib.cpp ZLLClient.cpp ftdi.cpp 
_SOURCES1=baseClient.cpp DaliClient.cpp DMXClient.cpp  serialib.cpp ZLLClient.cpp 
SOURCES1=$(addprefix  $(SDIR1)/,$(_SOURCES1))
OBJECTS1=$(addprefix  $(ODIR1)/, $(_SOURCES1:.cpp=.o))

#The Debug Server files
_SOURCES2=DebugServer.cpp tcpacceptor.cpp tcpstream.cpp tcpconnector.cpp
SOURCES2=$(addprefix  $(SDIR2)/,$(_SOURCES2))
OBJECTS2=$(addprefix  $(ODIR2)/, $(_SOURCES2:.cpp=.o))

#The LogIni files
_SOURCES3=IXMLParser.cpp Logger.cpp Settings.cpp
SOURCES3=$(addprefix  $(SDIR3)/,$(_SOURCES3))
OBJECTS3=$(addprefix  $(ODIR3)/, $(_SOURCES3:.cpp=.o))

#The Platform files
_SOURCES4=PlatformCygwin.cpp
SOURCES4=$(addprefix  $(SDIR4)/,$(_SOURCES4))
OBJECTS4=$(addprefix  $(ODIR4)/, $(_SOURCES4:.cpp=.o))

#The Test files
_SOURCES5=IPClient.cpp
SOURCES5=$(addprefix  $(SDIR5)/,$(_SOURCES5))
OBJECTS5=$(addprefix  $(ODIR5)/, $(_SOURCES5:.cpp=.o))

# the diresw/ptestrunctory where the include files are stored
IDIR = -I $(SDIR0) -I $(SDIR1) -I $(SDIR2) -I $(SDIR3) -I $(SDIR4) -I $(SDIR5)

# This is the compiler g++ is for C++ code
CC=g++
# the compiler flags: CYGWIN is my special #define to see if we are on Cygwin or windows or raspberry pi; gnu++11 is necessary to get all the function (std:stoi still does not work)
#CFLAGS=-c -v -I $(IDIR) -Wall -D CYGWIN -Wno-unknown-pragmas -Wno-switch -std=gnu++11 -Wno-write-strings 
#CFLAGS=-c -I $(IDIR) -Wall -D CYGWIN -Wno-unknown-pragmas -Wno-switch -std=gnu++11 -Wno-write-strings 
#CFLAGS=-c $(IDIR) -Wall -D CYGWIN -D _DEBUG -Wno-unknown-pragmas -Wno-switch -std=gnu++11 -Wno-write-strings
CFLAGS=-c $(IDIR) -g -Wall -D CYGWIN -Wno-unknown-pragmas -Wno-switch -std=gnu++11 -Wno-write-strings

# Linker flags:
#LDFLAGS= /home/57/ptestrun/lib/libftd2xx.a
#LDFLAGS=./lib/libftd2xx.a
#LDFLAGS=-lftd2xx
#LDFLAGS=-L./lib -ldl -lrt -lpthread 
#LDFLAGS=-L./lib
#LDFLAGS=-L./lib -Wl,-Bstatic -lftd2xx -Wl,-Bdynamic  -ldl -lrt -lpthread -Wl,--as-needed
#LDFLAGS=-L./lib -lftd2xx -ldl -lrt -lpthread -Wl,--as-needed
LDFLAGS=-L./lib -ldl -lrt -lpthread -Wl,--as-needed

# the name of the exe file
EXECUTABLE=llbox.exe

# target: dependencies
# [tab] system command

# "all" is the default "target", it has no command associated
all: $(EXECUTABLE)

# target is the exe. This is dependant on all the *.o files	
# the command is g++
$(EXECUTABLE): $(OBJECTS0) $(OBJECTS1) $(OBJECTS2) $(OBJECTS3) $(OBJECTS4) $(OBJECTS5)
	$(CC) $(LDFLAGS) $(OBJECTS0) $(OBJECTS1) $(OBJECTS2) $(OBJECTS3) $(OBJECTS4) $(OBJECTS5) -o $@

# this is the actual compile command
# it puts all the *.o files into the ../obj dir
#.cpp.o:
$(ODIR0)/%.o: $(SDIR0)/%.cpp 
	$(CC) $(CFLAGS) $< -o $@

$(ODIR1)/%.o: $(SDIR1)/%.cpp 
	$(CC) $(CFLAGS) $< -o $@

$(ODIR2)/%.o: $(SDIR2)/%.cpp 
	$(CC) $(CFLAGS) $< -o $@
	
$(ODIR3)/%.o: $(SDIR3)/%.cpp 
	$(CC) $(CFLAGS) $< -o $@		
	
$(ODIR4)/%.o: $(SDIR4)/%.cpp 
	$(CC) $(CFLAGS) $< -o $@		
	
$(ODIR5)/%.o: $(SDIR5)/%.cpp 
	$(CC) $(CFLAGS) $< -o $@		
	
# "make clean"  deletes ../obj/*.o files and what is the stuff here
clean:
	rm -f  $(ODIR0)/*.o $(ODIR1)/*.o $(ODIR2)/*.o $(ODIR3)/*.o  $(ODIR4)/*.o  $(ODIR5)/*.o *~ core $(INCDIR)/*~ 
	
