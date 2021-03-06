insert into LLRole(RoleID, Name, Rights, Remark)
values (1, 'Admin','1111111111', 'Administrator/In');

insert into LLRole(RoleID, Name, Rights, Remark)
values (10, 'Versuchsleiter/In','0000011111', 'Versuchsleiter/In');

insert into LLRole(RoleID, Name, Rights, Remark)
values (100, 'Proband/In','0000000001', 'Versuchsperson');

------------------------------------------------------------

insert into LLUser(FirstName, LastName, Birthday, Gender, RoleID)
values ('Fritz', 'Wagner', '18.6.1970', 'male', 1);

insert into LLUser(FirstName, LastName, Birthday, Gender, RoleID)
values ('Hans', 'Hoschopf', '1.1.2014', 'male', 1);

insert into LLUser(FirstName, LastName, Birthday, Gender, RoleID)
values ('Versuchs', 'Leiterin1', '1.1.2014', 'female', 10);

insert into LLUser(FirstName, LastName, Birthday, Gender, RoleID)
values ('Versuchs', 'Leiter2', '1.1.2014', 'male', 10);

insert into LLUser(FirstName, LastName, Birthday, Gender, RoleID)
values ('Proband', 'In1', '1.1.2014', 'female', 100);

insert into LLUser(FirstName, LastName, Birthday, Gender, RoleID)
values ('Proband', '2', '1.1.2014', 'male', 100);

------------------------------------------------------------

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (1, 'Testraum 1', '1:1','CCT=2700&Brightness=50');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (2, 'Testraum 2', '1:1','CCT=2700&Brightness=50');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (3, 'Vorraum', '','CCT=2700&Brightness=50');

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (10, 'Box11', '1:10, Helligkeit', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (11, 'Box12', '1:10, CCT', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (12, 'Box13', '1:10, Judd', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (13, 'Box13', '1:10, All', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (20, 'Box21', '1:10, Helligkeit', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (21, 'Box22', '1:10, CCT', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (22, 'Box23', '1:10, Judd', null);

insert into LLRoom(RoomID, Name, Remark, DefaultSettings)
values (23, 'Box23', '1:10, All', null);

------------------------------------------------------------

insert into LLFixtureType (FTypeID, Name, Remark)
values (1, 'SOVT','Kiteo SOVT Campus');

insert into LLFixtureType (FTypeID, Name, Remark)
values (2, 'AREA','Kiteo K-Aera 60x60');

------------------------------------------------------------

insert into LLGroup (GroupID, Name, Remark)
values (1, 'Gruppe Testraum1','');

insert into LLGroup (GroupID, Name, Remark)
values (2, 'Gruppe Testraum2','');

insert into LLGroup (GroupID, Name, Remark)
values (3, 'Gruppe Vorraum','');

insert into LLGroup (GroupID, Name, Remark)
values (10, 'Gruppe Box11','');

insert into LLGroup (GroupID, Name, Remark)
values (11, 'Gruppe Box12','');

insert into LLGroup (GroupID, Name, Remark)
values (12, 'Gruppe Box13','');

insert into LLGroup (GroupID, Name, Remark)
values (13, 'Gruppe Box14','');

insert into LLGroup (GroupID, Name, Remark)
values (20, 'Gruppe Box21','');

insert into LLGroup (GroupID, Name, Remark)
values (21, 'Gruppe Box22','');

insert into LLGroup (GroupID, Name, Remark)
values (22, 'Gruppe Box23','');

insert into LLGroup (GroupID, Name, Remark)
values (23, 'Gruppe Box24','');

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

