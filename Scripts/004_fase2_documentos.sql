-- =============================================================
-- GestaoRH — Fase 2: Documentos + Assinaturas
-- Execute na Query Tool do pgAdmin (F5)
-- =============================================================

-- 1. Assinatura salva no perfil do funcionário (reutilizável)
ALTER TABLE funcionario
    ADD COLUMN IF NOT EXISTS assinatura_base64 TEXT;

-- 2. Lote de geração (agrupa documentos do mesmo envio por setor)
CREATE TABLE IF NOT EXISTS documento_lote (
    id           SERIAL       PRIMARY KEY,
    modelo_id    INT          NOT NULL REFERENCES documento_modelo(id),
    setor_id     INT          REFERENCES setor(id),        -- NULL = individual
    criado_por   INT          NOT NULL,                    -- empresa.id
    total        INT          NOT NULL DEFAULT 0,
    status       VARCHAR(20)  NOT NULL DEFAULT 'gerado'
                     CHECK (status IN ('gerado','em_andamento','concluido','cancelado')),
    criado_em    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- 3. Instância de documento (1 por funcionário)
CREATE TABLE IF NOT EXISTS documento_instancia (
    id                  SERIAL       PRIMARY KEY,
    modelo_id           INT          NOT NULL REFERENCES documento_modelo(id),
    modelo_versao       INT          NOT NULL DEFAULT 1,
    modelo_nome_snapshot VARCHAR(200) NOT NULL DEFAULT '',
    lote_id             INT          REFERENCES documento_lote(id),
    funcionario_id      INT          NOT NULL REFERENCES funcionario(id),
    status              VARCHAR(30)  NOT NULL DEFAULT 'pendente'
                            CHECK (status IN (
                                'pendente','aguardando_assinatura',
                                'parcialmente_assinado','concluido','cancelado'
                            )),
    conteudo_html       TEXT         NOT NULL DEFAULT '', -- snapshot congelado
    pdf_base64          TEXT,                             -- PDF final gerado
    criado_em           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    concluido_em        TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_instancia_funcionario ON documento_instancia (funcionario_id);
CREATE INDEX IF NOT EXISTS idx_instancia_status      ON documento_instancia (status);
CREATE INDEX IF NOT EXISTS idx_instancia_lote        ON documento_instancia (lote_id);

-- 4. Valores preenchidos de variáveis manuais
CREATE TABLE IF NOT EXISTS documento_variavel_valor (
    id           SERIAL  PRIMARY KEY,
    instancia_id INT     NOT NULL REFERENCES documento_instancia(id) ON DELETE CASCADE,
    token        VARCHAR(100) NOT NULL,
    valor        TEXT    NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_variavel_instancia ON documento_variavel_valor (instancia_id);

-- 5. Assinaturas realizadas
CREATE TABLE IF NOT EXISTS documento_assinatura (
    id                    SERIAL       PRIMARY KEY,
    instancia_id          INT          NOT NULL REFERENCES documento_instancia(id) ON DELETE CASCADE,
    papel                 VARCHAR(30)  NOT NULL
                              CHECK (papel IN ('funcionario','rh','chefe')),
    signer_id             INT          NOT NULL,   -- id do funcionario ou empresa
    signer_tipo           VARCHAR(20)  NOT NULL
                              CHECK (signer_tipo IN ('funcionario','empresa')),
    signer_nome_snapshot  VARCHAR(200) NOT NULL DEFAULT '',
    signer_email_snapshot VARCHAR(200) NOT NULL DEFAULT '',
    status                VARCHAR(20)  NOT NULL DEFAULT 'pendente'
                              CHECK (status IN ('pendente','assinado','recusado')),
    obrigatorio           BOOLEAN      NOT NULL DEFAULT TRUE,
    ordem                 INT          NOT NULL DEFAULT 1,
    assinatura_base64     TEXT,         -- imagem da assinatura (PNG base64)
    assinado_em           TIMESTAMPTZ,
    ip_address            VARCHAR(45),
    criado_em             TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_assinatura_instancia ON documento_assinatura (instancia_id);
CREATE INDEX IF NOT EXISTS idx_assinatura_signer    ON documento_assinatura (signer_id, signer_tipo);

-- =============================================================
-- Verificação
-- =============================================================
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'documento_%'
ORDER BY table_name;