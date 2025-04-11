CREATE TABLE IF NOT EXISTS jumbled_letters_questions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    question_text VARCHAR(255) NOT NULL,
    answer VARCHAR(50) NOT NULL,
    fk_subject_id INT NOT NULL,
    fk_module_id INT NOT NULL,
    fk_lesson_id INT NOT NULL
);

-- Insert sample questions
INSERT INTO jumbled_letters_questions (question_text, answer, fk_subject_id, fk_module_id, fk_lesson_id)
VALUES
    ("Unscramble this word", "UNITY", 1, 1, 1),
    ("Another word to solve", "GAME", 1, 1, 1),
    ("A planet in our solar system", "EARTH", 2, 1, 2),
    ("The hottest planet", "VENUS", 2, 1, 2),
    ("The red planet", "MARS", 2, 1, 2),
    ("A gas giant", "SATURN", 2, 1, 2),
    ("The star at the center of our solar system", "SUN", 2, 1, 2);
