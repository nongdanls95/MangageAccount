-- =============================================
-- Author:		Suttikeat Witchayakul
-- Create date: 29 June 2017
-- Description:	
-- =============================================

-- DROP PROCEDURE [brownie].[APP_LOGIN_Q]
-- GO

CREATE PROCEDURE [brownie].[APP_LOGIN_Q]
	@PI_USERNAME nvarchar(50)
	, @PI_PASSWORD nvarchar(100)

	, @PO_STATUS int OUTPUT
    , @PO_STATUS_MSG nvarchar(max) OUTPUT
AS
BEGIN
	BEGIN TRANSACTION;
	BEGIN TRY

		-- == START ==
		SELECT
			USER_ID,
			TITLE,
			NAME,
			SURNAME,
			DEPT1,
			DEPT2,
			DEPT3,
			DEPT4,
			POSITION,
			ROLE_ID,
			ADDRESS,
			TEL,
			EMAIL,
			STATUS,
			IMG
		FROM UM_USER
		WHERE USER_ID=@PI_USERNAME AND PASSWORD=@PI_PASSWORD;

		IF(@@ROWCOUNT = 0)  
			BEGIN
				SET @PO_STATUS = 0;
				SET @PO_STATUS_MSG = 'INVALID_USERNAME_OR_PASSWORD';			
			END
		ELSE
			BEGIN
				SET @PO_STATUS = 1;
				SET @PO_STATUS_MSG = '';
			END
		-- == END ==

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
		ROLLBACK TRANSACTION;
	END CATCH
END