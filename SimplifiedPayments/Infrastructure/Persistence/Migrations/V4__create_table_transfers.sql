alter table dbo.tb_wallets add [row_version] rowversion not null;

create table dbo.tb_transfers (
 [id] int identity(1,1) primary key not null,
 [payer_id] int not null,
 [payee_id] int not null,
 [amount] decimal(18,2) not null,
 [created_at] datetime2(3) not null default sysutcdatetime(),

 constraint fk_tranfers_payer foreign key ([payer_id]) references dbo.tb_users([id]),
 constraint fk_tranfers_payee foreign key ([payee_id]) references dbo.tb_users([id]),
 constraint chk_amount check ([amount] > 0),
 constraint chk_transfer_distinct_users check ([payer_id] <> [payee_id])
)

create index ix_transfers_payer on dbo.tb_transfers ([payer_id], [created_at] desc);
create index ix_transfers_payee on dbo.tb_transfers ([payee_id], [created_at] desc);