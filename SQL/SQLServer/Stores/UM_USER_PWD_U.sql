-- =============================================
-- Author: Piyapan
-- Create date: 18/9/2017
-- Description:	[UM_USER_PWD_U]
-- Data test : 

-- Modified by:
-- Modified date:
-- Description:
-- =============================================


-- DROP PROCEDURE [brownie].[UM_USER_PWD_U]
-- GO

CREATE PROCEDURE [brownie].[UM_USER_PWD_U]
	@PI_USER_ID			nvarchar(50) = null,
    --PASSWORD จะต้องเป็นค่า hash นะ
	@PI_PASSWORD		nvarchar(100) = null,

	@PO_STATUS			int output,
    @PO_STATUS_MSG		nvarchar(4000) output
AS
BEGIN
	SET @PO_STATUS = 1;
	SET @PO_STATUS_MSG = '';

	BEGIN TRANSACTION UM_USER_PWD_U;	

	BEGIN TRY

    IF((SELECT COUNT(*) FROM UM_USER WHERE USER_ID = @PI_USER_ID) = 0 ) 
		BEGIN
		    SET @PO_STATUS = 0;
		    SET @PO_STATUS_MSG = 'USER_NOT_EXIST'
		END
	ELSE 
		BEGIN
			UPDATE UM_USER
			SET
				PASSWORD = @PI_PASSWORD,
				PASSWORD_MODIFIED = CURRENT_TIMESTAMP
			WHERE USER_ID = @PI_USER_ID;
        END

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION UM_USER_PWD_U;

	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION UM_USER_PWD_U;

	END CATCH
END