-- =============================================
-- Author: Piyapan
-- Create date: 18/9/2017
-- Description:	[UM_USER_EXISTS]
-- Data test : 

-- Modified by:
-- Modified date:
-- Description:
-- =============================================


-- DROP PROCEDURE [brownie].[UM_USER_EXISTS]
-- GO

CREATE PROCEDURE [brownie].[UM_USER_EXISTS]
	@PI_USER_ID			nvarchar(50) = null,

	@PO_STATUS			int output,
    @PO_STATUS_MSG		nvarchar(4000) output
AS
BEGIN
	SET @PO_STATUS = 1;
	SET @PO_STATUS_MSG = '';

	BEGIN TRY

    IF((SELECT COUNT(*) FROM UM_USER WHERE USER_ID = @PI_USER_ID) > 0 ) 
		BEGIN
		    SET @PO_STATUS = 1;
		    SET @PO_STATUS_MSG = ''
		END
	ELSE 
		BEGIN
            SET @PO_STATUS = 0;
		    SET @PO_STATUS_MSG = 'USER_NOT_EXIST'
        END

	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
	END CATCH
END