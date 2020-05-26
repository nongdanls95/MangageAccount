CREATE TABLE [brownie].[UM_USER] (
    [USER_ID]           NVARCHAR (50)  NOT NULL,
    [PASSWORD]          NVARCHAR (100) NOT NULL,
    [TITLE]             NVARCHAR (50)  NULL,
    [NAME]              NVARCHAR (50)  NULL,
    [SURNAME]           NVARCHAR (250) NULL,
    [DEPT1]             NVARCHAR (50)  NULL,
    [DEPT2]             NVARCHAR (50)  NULL,
    [DEPT3]             NVARCHAR (50)  NULL,
    [DEPT4]             NVARCHAR (50)  NULL,
    [POSITION]          NVARCHAR (50)  NULL,
    [ROLE_ID]           NVARCHAR (50)  NULL,
    [ADDRESS]           NVARCHAR (250) NULL,
    [TEL]               NVARCHAR (50)  NULL,
    [EMAIL]             NVARCHAR (50)  NOT NULL,
    [STATUS]            NVARCHAR(50)       NOT NULL,
    [DATE_CREATED]      DATETIME       NOT NULL,
    [DATE_MODIFIED]     DATETIME       NULL,
    [IMG]               NVARCHAR (250) NULL,
    [PASSWORD_MODIFIED] DATETIME       NULL,
    CONSTRAINT [PK__UM_USER__F3BEEBFF760D96C0] PRIMARY KEY CLUSTERED ([USER_ID] ASC)
);

