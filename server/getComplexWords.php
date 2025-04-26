<?php
header('Content-Type: application/json');
include 'db_connection.php';

// Array to hold complex words from all tables
$complexWords = array();

// List of tables to query
$tables = ['crossword_data', 'jumbled_letters_questions', 'classic_questions_tbl']; 

foreach ($tables as $table) {
    $sql = "SELECT word FROM $table WHERE is_complex = 1";
    $result = $conn->query($sql);

    if ($result && $result->num_rows > 0) {
        while ($row = $result->fetch_assoc()) {
            $complexWords[] = $row['word'];
        }
    }
}

// Remove duplicates
$complexWords = array_unique($complexWords);

// Return JSON response
echo json_encode(array_values($complexWords));
?>
