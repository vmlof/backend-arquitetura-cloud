-- =============================================================
-- GestaoRH — Migração: ativo em funcionario + unique condicional
-- Execute na Query Tool do pgAdmin (F5)
-- =============================================================

-- 1. Setor: remover UNIQUE absoluto do nome e criar UNIQUE
--    apenas para setores ativos (partial unique index)
--    Isso permite reutilizar o nome de um setor desativado.
ALTER TABLE setor DROP CONSTRAINT IF EXISTS setor_nome_key;

DROP INDEX IF EXISTS idx_setor_nome;

CREATE UNIQUE INDEX uq_setor_nome_ativo
    ON setor (LOWER(nome))
    WHERE ativo = TRUE;

-- 2. Funcionario: o campo ativo já existe.
--    Mesma lógica: unique de CPF e email apenas para ativos.
ALTER TABLE funcionario DROP CONSTRAINT IF EXISTS funcionario_cpf_key;
ALTER TABLE funcionario DROP CONSTRAINT IF EXISTS funcionario_email_key;

DROP INDEX IF EXISTS idx_funcionario_cpf;
DROP INDEX IF EXISTS idx_funcionario_email;

CREATE UNIQUE INDEX uq_funcionario_cpf_ativo
    ON funcionario (cpf)
    WHERE ativo = TRUE;

CREATE UNIQUE INDEX uq_funcionario_email_ativo
    ON funcionario (email)
    WHERE ativo = TRUE;

-- Verificação final
SELECT
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('setor', 'funcionario')
ORDER BY tablename, indexname;
