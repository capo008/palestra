CREATE TABLE sala (
    id_sala INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL UNIQUE,
    capienza INT NOT NULL CHECK (capienza > 0)
);

CREATE TABLE istruttore (
    id_istruttore INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    cognome VARCHAR(50) NOT NULL
);

CREATE TABLE corso (
    id_corso INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE iscritto (
    id_iscritto INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(50) NOT NULL,
    cognome VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE lezione (
    id_lezione INT AUTO_INCREMENT PRIMARY KEY,
    id_corso INT NOT NULL,
    id_sala INT NOT NULL,
    id_istruttore INT NOT NULL,
    giorno_settimana SMALLINT NOT NULL
        CHECK (giorno_settimana BETWEEN 1 AND 7),
    ora_inizio TIME NOT NULL,

    UNIQUE (id_sala, giorno_settimana, ora_inizio),

    FOREIGN KEY (id_corso) REFERENCES corso(id_corso),
    FOREIGN KEY (id_sala) REFERENCES sala(id_sala),
    FOREIGN KEY (id_istruttore) REFERENCES istruttore(id_istruttore)
);
