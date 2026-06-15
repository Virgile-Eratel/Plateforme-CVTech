---
status: accepted
---
# 0009 — Notifications in-app temps réel via SignalR

Le canal de notification des abonnements par domaine est **in-app temps réel**, implémenté
avec **SignalR**. À la réception d'un événement `AnnoncePubliee` / `AppelOffrePublie`,
`ActualiteEtAbonnement` pousse une `Notification` aux seuls abonnés du `DomaineMetier`
concerné, connectés via un hub.

**Pourquoi** : démonstration visuelle immédiate (la notif apparaît dans l'UI à la
publication), 100 % .NET, sans dépendance externe ni serveur SMTP à provisionner sur Azure.

## Conséquences
- `CanalDiffusion` retenu = in-app. Le modèle reste ouvert à un canal e-mail ultérieur.
- Le ciblage se fait par groupe SignalR = `DomaineMetier`.
