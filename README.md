# Age of War — USPN

Jeu de stratégie en 2D réalisé avec **Unity**, inspiré du principe d'*Age of War* :
deux châteaux se font face aux extrémités d'un champ de bataille, chaque camp envoie des
soldats qui avancent vers la base adverse, et le premier château détruit perd la partie.

Projet réalisé dans le cadre du cours *Méthodologie et Langages Avancés* (Master 2).

---

## Sommaire

- [Le jeu](#le-jeu)
- [Récupérer et lancer le projet](#récupérer-et-lancer-le-projet)
- [Compiler une version web (WebGL)](#compiler-une-version-web-webgl)
- [Organisation du projet](#organisation-du-projet)
- [Les classes](#les-classes)
- [Crédits](#crédits)

---

## Le jeu

### But
Détruire le château ennemi (à droite, en rouge) avant que le tien (à gauche, en bleu) ne tombe.

### Déroulement
- L'ennemi envoie automatiquement un soldat toutes les quelques secondes.
- Tu dépenses ton **or** pour invoquer tes propres unités depuis ton château.
- Les unités avancent sur une seule ligne, **font la queue derrière leurs alliés**,
  s'arrêtent au contact d'un ennemi et frappent à intervalle régulier.
- Une unité qui atteint le château adverse l'attaque jusqu'à sa destruction.
- Chaque ennemi tué rapporte de l'or (« +10 or » s'affiche au-dessus de lui).

### Unités

| Unité | Coût | Rôle |
|---|---|---|
| **Guerrier** | 20 or | Corps à corps : se bat au contact |
| **Tireur** | 15 or | À distance : s'arrête et tire dès qu'un ennemi ou le château est à portée |

Chaque unité et chaque château a une **barre de vie** (verte pour ton camp, rouge pour l'ennemi).

### Commandes
- **GUERRIER / TIREUR** (en bas) : invoquer une unité (bouton grisé si tu n'as pas assez d'or).
- **PAUSE** (en haut à droite) : mettre le jeu en pause, reprendre, rejouer ou revenir à l'accueil.
- Le compteur d'**or** est affiché en haut à gauche.

### Scènes
1. **MainMenu** — écran d'accueil : titre, boutons *Jouer* / *Quitter*, et une escarmouche animée en fond.
2. **GameScene** — la partie.

---

## Récupérer et lancer le projet

### Prérequis
- [Unity Hub](https://unity.com/download)
- **Unity 2022.3.62f2** (LTS) — à installer depuis Unity Hub.
  Ajoute le module **WebGL Build Support** si tu veux exporter la version web,
  et **Android Build Support** pour la version mobile.
- Git

### Étapes
1. Cloner le dépôt :
   ```bash
   git clone https://github.com/kuchiro/AgeOfWar.git
   ```
2. Ouvrir **Unity Hub** → **Add** (Ajouter) → **Add project from disk** → choisir le dossier `AgeOfWar`.
3. Ouvrir le projet avec la version **2022.3.62f2**.
   Le premier lancement est long : Unity régénère le dossier `Library/` (non versionné).
4. Ouvrir la scène `Assets/Scenes/MainMenu.unity` puis cliquer sur **Play** ▶.

> Si Unity propose de changer de version de l'éditeur, préférer installer la 2022.3.62f2
> pour éviter les conversions de projet.

---

## Compiler une version web (WebGL)

1. **File → Build Settings…** → sélectionner **WebGL** → *Switch Platform*.
2. Vérifier que les scènes **MainMenu** (en premier) et **GameScene** sont cochées.
3. **Build** dans le dossier `buildWeb/`.
4. Le build utilise la compression **Brotli** (`.br`). Le fichier `buildWeb/_headers`
   indique à l'hébergeur (Netlify) les bons en-têtes `Content-Encoding` ; il doit être
   présent dans le dossier publié, sinon le jeu ne se charge pas.
5. Déployer : glisser le dossier `buildWeb` dans l'onglet **Deploys** du site Netlify.

Les dossiers de build (`build/`, `buildWeb/`, `buildAndroid/`) sont ignorés par Git.

---

## Organisation du projet

```
Assets/
├── Scenes/
│   ├── MainMenu.unity        Écran d'accueil
│   └── GameScene.unity       Partie
├── Scripts/                  Logique du jeu
│   └── Visuals/              Rendu : décor, animations, barres de vie, interface
├── Prefabs/                  Unités, projectile, ancienne barre de vie
│   ├── Fall (32x32).prefab       Guerrier du joueur (Virtual Guy)
│   ├── Fall (32x32) 2.prefab     Tireur du joueur (Ninja Frog)
│   ├── Fall (32x32) 1.prefab     Soldat ennemi (Mask Dude)
│   └── Projectile.prefab
├── Pixel Adventure 1/        Pack graphique des personnages
└── TextMesh Pro/             Rendu du texte
ProjectSettings/              Réglages Unity (version, couches d'affichage…)
Packages/                     Dépendances Unity
```

Le décor (ciel, montagnes, sol, nuages), les châteaux, les barres de vie et le style de
l'interface **ne sont pas des images** : ils sont générés par le code au chargement de la scène
(voir `Battlefield` et `PixelArt`). Il n'y a donc rien à régler dans l'éditeur pour eux.

---

## Les classes

### Logique du jeu — `Assets/Scripts/`

| Classe | Rôle |
|---|---|
| **`Unit`** | Unité de corps à corps (classe de base). Avance vers la base ennemie, attend derrière un allié, frappe un ennemi au contact puis la base adverse. Gère sa vie, les dégâts, la mort et la récompense en or du tueur. |
| **`RangedUnit`** | Hérite de `Unit`. Unité à distance : s'arrête et tire un `Projectile` dès qu'un ennemi (ou le château) est à portée (`attackRange`, `fireRate`). |
| **`Projectile`** | Tir en ligne droite. Inflige des dégâts à la première unité ou base adverse touchée, crédite l'or au camp du tireur (même s'il est mort entre-temps). |
| **`BasePlayer`** | Château d'un camp (`team` 0 = joueur, 1 = ennemi). Points de vie, réserve d'or, barre de vie avec affichage « PV / PV max ». Sa destruction déclenche la fin de partie. |
| **`Spawner`** | Point d'apparition des unités. Côté ennemi, invoque automatiquement toutes les `spawnDelay` secondes ; côté joueur, utilisé par les boutons. |
| **`UIManager`** | HUD de la partie : boutons d'invocation (coût, grisés si pas assez d'or) et compteur d'or. |
| **`PauseMenu`** | Pause / reprise, écran de victoire ou de défaite, rejouer, retour à l'accueil. |
| **`MenuController`** | Boutons de l'accueil : changer de scène, quitter. |
| **`HealthBar`** | Ancienne barre de vie en UI (Slider). N'est plus utilisée, conservée pour la compatibilité du prefab. |

### Rendu — `Assets/Scripts/Visuals/`

| Classe | Rôle |
|---|---|
| **`Battlefield`** | Au chargement d'une scène : génère le décor pixel-art (ciel crépusculaire, montagnes, forêt, sol), les nuages et les châteaux de chaque camp, aligne bases, points d'apparition et unités sur la même ligne de sol (`GroundY`), puis lance le restylage de l'interface. |
| **`PixelArt`** | Boîte à outils de génération : textures et sprites, découpe des planches d'animation, bruit déterministe, tramage, sprites de l'interface (boutons, panneaux). |
| **`UnitAnimator`** | Affichage d'une unité : animation course / repos, orientation vers l'ennemi, ombre au sol, fente d'attaque, flash rouge quand elle est touchée, apparition. |
| **`WorldHealthBar`** | Barre de vie affichée dans le monde au-dessus des unités et des châteaux, avec traînée claire qui montre les dégâts récents. |
| **`HUDStyler`** | Restyle les boutons et panneaux Unity (fond sombre, liseré doré) et replace le HUD de la partie. |
| **`MenuScene`** | Écran d'accueil : décor du jeu, châteaux, escarmouche décorative en fond, titre et boutons. |
| **`DeathFade`** | Silhouette qui s'efface en montant à la mort d'une unité. |
| **`FloatingText`** | Texte flottant (ex. « +10 or »). |
| **`CloudDrift`** | Fait défiler les nuages et les fait reboucler. |

### Fonctionnement d'une partie

```
Spawner ──instancie──▶ Unit / RangedUnit ──tire──▶ Projectile
                            │                          │
                            └──── dégâts ──▶ Unit ◀────┘
                                             BasePlayer ──détruite──▶ PauseMenu.ShowGameOver()
UIManager ──bouton + or──▶ Spawner (camp joueur)
Unit tuée ──or──▶ BasePlayer du tueur
```

---

## Crédits

- Personnages et tuiles : pack **Pixel Adventure 1** de *Pixel Frog* (Unity Asset Store).
- Texte : **TextMesh Pro** (Unity).
- Décor, châteaux et interface : générés par le code du projet.
