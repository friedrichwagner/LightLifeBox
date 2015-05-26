insert into LLRole(RoleID, Name, Rights, Remark)
values (0, 'NoName','0000000000', 'Dummy');

insert into LLRole(RoleID, Name, Rights, Remark)
values (1, 'Admin','1111111111', 'Administrator/In');

insert into LLRole(RoleID, Name, Rights, Remark)
values (10, 'Versuchsleiter/In','0000011111', 'Versuchsleiter/In');

insert into LLRole(RoleID, Name, Rights, Remark)
values (100, 'Proband/In','0000000001', 'Versuchsperson');

------------------------------------------------------------

DECLARE @cnt INT = 1;

WHILE @cnt <= 80
BEGIN
	
	insert into LLUser(UserID, FirstName, LastName,  RoleID)
	values (@cnt, 'Proband', 'In' + Cast(@cnt as varchar(3)),  100);

   SET @cnt = @cnt + 1;
END;

Go



insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (0, 'No', 'Name', '1.1.2014', 'androgyn', 0, 'nn', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (1000, 'Fritz', 'Wagner', Cast('1970-06-18' as date), 'male', 1, 'fw', 'test')

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (1001, 'Hans', 'Hoschopf', '1.1.2014', 'male', 1, 'hh', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (1002, 'Versuchs', 'Leiterin1', '1.1.2014', 'female', 10, 'vl1', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (1003, 'Versuchs', 'Leiter2', '1.1.2014', 'male', 10, 'vl2', 'test');

------------------------------------------------------------

insert into LLFixtureType (FTypeID, Name, Remark)
values (1, 'SOVT','Kiteo SOVT Campus');

insert into LLFixtureType (FTypeID, Name, Remark)
values (2, 'AREA','Kiteo K-Aera 60x60');


------------------------------------------------------------

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT11','Box11', 'CCT=2700&Brightness=50',1, 10);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT12','Box12', 'CCT=2700&Brightness=50',1, 11);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT13','Box13', 'CCT=2700&Brightness=50',1, 12);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT14','Box14', 'CCT=2700&Brightness=50',1, 13);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT21','Box21', 'CCT=2700&Brightness=50',1, 20);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT22','Box22', 'CCT=2700&Brightness=50',1, 21);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT23','Box23', 'CCT=2700&Brightness=50',1, 22);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('SOVT24','Box24', 'CCT=2700&Brightness=50',1, 23);


---------

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA11','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA12','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA13','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA14','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA15','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA16','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA17','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA18','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA19','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA110','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA111','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA112','Testraum 1', 'CCT=2700&Brightness=50',2, 1);

---------

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA21','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA22','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA23','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA24','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA25','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA26','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA27','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA28','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA29','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA210','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA211','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA212','Testraum 2', 'CCT=2700&Brightness=50',2, 1);

---------

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA V1','Vorraum', 'CCT=2700&Brightness=50',2, 3);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA V2','Vorraum', 'CCT=2700&Brightness=50',2, 3);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA V3','Vorraum', 'CCT=2700&Brightness=50',2, 3);

insert into LLFixture (Name, Remark, DefaultSettings, FTypeId, RoomId)
values ('AREA V4','Vorraum', 'CCT=2700&Brightness=50',2, 3);

---------

