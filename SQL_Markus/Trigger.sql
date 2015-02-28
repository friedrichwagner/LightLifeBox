
CREATE TRIGGER tr_UserInfo_Update
ON LLUserInfo
FOR UPDATE
AS
BEGIN
	select * from deleted
	select * from inserted
END