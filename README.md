# CNAM-DB-Dungeons-and-Databases
Projet de base de données pour le 2e semestre du CNAM Enjmin

## Lancement du projet
Il suffit de se mettre à la racine du projet et de lancer un `docker compose up --build`.  
Ensuite, l'application sera disponible à l'adresse https://localhost:49169/

## Positionnement du code SQL
Le code SQL, entièrement testé et fonctionnel se trouve dans le dossier /Dungeons&Databases/SqlScripts  
Il est également possible de trouver du code de requêtage, de mise à jour et d'insertion dans les `#region Queries` des différents fichiers de service (situés sous /Dungeons&Databases/Data)

## Fonctionnalités et concepts

Il est possible de créer un compte au lancement de l'application  
- Salage et hash du mot de passe, formulaire de création avec validation des champs.

Il est demandé de créer un aventurier une fois connecté
- Encore formulaire avec validation des champs, mais contenant aussi des contraintes inter-champs et faisant usage de domaines.

Un aventurier peut participer à des quêtes 
- La quête dépend de beaucoup de contraintes et offre de nombreuses récompenses
- Géré par une fonction PL/PGSQL complexe qui vérifie et distribue
- Une fonction s'occupe également du gain d'expérience du personnage

Un aventurier peut créer des objets 
- À l'aide des récompenses obtenues pendant les quêtes, un aventurier peut créer des accessoires
- Une fonction gère la création d'accessoires avec les bonnes contraintes et l'ajoute à l'inventaire
- Un trigger vérifie qu'un utilisateur ne peut pas équiper plus de deux accessoires à la fois.

Tous les éléments intéragissent entre eux pour donner une dynamique à l'application  (l'expérience fait monter de niveau, ce qui permet d'avoir plus de compétences et donc de faire des quêtes plus complexes, ce qui permet d'avoir d'autres accessoires, qui augmentent ces compétences...)

# Rétrospective de projet
J'ai appris énormément de choses pendant ce projet. Je suis content d'avoir choisi blazor comme langage front, car j'ai pu découvrir cette technologie qui m'était totalement étrangère auparavant.  
Malgré mes compétences passées en Postgresql, j'ai été surpris à découvrir des choses, surtout la vitesse d'exécution des requêtes en base avec l'utilité des triggers qui accélèrent le tout.  
Je suis très content d'avoir réussi à finir le projet (malgré 3h30 de retard sur le rendu final), et d'avoir pu mettre en place tout ce que je souhaitais, mis à part l'interaction entre les utilisateurs.
