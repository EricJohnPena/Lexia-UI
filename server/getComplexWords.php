<?php
header('Content-Type: application/json');
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

include 'db_connection.php';

$complexWords = array();

$sql = "
    SELECT CAST(word AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci) AS value FROM crossword_data WHERE is_complex = 1
    UNION
    SELECT CAST(answer AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci) AS value FROM jumbled_letters_questions WHERE is_complex = 1
    UNION
    SELECT CAST(answer AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci) AS value FROM classic_questions_tbl WHERE is_complex = 1
";

$result = $conn->query($sql);

if (!$result) {
    echo json_encode(["error" => "Query failed: " . $conn->error]);
    exit;
}

while ($row = $result->fetch_assoc()) {
    $complexWords[] = $row['value'];
}

echo json_encode(array_values(array_unique($complexWords)));
?>
