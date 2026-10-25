create table dbo.tb_wallets (
  [id] int identity(1,1) not null primary key,
  [user_id] int not null unique,
  [balance] decimal(18,2) not null default 0.00,

  constraint fk_user_id foreign key ([user_id]) references dbo.tb_users([id]),
  constraint chk_balance check ([balance] >= 0)
)