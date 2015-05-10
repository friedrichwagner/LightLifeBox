alter table LLBox add isPracticeBox int not null default 0;
alter table LLBox add ActualSequenceID int not null default 0;

drop table LLTestSequenceDefinition;
create table LLTestSequenceDefinition
(
	SequenceDef varchar(10) not null primary key,
	Name varchar(50) not null,
	StepID1 int not null default 0,
	StepID2 int not null default 0,
	StepID3 int not null default 0,
	StepID4 int not null default 0,
	StepID5 int not null default 0,
	StepID6 int not null default 0,
	Remark varchar(max),
	added datetime default getdate()	
)

drop table  LLTestSequenceHead;
create table LLTestSequenceHead
(
	SequenceID int not null,
	SequenceDef varchar(10) not null,
	BoxID int not null,
	UserID int not null,
	VLID int not null,
	TestStateID int not null,
	ActualPosID int not null default 0,
	Remark varchar(max),
	added datetime default getdate(),
	primary key (SequenceID)
);


Create TRIGGER [LLTestSequenceHead_Delete] ON LLTestSequenceHead
after delete
AS
begin
	delete from LLTestSequencePos where SequenceID in (select SequenceID from deleted);
END;



drop table  LLTestSequencePos;
create table LLTestSequencePos
(
	PosID int identity not null,
	SequenceID int NOT NULL,
	CycleId int not null,
	ActivationID int NOT NULL,
	StepID int not null,
	PILEDID int not null,
	Brightness int not null,
	CCT	int,
	duv float,
	x float , 
	y float,
	Remark varchar(max),
	added datetime default getdate(),
	updated datetime,
	primary key(SequenceID, CycleID, ActivationID, StepID)
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


drop view V_TestSequence 
create view V_TestSequence 
(
BoxID, SequenceID, SequenceDef, UserID, TestStateID, StateName, PosID, ActivationID, ActivationName, StepID, StepName, 
PILEDID, PILEDMode, brightness, CCT, duv, added, updated
)
as
select h.SequenceID, h.SequenceDef, h.BoxID, h.UserID, h.TestStateID, st.Name,
		p.PosID, p.ActivationID, a.Name, p.StepID, s.Name, 
		p.PILEDID, m.PILEDMode, p.Brightness, p.CCT, p.duv, p.added, p.updated
		from LLTestSequenceHead h, LLTestSequencePos p, LLActivationState a, LLStep s, LlTestSequenceState st, LLPILedMode m
where h.SequenceID = p.SequenceID
and p.ActivationID = a.ActivationID
and p.StepID = s.StepID
and h.TestStateID = st.StateID
and p.PILEDID = m.PILEDID


drop view V_BoxState;

create View V_BoxState
as
select b.BoxID, b.Name, b.active, b.isPracticeBox,  
v.SequenceID, v.UserID, v.TestStateID, v.StateName, v.PosID, v.ActivationID, v.ActivationName, v.StepID, v.StepName, 
v.brightness, v.CCT, v.duv, v.added, v.updated
from LLBox b 
left outer join V_TestSequence v 
on (b.BoxID = v.BoxID
and v.SequenceID = (select ActualSequenceID from LLBox where BoxID = b.BoxID)  
and v.PosID = (select ActualPosID from LLTestSequenceHead where SequenceID= v.SequenceId) )



drop table LLData;
create table LLData
(
	DataID int identity NOT NULL primary key,
	RoomID int not null,
	GroupID int not null,
	UserID int not null,	
	VLID int not null,	
	SceneID int not null,
	CycleID int not null,
	SequenceID int not null,
	PosID int not null,
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

create table LLDeltaTestMode
(
	TestModeID int NOT NULL primary key,
	Name varchar(50) not null,
	added datetime default getdate()
);
