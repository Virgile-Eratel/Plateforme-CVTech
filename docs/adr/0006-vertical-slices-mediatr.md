---
status: accepted
---
# 0006 — Vertical slices avec MediatR

La couche Application est organisée en **slices verticales** : un dossier par fonctionnalité
(ex : `Features/PostulerAnnonce/`) contenant Command/Query + Handler + Validator. Le
dispatch passe par **MediatR** ; la validation par **FluentValidation** dans un pipeline.

**Pourquoi** : chaque cas d'usage est isolé et lisible, ce qui colle au découpage métier et
facilite le TDD slice par slice. MediatR fournit aussi un point d'accroche naturel pour le
pipeline de permission/validation.

## Conséquences
- Un handler = un cas d'usage. Pas de « service fourre-tout ».
- La vérification de permission s'insère en tête de handler (cf. ADR 0004).
