-- =============================================================
-- GestaoRH — Script COMPLETO do banco de dados
-- Banco: GestaoRHDB  |  PostgreSQL 17
-- Execute TUDO de uma vez na Query Tool do pgAdmin (F5)
-- =============================================================

-- 1. Tabela empresa
CREATE TABLE IF NOT EXISTS empresa (
    id                    SERIAL       PRIMARY KEY,
    cnpj                  VARCHAR(18)  NOT NULL UNIQUE,
    razao_social          VARCHAR(200) NOT NULL,
    endereco              VARCHAR(300) NOT NULL DEFAULT '',
    telefone              VARCHAR(20)  NOT NULL DEFAULT '',
    logo_base64           TEXT,
    responsavel_nome      VARCHAR(100) NOT NULL,
    responsavel_sobrenome VARCHAR(100) NOT NULL,
    senha                 VARCHAR(255) NOT NULL,
    ativo                 BOOLEAN      NOT NULL DEFAULT TRUE,
    criado_em             TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_empresa_cnpj ON empresa (cnpj);

-- 2. Tabela setor
CREATE TABLE IF NOT EXISTS setor (
    id        SERIAL       PRIMARY KEY,
    nome      VARCHAR(100) NOT NULL UNIQUE,
    descricao TEXT         NOT NULL DEFAULT '',
    ativo     BOOLEAN      NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_setor_nome ON setor (LOWER(nome));

-- 3. Tabela funcionario
CREATE TABLE IF NOT EXISTS funcionario (
    id                 SERIAL       PRIMARY KEY,
    cpf                VARCHAR(14)  NOT NULL UNIQUE,
    nome               VARCHAR(200) NOT NULL,
    telefone           VARCHAR(20)  NOT NULL DEFAULT '',
    email              VARCHAR(200) NOT NULL UNIQUE,
    genero             VARCHAR(20)  NOT NULL
                           CHECK (genero IN ('masculino', 'feminino', 'sem_genero')),
    turno              VARCHAR(20)  NOT NULL
                           CHECK (turno IN ('matutino', 'vespertino', 'noturno')),
    setor_id           INT          NOT NULL REFERENCES setor(id),
    senha_temporaria   VARCHAR(100) NOT NULL,
    senha              VARCHAR(255) NOT NULL,
    senha_trocada      BOOLEAN      NOT NULL DEFAULT FALSE,
    ativo              BOOLEAN      NOT NULL DEFAULT TRUE,
    criado_em          TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_funcionario_cpf      ON funcionario (cpf);
CREATE INDEX IF NOT EXISTS idx_funcionario_email    ON funcionario (email);
CREATE INDEX IF NOT EXISTS idx_funcionario_setor_id ON funcionario (setor_id);

-- Verificacao final
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
