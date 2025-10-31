README
======

Résumé
------

Ce projet est obsolète.

Utilisation actuelle
--------------------

- Le code reste utilisé par les applications suivantes : `Epsilon.App` et `EpCrm`.

Paquets NuGet
-------------

- Les paquets NuGet liés à ce projet sont stockés sur le feed AppVeyor.
- Pour que le script `pack-and-push.ps1` puisse pousser les paquets, il faut configurer la variable d'environnement `NUGET_API_KEY` sur la machine ou l'agent CI qui exécute le script.
- La clé API (ApiKey) se trouve dans la page « Account » du projet AppVeyor — copiez la valeur et placez-la dans `NUGET_API_KEY` avant d'exécuter le script.

Génération des nouvelles versions
---------------------------------

- La génération et la publication des nouvelles versions se font via le script situé à la racine du dépôt : `pack-and-push.ps1`.

- Pour créer et pousser une nouvelle version, exécutez le script dans PowerShell depuis la racine du dépôt :

  .\pack-and-push.ps1

Remarques
---------

- Le projet est conservé pour compatibilité avec les applications mentionnées ; il n'est plus en développement actif.
- Avant d'apporter des modifications et de publier de nouveaux paquets, vérifier l'impact sur `Epsilon.App` et `EpCrm`.
