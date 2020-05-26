CREATE FUNCTION [brownie].[datetimeToUnix] (
@ctimestamp datetime
)
RETURNS integer
AS
BEGIN
  declare @return integer

  SELECT @return = DATEDIFF(SECOND,{d '1970-01-01'}, @ctimestamp)

  return @return
END