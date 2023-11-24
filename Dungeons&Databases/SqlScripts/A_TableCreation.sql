CREATE DOMAIN adventurer_class
AS CHAR(3)
CHECK (VALUE IN ('ATK', 'TNK', 'SGN'));

CREATE DOMAIN adventurer_element
AS VARCHAR(8)
CHECK (VALUE IN ('FIRE', 'ICE', 'EARTH', 'LIGHT', 'WATER', 'WIND', 'ELECTRIC', 'DARK'));

CREATE DOMAIN adventurer_stat
AS CHAR(3)
CHECK (VALUE IN ('STR', 'ETH', 'DEX', 'AGI', 'MHP', 'GRD', 'ETR', 'POW', 'ETP'));

CREATE TABLE dndb_user
(
   user_id SERIAL PRIMARY KEY,
   email VARCHAR(254) NOT NULL,
   password bytea NOT NULL,
   is_admin boolean NOT NULL DEFAULT FALSE,
   unique(email)
);

CREATE TABLE weapon  
(
   weapon_id SERIAL PRIMARY KEY, 
   name VARCHAR(32) NOT NULL, 
   power INTEGER NOT NULL CHECK (power > 0), 
   ether INTEGER NOT NULL CHECK (ether > 0), 
   guard INTEGER NOT NULL CHECK (guard > 0 AND guard <= 40),  
   ether_resistance INTEGER NOT NULL CHECK (ether_resistance > 0 AND ether_resistance <= 30),
   unique(name)
);

CREATE TABLE adventurer
(
   adventurer_id SERIAL PRIMARY KEY,
   user_id INT NOT NULL,
   name VARCHAR(32) NOT NULL,
   weapon_id INT NOT NULL,
   description TEXT NOT NULL DEFAULT '',
   level INTEGER NOT NULL DEFAULT 1 CHECK (LEVEL > 0 AND LEVEL <= 10),
   xp_count INTEGER NOT NULL DEFAULT 0 CHECK (xp_count >= 0 AND xp_count < (1000+250*POW(level, 2))),
   CLASS adventurer_class NOT NULL,
   ELEMENT adventurer_element NOT NULL,
   max_health INTEGER NOT NULL DEFAULT 5 CHECK (max_health >= 5 AND max_health <= 50),
   strength INTEGER NOT NULL DEFAULT 1 CHECK (strength > 0 AND strength <= 10),
   ether INTEGER NOT NULL DEFAULT 1 CHECK (ether > 0 AND ether <= 10),
   dexterity INTEGER NOT NULL DEFAULT 1 CHECK (dexterity > 0 AND dexterity <= 10),
   agility INTEGER NOT NULL DEFAULT 1 CHECK (agility > 0 AND agility <= 10),
   charisma INTEGER NOT NULL DEFAULT 1 CHECK (charisma > 0 AND charisma <= 10),
   gold INTEGER NOT NULL DEFAULT 0 CHECK (gold > 0),
   CHECK(strength + ether + dexterity + agility + charisma + ((max_health-5)/5) - 10 <= (10+ (LEVEL-1)*4)),
   FOREIGN KEY (weapon_id) REFERENCES weapon,
   FOREIGN KEY (user_id) REFERENCES dndb_user,
   unique(name),
   unique(user_id)
); 

CREATE TABLE quest
(
      quest_id SERIAL PRIMARY KEY,
      name VARCHAR(32) NOT NULL,
      gold_reward INTEGER NOT NULL CHECK (gold_reward >= 0),
      xp_reward INTEGER NOT NULL CHECK (xp_reward > 0),
      unique(name)
);

CREATE TABLE quest_requirement
(
   quest_id INT NOT NULL,
   requirement_type adventurer_stat NOT NULL,
   amount INTEGER NOT NULL CHECK (amount > 0),
   PRIMARY KEY (quest_id, requirement_type),
   FOREIGN KEY (quest_id) REFERENCES quest ON DELETE CASCADE
);

CREATE TABLE quest_log
(
   adventurer_id INT NOT NULL,
   quest_id INT NOT NULL,
   PRIMARY KEY (adventurer_id, quest_id),
   FOREIGN KEY (adventurer_id) REFERENCES adventurer ON DELETE CASCADE,
   FOREIGN KEY (quest_id) REFERENCES quest ON DELETE CASCADE
);

CREATE TABLE trinket
(
   trinket_id SERIAL PRIMARY KEY,
   name VARCHAR(32) NOT NULL,
   unique(name)
);

CREATE TABLE quest_reward
(
   quest_id INT NOT NULL,
   trinket_id INT NOT NULL,
   amount INTEGER NOT NULL CHECK (amount > 0),
   PRIMARY KEY (quest_id, trinket_id),
   FOREIGN KEY (quest_id) REFERENCES quest ON DELETE CASCADE,
   FOREIGN KEY (trinket_id) REFERENCES trinket ON DELETE CASCADE
);

CREATE TABLE trinket_pouch
(
   adventurer_id INT NOT NULL,
   trinket_id INT NOT NULL,
   amount INTEGER NOT NULL CHECK (amount > 0),
   PRIMARY KEY (adventurer_id, trinket_id),
   FOREIGN KEY (adventurer_id) REFERENCES adventurer ON DELETE CASCADE,
   FOREIGN KEY (trinket_id) REFERENCES trinket ON DELETE CASCADE
);

CREATE TABLE accessory
(
   accessory_id SERIAL PRIMARY KEY,
   name VARCHAR(32) NOT NULL,
   gold_cost INTEGER NOT NULL CHECK (gold_cost > 0),
   unique(name)
);

CREATE TABLE inventory
(
   adventurer_id INT NOT NULL,
   accessory_id INT NOT NULL,
   is_equipped boolean NOT NULL DEFAULT FALSE,
   PRIMARY KEY (adventurer_id, accessory_id),
   FOREIGN KEY (adventurer_id) REFERENCES adventurer ON DELETE CASCADE,
   FOREIGN KEY (accessory_id) REFERENCES accessory ON DELETE CASCADE
);

CREATE TABLE accessory_boost
(
   accessory_id INT NOT NULL,
   stat_type adventurer_stat NOT NULL,
   stat_boost INTEGER NOT NULL,
   PRIMARY KEY (accessory_id, stat_type),
   FOREIGN KEY (accessory_id) REFERENCES accessory ON DELETE CASCADE
);

CREATE TABLE crafting
(
   accessory_id INT NOT NULL,
   trinket_id INT NOT NULL,
   amount INTEGER NOT NULL CHECK (amount > 0),
   PRIMARY KEY (accessory_id, trinket_id),
   FOREIGN KEY (accessory_id) REFERENCES accessory ON DELETE CASCADE,
   FOREIGN KEY (trinket_id) REFERENCES trinket ON DELETE CASCADE
);

