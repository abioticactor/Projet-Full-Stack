# 🐲 PokéDesc

> **Devinez. Collectionnez. Combattez.**
> Un jeu web stratégique où votre connaissance des Pokémon détermine votre puissance au combat.

![Status](https://img.shields.io/badge/Status-Development-orange)
![Backend](https://img.shields.io/badge/Backend-ASP.NET%20Core-purple)
![Frontend](https://img.shields.io/badge/Frontend-Razor%20Pages-blue)
![Database](https://img.shields.io/badge/Database-MongoDB-green)

---

## 📖 À propos

**PokéDesc** est un jeu multijoueur qui teste votre culture Pokémon. Le concept est unique : **plus vous avez besoin d'indices pour identifier un Pokémon, plus ses statistiques seront faibles lors du combat qui suivra.**

Le projet est construit sur une architecture **N-Tiers** robuste utilisant la stack .NET moderne et MongoDB.

---

## 🎮 Mécaniques de Jeu

Le jeu se déroule en deux phases distinctes liées par un système de risque/récompense.

### Phase 1 : La Devinette (PokéDesc)
Au début d'une manche, 6 Pokémon sont tirés au sort (1% de chance d'être Légendaire/Mythique).
Le joueur doit identifier le Pokémon caché.

* **Score de départ :** 100 Points.
* **Indices :** Le joueur peut acheter des indices, ce qui réduit son score potentiel.
    * *Exemples :* Type (-15 pts), Silhouette (-15 pts), Cri (-10 pts), Talents (-10 pts), etc.
* **Impact sur le combat :** Le score final détermine un coefficient de puissance ($K$).
    * Score 100 (Parfait) = 100% des statistiques.
    * Score 0 (Tout révélé) = 50% des statistiques.

$$K = 0.5 + \frac{Score}{200}$$

### Phase 2 : Le Combat
Affrontez l'adversaire (ou l'IA) avec le Pokémon que vous venez de deviner.

* **Système :** Tour par tour classique.
* **Stats :** Calculées ainsi : $Stat_{Combat} = Stat_{Base} \times K$.
* **Modes :**
    * *Équitable :* Tous les Pokémon sont ramenés au Niveau 50.
    * *Classique :* Utilise le niveau réel de votre collection.
* **Récompenses :** Objets de devinette (Loupe, Joker de type...).

### 📈 Progression
* **XP Dresseur :** Augmente à chaque bonne réponse et victoire.
* **Collection (Pokédex) :** Chaque Pokémon deviné rejoint votre Pokédex personnel.
* **Évolution :** Un Pokémon évolue s'il est présent dans votre Pokédex au **Niveau > 10**.

---

## 🛠️ Architecture Technique

Ce projet respecte une séparation stricte des responsabilités (Architecture N-Tiers).

### Stack Technologique
* **Frontend :** ASP.NET Core Razor Pages (HTML/CSS/JS).
* **Backend :** ASP.NET Core Web API (.NET 8+).
* **Base de données :** MongoDB (NoSQL).
* **DevOps :** Azure DevOps (CI/CD).

### Modèle de Données (Aperçu)
* **Pokémons :** Données statiques (Stats base, Sprites, Cris...).
* **Players :** Données dynamiques (Inventaire, Pokédex avec niveaux individuels, Historique).

---

## 🚀 Installation & Démarrage

### Prérequis
* [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0 ou supérieure)
* [MongoDB](https://www.mongodb.com/try/download/community) (Local ou Atlas)

### Étapes
1.  **Cloner le dépôt :**
    ```bash
    git clone [https://github.com/votre-pseudo/pokedesc.git](https://github.com/votre-pseudo/pokedesc.git)
    ```
2.  **Configurer la Base de Données :**
    Mettez à jour la chaîne de connexion dans `appsettings.json` :
    ```json
    "ConnectionStrings": {
      "MongoDbConnection": "mongodb://localhost:27017/PokeDescDB"
    }
    ```
3.  **Lancer l'application :**
    ```bash
    dotnet run
    ```
4.  Accédez à `https://localhost:5001` dans votre navigateur.

---

## 🗺️ Roadmap

### Phase 1 (Actuelle)
- [ ] Moteur de devinette et calcul de pénalités.
- [ ] Système de combat basique (Dégâts directs).
- [ ] Gestion des comptes et persistance MongoDB.

### Phase 2 (Prochainement)
- [ ] Implémentation des Status (Poison, Paralysie, Sommeil).
- [ ] Boosts de statistiques en combat (Buffs/Debuffs).
- [ ] Talents passifs des Pokémon.

### Phase 3 (Futur)
- [ ] Modes de jeu alternatifs (Draft, Coop).
- [ ] Système de Guildes.

---

## 🤝 Contribuer
Les contributions sont les bienvenues ! Veuillez consulter le fichier `CONTRIBUTING.md` pour les directives.

## 📄 Licence
Distribué sous la licence MIT. Voir `LICENSE` pour plus d'informations.

---
*PokéDesc est un projet fan-made à but non lucratif. Pokémon est une marque déposée de Nintendo, Creatures Inc. et Game Freak.*