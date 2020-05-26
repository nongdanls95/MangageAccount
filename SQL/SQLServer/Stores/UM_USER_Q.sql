-- =============================================
-- Author:		Suttikeat Witchayakul
-- Create date: 29 June 2017
-- Description:	
-- =============================================

-- DROP PROCEDURE [brownie].[UM_USER_Q]
-- GO

CREATE PROCEDURE [brownie].[UM_USER_Q]
	@PI_USER_ID nvarchar(50)

	, @PO_STATUS int OUTPUT
    , @PO_STATUS_MSG nvarchar(max) OUTPUT
AS
BEGIN
	BEGIN TRY

		-- == START ==
		SELECT
			*
		FROM UM_USER
		WHERE USER_ID=@PI_USER_ID;

		IF(@@ROWCOUNT = 0)  
			BEGIN
				SET @PO_STATUS = 0;
				SET @PO_STATUS_MSG = 'USER_NOT_EXIST';			
			END
		ELSE
			BEGIN
				SET @PO_STATUS = 1;
				SET @PO_STATUS_MSG = '';
			END
	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
	END CATCH
END