CREATE FUNCTION [brownie].[unixToDateTime] (@Datetime BIGINT) RETURNS DATETIME
AS
BEGIN
    IF(@Datetime > 0)
        BEGIN
            DECLARE @LocalTimeOffset BIGINT
                ,@AdjustedLocalDatetime BIGINT;
            SET @LocalTimeOffset = DATEDIFF(second,GETDATE(),GETUTCDATE())
            SET @AdjustedLocalDatetime = @Datetime - @LocalTimeOffset
            RETURN (SELECT DATEADD(second,@AdjustedLocalDatetime, CAST('1970-01-01 00:00:00' AS datetime)))
        END
	RETURN NULL;
END;
GO