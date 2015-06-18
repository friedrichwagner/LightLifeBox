delete from LLUser where Userid >0 and UserID<1000
delete from LLTestSequenceHead;

DECLARE @cnt INT = 1;
DECLARE @UserID INT = 0;

WHILE @cnt <= 24
BEGIN
	-- Frau < 30
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt + 100, 'Proband', 'In',  100, 'female', Cast ('1.1.1985' as date));
	
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt + 200, 'Proband', 'In',  100, 'female', Cast ('1.1.1985' as date));

	-- Frau > 30
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt +300, 'Proband', 'In' ,  100, 'female', Cast ('1.1.1965' as date));
	
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt + 400, 'Proband', 'In',  100, 'female', Cast ('1.1.1965' as date));

		--Mann < 30
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt +500, 'Proband', '' ,  100, 'male', Cast ('1.1.1985' as date));
	
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt + 600, 'Proband', '',  100, 'male', Cast ('1.1.1985' as date));
	
		--Mann > 50
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt +700, 'Proband', '' ,  100, 'male', Cast ('1.1.1965' as date));
	
	insert into LLUser(UserID, FirstName, LastName,  RoleID, Gender, Birthday)
	values (@UserID +@cnt + 800, 'Proband', '',  100, 'male', Cast ('1.1.1965' as date));	

   SET @cnt = @cnt + 1;
END;

Go


