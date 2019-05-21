use elight;

CREATE TABLE Sys_Item(
    Id varchar(50) NOT NULL ,
    EnCode varchar(50) NULL,
    ParentId varchar(50) NULL,
    Name varchar(50) NULL,
    Layer int NULL,
    SortCode int NULL,
    IsTree bit NULL,
    DeleteMark bit NULL,
    IsEnabled bit NULL,
    Remark varchar(500) NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
    ModifyUser varchar(50) NULL,
    ModifyTime datetime NULL,
  PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_ItemsDetail(
    Id varchar(50) NOT NULL ,
    ItemId varchar(50) NULL,
    EnCode varchar(50) NULL,
    Name varchar(50) NULL,
    IsDefault bit NULL,
    SortCode int NULL,
    DeleteMark bit NULL,
    IsEnabled bit NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
    ModifyUser varchar(50) NULL,
    ModifyTime datetime NULL,
   PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_Log(
    Id varchar(50) NOT NULL ,
    CreateTime datetime NULL,
    LogLevel varchar(50) NULL,
    Operation varchar(50) NULL,
    Message varchar(500) NULL,
    Account varchar(50) NULL,
    RealName varchar(50) NULL,
    IP varchar(50) NULL,
    IPAddress varchar(50) NULL,
    Browser varchar(50) NULL,
    StackTrace varchar(500) NULL,
 PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_Organize(
    Id varchar(50) NOT NULL ,
    ParentId varchar(50) NULL,
    Layer int NULL,
    EnCode varchar(50) NULL,
    FullName varchar(50) NULL,
    Type smallint NULL,
    ManagerId varchar(50) NULL,
    TelePhone varchar(50) NULL,
    WeChat varchar(50) NULL,
    Fax varchar(50) NULL,
    Email varchar(50) NULL,
    Address varchar(50) NULL,
    SortCode int NULL,
    DeleteMark bit NULL,
    IsEnabled bit NULL,
    Remark varchar(500) NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
    ModifyUser varchar(50) NULL,
    ModifyTime datetime NULL,
  PRIMARY KEY (`Id`)
) ;

CREATE TABLE Sys_Permission(
    Id varchar(50) NOT NULL ,
    ParentId varchar(50) NULL,
    Layer int NULL,
    EnCode varchar(50) NULL,
    Name varchar(50) NULL,
    JsEvent varchar(50) NULL,
    Icon varchar(50) NULL,
    Url varchar(300) NULL,
    Remark varchar(500) NULL,
    Type int NULL,
    SortCode int NULL,
    IsPublic bit NULL  DEFAULT 0,
    IsEnable bit NULL   DEFAULT 1,
    IsEdit bit NULL  DEFAULT 1,
    DeleteMark bit NULL DEFAULT 0,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
    ModifyUser varchar(50) NULL,
    ModifyTime datetime NULL,
 PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_Role(
    Id varchar(50) NOT NULL ,
    OrganizeId varchar(50) NULL,
    EnCode varchar(50) NULL,
    Type smallint NULL,
    Name varchar(50) NULL,
    AllowEdit bit NULL,
    DeleteMark bit NULL,
    IsEnabled bit NULL,
    Remark varchar(500) NULL,
    SortCode int NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
    ModifyUser varchar(50) NULL,
    ModifyTime datetime NULL,
  PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_RoleAuthorize(
    Id varchar(50) NOT NULL ,
    RoleId varchar(50) NULL,
    ModuleId varchar(50) NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
 PRIMARY KEY (`Id`)
) ;

CREATE TABLE Sys_User(
    Id varchar(50) NOT NULL ,
    Account varchar(50) NULL,
    RealName varchar(50) NULL,
    NickName varchar(50) NULL,
    Avatar varchar(200) NULL,
    Gender bit NULL,
    Birthday datetime NULL,
    MobilePhone varchar(20) NULL,
    Email varchar(50) NULL,
    Signature varchar(500) NULL,
    Address varchar(500) NULL,
    CompanyId varchar(50) NULL,
    DepartmentId varchar(50) NULL,
    IsEnabled bit NULL,
    SortCode int NULL,
    DeleteMark bit NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
    ModifyUser varchar(50) NULL,
    ModifyTime datetime NULL,
 PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_UserLogOn(
    Id varchar(50) NOT NULL ,
    UserId varchar(50) NULL,
    Password varchar(50) NULL,
    SecretKey varchar(50) NULL,
    PrevVisitTime datetime NULL,
    LastVisitTime datetime NULL,
    ChangePwdTime datetime NULL,
    LoginCount int NOT NULL   DEFAULT 0,
    AllowMultiUserOnline bit NULL,
    IsOnLine bit NULL,
    Question varchar(100) NULL,
    AnswerQuestion varchar(200) NULL,
    CheckIPAddress bit NULL,
    Language varchar(50) NULL,
    Theme varchar(50) NULL,
 PRIMARY KEY (`Id`)
) ;


CREATE TABLE Sys_UserRoleRelation(
    Id varchar(50) NOT NULL ,
    UserId varchar(50) NULL,
    RoleId varchar(50) NULL,
    CreateUser varchar(50) NULL,
    CreateTime datetime NULL,
 PRIMARY KEY (`Id`)
) ;


INSERT Sys_Item (Id, EnCode, ParentId, Name, Layer, SortCode, IsTree, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'xueli', N'8238c495-8376-4004-9a34-56d0dcbd11ea', N'学历', 1, 3, NULL, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-05-13 19:14:25.013' AS DateTime));
INSERT Sys_Item (Id, EnCode, ParentId, Name, Layer, SortCode, IsTree, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'7b247f60-4095-4ffe-96e0-1935a25852de', N'hunyin', N'8238c495-8376-4004-9a34-56d0dcbd11ea', N'婚姻', 1, 4, NULL, 0, 1, NULL, NULL, NULL, NULL, NULL);
INSERT Sys_Item (Id, EnCode, ParentId, Name, Layer, SortCode, IsTree, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'8238c495-8376-4004-9a34-56d0dcbd11ea', N'all_items', N'0', N'数据字典', 0, 0, NULL, 0, 1, NULL, NULL, NULL, NULL, NULL);
INSERT Sys_Item (Id, EnCode, ParentId, Name, Layer, SortCode, IsTree, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'9c51a17c-7afd-4986-bfc9-94f9dd818ecf', N'role_type', N'8238c495-8376-4004-9a34-56d0dcbd11ea', N'角色类型', 1, 1, NULL, 0, 1, NULL, NULL, NULL, NULL, NULL);
INSERT Sys_Item (Id, EnCode, ParentId, Name, Layer, SortCode, IsTree, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'd2f966ba-d541-4ac9-8837-b5303d5c3502', N'org_type', N'8238c495-8376-4004-9a34-56d0dcbd11ea', N'机构类型', 1, 2, NULL, 0, 1, NULL, NULL, NULL, NULL, NULL);
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'14f0c64a-f3d8-439d-bc0a-d9a5a41a2d46', N'd2f966ba-d541-4ac9-8837-b5303d5c3502', N'org-team', N'小组', 0, 4, 0, 1, NULL, NULL, N'admin', CAST(N'2017-07-12 11:00:47.137' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'16c3d367-d63e-4426-9745-ed6824e8454d', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'shuoshi', N'硕士', 0, 7, 0, 1, N'admin', CAST(N'2017-04-29 16:49:54.183' AS DateTime), N'admin', CAST(N'2017-04-29 16:49:54.183' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'557427ff-8bb7-4e8b-ba3d-91f31ab02b59', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'xiaoxue', N'小学及以下', 0, 1, 0, 1, N'admin', CAST(N'2017-04-29 16:44:34.410' AS DateTime), N'admin', CAST(N'2017-04-29 16:50:15.873' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'738aee95-3597-412e-9a0a-e7e3161c86cf', N'9c51a17c-7afd-4986-bfc9-94f9dd818ecf', N'role-business', N'业务角色', 1, 2, 0, 1, NULL, NULL, N'admin', CAST(N'2017-06-03 17:38:50.330' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'7c51742f-fed3-48c4-8c5b-7f8b8c64cff0', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'benke', N'本科', 1, 5, 0, 1, N'admin', CAST(N'2017-04-29 16:46:24.133' AS DateTime), N'admin', CAST(N'2017-04-29 16:50:25.883' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'85d02da8-06f2-4fba-9dcf-7e3b971f9028', N'd2f966ba-d541-4ac9-8837-b5303d5c3502', N'org-company', N'公司', 1, 1, 0, 1, NULL, NULL, N'admin', CAST(N'2017-06-03 17:40:04.350' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'85e46a33-b065-4ba2-99da-c02947bfc5e6', N'd2f966ba-d541-4ac9-8837-b5303d5c3502', N'org-department', N'部门', 0, 2, 0, 1, NULL, NULL, NULL, NULL);
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'ac53424f-adbb-4477-b534-b0bc72ea5f41', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'chuzhong', N'初中', 0, 2, 0, 1, N'admin', CAST(N'2017-04-29 16:44:56.470' AS DateTime), N'admin', CAST(N'2017-04-29 16:44:56.470' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'C52CBE29-CB92-465F-9697-2AAB7C214FFD', N'd2f966ba-d541-4ac9-8837-b5303d5c3502', N'org-child-dept', N'子部门', 0, 3, 0, 1, N'admin', CAST(N'2017-07-12 11:00:40.667' AS DateTime), N'admin', CAST(N'2017-07-12 11:00:40.667' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'cb579de4-b816-435f-aaa5-f666a6838ca5', N'9c51a17c-7afd-4986-bfc9-94f9dd818ecf', N'role-system', N'系统角色', 0, 1, 0, 1, NULL, NULL, NULL, NULL);
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'cf5d4197-678f-47b9-8f35-ffc23ba68cee', N'9c51a17c-7afd-4986-bfc9-94f9dd818ecf', N'role-other', N'其他角色', 0, 3, 0, 1, NULL, NULL, NULL, NULL);
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'd327c3ca-a557-4f95-8bbf-659fcf09782d', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'dazhuan', N'大专', 0, 4, 0, 1, N'admin', CAST(N'2017-04-29 16:45:27.437' AS DateTime), N'admin', CAST(N'2017-04-29 16:45:27.437' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'f500ed63-e91a-40a5-8e80-6b58895007d3', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'yanjiusheng', N'研究生', 0, 6, 0, 1, N'admin', CAST(N'2017-04-29 16:46:45.813' AS DateTime), N'admin', CAST(N'2017-04-29 16:46:45.813' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'f51b746e-476a-4e39-839f-abed4be676cf', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'gaozhong', N'高中', 0, 3, 0, 1, N'admin', CAST(N'2017-04-29 16:45:06.660' AS DateTime), N'admin', CAST(N'2017-04-29 16:45:06.660' AS DateTime));
INSERT Sys_ItemsDetail (Id, ItemId, EnCode, Name, IsDefault, SortCode, DeleteMark, IsEnabled, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'fff309f2-9baa-4283-84a8-74c97fcd83e2', N'0e9a3b52-1cfc-41a4-8f6d-3ed8b321aecf', N'boshi', N'博士', 0, 8, 0, 0, N'admin', CAST(N'2017-04-29 16:50:10.027' AS DateTime), N'admin', CAST(N'2017-06-05 18:30:48.030' AS DateTime));

INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'25fa48f8-00d3-4b5d-bee9-b49324410906', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', NULL, N'market', N'市场部', 1, NULL, NULL, NULL, NULL, NULL, NULL, 20, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-04-04 17:59:32.370' AS DateTime));
INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'2a871804-5c78-481f-a167-7874b56a54d7', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', NULL, N'afterSale', N'售后部', 1, NULL, NULL, NULL, NULL, NULL, NULL, 70, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-07-12 10:49:09.703' AS DateTime));
INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'339a409a-a5a6-49b4-9071-86d7699a9ddd', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', NULL, N'administration', N'行政人事部', 1, NULL, NULL, NULL, NULL, NULL, NULL, 40, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-07-12 10:48:56.980' AS DateTime));
INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'5fc64d6e-d790-4f53-ab51-99c369860965', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', NULL, N'financial', N'财务部', 1, NULL, NULL, NULL, NULL, NULL, NULL, 50, 0, 0, NULL, NULL, NULL, N'admin', CAST(N'2017-07-12 10:49:03.823' AS DateTime));
INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'67ccf447-0f20-4cf8-9159-a5552cf7dc10', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', NULL, N'project', N'项目部', 1, NULL, NULL, NULL, NULL, NULL, NULL, 80, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-04-04 18:00:44.433' AS DateTime));
INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', N'0', NULL, N'company', N'XX科技股份有限公司', 0, NULL, NULL, NULL, NULL, NULL, NULL, 10, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-07-12 10:48:45.380' AS DateTime));
INSERT Sys_Organize (Id, ParentId, Layer, EnCode, FullName, Type, ManagerId, TelePhone, WeChat, Fax, Email, Address, SortCode, DeleteMark, IsEnabled, Remark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'a93c66e2-b8dc-4d00-84ed-e6071b5f5318', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', NULL, N'product', N'产品事业部', 1, NULL, NULL, NULL, NULL, NULL, NULL, 30, 0, 1, NULL, NULL, NULL, N'admin', CAST(N'2017-04-04 17:59:39.780' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'026550fd-2578-42ae-a041-625cda12325f', N'855f3590-b233-4224-aaff-47fb95c8353d', 2, N'role-add', N'新增角色', N'btn_add()', N'
fa fa-plus-square-o', N'/System/Role/Form', NULL, 1, 10301, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:30:21.277' AS DateTime), N'admin', CAST(N'2017-03-28 16:30:21.277' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'069f00f6-2a82-4bbe-90d6-418f37d5ef1f', N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', 2, N'item-detail', N'查看选项', N'btn_detail()', N'fa fa-eye', N'/System/ItemsDetail/Detail', NULL, 1, 10505, 0, 1, 1, 0, N'admin', CAST(N'2017-04-04 20:16:02.347' AS DateTime), N'admin', CAST(N'2017-04-04 20:18:13.513' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'086ee328-5a15-40b0-8e15-291093e2e8b1', N'09157352-1252-4964-8fee-479759a95db8', 2, N'org-edit', N'修改机构', N'btn_edit()', N'fa fa-pencil-square-o', N'/System/Organize/Form', NULL, 1, 10402, 0, 1, 1, 0, N'admin', CAST(N'2017-04-02 09:38:32.333' AS DateTime), N'admin', CAST(N'2017-04-02 09:38:32.333' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'09157352-1252-4964-8fee-479759a95db8', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', 1, N'sys-organize', N'组织机构', NULL, N'fa fa-building', N'/System/Organize/Index', NULL, 0, 10400, 0, 1, 1, 0, N'admin', CAST(N'2017-04-02 09:31:00.937' AS DateTime), N'admin', CAST(N'2017-04-02 16:45:06.263' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'0d2ea3c9-5b29-4bb6-9f91-0322419ded8e', N'e5346fa2-76ec-498f-8f54-3b443959335a', 2, N'per-delete', N'删除权限', N'btn_delete()', N'fa fa-trash-o', N'/System/Permission/Delete', NULL, 1, 10203, 0, 1, 1, 0, N'admin', CAST(N'2017-02-20 09:51:18.693' AS DateTime), N'admin', NULL);
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'216d09a8-575f-43d1-85f6-acc025fa94b3', N'6d90439c-eb6b-4521-ab4d-5e481406a861', 2, N'user-detail', N'查看用户', N'btn_detail()', N'fa fa-eye', N'/System/User/Detail', NULL, 1, 10104, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:20:17.830' AS DateTime), N'admin', CAST(N'2017-03-28 16:20:17.830' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'233e50fd-4860-42f9-aa7a-93853ac0434b', N'277c8647-ea81-42cf-8f7b-db353da95bbe', 1, N'data-backup', N'数据备份', NULL, N'fa fa-list', N'/System/Data/Index', NULL, 0, 20100, 0, 1, 1, 0, N'admin', NULL, N'admin', CAST(N'2017-07-12 10:50:53.657' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'277c8647-ea81-42cf-8f7b-db353da95bbe', N'0', 0, NULL, N'系统安全', NULL, N'fa fa-desktop', NULL, NULL, 0, 20000, 0, 1, 1, 0, N'admin', NULL, N'admin', NULL);
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'28a045a6-61f4-4784-8578-837ad307e4e3', N'e5346fa2-76ec-498f-8f54-3b443959335a', 2, N'per-add', N'新增权限', N'btn_add()', N'
fa fa-plus-square-o', N'/System/Permission/Form', NULL, 1, 10201, 0, 1, 1, 0, N'admin', CAST(N'2017-02-13 14:28:21.813' AS DateTime), N'admin', NULL);
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'2c24cdfc-8f26-4947-bcb2-0cb4d9111e80', N'e5346fa2-76ec-498f-8f54-3b443959335a', 2, N'per-detail', N'查看权限', N'btn_detail()', N'fa fa-eye', N'/System/Permission/Detail', NULL, 1, 10204, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:22:05.590' AS DateTime), N'admin', CAST(N'2017-03-28 16:22:05.590' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'2d0b02db-09f7-4404-bbdd-c8a516f48288', N'0', 0, NULL, N'系统管理', NULL, N'fa fa-cubes', NULL, NULL, 0, 10000, 0, 1, 1, 0, N'admin', NULL, N'admin', NULL);
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'328b5383-79be-4b34-b57a-49fa3ebc7803', N'855f3590-b233-4224-aaff-47fb95c8353d', 2, N'role-delete', N'删除角色', N'btn_delete()', N'fa fa-trash-o', N'/System/Role/Delete', NULL, 1, 10303, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:32:43.533' AS DateTime), N'admin', CAST(N'2017-03-28 16:32:43.533' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', 1, N'lay-item', N'数据字典', NULL, N'fa fa-sitemap', N'/System/ItemsDetail/Index', NULL, 0, 10500, 0, 1, 1, 0, N'admin', CAST(N'2017-04-03 15:33:02.587' AS DateTime), N'admin', CAST(N'2017-04-03 15:33:02.587' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'3de13971-a51f-40f7-be40-eb035b7f0fae', N'6d90439c-eb6b-4521-ab4d-5e481406a861', 2, N'user-edit', N'修改用户', N'btn_edit()', N'fa fa-edit', N'/System/User/Edit', NULL, 1, 10102, 0, 1, 1, 0, N'admin', CAST(N'2017-04-14 17:21:43.573' AS DateTime), N'admin', CAST(N'2017-06-05 10:48:07.950' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'5fe0cee6-0452-493d-9b55-ff23a5da5e2d', N'e5346fa2-76ec-498f-8f54-3b443959335a', 2, N'per-edit', N'修改权限', N'btn_edit()', N'fa fa-pencil-square-o', N'/System/Permission/Form', NULL, 1, 10202, 0, 1, 1, 0, N'admin', CAST(N'2017-02-20 09:47:19.040' AS DateTime), N'admin', NULL);
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'625cf550-4aad-4158-aff4-2a63d4f25819', N'855f3590-b233-4224-aaff-47fb95c8353d', 2, N'role-detail', N'查看角色', N'btn_detail()', N'fa fa-eye', N'/System/Role/Detail', NULL, 1, 10304, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:34:05.080' AS DateTime), N'admin', CAST(N'2017-03-28 16:34:05.080' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'6d90439c-eb6b-4521-ab4d-5e481406a861', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', 1, N'sys-user', N'系统用户', NULL, N'fa fa-user-circle', N'/System/User/Index', NULL, 0, 10100, 0, 1, 1, 0, N'admin', NULL, N'admin', CAST(N'2017-06-09 16:21:22.800' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'752c9d3f-a744-42ba-87a2-79849fc3fc66', N'6d90439c-eb6b-4521-ab4d-5e481406a861', 2, N'user-delete', N'删除用户', N'btn_delete()', N'fa fa-trash-o', N'/System/User/Delete', NULL, 1, 10103, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:18:25.347' AS DateTime), N'admin', CAST(N'2017-03-28 16:18:25.347' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'7ae2e6aa-0433-4eaa-9357-1adec2507345', N'aeeb56d1-5f27-42df-9d34-97ac18078390', 2, N'log-delete', N'删除日志', N'btn_delete()', N'fa fa-trash-o', N'/System/Log/Delete', NULL, 1, 10601, 0, 1, 0, 0, N'admin', CAST(N'2017-04-19 13:21:33.503' AS DateTime), N'admin', CAST(N'2017-04-19 13:22:35.420' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'81d1cbf0-3cff-4cde-8128-7d0d844450de', N'855f3590-b233-4224-aaff-47fb95c8353d', 2, N'role-authorize', N'角色授权', N'btn_authorize()', N'fa fa-hand-pointer-o', N'/System/RoleAuthorize/Index', NULL, 1, 10305, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:36:42.613' AS DateTime), N'admin', CAST(N'2017-03-28 16:36:42.613' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'82b06e80-103e-4a38-b171-740d2b0e194b', N'09157352-1252-4964-8fee-479759a95db8', 2, N'org-add', N'新增机构', N'btn_add()', N'fa fa-plus-square-o', N'/System/Organize/Form', NULL, 1, 10401, 0, 1, 1, 0, N'admin', CAST(N'2017-04-02 09:37:47.900' AS DateTime), N'admin', CAST(N'2017-04-02 09:37:47.900' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'85438f3b-0634-4b17-b778-aee3a5819669', N'855f3590-b233-4224-aaff-47fb95c8353d', 2, N'role-edit', N'修改角色', N'btn_edit()', N'fa fa-pencil-square-o', N'/System/Role/Form', NULL, 1, 10302, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:31:10.750' AS DateTime), N'admin', CAST(N'2017-03-28 16:31:10.750' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'855f3590-b233-4224-aaff-47fb95c8353d', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', 1, N'sys-role', N'角色管理', NULL, N'fa fa-users', N'/System/Role/Index', NULL, 0, 10300, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:27:50.183' AS DateTime), N'admin', CAST(N'2017-03-28 16:54:58.910' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'87f0aa68-fa57-43cb-84d0-e979fc4af24c', N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', 2, N'item-delete', N'删除选项', N'btn_delete()', N'fa fa-trash-o', N'/System/ItemsDetail/Delete', NULL, 1, 10504, 0, 1, 1, 0, N'admin', CAST(N'2017-04-04 20:06:34.753' AS DateTime), N'admin', CAST(N'2017-04-04 20:17:29.043' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'aeeb56d1-5f27-42df-9d34-97ac18078390', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', 1, N'sys-log', N'操作日志', NULL, N'fa fa-folder-open', N'/System/Log/Index', NULL, 0, 10600, 0, 1, 0, 0, N'admin', CAST(N'2017-04-18 13:25:49.407' AS DateTime), N'admin', CAST(N'2017-04-19 13:22:14.603' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'c04bfd8f-7e2e-4312-9148-a2e14007fa46', N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', 2, N'item-edit', N'修改选项', N'btn_edit()', N'fa fa-pencil-square-o', N'/System/ItemsDetail/Form', NULL, 0, 10503, 0, 1, 1, 0, N'admin', CAST(N'2017-04-04 20:05:36.310' AS DateTime), N'admin', CAST(N'2017-04-04 20:05:36.310' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'cd4e9f8b-f56a-42dc-94e1-b76f3d0b38fc', N'09157352-1252-4964-8fee-479759a95db8', 2, N'org-detail', N'查看机构', N'btn_detail()', N'fa fa-eye', N'/System/Organize/Detail', NULL, 1, 10404, 0, 1, 1, 0, N'admin', CAST(N'2017-04-02 09:47:18.190' AS DateTime), N'admin', CAST(N'2017-04-02 09:47:18.190' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'd9cfc79d-55f6-4890-b604-49f1d2a0d971', N'6d90439c-eb6b-4521-ab4d-5e481406a861', 2, N'user-add', N'新增用户', N'btn_add()', N'
fa fa-plus-square-o', N'/System/User/Form', NULL, 1, 10101, 0, 1, 1, 0, N'admin', CAST(N'2017-03-28 16:14:58.087' AS DateTime), N'admin', CAST(N'2017-03-28 16:14:58.087' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'e32b7507-aaf0-42dc-8008-139250c352ee', N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', 2, N'item-manage', N'字典管理', N'btn_manage()', N'fa fa-folder-open-o', N'/System/Item/Index', NULL, 1, 10501, 0, 1, 1, 0, N'admin', CAST(N'2017-04-03 21:30:55.433' AS DateTime), N'admin', CAST(N'2017-04-04 10:48:52.763' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'e5346fa2-76ec-498f-8f54-3b443959335a', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', 1, N'sys-permission', N'权限管理', NULL, N'fa fa-suitcase', N'/System/Permission/Index', NULL, 0, 10200, 0, 1, 1, 0, N'admin', NULL, N'admin', CAST(N'2017-03-28 16:58:50.730' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'e9478f45-0c00-435f-9a7a-35c7af1f86f7', N'09157352-1252-4964-8fee-479759a95db8', 2, N'org-delete', N'删除机构', N'btn_delete()', N'fa fa-trash-o', N'/System/Organize/Delete', NULL, 1, 10403, 0, 1, 1, 0, N'admin', CAST(N'2017-04-02 09:45:55.757' AS DateTime), N'admin', CAST(N'2017-04-02 09:45:55.757' AS DateTime));
INSERT Sys_Permission (Id, ParentId, Layer, EnCode, Name, JsEvent, Icon, Url, Remark, Type, SortCode, IsPublic, IsEnable, IsEdit, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'fbee5749-8694-495f-b140-b5b3399df7ee', N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', 2, N'item-add', N'新增选项', N'btn_add()', N'fa fa-plus-square-o', N'/System/ItemsDetail/Form', NULL, 1, 10502, 0, 1, 1, 0, N'admin', CAST(N'2017-04-04 19:46:18.233' AS DateTime), N'admin', CAST(N'2017-04-04 19:46:50.650' AS DateTime));
INSERT Sys_Role (Id, OrganizeId, EnCode, Type, Name, AllowEdit, DeleteMark, IsEnabled, Remark, SortCode, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'55440eec-5622-491b-9ade-879dae179c23', N'67ccf447-0f20-4cf8-9159-a5552cf7dc10', N'implement', 1, N'实施人员', 1, 0, 0, NULL, 5, N'admin', NULL, N'admin', CAST(N'2017-06-05 17:33:13.370' AS DateTime));
INSERT Sys_Role (Id, OrganizeId, EnCode, Type, Name, AllowEdit, DeleteMark, IsEnabled, Remark, SortCode, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'78516ecc-e0ad-4f7a-a107-6a4a4ebe64a7', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', N'developer', 0, N'系统开发人员', 0, 0, 1, NULL, 3, N'admin', NULL, NULL, NULL);
INSERT Sys_Role (Id, OrganizeId, EnCode, Type, Name, AllowEdit, DeleteMark, IsEnabled, Remark, SortCode, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'97223b06-6b7a-47fb-b74c-173f52c519c4', N'339a409a-a5a6-49b4-9071-86d7699a9ddd', N'fileattache', 1, N'档案专员', 1, 0, 1, NULL, 7, N'admin', CAST(N'2017-03-13 16:08:55.867' AS DateTime), N'admin', CAST(N'2017-03-13 16:08:55.867' AS DateTime));
INSERT Sys_Role (Id, OrganizeId, EnCode, Type, Name, AllowEdit, DeleteMark, IsEnabled, Remark, SortCode, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', N'administrators', 0, N'超级管理员', 1, 0, 1, NULL, 1, N'admin', NULL, N'admin', CAST(N'2017-05-15 13:40:50.187' AS DateTime));
INSERT Sys_Role (Id, OrganizeId, EnCode, Type, Name, AllowEdit, DeleteMark, IsEnabled, Remark, SortCode, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'db60dc76-9632-44b3-ae4b-7177428bad35', N'771b628b-e43c-4592-b1ef-70ea23b0e3f2', N'configuration', 0, N'系统配置员', 0, 0, 1, NULL, 2, N'admin', NULL, NULL, NULL);
INSERT Sys_Role (Id, OrganizeId, EnCode, Type, Name, AllowEdit, DeleteMark, IsEnabled, Remark, SortCode, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'ed66f99c-d1bd-4fe3-948a-6520a5d6b7d9', N'339a409a-a5a6-49b4-9071-86d7699a9ddd', N'person', 1, N'人事专员', 0, 0, 1, NULL, 6, N'admin', NULL, NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'018c7b35-d79b-4b48-9fa5-dd44375875c4', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'87f0aa68-fa57-43cb-84d0-e979fc4af24c', N'admin', CAST(N'2017-04-04 21:10:58.337' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'04a8f35b-e43b-4f06-aa08-2bfc272fe2a1', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'85438f3b-0634-4b17-b778-aee3a5819669', N'admin', CAST(N'2017-03-28 16:47:59.850' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'083f6bd4-c086-486c-b25a-1f2e8a3a9563', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'216d09a8-575f-43d1-85f6-acc025fa94b3', N'admin', CAST(N'2017-03-28 16:47:59.803' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'09ac4a11-2d50-48e6-b1ae-d9c18384fa5c', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'6d90439c-eb6b-4521-ab4d-5e481406a861', N'admin', CAST(N'2017-03-28 16:47:59.727' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'09d88a4f-ef46-4ca0-a95a-a1ce15aa91c0', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'82b06e80-103e-4a38-b171-740d2b0e194b', N'admin', CAST(N'2017-04-02 09:48:32.997' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'164ad154-21e5-48ab-8e27-1c0ea38d066d', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'e9478f45-0c00-435f-9a7a-35c7af1f86f7', N'admin', CAST(N'2017-04-02 09:48:33.027' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'1adef545-559b-4cc3-b3c0-1debdce21f73', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'c04bfd8f-7e2e-4312-9148-a2e14007fa46', N'admin', CAST(N'2017-04-04 21:10:58.320' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'1f5af2cf-3d4a-4af6-b4e2-4c3dd76627ea', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'09157352-1252-4964-8fee-479759a95db8', N'admin', CAST(N'2017-04-02 09:48:32.977' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'23bbb1ff-9d3a-408a-a9fa-c203ef26c66a', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'fbee5749-8694-495f-b140-b5b3399df7ee', N'admin', CAST(N'2017-04-04 21:10:58.293' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'2ba622d6-60e9-4918-a3cf-f634b969bc98', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'7ae2e6aa-0433-4eaa-9357-1adec2507345', N'admin', CAST(N'2017-04-19 13:22:54.643' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'3bd543bc-3e10-4bf8-96b3-c888987c636e', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'3c69e3fb-e1fe-4911-8417-6f6d55a1ce72', N'admin', CAST(N'2017-04-03 15:34:35.697' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'3f5cf11a-4b6a-4e2f-94e5-dcc390374f75', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'086ee328-5a15-40b0-8e15-291093e2e8b1', N'admin', CAST(N'2017-04-02 09:48:33.013' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'41b4ffda-cd44-4bad-90d0-0ebec361c35e', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'233e50fd-4860-42f9-aa7a-93853ac0434b', N'admin', CAST(N'2017-03-15 13:40:27.933' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'45e1cd76-8c78-4158-a689-87c8d24ba024', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'752c9d3f-a744-42ba-87a2-79849fc3fc66', N'admin', CAST(N'2017-03-28 16:47:59.787' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'4f5bd239-c484-4518-85c3-2c8f65aebe52', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'cd4e9f8b-f56a-42dc-94e1-b76f3d0b38fc', N'admin', CAST(N'2017-04-02 09:48:33.043' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'68e36f44-9a77-4377-bb71-9af61adc7b11', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'2c24cdfc-8f26-4947-bcb2-0cb4d9111e80', N'admin', CAST(N'2017-03-28 16:47:59.817' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'6a8d7415-d228-4316-abdc-6465dd8baf60', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'3de13971-a51f-40f7-be40-eb035b7f0fae', N'admin', CAST(N'2017-04-14 17:28:14.800' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'6bd7028f-00d1-4fd9-89d9-6ddc7ce822ce', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'2d0b02db-09f7-4404-bbdd-c8a516f48288', NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'74604022-d5f2-4855-b07a-f7e1235e2ef6', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'e32b7507-aaf0-42dc-8008-139250c352ee', N'admin', CAST(N'2017-04-03 21:31:55.373' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'76e9aef6-8030-4588-9a63-551a4a0b376e', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'0d2ea3c9-5b29-4bb6-9f91-0322419ded8e', NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'78919e4f-e65d-461a-9af6-f8b5e13232e0', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'28a045a6-61f4-4784-8578-837ad307e4e3', NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'7b3577cf-11d2-46a0-a859-9b17a07328c7', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'328b5383-79be-4b34-b57a-49fa3ebc7803', N'admin', CAST(N'2017-03-28 16:47:59.863' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'80b5d2c9-74b3-42d2-897d-70fffa050fa8', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'277c8647-ea81-42cf-8f7b-db353da95bbe', N'admin', CAST(N'2017-03-15 13:40:27.910' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'810dddfa-870b-482f-a419-6326eea29c84', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'625cf550-4aad-4158-aff4-2a63d4f25819', N'admin', CAST(N'2017-03-28 16:47:59.880' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'83c14f08-2046-4ea4-b01c-a7420a264b2b', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'e5346fa2-76ec-498f-8f54-3b443959335a', NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'98885c60-d3bc-49df-8eaa-f8ccb7eafaa3', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'5fe0cee6-0452-493d-9b55-ff23a5da5e2d', NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'ad17561c-4aea-4eb3-8933-23670a0ee8bd', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'026550fd-2578-42ae-a041-625cda12325f', N'admin', CAST(N'2017-03-28 16:47:59.833' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'b0fe6d22-f29b-4123-95d3-24a613e2e979', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'069f00f6-2a82-4bbe-90d6-418f37d5ef1f', N'admin', CAST(N'2017-04-04 21:10:58.350' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'dc6d7e33-daaa-4df5-9561-cc912f3a26f6', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'aeeb56d1-5f27-42df-9d34-97ac18078390', N'admin', CAST(N'2017-04-18 13:25:58.247' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'e5d6408b-c397-4895-bd00-ac5caffe3c4a', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'd9cfc79d-55f6-4890-b604-49f1d2a0d971', N'admin', CAST(N'2017-03-28 16:47:59.757' AS DateTime));
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'f44cc7d8-4495-42bb-91a0-f56b539b6fc4', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'81d1cbf0-3cff-4cde-8128-7d0d844450de', NULL, NULL);
INSERT Sys_RoleAuthorize (Id, RoleId, ModuleId, CreateUser, CreateTime) VALUES (N'NULL3e7e6244-080b-49a6-9fb5-654af2e0fd33', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'855f3590-b233-4224-aaff-47fb95c8353d', NULL, NULL);
INSERT Sys_User (Id, Account, RealName, NickName, Avatar, Gender, Birthday, MobilePhone, Email, Signature, Address, CompanyId, DepartmentId, IsEnabled, SortCode, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'897C3C01-ACAF-4169-AD2F-B3E101AAE3EA', N'xiaofei', N'王菲', N'菲菲菲菲菲', N'/Content/framework/images/avatar.png', 0, CAST(N'2017-05-02 00:00:00.000' AS DateTime), N'15622684596', N'jakhf@163.com', N'努力工作', N'山东济南', NULL, N'339a409a-a5a6-49b4-9071-86d7699a9ddd', 1, 5, 0, N'admin', CAST(N'2017-03-22 10:58:43.107' AS DateTime), N'admin', CAST(N'2017-06-08 17:12:03.367' AS DateTime));
INSERT Sys_User (Id, Account, RealName, NickName, Avatar, Gender, Birthday, MobilePhone, Email, Signature, Address, CompanyId, DepartmentId, IsEnabled, SortCode, DeleteMark, CreateUser, CreateTime, ModifyUser, ModifyTime) VALUES (N'D1EF3DCD-2C7D-4E8F-8F29-9F73625DD5DF', N'admin', N'小高', N'Esofar', N'/Uploads/Avatar/d1ef3dcd-2c7d-4e8f-8f29-9f73625dd5df.jpg', 1, CAST(N'2017-07-12 00:00:00.000' AS DateTime), N'13688888888', N'973095739@qq.com', N'啦啦啦啦啦啦啦啦啦咯！', N'山东济宁', NULL, N'a93c66e2-b8dc-4d00-84ed-e6071b5f5318', 1, 1, 0, N'admin', CAST(N'2017-03-22 10:58:43.107' AS DateTime), N'admin', CAST(N'2017-07-12 11:01:50.053' AS DateTime));
INSERT Sys_UserLogOn (Id, UserId, Password, SecretKey, PrevVisitTime, LastVisitTime, ChangePwdTime, LoginCount, AllowMultiUserOnline, IsOnLine, Question, AnswerQuestion, CheckIPAddress, Language, Theme) VALUES (N'6bde15b3-88a9-4522-817e-3d5877130a05', N'd1ef3dcd-2c7d-4e8f-8f29-9f73625dd5df', N'1e3d472896cff02f8281a86b70046377', N'juhgtdjc', CAST(N'2017-07-12 10:35:21.470' AS DateTime), CAST(N'2017-07-12 10:35:21.470' AS DateTime), CAST(N'2017-07-11 09:38:40.070' AS DateTime), 1040, 1, 1, N'lovecoding?', N'no', 1, NULL, NULL);
INSERT Sys_UserLogOn (Id, UserId, Password, SecretKey, PrevVisitTime, LastVisitTime, ChangePwdTime, LoginCount, AllowMultiUserOnline, IsOnLine, Question, AnswerQuestion, CheckIPAddress, Language, Theme) VALUES (N'e86c5360-b710-475a-8f80-6f7e0d0b0a1a', N'897c3c01-acaf-4169-ad2f-b3e101aae3ea', N'552547bc4c511ce82e66265e370d49b6', N'juhgtdjc', CAST(N'2017-03-30 14:14:37.153' AS DateTime), CAST(N'2017-03-30 14:14:37.153' AS DateTime), NULL, 3, 0, 1, N'我是谁？', N'王菲', 0, NULL, NULL);
INSERT Sys_UserRoleRelation (Id, UserId, RoleId, CreateUser, CreateTime) VALUES (N'45e0a953-fd82-42f4-afe5-cbbbd2a263b0', N'd1ef3dcd-2c7d-4e8f-8f29-9f73625dd5df', N'a3a3857c-51fb-43a6-a7b5-3a612e887b3a', N'admin', CAST(N'2017-01-20 09:37:08.343' AS DateTime));
INSERT Sys_UserRoleRelation (Id, UserId, RoleId, CreateUser, CreateTime) VALUES (N'5c7b0e32-9dd5-40d8-82ec-e26a331d6059', N'897c3c01-acaf-4169-ad2f-b3e101aae3ea', N'ed66f99c-d1bd-4fe3-948a-6520a5d6b7d9', N'admin', CAST(N'2017-03-25 16:23:57.783' AS DateTime));
INSERT Sys_UserRoleRelation (Id, UserId, RoleId, CreateUser, CreateTime) VALUES (N'b37e496a-918b-4876-a09e-22449aac1bb7', N'897c3c01-acaf-4169-ad2f-b3e101aae3ea', N'97223b06-6b7a-47fb-b74c-173f52c519c4', N'admin', CAST(N'2017-03-25 16:23:57.810' AS DateTime));
