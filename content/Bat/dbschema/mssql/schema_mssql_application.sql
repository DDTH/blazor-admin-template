DROP TABLE IF EXISTS [bat_apps];
CREATE TABLE [bat_apps] (
    [app_id] nvarchar(64) NOT NULL,
    [display_name] nvarchar(128) NOT NULL,
    [public_key_pem] nvarchar(max) NULL,
    [created_at] datetimeoffset NOT NULL,
    [updated_at] datetimeoffset NOT NULL,
    [concurrency_stamp] nvarchar(48) NULL,
    CONSTRAINT [PK_bat_apps] PRIMARY KEY ([app_id])
);
