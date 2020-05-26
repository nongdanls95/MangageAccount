-- =============================================
-- Author: Piyapan
-- Create date: 18/9/2017
-- Description:	[UM_USER_U]
-- Data test : 

-- Modified by: Pariya
-- Modified date: 14/10/2017
-- Description:
-- =============================================


-- DROP PROCEDURE [brownie].[UM_USER_U]
-- GO

ALTER PROCEDURE [brownie].[UM_USER_U]
	@PI_USER_ID			nvarchar(50) = null,
	@PI_TITLE		    nvarchar(50) = null,
	@PI_NAME		    nvarchar(50) = null,
	@PI_SURNAME		    nvarchar(250) = null,
	@PI_DEPT1		    nvarchar(50) = null,
	@PI_DEPT2		    nvarchar(50) = null,
	@PI_DEPT3		    nvarchar(50) = null,
	@PI_DEPT4		    nvarchar(50) = null,
	@PI_POSITION		nvarchar(50) = null,
	--@PI_ROLE_ID		    nvarchar(50) = null,
	@PI_ADDRESS		    nvarchar(250) = null,
	@PI_TEL		        nvarchar(50) = null,
	@PI_EMAIL		    nvarchar(50) = null,
    @PI_STATUS		    nvarchar(50) = null,
	@PI_IMAGE	        nvarchar(250) = null,

	@PO_STATUS			int output,
    @PO_STATUS_MSG		nvarchar(4000) output
AS
BEGIN
	SET @PO_STATUS = 1;
	SET @PO_STATUS_MSG = '';

	BEGIN TRANSACTION UM_USER_U;	

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
                TITLE = @PI_TITLE,            
                NAME = @PI_NAME,             
                SURNAME = @PI_SURNAME,
                DEPT1 = @PI_DEPT1,            
                DEPT2 = @PI_DEPT2,            
                DEPT3 = @PI_DEPT3,            
                DEPT4 = @PI_DEPT4,            
                POSITION = @PI_POSITION,       
                --ROLE_ID = @PI_ROLE_ID,          
                ADDRESS = @PI_ADDRESS,          
                TEL = @PI_TEL,              
                EMAIL = @PI_EMAIL,            
                STATUS = @PI_STATUS,           
                DATE_MODIFIED = CURRENT_TIMESTAMP,    
                IMAGE = @PI_IMAGE   
            WHERE USER_ID = @PI_USER_ID;
        END

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION UM_USER_U;

	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION UM_USER_U;

	END CATCH
END