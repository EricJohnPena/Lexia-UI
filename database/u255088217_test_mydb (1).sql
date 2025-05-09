-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1:3306
-- Generation Time: May 09, 2025 at 06:35 AM
-- Server version: 10.11.10-MariaDB-log
-- PHP Version: 7.2.34

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `u255088217_test_mydb`
--

-- --------------------------------------------------------

--
-- Table structure for table `classic_questions_tbl`
--

CREATE TABLE `classic_questions_tbl` (
  `question_id` int(11) NOT NULL,
  `fk_subject_id` int(11) DEFAULT NULL,
  `fk_module_id` int(11) DEFAULT NULL,
  `fk_lesson_id` int(11) DEFAULT NULL,
  `question_text` text NOT NULL,
  `answer` varchar(255) NOT NULL,
  `is_complex` tinyint(1) NOT NULL,
  `image_path` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `classic_questions_tbl`
--

INSERT INTO `classic_questions_tbl` (`question_id`, `fk_subject_id`, `fk_module_id`, `fk_lesson_id`, `question_text`, `answer`, `is_complex`, `image_path`) VALUES
(4, 1, 1, 2, 'Wht is at the center of the solar system?', 'SUN', 0, 'Images/sun.jpg'),
(5, 1, 1, 1, 'What is the third planet from the Sun?', 'EARTH', 0, 'Images/earth.jpg'),
(6, 1, 1, 1, 'Largest planet in our solar system', 'JUPITER', 1, 'Images/jupiter.jpg'),
(7, 1, 1, 1, 'What is this', 'SUN', 0, 'Images/sun.jpg');

-- --------------------------------------------------------

--
-- Table structure for table `class_summary_tbl`
--

CREATE TABLE `class_summary_tbl` (
  `summary_id` int(11) NOT NULL,
  `fk_section_id` int(11) DEFAULT NULL,
  `fk_subject_id` int(11) DEFAULT NULL,
  `avg_accuracy` int(11) DEFAULT NULL,
  `avg_speed` int(11) DEFAULT NULL,
  `avg_consistency` int(11) DEFAULT NULL,
  `avg_retention` int(11) DEFAULT NULL,
  `avg_problem_solving_skills` int(11) DEFAULT NULL,
  `avg_vocabulary_range` int(11) DEFAULT NULL,
  `cluster_label` int(11) DEFAULT NULL,
  `avg_feedback` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `crossword_data`
--

CREATE TABLE `crossword_data` (
  `id` int(11) NOT NULL,
  `subject_id` int(11) NOT NULL,
  `module_id` int(11) NOT NULL,
  `lesson_id` int(11) NOT NULL,
  `word` varchar(50) NOT NULL,
  `start_row` int(11) NOT NULL,
  `start_col` int(11) NOT NULL,
  `horizontal` tinyint(1) NOT NULL,
  `clue` text NOT NULL,
  `is_complex` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Dumping data for table `crossword_data`
--

INSERT INTO `crossword_data` (`id`, `subject_id`, `module_id`, `lesson_id`, `word`, `start_row`, `start_col`, `horizontal`, `clue`, `is_complex`) VALUES
(1, 1, 1, 1, 'ABCDEFG', 6, 2, 1, 'It is a gas giant, and the second largest and second most massive planet in our Solar System.', 1),
(2, 1, 1, 1, 'QWERTYU', 3, 4, 0, 'The third planet from the Sun and the only astronomical object known to harbor life.', 0),
(3, 1, 1, 1, 'VENUS', 3, 5, 0, 'It\'s the hottest planet in our solar system.', 0),
(4, 1, 1, 1, 'MARS', 4, 6, 0, 'The second smallest planet in the solar system, only larger than Mercury and slightly more than half the size of Earth.', 0),
(5, 2, 1, 1, 'SUN', 6, 2, 0, 'The star that provides light and heat for the earth and around which the earth moves.', 0);

-- --------------------------------------------------------

--
-- Table structure for table `game_modes_tbl`
--

CREATE TABLE `game_modes_tbl` (
  `game_mode_id` int(11) NOT NULL,
  `game_mode_name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `game_modes_tbl`
--

INSERT INTO `game_modes_tbl` (`game_mode_id`, `game_mode_name`) VALUES
(1, 'Classic'),
(2, 'Jumbled Letters'),
(3, 'Crossword');

-- --------------------------------------------------------

--
-- Table structure for table `game_mode_mapping_tbl`
--

CREATE TABLE `game_mode_mapping_tbl` (
  `mapping_id` int(11) NOT NULL,
  `fk_game_mode_id` int(11) NOT NULL,
  `fk_subject_id` int(11) NOT NULL,
  `fk_module_id` int(11) NOT NULL,
  `fk_lesson_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `game_mode_mapping_tbl`
--

INSERT INTO `game_mode_mapping_tbl` (`mapping_id`, `fk_game_mode_id`, `fk_subject_id`, `fk_module_id`, `fk_lesson_id`) VALUES
(1, 1, 1, 1, 1),
(2, 2, 1, 1, 1),
(3, 3, 2, 1, 2);

-- --------------------------------------------------------

--
-- Table structure for table `jumbled_letters_questions`
--

CREATE TABLE `jumbled_letters_questions` (
  `id` int(11) NOT NULL,
  `question_text` varchar(255) NOT NULL,
  `answer` varchar(50) NOT NULL,
  `is_complex` tinyint(1) NOT NULL,
  `fk_subject_id` int(11) NOT NULL,
  `fk_module_id` int(11) NOT NULL,
  `fk_lesson_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Dumping data for table `jumbled_letters_questions`
--

INSERT INTO `jumbled_letters_questions` (`id`, `question_text`, `answer`, `is_complex`, `fk_subject_id`, `fk_module_id`, `fk_lesson_id`) VALUES
(1, 'Color of the sky', 'BLUE', 0, 1, 1, 1),
(2, 'Opposite of rich', 'POOR', 0, 1, 1, 1),
(3, 'A planet in our solar system', 'EARTH', 0, 2, 1, 2),
(4, 'The hottest planet', 'VENUS', 0, 2, 1, 2),
(5, 'The red planet', 'MARS', 0, 2, 1, 2),
(6, 'A gas giant', 'SATURN', 1, 1, 1, 2),
(7, 'The star at the center of our solar system', 'SUN', 0, 1, 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `lessons_tbl`
--

CREATE TABLE `lessons_tbl` (
  `lesson_id` int(11) NOT NULL,
  `fk_module_number` int(11) DEFAULT NULL,
  `fk_subject_id` int(11) DEFAULT NULL,
  `lesson_number` int(11) DEFAULT NULL,
  `lesson_name` varchar(256) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `lessons_tbl`
--

INSERT INTO `lessons_tbl` (`lesson_id`, `fk_module_number`, `fk_subject_id`, `lesson_number`, `lesson_name`) VALUES
(1, 1, 1, 1, 'Lesson 1 Title Here: English'),
(2, 1, 2, 1, 'Lesson 1 Title Here: Science'),
(3, 2, 1, 2, 'Lesson 2 Title Here: English'),
(4, 2, 2, 2, 'Lesson 2 Title Here: Science'),
(5, 3, 1, 3, 'Lesson 3 Title Here: English'),
(6, 1, 1, 2, 'Lesson 1.1 ');

-- --------------------------------------------------------

--
-- Table structure for table `levels_tbl`
--

CREATE TABLE `levels_tbl` (
  `level_id` int(11) NOT NULL,
  `fk_lesson_id` int(11) DEFAULT NULL,
  `level_number` int(11) NOT NULL,
  `total_items` int(11) NOT NULL,
  `game_mode` varchar(12) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `levels_tbl`
--

INSERT INTO `levels_tbl` (`level_id`, `fk_lesson_id`, `level_number`, `total_items`, `game_mode`) VALUES
(1, 1, 1, 5, 'jumbled'),
(2, 2, 1, 5, 'crossword');

-- --------------------------------------------------------

--
-- Table structure for table `modules_tbl`
--

CREATE TABLE `modules_tbl` (
  `module_id` int(11) NOT NULL,
  `fk_subject_id` int(11) DEFAULT NULL,
  `module_number` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `modules_tbl`
--

INSERT INTO `modules_tbl` (`module_id`, `fk_subject_id`, `module_number`) VALUES
(1, 1, 1),
(2, 2, 1),
(3, 1, 2),
(4, 2, 2),
(5, 1, 3);

-- --------------------------------------------------------

--
-- Table structure for table `sections_tbl`
--

CREATE TABLE `sections_tbl` (
  `section_id` int(11) NOT NULL,
  `section_name` varchar(12) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `sections_tbl`
--

INSERT INTO `sections_tbl` (`section_id`, `section_name`) VALUES
(1, 'Emerald'),
(2, 'Ruby'),
(3, 'Sapphire'),
(4, 'Diamond');

-- --------------------------------------------------------

--
-- Table structure for table `students_progress_tbl`
--

CREATE TABLE `students_progress_tbl` (
  `progress_id` int(11) NOT NULL,
  `student_id` int(11) DEFAULT NULL,
  `module_id` int(11) NOT NULL,
  `fk_game_mode_id` int(11) NOT NULL,
  `fk_subject_id` int(11) NOT NULL,
  `no_of_attempts` int(11) NOT NULL,
  `solve_time` int(11) DEFAULT NULL,
  `completion_status` varchar(10) DEFAULT NULL,
  `accuracy` int(11) DEFAULT NULL,
  `consistency` int(11) DEFAULT NULL,
  `speed` int(11) DEFAULT NULL,
  `retention` int(11) DEFAULT NULL,
  `problem_solving_skills` int(11) DEFAULT NULL,
  `vocabulary_range` int(11) DEFAULT NULL,
  `post_assessment_score` int(11) DEFAULT NULL,
  `cluster_label` int(11) DEFAULT NULL,
  `feedback` varchar(45) DEFAULT NULL,
  `score` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `students_tbl`
--

CREATE TABLE `students_tbl` (
  `student_id` int(11) NOT NULL,
  `fk_section_id` int(11) DEFAULT NULL,
  `username` varchar(10) NOT NULL,
  `email` varchar(45) NOT NULL,
  `password` varchar(12) NOT NULL,
  `first_name` varchar(45) NOT NULL,
  `last_name` varchar(45) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `students_tbl`
--

INSERT INTO `students_tbl` (`student_id`, `fk_section_id`, `username`, `email`, `password`, `first_name`, `last_name`) VALUES
(1, 1, 'user1', 'user1@email.com', '123', 'John', 'Doe'),
(2, 2, 'user2', 'user2@email.com', '123', 'Mary', 'Sue'),
(3, 3, 'user3', 'user3@email.com', '123', 'Juan', 'Dela Cruz'),
(4, 1, 'user4', 'user4@email.com', '123', 'Gary', 'Tsu'),
(5, 1, 'user5', 'user5@email.com', '123', 'Jane', 'Smith'),
(6, 1, 'user6', 'user6@email.com', '123', 'John', 'Smith'),
(7, 1, 'user7', 'user6@email.com', '123', 'Alden', 'Richards'),
(8, 1, 'user17', 'user7@email.com', '123', 'Emily', 'Johnson'),
(9, 1, 'user8', 'user8@email.com', '123', 'Michael', 'Williams'),
(10, 1, 'user9', 'user9@email.com', '123', 'Jessica', 'Brown'),
(11, 1, 'user10', 'user10@email.com', '123', 'David', 'Davis'),
(12, 1, 'user11', 'user11@email.com', '123', 'Ashley', 'Rodriguez'),
(13, 1, 'user12', 'user12@email.com', '123', 'Christopher', 'Wilson'),
(14, 1, 'user13', 'user13@email.com', '123', 'Sarah', 'Martinez'),
(15, 1, 'user14', 'user14@email.com', '123', 'Kevin', 'Anderson'),
(16, 1, 'user15', 'user15@email.com', '123', 'Amanda', 'Taylor'),
(17, 1, 'user16', 'user16@email.com', '123', 'Joshua', 'Thomas');

-- --------------------------------------------------------

--
-- Table structure for table `subjects_tbl`
--

CREATE TABLE `subjects_tbl` (
  `subject_id` int(11) NOT NULL,
  `subject_name` varchar(45) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `subjects_tbl`
--

INSERT INTO `subjects_tbl` (`subject_id`, `subject_name`) VALUES
(1, 'English'),
(2, 'Science');

-- --------------------------------------------------------

--
-- Table structure for table `teachers_tbl`
--

CREATE TABLE `teachers_tbl` (
  `teacher_id` int(11) NOT NULL,
  `fk_section_id` int(11) DEFAULT NULL,
  `first_name` varchar(45) NOT NULL,
  `last_name` varchar(45) NOT NULL,
  `email` varchar(45) NOT NULL,
  `password` varchar(45) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

--
-- Dumping data for table `teachers_tbl`
--

INSERT INTO `teachers_tbl` (`teacher_id`, `fk_section_id`, `first_name`, `last_name`, `email`, `password`) VALUES
(1, 1, 'Teacher', 'Ai', 'teaacher1@email.com', '123'),
(2, 2, 'Teacher', 'Bee', 'teacher2@email.com', '123'),
(3, 3, 'Teacher', 'Sy', 'teacher3@email.com', '123'),
(4, 1, 'eqweqwe', 'qwe', 'wewqeqw@email.com', '123'),
(5, 1, 'Teacher', 'Eli', 'teacher5@email.com', '123');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `classic_questions_tbl`
--
ALTER TABLE `classic_questions_tbl`
  ADD PRIMARY KEY (`question_id`),
  ADD KEY `classic_subject_id` (`fk_subject_id`),
  ADD KEY `classic_module_number` (`fk_module_id`),
  ADD KEY `classic_lesson_id` (`fk_lesson_id`);

--
-- Indexes for table `class_summary_tbl`
--
ALTER TABLE `class_summary_tbl`
  ADD PRIMARY KEY (`summary_id`),
  ADD KEY `summary_section_id_idx` (`fk_section_id`),
  ADD KEY `summary_subject_id_idx` (`fk_subject_id`);

--
-- Indexes for table `crossword_data`
--
ALTER TABLE `crossword_data`
  ADD PRIMARY KEY (`id`),
  ADD KEY `crossword_subject_id` (`subject_id`),
  ADD KEY `crossword_module_id` (`module_id`),
  ADD KEY `crossword_lesson_id` (`lesson_id`);

--
-- Indexes for table `game_modes_tbl`
--
ALTER TABLE `game_modes_tbl`
  ADD PRIMARY KEY (`game_mode_id`);

--
-- Indexes for table `game_mode_mapping_tbl`
--
ALTER TABLE `game_mode_mapping_tbl`
  ADD PRIMARY KEY (`mapping_id`),
  ADD KEY `fk_game_mode_id` (`fk_game_mode_id`),
  ADD KEY `fk_subject_id` (`fk_subject_id`),
  ADD KEY `fk_module_id` (`fk_module_id`),
  ADD KEY `fk_lesson_id` (`fk_lesson_id`);

--
-- Indexes for table `jumbled_letters_questions`
--
ALTER TABLE `jumbled_letters_questions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `jumbled_subject_id` (`fk_subject_id`),
  ADD KEY `jumbled_module_id` (`fk_module_id`),
  ADD KEY `jumbled_lesson_id` (`fk_lesson_id`);

--
-- Indexes for table `lessons_tbl`
--
ALTER TABLE `lessons_tbl`
  ADD PRIMARY KEY (`lesson_id`),
  ADD KEY `fk_module_number` (`fk_module_number`),
  ADD KEY `fk_subject_id` (`fk_subject_id`);

--
-- Indexes for table `levels_tbl`
--
ALTER TABLE `levels_tbl`
  ADD PRIMARY KEY (`level_id`),
  ADD KEY `level_lesson_number_idx` (`fk_lesson_id`) USING BTREE;

--
-- Indexes for table `modules_tbl`
--
ALTER TABLE `modules_tbl`
  ADD PRIMARY KEY (`module_id`),
  ADD KEY `fk_subject_id` (`fk_subject_id`),
  ADD KEY `fk_module_number` (`module_number`);

--
-- Indexes for table `sections_tbl`
--
ALTER TABLE `sections_tbl`
  ADD PRIMARY KEY (`section_id`);

--
-- Indexes for table `students_progress_tbl`
--
ALTER TABLE `students_progress_tbl`
  ADD PRIMARY KEY (`progress_id`),
  ADD KEY `progress_student_id_idx` (`student_id`),
  ADD KEY `fk_game_mode` (`fk_game_mode_id`),
  ADD KEY `subject_id_progress` (`fk_subject_id`),
  ADD KEY `fk_module_id` (`module_id`);

--
-- Indexes for table `students_tbl`
--
ALTER TABLE `students_tbl`
  ADD PRIMARY KEY (`student_id`),
  ADD KEY `student_section_id_idx` (`fk_section_id`);

--
-- Indexes for table `subjects_tbl`
--
ALTER TABLE `subjects_tbl`
  ADD PRIMARY KEY (`subject_id`);

--
-- Indexes for table `teachers_tbl`
--
ALTER TABLE `teachers_tbl`
  ADD PRIMARY KEY (`teacher_id`),
  ADD KEY `teacher_section_id_idx` (`fk_section_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `classic_questions_tbl`
--
ALTER TABLE `classic_questions_tbl`
  MODIFY `question_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT for table `class_summary_tbl`
--
ALTER TABLE `class_summary_tbl`
  MODIFY `summary_id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `crossword_data`
--
ALTER TABLE `crossword_data`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `game_modes_tbl`
--
ALTER TABLE `game_modes_tbl`
  MODIFY `game_mode_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `game_mode_mapping_tbl`
--
ALTER TABLE `game_mode_mapping_tbl`
  MODIFY `mapping_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `jumbled_letters_questions`
--
ALTER TABLE `jumbled_letters_questions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT for table `lessons_tbl`
--
ALTER TABLE `lessons_tbl`
  MODIFY `lesson_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `levels_tbl`
--
ALTER TABLE `levels_tbl`
  MODIFY `level_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `modules_tbl`
--
ALTER TABLE `modules_tbl`
  MODIFY `module_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `sections_tbl`
--
ALTER TABLE `sections_tbl`
  MODIFY `section_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `students_progress_tbl`
--
ALTER TABLE `students_progress_tbl`
  MODIFY `progress_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT for table `students_tbl`
--
ALTER TABLE `students_tbl`
  MODIFY `student_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT for table `subjects_tbl`
--
ALTER TABLE `subjects_tbl`
  MODIFY `subject_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `teachers_tbl`
--
ALTER TABLE `teachers_tbl`
  MODIFY `teacher_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `classic_questions_tbl`
--
ALTER TABLE `classic_questions_tbl`
  ADD CONSTRAINT `classic_module_number` FOREIGN KEY (`fk_module_id`) REFERENCES `modules_tbl` (`module_number`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `classic_subject_id` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `class_summary_tbl`
--
ALTER TABLE `class_summary_tbl`
  ADD CONSTRAINT `summary_section_id` FOREIGN KEY (`fk_section_id`) REFERENCES `sections_tbl` (`section_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `summary_subject_id` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `crossword_data`
--
ALTER TABLE `crossword_data`
  ADD CONSTRAINT `crossword_module_id` FOREIGN KEY (`module_id`) REFERENCES `modules_tbl` (`module_number`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `crossword_subject_id` FOREIGN KEY (`subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `game_mode_mapping_tbl`
--
ALTER TABLE `game_mode_mapping_tbl`
  ADD CONSTRAINT `game_mode_mapping_tbl_ibfk_1` FOREIGN KEY (`fk_game_mode_id`) REFERENCES `game_modes_tbl` (`game_mode_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `game_mode_mapping_tbl_ibfk_2` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `game_mode_mapping_tbl_ibfk_3` FOREIGN KEY (`fk_module_id`) REFERENCES `modules_tbl` (`module_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `game_mode_mapping_tbl_ibfk_4` FOREIGN KEY (`fk_lesson_id`) REFERENCES `lessons_tbl` (`lesson_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `jumbled_letters_questions`
--
ALTER TABLE `jumbled_letters_questions`
  ADD CONSTRAINT `jumbled_module_id` FOREIGN KEY (`fk_module_id`) REFERENCES `modules_tbl` (`module_number`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `jumbled_subject_id` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `lessons_tbl`
--
ALTER TABLE `lessons_tbl`
  ADD CONSTRAINT `fk_module_number` FOREIGN KEY (`fk_module_number`) REFERENCES `modules_tbl` (`module_number`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk_subject_id` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `levels_tbl`
--
ALTER TABLE `levels_tbl`
  ADD CONSTRAINT `level_lesson_id` FOREIGN KEY (`fk_lesson_id`) REFERENCES `lessons_tbl` (`lesson_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `modules_tbl`
--
ALTER TABLE `modules_tbl`
  ADD CONSTRAINT `modules_subject_id` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `students_progress_tbl`
--
ALTER TABLE `students_progress_tbl`
  ADD CONSTRAINT `fk_game_mode_id` FOREIGN KEY (`fk_game_mode_id`) REFERENCES `game_modes_tbl` (`game_mode_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk_module_id` FOREIGN KEY (`module_id`) REFERENCES `modules_tbl` (`module_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `progress_student_id` FOREIGN KEY (`student_id`) REFERENCES `students_tbl` (`student_id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `subject_fk_id` FOREIGN KEY (`fk_subject_id`) REFERENCES `subjects_tbl` (`subject_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `students_tbl`
--
ALTER TABLE `students_tbl`
  ADD CONSTRAINT `student_section_id` FOREIGN KEY (`fk_section_id`) REFERENCES `sections_tbl` (`section_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `teachers_tbl`
--
ALTER TABLE `teachers_tbl`
  ADD CONSTRAINT `teacher_section_id` FOREIGN KEY (`fk_section_id`) REFERENCES `sections_tbl` (`section_id`) ON DELETE NO ACTION ON UPDATE NO ACTION;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
