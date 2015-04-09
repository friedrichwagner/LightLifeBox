create table LLRole
(
	RoleID int NOT NULL primary key,
	Name varchar(50) not null,
	Rights varchar(20),
	Remark varchar(max),	
	added datetime default getdate()
);

drop table LLUser;
create Table LLUser
(
	UserID int NOT NULL primary key,
	FirstName varchar(50) not null,
	LastName varchar(50) not null,
	Birthday datetime ,
	Gender	varchar(15) ,
	Remark varchar(max),
	Username varchar(20),
	Password varchar(20),
	RoleID int not null,
	added datetime default getdate()
);

drop table LLUSerInfo;

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
	added datetime default getdate(),
	updated datetime
);

CREATE TRIGGER trUpdateLLUSerInfo on LLUserInfo
   AFTER UPDATE
AS 
BEGIN
	Update LLUserInfo set updated = getdate()
	from inserted i, LLUserInfo a
	where i.DataId = a.DataId
END
GO



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

drop table LlScene;
create table LLScene
(
	SceneID int not null primary key,
	SceneName varchar(50),
	Brightness int not null,
	CCT	int,
	duv	float,
	x float , 
	y float,
	pimode int not null,	
	Remark varchar(max),
	added datetime default getdate()
);

drop table LLMsgType;
create table LLMsgType
(
	MsgID int not null primary key,
	MsgName varchar(30),
	Remark varchar(max),
	added datetime default getdate()
);

drop table LLData;
create table LLData
(
	DataID int identity NOT NULL primary key,
	RoomID int not null,
	GroupID int not null,
	UserID int not null,	
	VLID int not null,
	SceneID int not null,
	SequenceID int not null,
	ActivationID int not null,
	StepID int not null,
	Brightness int not null,
	CCT	int not null,
	duv	float not null,
	x float not null, 
	y float not null,
	fadetime int not null,
	pimode varchar(15) not null,	
	sender varchar(15) not null,
	receiver varchar(15) not null,
	MsgTypeID int not null,	
	Remark varchar(max),
	IP varchar(25),
	added datetime default getdate()
);

drop view V_LLDATA
create view V_LLDATA  
		(	DataID, RoomID, RoomName, GroupID, UserID, Proband, VLID, Versuchsleiter,
			SceneID, SceneName, SequenceID, ActivationID, StepID,
			Brightness, CCT, duv, x, y, pimode, Remark, sender, receiver, MsgTypeID, MsgName, added
		)
as
select d.DataID, d.RoomID, r.Name, d.UserID, (u.LastName + ' ' + u.FirstName), d.GroupID, d.VLID, (v.LastName + ' '+ v.FirstName),
		d.SceneID, s.SceneName, d.SequenceID, d.ActivationID, d.StepID,
		d.Brightness, d.CCT, d.duv, d.x, d.y, d.pimode, d.Remark, d.sender, d.receiver, d.MsgTypeID, m.MsgName, d.added
from LLData d left outer join LLScene s on  d.SceneID = s.SceneID, LLRoom r, LLUser u, llMsgType m, LLUser v
where d.Roomid = r.Roomid
and d.UserId = u.USerId
and d.VLID =v.UserID
and d.MsgTypeId = m.MsgID


drop table LLTestSequenceDefinition;
drop table LLTestSequence;

drop table  LLTestSequenceHead;
create table LLTestSequenceHead
(
	SequenceID int not null,
	BoxID int not null,
	UserID int not null,
	VLID int not null,
	TestStateID int not null,
	ActualPosID int not null default 0,
	Remark varchar(max),
	added datetime default getdate(),
	primary key (SequenceID)
);

drop table  LLTestSequencePos;
create table LLTestSequencePos
(
	PosID int identity not null,
	SequenceID int NOT NULL,
	ActivationID int NOT NULL,
	StepID int not null,
	pimode varchar(20) not null,
	Brightness int not null,
	CCT	int,
	duv float,
	x float , 
	y float,
	Remark varchar(max),
	added datetime default getdate(),
	updated datetime,
	primary key(SequenceID, ActivationID, StepID)
);

CREATE TRIGGER trUpdateLLTestSequencePos on LLTestSequencePos
   AFTER UPDATE
AS 
BEGIN
	Update LLTestSequencePos set updated = getdate()
	from inserted i, LLTestSequencePos a
	where i.PosID  = a.PosID
END
GO


Create TRIGGER [LLTestSequenceHead_Delete] ON LLTestSequenceHead
after delete
AS
begin
	delete from LLTestSequencePos where SequenceID in (select SequenceID from deleted);
END;

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

drop table LLBox;
create Table LLBox
(
	BoxID int primary key,
	BoxIP varchar(15) not null,
	GroupID int not null default 0, --Broadcast to all Lights on Zigbee
	sendPort int not null, -- Port on which the AdminConsole sends RemoteCommands to this Box
	recvPort int not null, -- Port on which the AdminConsole listens to incoming RemoteCommands from this Box
	Name varchar(50) not null,
	active int default 1,
	Remark varchar(max),	
	added datetime default getdate()
);

create table LLActivationState
(
	ActivationID int not null primary key,
	Name varchar(30)
)

create table LLStep
(
	StepID int not null primary key,
	Name varchar(30),
	EnabledButtons varchar(10)
)

create table LLTestSequenceState
(
	StateID int not null primary key,
	Name varchar(30),
)

drop view V_TestSequence 
create view V_TestSequence 
(
BoxID, SequenceID, UserID, TestStateID, StateName, PosID, ActivationID, ActivationName, StepID, StepName, 
pimode, brightness, CCT, duv, added, updated
)
as
select h.SequenceID, h.BoxID, h.UserID, h.TestStateID, st.Name,
		p.PosID, p.ActivationID, a.Name, p.StepiD, s.Name, 
		p.pimode, p.Brightness, p.CCT, p.duv, p.added, p.updated
		from LLTestSequenceHead h, LLTestSequencePos p, LLActivationState a, LLStep s, LlTestSequenceState st
where h.SequenceID = p.SequenceID
and p.ActivationID = a.ActivationID
and p.StepID = s.StepID
and h.TestStateID = st.StateID

create View V_BoxState
as
select b.BoxID, b.Name, b.active,   
v.SequenceID, v.UserID, v.TestStateID, v.StateName, v.PosID, v.ActivationID, v.ActivationName, v.StepID, v.StepName, 
v.pimode, v.brightness, v.CCT, v.duv, v.added, v.updated
from LLBox b 
left outer join V_TestSequence v 
on (b.BoxID = v.BoxID
and v.SequenceID = (select max(SequenceID) from LLTestSequencehead where BoxID=b.BoxID)  
and v.PosID = (select max(PosID) from LLTestSequencePos where SequenceID= v.SequenceId) )