delete from LLMsgType;
insert into LLMsgType(MsgID, MsgName, Remark)
values (0, 'LL_NONE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (10, 'LL_SET_LIGHTS', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (20, 'LL_CALL_SCENE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (30, 'LL_START_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (31, 'LL_STOP_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (32, 'LL_PAUSE_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (33, 'LL_NEXT_TESTSEQUENCE_STEP', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (34, 'LL_PREV_TESTSEQUENCE_STEP', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (35, 'LL_RELOAD_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (50, 'LL_DISCOVER', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (51, 'LL_ENABLE_BUTTONS', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (52, 'LL_SET_PILED', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (53, 'LL_GET_PILED', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (54, 'LL_SET_SEQUENCEDATA', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (100, 'LL_SET_LOCKED', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (101, 'LL_SET_DEFAULT', null);


---------

delete from LlPILedMode;

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (0, 'NO_MODE', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (1, 'SET_BRIGHTNESS', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (2, 'SET_CCT', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (3, 'SET_XY', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (4, 'SET_RGB', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (5, 'SET_DUV', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (99, 'LL_SET_LOCKED', 'Lock current lightstate, used in LightLife Control Boxes');

----------
insert into LLScene (SceneID, SceneName, Brightness, CCT, x, y, pimode, Remark) 
values(0, 'No Scene', 0,0,0,0,0,'');

----------
insert into LLTestSequenceDefinition (SequenceDefID, SequenceName, StepID, Remark) 
values(0, 'No Sequence', 0, null);

------------------------------------------------------------

delete from LLRoom;

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (0, 'Alle', '','');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (1, 'Testraum 1', '','');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (2, 'Testraum 2', '','');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (3, 'Vorraum', '','');

------------------------------------------------------------

delete from LLGroup;

insert into LLGroup (GroupID, Name, Remark)
values (0, 'Alle','');

insert into LLGroup (GroupID, Name, Remark)
values (200, 'Testraum1','');

insert into LLGroup (GroupID, Name, Remark)
values (201, 'Testraum2','');

insert into LLGroup (GroupID, Name, Remark)
values (202, 'Vorraum','');

insert into LLGroup (GroupID, Name, Remark)
values (203, 'Box T1','');

insert into LLGroup (GroupID, Name, Remark)
values (204, 'Box T2','');

insert into LLGroup (GroupID, Name, Remark)
values (205, 'Box V1','');

insert into LLGroup (GroupID, Name, Remark)
values (206, 'Box Reserve','');
----------

delete from LLRoomGroup;

insert into LLRoomGroup(RoomID, GroupID) values (0,0);
insert into LLRoomGroup(RoomID, GroupID) values (1,200);
insert into LLRoomGroup(RoomID, GroupID) values (1,203);

insert into LLRoomGroup(RoomID, GroupID) values (2,201);
insert into LLRoomGroup(RoomID, GroupID) values (2,204);

insert into LLRoomGroup(RoomID, GroupID) values (3,202);
insert into LLRoomGroup(RoomID, GroupID) values (3,205);

----------
insert into LLBox(BoxID, Name, BOXIP, GroupID, sendPort, recvPort) values (1,'Box Raum 1', '127.0.0.1', 203, 1748, 1749);
insert into LLBox(BoxID, Name, BOXIP, GroupID, sendPort, recvPort) values (2,'Box Raum 2', '127.0.0.1', 204, 1758, 1759);
insert into LLBox(BoxID, Name, BOXIP, GroupID, sendPort, recvPort) values (3,'Box 1 Vorraum', '127.0.0.1', 205, 1768, 1769);
insert into LLBox(BoxID, Name, BOXIP, GroupID, sendPort, recvPort) values (4,'Box 2 Vorraum - Reserve', '127.0.0.1', 206, 1778, 1779);

----------

delete from  LLActivationState;
insert into LLActivationState(ActivationID, Name) values(0, 'none');
insert into LLActivationState(ActivationID, Name) values(1, 'activating');
insert into LLActivationState(ActivationID, Name) values(2, 'relaxing');

----------
delete from LLStep;
insert into LLStep(StepID, Name, EnabledButtons) values (0, 'STEP_STOPPED', '000000000');
insert into LLStep(StepID, Name, EnabledButtons) values (1, 'STEP_BRIGHTNESS', '1000100000');
insert into LLStep(StepID, Name, EnabledButtons) values (2, 'STEP_CCT', '0100100000');
insert into LLStep(StepID, Name, EnabledButtons) values (3, 'STEP_JUDD', '0010100000');
insert into LLStep(StepID, Name, EnabledButtons) values (4, 'STEP_ALL', '1110100000');
insert into LLStep(StepID, Name, EnabledButtons) values (5, 'STEP_ALL_BIG', '1110100000');
insert into LLStep(StepID, Name, EnabledButtons) values (6, 'STEP_DELTATEST', '0001100000');

----------
delete from LLTestSequenceState;
insert into LLTestSequenceState (StateID, Name) values (0, 'NONE');
insert into LLTestSequenceState (StateID, Name) values (10, 'IN_PROGRESS');
insert into LLTestSequenceState (StateID, Name) values (20, 'TESTING');
insert into LLTestSequenceState (StateID, Name) values (30, 'FADING_OUT');
insert into LLTestSequenceState (StateID, Name) values (80, 'PAUSED');
insert into LLTestSequenceState (StateID, Name) values (90, 'STOPPED');
insert into LLTestSequenceState (StateID, Name) values (99, 'FINISHED');

