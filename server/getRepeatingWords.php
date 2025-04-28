<?php
header('Content-Type: application/json');
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

include 'db_connection.php';

$repeatingWords = array();

$sql = "
    SELECT CAST(word AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci) AS value 
    FROM crossword_data 
    GROUP BY word 
    HAVING COUNT(*) > 1
    UNION
    SELECT CAST(answer AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci) AS value 
    FROM jumbled_letters_questions 
    GROUP BY answer 
    HAVING COUNT(*) > 1
    UNION
    SELECT CAST(answer AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci) AS value 
    FROM classic_questions_tbl 
    GROUP BY answer 
    HAVING COUNT(*) > 1
";

$result = $conn->query($sql);

if (!$result) {
    echo json_encode(["error" => "Query failed: " . $conn->error]);
    exit;
}

while ($row = $result->fetch_assoc()) {
    $repeatingWords[] = $row['value'];
}

echo json_encode(array_values(array_unique($repeatingWords)));
?>
