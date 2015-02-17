insert into LLRole(RoleID, Name, Rights, Remark)
values (0, 'NoName','0000000000', 'Dummy');

insert into LLRole(RoleID, Name, Rights, Remark)
values (1, 'Admin','1111111111', 'Administrator/In');

insert into LLRole(RoleID, Name, Rights, Remark)
values (10, 'Versuchsleiter/In','0000011111', 'Versuchsleiter/In');

insert into LLRole(RoleID, Name, Rights, Remark)
values (100, 'Proband/In','0000000001', 'Versuchsperson');

------------------------------------------------------------

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (0, 'No', 'Name', '1.1.2014', 'androgyn', 0, 'nn', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (1, 'Fritz', 'Wagner', Cast('1970-06-18' as date), 'male', 1, 'fw', 'test')

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (2, 'Hans', 'Hoschopf', '1.1.2014', 'male', 1, 'hh', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (3, 'Versuchs', 'Leiterin1', '1.1.2014', 'female', 10, 'vl1', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (4, 'Versuchs', 'Leiter2', '1.1.2014', 'male', 10, 'vl2', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (5, 'Proband', 'In1', '1.1.2014', 'female', 100, 'pb1', 'test');

insert into LLUser(UserID, FirstName, LastName, Birthday, Gender, RoleID, [Username], [Password])
values (6, 'Proband', '2', '1.1.2014', 'male', 100, 'pb2', 'test');

------------------------------------------------------------

delete from LLRoom;

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (0, 'Dummy Raum', '','');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (1, 'Testraum 1', '','');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (2, 'Testraum 2', '','');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (3, 'Vorraum', '','');

------------------------------------------------------------

insert into LLFixtureType (FTypeID, Name, Remark)
values (1, 'SOVT','Kiteo SOVT Campus');

insert into LLFixtureType (FTypeID, Name, Remark)
values (2, 'AREA','Kiteo K-Aera 60x60');

------------------------------------------------------------

delete from LLGroup;

insert into LLGroup (GroupID, Name, Remark)
values (0, 'Alle','');

insert into LLGroup (GroupID, Name, Remark)
values (1, 'Testraum1','');

insert into LLGroup (GroupID, Name, Remark)
values (2, 'Testraum2','');

insert into LLGroup (GroupID, Name, Remark)
values (3, 'Vorraum','');

insert into LLGroup (GroupID, Name, Remark)
values (10, 'Box T1','');

insert into LLGroup (GroupID, Name, Remark)
values (20, 'Box T2','');

insert into LLGroup (GroupID, Name, Remark)
values (30, 'Box V1','');

insert into LLGroup (GroupID, Name, Remark)
values (40, 'Box Reserve','');


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

insert into LLMsgType(MsgID, MsgName, Remark)
values (10, 'SET_LIGHTS', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (20, 'CALL_SCENE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (30, 'START_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (31, 'STOP_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (32, 'PAUSE_TESTSEQUENCE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (33, 'NEXT_TESTSEQUENCE_STEP', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (34, 'PREV_TESTSEQUENCE_STEP', null);

---------

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (0, 'NO_MODE', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (1, 'SET_BRIGHTNESS', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (2, 'LL_SET_CCT', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (3, 'LL_SET_XY', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (4, 'LL_SET_RGB', null);

insert into LlPILedMode(PiledID, PILEDMode, Remark)
values (99, 'LL_SET_LOCKED', 'Lock current lightstate, used in LightLife Control Boxes');

----------
insert into LLScene (SceneID, SceneName, Brightness, CCT, x, y, pimode, Remark) 
values(0, 'No Scene', 0,0,0,0,0,'');

----------
insert into LLTestSequenceDefinition (SequenceDefID, SequenceName, StepID, Remark) 
values(0, 'No Sequence', 0, null);

----------
insert into LLRoomGroup(RoomID, GroupID) values (1,1);
insert into LLRoomGroup(RoomID, GroupID) values (1,10);

insert into LLRoomGroup(RoomID, GroupID) values (2,2);
insert into LLRoomGroup(RoomID, GroupID) values (2,20);

insert into LLRoomGroup(RoomID, GroupID) values (2,3);
insert into LLRoomGroup(RoomID, GroupID) values (3,30);
insert into LLRoomGroup(RoomID, GroupID) values (3,40);


