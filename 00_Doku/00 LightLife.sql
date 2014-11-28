create table LLRole
(
	RoleID int NOT NULL primary key,
	Name varchar(50) not null,
	Rights varchar(20),
	Remark varchar(max),	
	added datetime default getdate()
);

create Table LLUser
(
	UserID int identity NOT NULL primary key,
	FirstName varchar(50) not null,
	LastName varchar(50) not null,
	Birthday datetime not null,
	Gender	varchar(15)  not null,
	Remark varchar(max),
	RoleID int not null,
	added datetime default getdate()
);

create Table LLRoom
(
	RoomID int primary key,
	Name varchar(50) not null,
	DefaultSettings varchar(50),
	Remark varchar(max),
	added datetime default getdate()
);

create Table LLGroup
(
	GroupID int primary key,
	Name varchar(50) not null,
	Remark varchar(max),
	added datetime default getdate()
);

create table LLGroupFixture
(
	GroupID int,
	FixtureID int,
	added datetime default getdate(),
	primary key (GroupID, FixtureID)
);

create table LLFixtureType
(
	FTypeID int not null primary key,
	Name varchar(50) not null,	
	Remark varchar(max),
	added datetime default getdate()
);

create table LLFixture
(
	FixtureID int identity NOT NULL primary key,
	Name varchar(50) not null,	
	DefaultSettings varchar(50),
	FTypeID int not null,
	RoomID int not null,
	Remark varchar(max),
	added datetime default getdate()
);

create table LLScene
(
	SceneID int not null primary key,
	Brightness int not null,
	CCT	int,
	x float , 
	y float,
	pimode int not null,	
	Remark varchar(max),
	added datetime default getdate()
);

create table LLData
(
	DataID int identity NOT NULL primary key,
	RoomID int not null,
	UserID int not null,	
	VLID int not null,
	SceneID int not null,
	SequenceID int not null,
	Brightness int not null,
	CCT	int,
	x float , 
	y float,
	pimode int not null,	
	Remark varchar(max),
	added datetime default getdate()
);

create table LLTestSequenceDefinition
(
	SequenceDefID int not null,
	StepID int not null,
	Remark varchar(max),
	added datetime default getdate(),
	primary key (SequenceDefID,StepID)
);

create table LLTestSequence
(
	SequenceID int identity NOT NULL primary key,
	SequenceDefID int not null,
	StepID int not null,
	Action varchar(50), --START, STOP, PAUSE, RUNNING
	Remark varchar(max),
	added datetime default getdate()
);
















