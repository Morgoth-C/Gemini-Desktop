# 🌌 Gemini Desktop (Unofficial)

Une application de bureau fluide, moderne et autonome pour Google Gemini, développée en C# / WPF avec WebView2.

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)
![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-lightgrey.svg)

---

## 📥 Téléchargement Direct (Version Portable)

Pour exécuter directement l'application sans compiler ni installer Visual Studio :

👉 **[Télécharger Gemini Desktop v1.0.0 (Portable .zip)](https://github.com/Morgoth-C/Gemini-Desktop/releases/tag/v1.0.0)**

1. Téléchargez le fichier `.zip` depuis la release.
2. Extrayez le dossier où vous le souhaitez.
3. Lancez `Gemini.exe`.

---

## ⚠️ Avertissement / Disclaimer
> **Note importante :** Cette application est un projet **non-officiel** développé de manière indépendante. 
> Elle n'est **en aucun cas affiliée, associée, autorisée, maintenue ou sponsorisée par Google LLC** ou l'une de ses filiales.

---

## ✨ Fonctionnalités
- 🎨 **Design Windows 11 Fluent** : Fenêtre personnalisée avec effets de transparence DWM.
- 🖼️ **Icône Haute Définition** : Format `.ico` multi-résolution réajusté sans marges transparentes.
- 🔒 **Mode Portable & Sécurisé** : Isolation de la session WebView2.
- ⌨️ **Raccourci Global** : Masquer / Afficher l'application rapidement (`Ctrl` + `Alt` + `G`).
- ↩️ **Bouton de Retour Rapide** : Retour direct à Gemini en cas de navigation externe.

---

## 🛠️ Reconstruire / Compiler l'Application

Si vous préférez compiler l'application vous-même depuis le code source :

### Prérequis
- [SDK .NET 10.0](https://dotnet.microsoft.com/) (ou version supérieure).
- Windows 10 / 11 (x64).

### Étapes de compilation

1. **Cloner le dépôt :**
   ```bash
   git clone [https://github.com/Morgoth-C/Gemini-Desktop.git](https://github.com/Morgoth-C/Gemini-Desktop.git)
   cd Gemini-Desktop
