---
status: accepted
---
# 0002 — Découpage en 5 couches par module

Chaque module est structuré en 5 couches : **Client** (Minimal APIs, DTOs, mappings),
**Application** (vertical slices : Commands/Queries/Handlers/Validators),
**Domaine** (entités riches, agrégats, value objects, exceptions métier, 100 % FR),
**Infrastructure** (EF Core, services externes, génération RSS),
**Loader** (`ModuleLoader.cs`, composition root d'injection de dépendances).

**Pourquoi** : la dépendance ne va que vers l'intérieur (Client → Application → Domaine ;
Infrastructure implémente des ports du Domaine). Le Domaine ne référence aucune techno.
C'est ce qui rend chaque module testable et étanche, et c'est explicitement noté.

## Conséquences
- Interdit : `DbContext` dans le Domaine, logique métier dans un Controller.
- La couche Client est la **seule** porte d'entrée visible d'un module.
