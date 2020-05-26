-- =============================================
-- Author: Nattaphon
-- Create date: 12/9/2017
-- Description:	[APP_TOKEN_I]
-- Data test : 

-- Modified by:
-- Modified date:
-- Description:
-- =============================================

CREATE PROCEDURE [brownie].[APP_TOKEN_I]
	@PI_USER_ID			nvarchar(100) = null,
	@PI_TOKEN			nvarchar(100) = null,

	@PO_STATUS			int output,
    @PO_STATUS_MSG		nvarchar(4000) output
AS
BEGIN
	SET @PO_STATUS = 1;
	SET @PO_STATUS_MSG = '';

	BEGIN TRANSACTION APP_TOKEN_I;	

	BEGIN TRY

	IF((SELECT COUNT(*) FROM APP_TOKEN WHERE USER_ID = @PI_USER_ID) > 0 ) 
		BEGIN
		  UPDATE APP_TOKEN
			SET TOKEN = @PI_TOKEN	
			WHERE USER_ID = @PI_USER_ID
		END
	ELSE 
		BEGIN
			INSERT
			  INTO APP_TOKEN
				(  
				  USER_ID
				  ,TOKEN
				)
				VALUES
				(
				  @PI_USER_ID	
				  ,@PI_TOKEN
				);
		END

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION APP_TOKEN_I;

	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION APP_TOKEN_I;

	END CATCH
END