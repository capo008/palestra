
-- =========================================
-- SCHEMA DATABASE PALESTRA - MYSQL 8.0+
-- =========================================

CREATE TABLE sala (
    id_sala INTEGER AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL UNIQUE,
    capienza INTEGER NOT NULL CHECK (capienza > 0)
) ENGINE=InnoDB;


CREATE TABLE istruttore (
    id_istruttore INTEGER AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    cognome VARCHAR(50) NOT NULL
) ENGINE=InnoDB;


CREATE TABLE corso (
    id_corso INTEGER AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB;


CREATE TABLE lezione (
    id_lezione INTEGER AUTO_INCREMENT PRIMARY KEY,
    id_corso INTEGER NOT NULL,
    id_sala INTEGER NOT NULL,
    id_istruttore INTEGER NOT NULL,
    giorno_settimana SMALLINT NOT NULL
        CHECK (giorno_settimana BETWEEN 1 AND 7),
    ora_inizio TIME NOT NULL,

    CONSTRAINT fk_lezione_corso
        FOREIGN KEY (id_corso)
        REFERENCES corso (id_corso),

    CONSTRAINT fk_lezione_sala
        FOREIGN KEY (id_sala)
        REFERENCES sala (id_sala),

    CONSTRAINT fk_lezione_istruttore
        FOREIGN KEY (id_istruttore)
        REFERENCES istruttore (id_istruttore),

    UNIQUE (id_sala, giorno_settimana, ora_inizio)
) ENGINE=InnoDB;


CREATE TABLE iscritto (
    id_iscritto INTEGER AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    cognome VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;


CREATE TABLE abbonamento (
    id_abbonamento INTEGER AUTO_INCREMENT PRIMARY KEY,
    id_iscritto INTEGER NOT NULL,
    tipo VARCHAR(12) NOT NULL
        CHECK (tipo IN ('mensile', 'trimestrale', 'annuale')),
    data_inizio DATE NOT NULL,
    data_fine DATE NOT NULL,

    CONSTRAINT fk_abbonamento_iscritto
        FOREIGN KEY (id_iscritto)
        REFERENCES iscritto (id_iscritto),

    CHECK (data_fine > data_inizio)
) ENGINE=InnoDB;


CREATE TABLE prenotazione (
    id_prenotazione INTEGER AUTO_INCREMENT PRIMARY KEY,
    id_iscritto INTEGER NOT NULL,
    id_lezione INTEGER NOT NULL,
    data_lezione DATE NOT NULL,
    data_prenotazione TIMESTAMP NOT NULL
        DEFAULT CURRENT_TIMESTAMP,
    stato VARCHAR(10) NOT NULL DEFAULT 'attiva'
        CHECK (stato IN ('attiva', 'annullata')),
    presente BOOLEAN,
    data_disdetta TIMESTAMP,

    -- Colonna generata per simulare l'indice parziale PostgreSQL.
    -- Vale 1 per le prenotazioni attive e NULL per quelle annullate.
    prenotazione_attiva TINYINT
        GENERATED ALWAYS AS (
            CASE
                WHEN stato = 'attiva' THEN 1
                ELSE NULL
            END
        ) STORED,

    CONSTRAINT fk_prenotazione_iscritto
        FOREIGN KEY (id_iscritto)
        REFERENCES iscritto (id_iscritto),

    CONSTRAINT fk_prenotazione_lezione
        FOREIGN KEY (id_lezione)
        REFERENCES lezione (id_lezione),

    -- La data di disdetta deve esserci solo se annullata
    CHECK (
        (stato = 'annullata' AND data_disdetta IS NOT NULL)
        OR
        (stato = 'attiva' AND data_disdetta IS NULL)
    ),

    -- La presenza si registra solo su prenotazioni attive
    CHECK (
        presente IS NULL OR stato = 'attiva'
    ),

    -- Un iscritto non può avere due prenotazioni attive
    -- per la stessa lezione nella stessa data
    UNIQUE (
        id_iscritto,
        id_lezione,
        data_lezione,
        prenotazione_attiva
    ),

    INDEX idx_prenotazione_lezione_data
        (id_lezione, data_lezione, stato)

) ENGINE=InnoDB;
