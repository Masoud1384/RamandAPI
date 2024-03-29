USE Ramand;
GO

CREATE PROCEDURE SelectUserByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SELECT u.*, t.Token, t.Expire, t.RefreshToken, t.RefreshTokenExp
    FROM Users u
    LEFT JOIN UserToken t ON u.Id = t.Id
    WHERE u.Username = @Username
END
