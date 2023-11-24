CREATE OR REPLACE FUNCTION check_max_accessory_slots()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
   IF (NEW.is_equipped)
   THEN
      IF (SELECT COUNT(*) FROM inventory WHERE adventurer_id = NEW.adventurer_id AND is_equipped) > 1
      THEN
         RETURN NULL;
      ELSE
         RETURN NEW;
      END IF;
   ELSE
      RETURN NEW;
   END IF;
END;
$$;

CREATE OR REPLACE TRIGGER max_accessory_slots
BEFORE INSERT OR UPDATE
ON inventory
FOR EACH ROW
EXECUTE PROCEDURE check_max_accessory_slots();

CREATE OR REPLACE PROCEDURE add_exp_to_adventurer(
    IN adventurer_id_param INT,
    IN xp_count_param INT
 )
 LANGUAGE plpgsql
 AS $$
 BEGIN
    IF (SELECT a.xp_count + xp_count_param > (1000+250*POW(2, a."level")) FROM public.adventurer a WHERE a.adventurer_id = adventurer_id_param)
    THEN
      UPDATE adventurer SET xp_count=xp_count+xp_count_param-(1000+250*POW("level", 2)), "level"="level"+1 WHERE adventurer_id=adventurer_id_param;
    ELSE
      UPDATE adventurer SET xp_count=xp_count+xp_count_param WHERE adventurer_id=adventurer_id_param;
    END IF;
 END;
 $$;
 
CREATE OR REPLACE FUNCTION purchase_accessory(
   IN accessory_id_param INT,
   IN adventurer_id_param INT
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
   gold_cost INT := (SELECT acc.gold_cost FROM accessory acc WHERE acc.accessory_id = accessory_id_param);
BEGIN
   IF (
      SELECT gold_cost < adv.gold 
      FROM adventurer adv 
      WHERE adv.adventurer_id = adventurer_id_param
   )
   THEN
      INSERT INTO public.inventory
      (adventurer_id, accessory_id, is_equipped)
      VALUES(adventurer_id_param, accessory_id_param, false);
      UPDATE adventurer
      SET gold=gold-gold_cost
      WHERE adventurer_id=adventurer_id_param;
      RETURN TRUE;
   ELSE
      RETURN FALSE;
   END IF;
END;
$$;

CREATE OR REPLACE FUNCTION complete_quest(
   IN quest_id_param INT,
   IN adventurer_id_param INT
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
   v_gold_reward INT;
   v_xp_reward INT;
   adventurer_rec record;
   requirement_rec record;
   requirement_cursor CURSOR FOR SELECT * FROM quest_requirement WHERE quest_id = quest_id_param;
   reward_rec record;
   reward_cursor CURSOR FOR SELECT * FROM quest_reward WHERE quest_id = quest_id_param;
BEGIN
   SELECT INTO adventurer_rec * FROM adventurer WHERE adventurer_id = adventurer_id_param;
   SELECT INTO v_xp_reward, v_gold_reward q.xp_reward, q.gold_reward
   FROM quest q WHERE q.quest_id = quest_id_param;
   OPEN requirement_cursor;
   LOOP
      FETCH requirement_cursor INTO requirement_rec;
      EXIT WHEN NOT FOUND;
      CASE requirement_rec.requirement_type
      WHEN 'STR' THEN
       IF (
           SELECT COALESCE(t.stat_boost,0) + adv.strength < requirement_rec.amount
           FROM adventurer adv
           LEFT JOIN (
                SELECT SUM(stat_boost) AS stat_boost 
                FROM accessory_boost ab
                WHERE ab.accessory_id IN (
                    SELECT accessory_id FROM inventory 
                    WHERE adventurer_id = adventurer_id_param 
                    AND is_equipped
                    AND stat_type = 'STR'
                )
            ) t on TRUE 
            WHERE adv.adventurer_id = adventurer_id_param
       )
       THEN RETURN FALSE;
       END IF;
      WHEN 'ETH' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + adv.ether < requirement_rec.amount
              FROM adventurer adv
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'ETH'
                    ) 
                ) t on TRUE
                WHERE adv.adventurer_id = adventurer_id_param
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'DEX' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + adv.dexterity < requirement_rec.amount
              FROM adventurer adv
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'DEX'
                    ) 
                ) t on TRUE
                WHERE adv.adventurer_id = adventurer_id_param
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'AGI' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + adv.agility < requirement_rec.amount
              FROM adventurer adv
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'AGI'
                    ) 
                ) t on TRUE
                WHERE adv.adventurer_id = adventurer_id_param
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'MHP' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + adv.max_health < requirement_rec.amount
              FROM adventurer adv
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'MHP'
                    ) 
                ) t on TRUE
                WHERE adv.adventurer_id = adventurer_id_param
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'GRD' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + w.guard < requirement_rec.amount
              FROM weapon w
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'ETH'
                    ) 
                ) t on TRUE
                WHERE w.weapon_id = adventurer_rec.weapon_id
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'ETR' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + w.ether_resistance < requirement_rec.amount
              FROM weapon w
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'ETH'
                    ) 
                ) t on TRUE
                WHERE w.weapon_id = adventurer_rec.weapon_id
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'POW' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + w.power < requirement_rec.amount
              FROM weapon w
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'ETH'
                    ) 
                ) t on TRUE
                WHERE w.weapon_id = adventurer_rec.weapon_id
          )
          THEN RETURN FALSE;
          END IF;
      WHEN 'ETP' THEN
          IF (
              SELECT COALESCE(t.stat_boost,0) + w.ether < requirement_rec.amount
              FROM weapon w
              LEFT JOIN (
                    SELECT SUM(stat_boost) AS stat_boost 
                    FROM accessory_boost ab
                    WHERE ab.accessory_id IN (
                        SELECT accessory_id FROM inventory 
                        WHERE adventurer_id = adventurer_id_param 
                        AND is_equipped
                        AND stat_type = 'ETH'
                    ) 
                ) t on TRUE
                WHERE w.weapon_id = adventurer_rec.weapon_id
          )
          THEN RETURN FALSE;
         END IF;
      END CASE;
   END LOOP;
   CALL add_exp_to_adventurer(adventurer_id_param, v_xp_reward);
   UPDATE adventurer
   SET gold=gold+v_gold_reward
   WHERE adventurer_id=adventurer_id_param;
   OPEN reward_cursor;
   LOOP
      FETCH reward_cursor INTO reward_rec;
      EXIT WHEN NOT FOUND;
      IF (
      SELECT COUNT(*) > 0 FROM trinket_pouch WHERE trinket_id = reward_rec.trinket_id AND adventurer_id = adventurer_id_param)
      THEN
         UPDATE trinket_pouch
         SET amount=amount+reward_rec.amount
         WHERE trinket_id=reward_rec.trinket_id
         AND adventurer_id=adventurer_id_param;
      ELSE
         INSERT INTO trinket_pouch(adventurer_id, trinket_id, amount)
         VALUES(adventurer_id_param, reward_rec.trinket_id, reward_rec.amount);
      END IF;
   END LOOP;
   RETURN TRUE;
END;
$$;


