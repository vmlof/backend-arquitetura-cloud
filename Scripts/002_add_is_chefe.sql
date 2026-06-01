-- =============================================================
-- GestaoRH — Migração: campo is_chefe em funcionario
-- Execute na Query Tool do pgAdmin (F5)
-- =============================================================

ALTER TABLE funcionario
    ADD COLUMN IF NOT EXISTS is_chefe BOOLEAN NOT NULL DEFAULT FALSE;

CREATE INDEX IF NOT EXISTS idx_funcionario_is_chefe
    ON funcionario (is_chefe)
    WHERE is_chefe = TRUE;

-- Verificação
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'funcionario'
ORDER BY ordinal_position;