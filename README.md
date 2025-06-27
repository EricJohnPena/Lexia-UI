# Lexia-UI: Educational Learning Platform

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📖 Overview

Lexia-UI is an interactive educational learning platform built with Unity that provides engaging game-based learning experiences for students. Features multiple game modes, progress tracking, and comprehensive database integration.

### 🎯 Key Features

- **Multiple Game Modes**: Classic, Jumbled Letters, and Crossword puzzles
- **Subject Support**: English and Science curriculum
- **Progress Tracking**: Real-time analytics and performance metrics
- **User Profiles**: Customizable profiles with progress visualization
- **Leaderboard System**: Competitive learning environment
- **Adaptive Learning**: Complex word detection and difficulty adjustment

## 🏗️ Architecture

### Frontend (Unity)
- **Game Engine**: Unity 2022.3+
- **Scripting**: C#
- **UI Framework**: Unity UI (UGUI)
- **Data Serialization**: Newtonsoft.Json

### Backend (PHP)
- **Server Language**: PHP
- **Database**: MySQL
- **API Architecture**: RESTful endpoints

## 📁 Project Structure

```
Lexia-UI/
├── Assets/
│   ├── Scripts/
│   │   ├── DBScripts/           # Database integration
│   │   ├── GameScripts/         # Core game logic
│   │   └── MenuScripts/         # Menu and navigation
│   ├── Prefabs/                 # Reusable UI components
│   ├── Scenes/                  # Unity scenes
│   └── StreamingAssets/         # Game data and images
├── server/                      # PHP backend API
├── database/                    # Database schema
└── tracking/                    # Analytics tracking
```

## 🎮 Game Modes

### 1. Classic Mode
- Traditional word-based questions with keyboard input
- Image-based questions with hint system
- Progress tracking and skip functionality

### 2. Jumbled Letters
- Unscramble letters to form correct words
- Multiple difficulty levels with complex word detection

### 3. Crossword
- Interactive crossword puzzles
- Dynamic grid generation with clue-based hints

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or higher
- PHP 7.4+ with MySQL support
- MySQL 5.7+ or MariaDB 10.3+

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-username/Lexia-UI.git
   cd Lexia-UI
   ```

2. **Database Setup**
   ```bash
   mysql -u username -p database_name < database/u255088217_test_mydb.sql
   ```

3. **Backend Configuration**
   - Update `server/db_connection.php` with your database credentials

4. **Unity Setup**
   - Open project in Unity 2022.3+
   - Install required packages
   - Configure build settings

## 🔧 Development

### Adding New Game Modes
1. Create scripts in `Assets/Scripts/GameScripts/`
2. Add PHP endpoints in `server/`
3. Update database schema if needed
4. Create UI prefabs

### API Development
```php
<?php
require_once 'db_connection.php';
header('Content-Type: application/json');

try {
    $response = ['success' => true, 'data' => $result];
} catch (Exception $e) {
    $response = ['success' => false, 'error' => $e->getMessage()];
}

echo json_encode($response);
?>
```

## 📊 Analytics & Performance

### Performance Metrics
- Accuracy, Speed, Consistency
- Retention, Problem Solving
- Vocabulary Range

### Data Visualization
- Radar Charts for skill assessment
- Progress tracking
- Leaderboards

## 🚀 Deployment

### Android Build
1. Configure Android build settings
2. Set up signing configuration
3. Build APK/AAB

### WebGL Build
1. Configure WebGL settings
2. Deploy to web server
3. Configure PHP backend

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Open Pull Request

## 📝 License

MIT License - see [LICENSE](LICENSE) file for details.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/your-username/Lexia-UI/issues)
- **Email**: support@lexia-ui.com

---

**Made with ❤️ for educational excellence**
