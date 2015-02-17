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
	UserID int NOT NULL primary key,
	FirstName varchar(50) not null,
	LastName varchar(50) not null,
	Birthday datetime not null,
	Gender	varchar(15)  not null,
	Remark varchar(max),
	Username varchar(20),
	Password varchar(20),
	RoleID int not null,
	added datetime default getdate()
);

create Table LLUserInfo
(
	DataId int identity not null primary key,
	UserID int NOT NULL ,
	HelligkeitsSensitivitaet float,
	Farbempfindlichkeit float,
	Farbwahrnehming varchar(20),
	HelligkeitAugen int,
	Augenfarbe varchar(20),
	Hauttyp varchar(20),
	BMI float,
	Gewicht float,
	GroesseCM float,
	Schlaftyp varchar(20),
	Haendigkeit varchar(20),
	Lateralitaet varchar(20),
	Augendominanz varchar(20),
	Beanspruchungsgrad int,
	Wachheit int,
	Blutdruck_Beginn int,
	Blutdruck_Ende int,
	HRV_Beginn int,
	HRV_Ende int,	
	Antwort1 varchar(50),
	Antwort2 varchar(50),
	Lichtexperte varchar(20),
	Technikaffinität varchar(20),
	InteresseBeleuchtung varchar(20),
	Bedarf varchar(20),
	Bedeutung varchar(20),
	Berufstätigkeit varchar(20),
	Bildungsgrad varchar(20),
	HelligkeitAussen int,
	TemperaturAussen int,
	TemperaturInnen int,
	Luftfeuchtigkeit int,
	BewoelkungAussen varchar(20),
	SchneelageCM int,
	Lerneffekte varchar(20),
	Feld1 varchar(50),
	Feld2 varchar(50),
	Feld3 varchar(50),
	Feld4 varchar(50),
	Feld5 varchar(50),
	Feld6 varchar(50),
	Feld7 varchar(50),
	Feld8 varchar(50),
	Feld9 varchar(50),
	Feld10 varchar(50),
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
	SceneName varchar(50),
	Brightness int not null,
	CCT	int,
	x float , 
	y float,
	pimode int not null,	
	Remark varchar(max),
	added datetime default getdate()
);

create table LLMsgType
(
	MsgID int not null primary key,
	MsgName varchar(20),
	Remark varchar(max),
	added datetime default getdate()
);

create table LLData
(
	DataID int identity NOT NULL primary key,
	RoomID int not null,
	GroupID int not null,
	UserID int not null,	
	VLID int not null,
	SceneID int not null,
	SequenceID int not null,
	Brightness int not null,
	CCT	int,
	x float , 
	y float,
	pimode varchar(15) not null,	
	sender varchar(15) not null,
	receiver varchar(15) not null,
	MsgTypeID int not null,	
	Remark varchar(max),
	IP varchar(25),
	added datetime default getdate()
);

create view V_LLDATA  
		(	DataID, RoomID, RoomName, UserID, Proband, VLID, Versuchsleiter,
			SceneID, SceneName, SequenceID, SequenceName,
			Brightness, CCT, x, y, pimode, Remark, sender, receiver, MsgTypeID, MsgName, added
		)
as
select d.DataID, d.RoomID, r.Name, d.UserID, (u.LastName + ' ' + u.FirstName), d.VLID, (u.LastName + ' '+ u.FirstName),
		d.SceneID, s.SceneName, q.SequenceDefID, q.SequenceName,
		d.Brightness, d.CCT, d.x, d.y, d.pimode, d.Remark, d.sender, d.receiver, d.MsgTypeID, m.MsgName, d.added
from LLData d, LLRoom r, LLUser u, LLScene s, LLTestSequenceDefinition q, llMsgType m
where d.Roomid = r.Roomid
and d.UserId = u.USerId
and d.VLID=u.UserId
and d.SceneID = s.SceneID
and d.SequenceID=q.SequenceDefID
and d.MsgTypeId = m.MsgID

create table LLTestSequenceDefinition
(
	SequenceDefID int not null,
	SequenceName varchar(50),
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
create table LLPILedMode
(
	PIledID int not null primary key,
	PILEDMode varchar(20),
	Remark varchar(max),
	added datetime default getdate()
);


create table LLRoomGroup
(
	RoomID int not null,
	GroupID int not null,
	primary key (RoomId, GroupID)
);















