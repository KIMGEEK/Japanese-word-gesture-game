# 🪄 Wordcraft Magic
**Story-Based Japanese Vocabulary Learning Game with Gesture Recognition**

---

## 👥 Team Members

| Name | Role | GitHub ID |
|------|------|------------|
| 백하준 (Hajun Baek) | Game Logic / Gesture Recognition | [@KIMGEEK](https://github.com/KIMGEEK) |
| 윤현섭 (Hyunseob Yoon) | Project Lead / Game Logic & System Integration | [@Yoonhsub](https://github.com/Yoonhsub) |
| 박재인 (Jaein Park) | Database & Learning Module / Backend | @ |
| 최윤서 (Yunseo Choi) | UI / UX Design & Animation | @ |

---

## 🧩 Project Overview

**Wordcraft Magic** is a story-based Japanese vocabulary learning **SRPG (Strategy Role-Playing Game)** that combines **gesture recognition** and **language learning**.  
Players learn Japanese words through motion-based spellcasting—each gesture represents a magical command created by connecting hiragana characters.

> “Drawing a spell becomes learning itself.”

The system encourages motivation, creativity, and immersion by turning vocabulary learning into interactive storytelling.

---

## ⚙️ System Architecture

The system is composed of three main modules:

1. **Input Recognition Module**  
   Detects and interprets hand gestures or touch movements using computer vision (MediaPipe & OpenCV).
2. **Game Logic Module**  
   Links recognized gestures to vocabulary data, determining battle outcomes and story progress.
3. **Learning Data Management Unit**  
   Records user performance (accuracy, progress, and response time) using SQLite for local storage.

---

## 🕹️ Core Features

- 🎮 **Gesture-Based Word Formation**  
  Connect hiragana letters through motion input to craft spells.
- 📖 **Story-Driven Learning Progression**  
  Experience vocabulary as part of a wizard’s journey across Japan.
- ⚔️ **Interactive RPG Battles**  
  Use learned words as attacks in turn-based combat.
- 🧠 **Adaptive Learning System**  
  Difficulty adjusts to player performance through onboarding and tutorial stages.
- 💾 **Data Management**  
  Local save/load support for learning continuity.

---

## 🧠 Concept & Idea

Inspired by the notion that drawing gestures resembles **casting magic**,  
the player learns vocabulary by **literally crafting and casting words as spells**.  
Each gesture connects linguistic understanding with physical motion, reinforcing retention through action.

---

## 🎬 Demonstration

> Below is a short clip showing real-time gesture recognition and spellcasting demo.

![Gesture Recognition Demo](./assets/Vision_demo_finger.gif)

```markdown
