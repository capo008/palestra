-- =========================================
-- STORED PROCEDURE sp_prenota  (MariaDB 10.5+ / MySQL 8)
-- Eseguire DOPO create.sql, sul database "palestra".
-- Da riga di comando:  mariadb -u root -p palestra < procedure.sql
-- =========================================

DROP PROCEDURE IF EXISTS sp_prenota;

DELIMITER $$

CREATE PROCEDURE sp_prenota(
    IN  p_id_iscritto      INT,
    IN  p_id_lezione       INT,
    IN  p_data_lezione     DATE,
    OUT p_esito            VARCHAR(30),
    OUT p_messaggio        VARCHAR(200),
    OUT p_id_prenotazione  INT
)
sp: BEGIN
    DECLARE v_id_lezione INT DEFAULT NULL;
    DECLARE v_giorno     SMALLINT DEFAULT NULL;
    DECLARE v_ora        TIME DEFAULT NULL;
    DECLARE v_capienza   INT DEFAULT NULL;
    DECLARE v_occupati   INT DEFAULT 0;

    -- Qualsiasi errore imprevisto: annulla tutto e rilancia l'errore all'applicazione
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    SET p_esito = NULL;
    SET p_messaggio = NULL;
    SET p_id_prenotazione = NULL;

    START TRANSACTION;

    -- Blocca la riga della lezione: due prenotazioni contemporanee per la stessa
    -- lezione vengono serializzate, quindi il controllo dei posti non può essere
    -- aggirato. (La lettura bloccante avviene prima di qualunque lettura "normale",
    -- così il conteggio successivo vede anche le prenotazioni appena confermate.)
    SELECT id_lezione, giorno_settimana, ora_inizio
      INTO v_id_lezione, v_giorno, v_ora
      FROM lezione
     WHERE id_lezione = p_id_lezione
       FOR UPDATE;

    IF v_id_lezione IS NULL THEN
        ROLLBACK;
        SET p_esito = 'LEZIONE_NON_TROVATA';
        SET p_messaggio = 'La lezione selezionata non esiste.';
        LEAVE sp;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM iscritto WHERE id_iscritto = p_id_iscritto) THEN
        ROLLBACK;
        SET p_esito = 'ISCRITTO_NON_TROVATO';
        SET p_messaggio = 'L''iscritto selezionato non esiste.';
        LEAVE sp;
    END IF;

    IF p_data_lezione < CURDATE()
       OR (p_data_lezione = CURDATE() AND v_ora < CURTIME()) THEN
        ROLLBACK;
        SET p_esito = 'LEZIONE_PASSATA';
        SET p_messaggio = 'Non si può prenotare una lezione già iniziata o passata.';
        LEAVE sp;
    END IF;

    -- giorno_settimana: 1 = lunedì ... 7 = domenica; WEEKDAY(): 0 = lunedì ... 6 = domenica
    IF WEEKDAY(p_data_lezione) + 1 <> v_giorno THEN
        ROLLBACK;
        SET p_esito = 'GIORNO_ERRATO';
        SET p_messaggio = 'La data scelta non corrisponde al giorno della settimana in cui si tiene la lezione.';
        LEAVE sp;
    END IF;

    -- L'abbonamento deve coprire la data della lezione (estremi inclusi)
    IF NOT EXISTS (SELECT 1
                     FROM abbonamento
                    WHERE id_iscritto = p_id_iscritto
                      AND p_data_lezione BETWEEN data_inizio AND data_fine) THEN
        ROLLBACK;
        SET p_esito = 'ABBONAMENTO_NON_VALIDO';
        SET p_messaggio = 'L''iscritto non ha un abbonamento valido per la data della lezione.';
        LEAVE sp;
    END IF;

    IF EXISTS (SELECT 1
                 FROM prenotazione
                WHERE id_iscritto = p_id_iscritto
                  AND id_lezione = p_id_lezione
                  AND data_lezione = p_data_lezione
                  AND stato = 'attiva') THEN
        ROLLBACK;
        SET p_esito = 'GIA_PRENOTATO';
        SET p_messaggio = 'L''iscritto ha già una prenotazione attiva per questa lezione.';
        LEAVE sp;
    END IF;

    SELECT s.capienza
      INTO v_capienza
      FROM lezione l
      JOIN sala s ON s.id_sala = l.id_sala
     WHERE l.id_lezione = p_id_lezione;

    SELECT COUNT(*)
      INTO v_occupati
      FROM prenotazione
     WHERE id_lezione = p_id_lezione
       AND data_lezione = p_data_lezione
       AND stato = 'attiva';

    IF v_occupati >= v_capienza THEN
        ROLLBACK;
        SET p_esito = 'LEZIONE_PIENA';
        SET p_messaggio = CONCAT('Lezione al completo: ', v_occupati, ' posti su ', v_capienza, ' occupati.');
        LEAVE sp;
    END IF;

    INSERT INTO prenotazione (id_iscritto, id_lezione, data_lezione)
    VALUES (p_id_iscritto, p_id_lezione, p_data_lezione);

    SET p_id_prenotazione = LAST_INSERT_ID();

    COMMIT;

    SET p_esito = 'OK';
    SET p_messaggio = CONCAT('Prenotazione confermata. Posti rimasti: ', v_capienza - v_occupati - 1, '.');
END$$

DELIMITER ;
