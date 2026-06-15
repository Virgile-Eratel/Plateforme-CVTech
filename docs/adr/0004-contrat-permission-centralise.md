---
status: accepted
---
# 0004 — `IVerificateurPermission` centralisé dans GestionIdentite

`GestionIdentite` est le **seul** module à connaître la `MatricePermissions`. Les autres
modules l'interrogent via le contrat public `IVerificateurPermission`, injecté par leur
`ModuleLoader`. Chaque handler de cas d'usage vérifie la permission en **première ligne**,
avant toute action métier, et lève une `PermissionRefuseeException` en cas de refus.

**Pourquoi** : centraliser la matrice évite qu'elle se disperse et diverge entre modules.
La vérification systématique en tête de handler est un critère noté, prouvé par un test.

## Conséquences
- Aucun module ne lit la base de `GestionIdentite` directement.
- La matrice du README est la source de vérité traduite dans `MatricePermissions`.
