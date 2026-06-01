-- =============================================================
-- GestaoRH — Migração: Módulo Modelos de Documentos (Fase 1)
-- Execute na Query Tool do pgAdmin (F5)
-- =============================================================

-- 1. Modelo principal
CREATE TABLE IF NOT EXISTS documento_modelo (
    id          SERIAL       PRIMARY KEY,
    nome        VARCHAR(200) NOT NULL,
    descricao   TEXT         NOT NULL DEFAULT '',
    categoria   VARCHAR(50)  NOT NULL,
    tipo_uso    VARCHAR(20)  NOT NULL DEFAULT 'individual'
                    CHECK (tipo_uso IN ('individual', 'lote', 'ambos')),
    status      VARCHAR(20)  NOT NULL DEFAULT 'rascunho'
                    CHECK (status IN ('rascunho', 'publicado', 'arquivado')),
    versao      INT          NOT NULL DEFAULT 1,
    criado_em   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_modelo_status    ON documento_modelo (status);
CREATE INDEX IF NOT EXISTS idx_modelo_categoria ON documento_modelo (categoria);

-- 2. Seções do modelo
CREATE TABLE IF NOT EXISTS documento_modelo_secao (
    id        SERIAL       PRIMARY KEY,
    modelo_id INT          NOT NULL REFERENCES documento_modelo(id) ON DELETE CASCADE,
    titulo    VARCHAR(200) NOT NULL DEFAULT 'Seção',
    tipo      VARCHAR(20)  NOT NULL DEFAULT 'texto'
                  CHECK (tipo IN ('texto', 'campos', 'assinaturas', 'anexos')),
    conteudo  TEXT         NOT NULL DEFAULT '',
    ordem     INT          NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_secao_modelo_id ON documento_modelo_secao (modelo_id);

-- 3. Campos de cada seção (para seções tipo 'campos')
CREATE TABLE IF NOT EXISTS documento_modelo_campo (
    id          SERIAL       PRIMARY KEY,
    secao_id    INT          NOT NULL REFERENCES documento_modelo_secao(id) ON DELETE CASCADE,
    label       VARCHAR(200) NOT NULL DEFAULT '',
    tipo_campo  VARCHAR(30)  NOT NULL DEFAULT 'texto_curto',
    obrigatorio BOOLEAN      NOT NULL DEFAULT TRUE,
    ordem       INT          NOT NULL DEFAULT 0,
    config_json JSONB        -- placeholder para configurações futuras (opções de select, etc.)
);

CREATE INDEX IF NOT EXISTS idx_campo_secao_id ON documento_modelo_campo (secao_id);

-- 4. Assinantes configurados no modelo
CREATE TABLE IF NOT EXISTS documento_modelo_assinante (
    id          SERIAL       PRIMARY KEY,
    modelo_id   INT          NOT NULL REFERENCES documento_modelo(id) ON DELETE CASCADE,
    papel       VARCHAR(30)  NOT NULL
                    CHECK (papel IN ('funcionario', 'rh', 'chefe')),
    rotulo      VARCHAR(200) NOT NULL DEFAULT '',
    obrigatorio BOOLEAN      NOT NULL DEFAULT TRUE,
    ordem       INT          NOT NULL DEFAULT 1,
    exibir_pdf  BOOLEAN      NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS idx_assinante_modelo_id ON documento_modelo_assinante (modelo_id);

-- =============================================================
-- Verificação
-- =============================================================
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'documento_%'
ORDER BY table_name;
