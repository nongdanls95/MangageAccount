-- =============================================
-- Author: Piyapan
-- Create date: 18/9/2017
-- Description:	[UM_USER_I]
-- Data test : 

-- Modified by: Pariya
-- Modified date: 14/10/2017
-- Description:
-- =============================================


-- DROP PROCEDURE [brownie].[UM_USER_I]
-- GO

ALTER PROCEDURE [brownie].[UM_USER_I]
	@PI_USER_ID			nvarchar(50) = null,
    --PASSWORD จะต้องเป็นค่า hash นะ
	@PI_PASSWORD		nvarchar(100) = null,
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
	@PI_IMAGE		    nvarchar(250) = null,

	@PO_STATUS			int output,
    @PO_STATUS_MSG		nvarchar(4000) output
AS
BEGIN
	SET @PO_STATUS = 1;
	SET @PO_STATUS_MSG = '';

	BEGIN TRANSACTION UM_USER_I;	

	BEGIN TRY

    IF((SELECT COUNT(*) FROM UM_USER WHERE USER_ID = @PI_USER_ID) > 0 ) 
		BEGIN
		    SET @PO_STATUS = 0;
		    SET @PO_STATUS_MSG = 'USERNAME_ALREADY_EXISTS'
		END
	ELSE 
		BEGIN
            INSERT INTO UM_USER
            (  
                USER_ID,           
                PASSWORD,          
                TITLE,             
                NAME,              
                SURNAME,           
                DEPT1,             
                DEPT2,             
                DEPT3,             
                DEPT4,             
                POSITION,          
                --ROLE_ID,           
                ADDRESS,           
                TEL,               
                EMAIL,             
                STATUS,            
                DATE_CREATED,      
                DATE_MODIFIED,     
                IMAGE,               
                PASSWORD_MODIFIED 
            )
            VALUES
            (
                @PI_USER_ID,           
                @PI_PASSWORD,          
                @PI_TITLE,             
                @PI_NAME,              
                @PI_SURNAME,           
                @PI_DEPT1,             
                @PI_DEPT2,             
                @PI_DEPT3,             
                @PI_DEPT4,             
                @PI_POSITION,          
                --@PI_ROLE_ID,           
                @PI_ADDRESS,           
                @PI_TEL,               
                @PI_EMAIL,             
                'VALID',            
                CURRENT_TIMESTAMP,      
                NULL,     
                @PI_IMAGE,               
                NULL
            );
        END

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION UM_USER_I;

	END TRY
	BEGIN CATCH
		SET @PO_STATUS = 0;
		SET @PO_STATUS_MSG = brownie.FN_GET_ERROR_MESSAGE(ERROR_NUMBER(), (SELECT OBJECT_NAME(@@PROCID)), ERROR_LINE(), ERROR_MESSAGE());
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION UM_USER_I;

	END CATCH
END