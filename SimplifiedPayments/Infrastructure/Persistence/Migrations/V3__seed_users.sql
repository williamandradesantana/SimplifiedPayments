insert into dbo.tb_users ([full_name], [document], [email], [password], [user_type]) values
  (N'Maria Souza',      '29141777638',    'maria@example.com',  '$2b$11$E0BqbywCf813.dps0SuVFOQN9NnbrIL0bzRBjzYqKfjygNsRBJvPe', 'COMMON'),
  (N'João Pereira',     '01152449303',    'joao@example.com',   '$2b$11$E0BqbywCf813.dps0SuVFOQN9NnbrIL0bzRBjzYqKfjygNsRBJvPe', 'COMMON'),
  (N'Ana Lima',         '39825979194',    'ana@example.com',    '$2b$11$E0BqbywCf813.dps0SuVFOQN9NnbrIL0bzRBjzYqKfjygNsRBJvPe', 'COMMON'),
  (N'Padaria Pão Quente', '29141777000158', 'padaria@example.com', '$2b$11$E0BqbywCf813.dps0SuVFOQN9NnbrIL0bzRBjzYqKfjygNsRBJvPe', 'SHOPKEEPER'),
  (N'Mercado Bom Preço',  '01152449000138', 'mercado@example.com', '$2b$11$E0BqbywCf813.dps0SuVFOQN9NnbrIL0bzRBjzYqKfjygNsRBJvPe', 'SHOPKEEPER');

insert into dbo.tb_wallets ([user_id], [balance])
select u.[id],
       case u.[user_type] when 'COMMON' then 1000.00 else 0.00 end
from dbo.tb_users u
where u.[document] in ('29141777638', '01152449303', '39825979194', '29141777000158', '01152449000138');