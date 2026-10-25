create table dbo.tb_users (
  [id] int identity(1,1) not null primary key,
  [full_name] nvarchar(150) not null,
  [document] varchar(14) not null unique,
  [email] varchar(100) not null unique,
  [password] varchar(255) not null,
  [user_type] varchar(20) not null,

  constraint chk_user_type check ([user_type] in ('COMMON', 'SHOPKEEPER')),
  constraint chk_document_by_type check (
	([user_type] = 'COMMON' and len([document]) = 11) or
	([user_type] = 'SHOPKEEPER' and len([document]) = 14)
  )
)