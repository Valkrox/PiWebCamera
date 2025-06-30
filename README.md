# 📷 PiWebCamera – Mini serveur d'affichage d'image dans le navigateur

**PiWebCamera** est un petit programme qui permet d'afficher une image (prise par une webcam ou autre source) dans un navigateur via un simple serveur web local.

---

## 💡 Fonctionnement

- Lance un petit serveur HTTP sur le port de ton choix (par défaut : `http://localhost:8080`)
- Affiche une image unique ou actualisée à intervalles réguliers
- Compatible avec Raspberry Pi, Linux, Windows

---

## 📦 Fonctionnalités

- Affichage d’une image statique (`.jpg`, `.png`, etc.)
- Peut être couplé avec une capture périodique (à configurer)
- Léger, sans dépendances lourdes
- Peut être lancé en tâche de fond sur un système embarqué

---

## 🔧 Utilisation

1. Lance le programme :
   ```bash
   ./PiWebCamera
