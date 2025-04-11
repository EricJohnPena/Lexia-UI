CREATE TABLE crossword_data (
    id INT AUTO_INCREMENT PRIMARY KEY,
    subject_id INT NOT NULL,
    module_id INT NOT NULL,
    lesson_id INT NOT NULL,
    word VARCHAR(50) NOT NULL,
    start_row INT NOT NULL,
    start_col INT NOT NULL,
    horizontal BOOLEAN NOT NULL,
    clue TEXT NOT NULL
);

-- Insert initial crossword data
INSERT INTO crossword_data (subject_id, module_id, lesson_id, word, start_row, start_col, horizontal, clue) VALUES
(1, 1, 101, 'SATURN', 6, 2, TRUE, 'It is a gas giant, and the second largest and second most massive planet in our Solar System.'),
(1, 1, 101, 'EARTH', 3, 4, FALSE, 'The third planet from the Sun and the only astronomical object known to harbor life.'),
(1, 1, 101, 'VENUS', 3, 5, FALSE, 'It\'s the hottest planet in our solar system.'),
(1, 1, 101, 'MARS', 4, 6, FALSE, 'The second smallest planet in the solar system, only larger than Mercury and slightly more than half the size of Earth.'),
(1, 1, 101, 'SUN', 6, 2, FALSE, 'The star that provides light and heat for the earth and around which the earth moves.');
