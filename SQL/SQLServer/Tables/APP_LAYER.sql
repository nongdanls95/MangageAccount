CREATE TABLE [brownie].[APP_LAYER] (
    [LAYER_ID]       NVARCHAR (50)  NOT NULL,
    [SERVICE_ID]     NVARCHAR (50)  NOT NULL,
    [NAME]           NVARCHAR (250) NULL,
    [INDEX_NO]       BIGINT         NULL,
    [IDENTIFY]       BIGINT         NULL,
    [DISABLE_UM_MGT] NVARCHAR (1)   NULL
);

