---
status: accepted
---
# 0001 — Architecture en monolithe modulaire

Le sujet impose de réunir 4 domaines (identité, emploi, freelance, actualité) dans une
plateforme unique livrable en fin de semaine. Nous adoptons un **monolithe modulaire** :
un seul processus déployable, mais découpé en modules étanches communiquant par contrats
publics ou bus d'événements interne.

**Pourquoi** : on conserve la clarté des frontières d'un système distribué (DDD, étanchéité)
sans le coût opérationnel (réseau, orchestration, déploiements multiples) qui serait
ingérable dans le délai imparti et superflu pour la charge visée.

## Alternatives écartées
- **Microservices** : surcoût d'infra et de déploiement disproportionné pour un TP.
- **Monolithe en couches classique (non modulaire)** : ne garantit pas l'étanchéité métier
  exigée par les critères d'évaluation.
