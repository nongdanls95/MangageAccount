-- =============================================
-- Author: Piyapan
-- Create date: 12/9/2017
-- Description:	[APP_TOKEN_MATCH]
-- Data test : 

-- Modified by:
-- Modified date:
-- Description:
-- =============================================

CREATE PROCEDURE [brownie].[APP_TOKEN_MATCH]
	@PI_USER_ID			nvarchar(100) = null,
	@PI_TOKEN			nvarchar(100) = null,

	@PO_STATUS			int output,
    @PO_STATUS_MSG		nvarchar(4000) output
AS
BEGIN
	SET @PO_STATUS = 1;
	SET @PO_STATUS_MSG = '';

	BEGIN TRY

	IF((SELECT COUNT(*) FROM APP_TOKEN WHERE USER_ID = @PI_USER_ID AND TOKEN = @PI_TOKEN) > 0 ) 
		BEGIN
			SET @PO_STATUS = 1;
			SET @PO_STATUS_MSG = '';
		END
	ELSE 
		BEGIN
			SET @PO_STATUS = 0;
			SET @PO_STATUS_MSG = 'TOKEN NOT MATCH';
		END

	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());

	END CATCH
END