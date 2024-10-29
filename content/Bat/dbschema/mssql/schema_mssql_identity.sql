DROP TABLE IF EXISTS [bat_role_claims];
DROP TABLE IF EXISTS [bat_user_claims];
DROP TABLE IF EXISTS [bat_user_roles];
DROP TABLE IF EXISTS [bat_roles];
DROP TABLE IF EXISTS [bat_users];

CREATE TABLE [bat_roles] (
    [role_id] nvarchar(48) NOT NULL,
    [role_name] nvarchar(64) NULL,
    [normalized_name] nvarchar(64) NULL,
    [role_desc] nvarchar(256) NULL,
    [concurrency_stamp] nvarchar(48) NULL,
    CONSTRAINT [PK_bat_roles] PRIMARY KEY ([role_id])
);
CREATE UNIQUE INDEX [RoleNameIndex] ON [bat_roles] ([normalized_name]) WHERE [normalized_name] IS NOT NULL;

CREATE TABLE [bat_users] (
    [uid] nvarchar(48) NOT NULL,
    [given_name] nvarchar(128) NULL,
    [family_name] nvarchar(128) NULL,
    [uname] nvarchar(48) NULL,
    [normalized_name] nvarchar(48) NULL,
    [uemail] nvarchar(100) NULL,
    [normalized_email] nvarchar(100) NULL,
    [password_hash] nvarchar(256) NULL,
    [security_stamp] nvarchar(48) NULL,
    [concurrency_stamp] nvarchar(48) NULL,
    CONSTRAINT [PK_bat_users] PRIMARY KEY ([uid])
);
CREATE UNIQUE INDEX [EmailIndex] ON [bat_users] ([normalized_email]) WHERE [normalized_email] IS NOT NULL;
CREATE UNIQUE INDEX [UserNameIndex] ON [bat_users] ([normalized_name]) WHERE [normalized_name] IS NOT NULL;

CREATE TABLE [bat_role_claims] (
    [role_id] nvarchar(48) NOT NULL,
    [claim_type] nvarchar(32) NOT NULL,
    [claim_value] nvarchar(64) NOT NULL,
    CONSTRAINT [PK_bat_role_claims] PRIMARY KEY ([role_id], [claim_type], [claim_value]),
    CONSTRAINT [FK_bat_role_claims_bat_roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [bat_roles] ([role_id]) ON DELETE CASCADE
);

CREATE TABLE [bat_user_claims] (
    [user_id] nvarchar(48) NOT NULL,
    [claim_type] nvarchar(32) NOT NULL,
    [claim_value] nvarchar(64) NOT NULL,
    CONSTRAINT [PK_bat_user_claims] PRIMARY KEY ([user_id], [claim_type], [claim_value]),
    CONSTRAINT [FK_bat_user_claims_bat_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [bat_users] ([uid]) ON DELETE CASCADE
);

CREATE TABLE [bat_user_roles] (
    [user_id] nvarchar(48) NOT NULL,
    [role_id] nvarchar(48) NOT NULL,
    CONSTRAINT [PK_bat_user_roles] PRIMARY KEY ([user_id], [role_id]),
    CONSTRAINT [FK_bat_user_roles_bat_roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [bat_roles] ([role_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_bat_user_roles_bat_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [bat_users] ([uid]) ON DELETE CASCADE
);
CREATE INDEX [IX_bat_user_roles_role_id] ON [bat_user_roles] ([role_id]);
